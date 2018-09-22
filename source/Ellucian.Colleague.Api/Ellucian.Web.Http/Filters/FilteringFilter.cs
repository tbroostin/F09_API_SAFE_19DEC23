// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web;
using System.Net.Http;
using System.Net.Http.Formatting;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Extensions;
using System.Collections;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ellucian.Web.Http.Utilities;
using System.Net;
using slf4net;
using System.Collections.Specialized;
using System.IO;

namespace Ellucian.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    /// <summary>
    /// Action filter for filtering
    /// </summary>
    public class FilteringFilter : System.Web.Http.Filters.ActionFilterAttribute
    {

        private ILogger logger;
        public ILogger Logger { get { return logger; } }

        /// <summary>
        /// Initialize a new instance of the <see cref="FilteringFilter"/> class.
        /// </summary>
        public FilteringFilter()
        {
            IgnoreFiltering = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteringFilter"/> class with logger
        /// </summary>
        /// <param name="logger"></param>
        public FilteringFilter(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Get and Set the property to override the Filtering at ActionFilter level
        /// </summary>
        /// <returns></returns>
        public bool IgnoreFiltering { get; set; }

        /// <summary>
        /// Filters the list for all filtering requests
        /// </summary>
        /// <param name="context">context</param>
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            //Resolve the logger instance directly from the Unity IoC Container
            if (this.logger == null)
            {
                this.logger = context.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;
            }

            bool isEnumerable = false;
            try
            {
                //Check for Response object NOT NULL
                if (context.Response != null)
                {
                    if (context.Response.Content != null && context.Response.Content is ObjectContent)
                    {
                        var objectContent = context.Response.Content as ObjectContent;
                        if (objectContent != null)
                        {
                            if (objectContent.ObjectType.IsGenericType)
                            {

                                //Check if ObjectContent is IEnumerable OR Implements IEnumerable
                                isEnumerable = objectContent.ObjectType.Name == "IEnumerable`1" || objectContent.ObjectType.GetInterface("IEnumerable`1") != null;

                                //Check for Return Type
                                if (isEnumerable)
                                {
                                    IEnumerable<object> model = null;
                                    context.Response.TryGetContentValue(out model);
                                    if (model != null)
                                    {
                                        if (!IgnoreFiltering)
                                        {
                                            //Get the filter parameters from query string 
                                            var queryString = HttpUtility.ParseQueryString(context.Request.RequestUri.Query);
                                            if (queryString["filter"] != null)
                                            {
                                                //Retrieve the json values from querystring
                                                var parsedQueryString = HttpUtility.ParseQueryString(queryString["filter"]);
                                                //Decode the encoded string
                                                var decodedValue = HttpUtility.UrlDecode(parsedQueryString.ToString());

                                                //filtering logic
                                                Expression filterExpression;
                                                Type modeltype = model.FirstOrDefault().GetType(); //Get object type

                                                ParameterExpression param = Expression.Parameter(typeof(object), "p"); //Create parameter p of type object
                                                                                                                       //Override the default JObject.Parse which ignores duplicate proprty keys
                                                JToken parsedString = null;
                                                using (StringReader srdr = new StringReader(decodedValue))
                                                {
                                                    using (JsonTextReader jrdr = new JsonTextReader(srdr))
                                                    {
                                                        while (jrdr.Read())
                                                        {
                                                            if (jrdr.TokenType == JsonToken.StartObject)
                                                                parsedString = ParseJsonString(jrdr);
                                                        }
                                                    }
                                                }

                                                filterExpression = CreateExpression(parsedString, modeltype, param);

                                                var finalExpression = Expression.Lambda<Func<object, bool>>(filterExpression, param);

                                                model = model.AsQueryable().Where(finalExpression);
                                                context.Response.Content = new ObjectContent<IEnumerable<dynamic>>(model, new JsonMediaTypeFormatter());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                context.Response.SetErrorResponse(ex.Message);
            }
            base.OnActionExecuted(context);
        }


        /// <summary>
        /// Creates final expression needed for filtering
        /// Note: String comparison is sensitive
        /// </summary>
        /// <param name="queryString">filter parameter value from querystring</param>
        /// <param name="parameterType">Type of base class</param>
        /// <param name="param">parameter expression of p type</param>
        /// <returns>return the final filtering expression</returns>
        public Expression CreateExpression(object queryString, Type parameterType, ParameterExpression param)
        {
            Expression body = null;
            try
            {
                //Check if the qs has an "$or" operator or else assign "$And"
                var mainOperator = (!((JObject)queryString).Properties().FirstOrDefault().Name.Equals("$or", StringComparison.InvariantCultureIgnoreCase)) ? "$and" : ((JObject)queryString).Properties().FirstOrDefault().Name;

                //run through all the children in entire query string
                foreach (JToken property in ((JObject)queryString).Children())
                {
                    var key = ((JProperty)property).Name;
                    var val = ((JProperty)property).Value;

                    if (val is JArray)
                    {
                        Expression arrExpression = null;
                        var arrayItemType = val.Children().FirstOrDefault().GetType();

                        if (arrayItemType == typeof(JValue)) //Items in the array are JValue (eg: [red, blue, green] - for array Compare)
                        {
                            arrExpression = ExpressionHelper.BuildFilterExpression(key, "$exact", GenerateArrayToCompare(val.Children()), parameterType, param);
                        }
                        else //Items in the array are JObject (eg:[ Age : {$gt:20 }, { Name : Jane}]) - recurse till JValue is reached
                        {

                            foreach (JObject item in val.Children<JToken>())
                            {
                                Expression subExpression = CreateExpression(item, parameterType, param);
                                arrExpression = AppendExpression(arrExpression, subExpression, key);
                            }
                        }
                        body = AppendExpression(body, arrExpression, mainOperator);
                    }
                    else if (val is JObject)
                    {
                        //eg: {ne:15}
                        Expression objectExpression = null;
                        var firstChildInVal = ((JProperty)val.Children().First()).Name;

                        if (!firstChildInVal.StartsWith("$")) //{"nesteditem" : {"name" : "item1", "code" : "001"}} : the first child in this case will not begin with $
                        {
                            foreach (JProperty child in val.Children())
                            {
                                //modify fieldname to include the class name as well eg: nestedItem.name
                                var fullName = String.Format("{0}.{1}", key, child.Name);
                                JObject obj = new JObject(new JProperty(fullName, child.Value));
                                Expression subExpression = CreateExpression(obj, parameterType, param);
                                objectExpression = AppendExpression(objectExpression, subExpression, key);
                            }
                        }
                        else
                        {
                            foreach (JProperty objProp in ((JObject)val).Children())
                            {
                                var CompOper = objProp.Name;

                                //check if the value is an JArray or JValue
                                var data = objProp.Value;

                                if (data.GetType() == typeof(JArray)) //eg: {$all:[a, b, c]}
                                {
                                    objectExpression = ExpressionHelper.BuildFilterExpression(key, CompOper, GenerateArrayToCompare(data.Children()), parameterType, param);
                                }
                                else  //eg: {$ne:15} 
                                {
                                    Expression objExpression = ExpressionHelper.BuildFilterExpression(key, CompOper, data.ToString(), parameterType, param);
                                    objectExpression = AppendExpression(objectExpression, objExpression, key);
                                }
                            }
                        }
                        body = AppendExpression(body, objectExpression, key);
                    }
                    else //eg: {x:5} with no operator
                    {
                        //If field is compared to null instead of "null" in querystring, force it here
                        Expression simpleExpression = ExpressionHelper.BuildFilterExpression(key, string.Empty, val.Type == JTokenType.Null ? "null" : val.ToString(), parameterType, param);
                        body = AppendExpression(body, simpleExpression, key);
                    }
                }
            }
            catch (Exception) { throw; }
            return body;
        }

        /// <summary>
        /// Combines the two expressions with the logical operator
        /// </summary>
        /// <param name="originalExpression">left hand side of the expression</param>
        /// <param name="expressionToAppend">right hand side of the expression</param>
        /// <param name="logicalOperator">logical operator (OR, AND)</param>
        /// <returns></returns>
        private static Expression AppendExpression(Expression originalExpression, Expression expressionToAppend, string logicalOperator)
        {
            if (originalExpression == null)
                originalExpression = expressionToAppend;
            else
            {
                if (logicalOperator.Equals("$or", StringComparison.InvariantCultureIgnoreCase))
                    originalExpression = Expression.OrElse(originalExpression, expressionToAppend);
                else
                    originalExpression = Expression.AndAlso(originalExpression, expressionToAppend);
            }
            return originalExpression;
        }

        /// <summary>
        /// Generates the array to compare.
        /// </summary>
        /// <param name="jarray">The jarray.</param>
        /// <returns></returns>
        private List<object> GenerateArrayToCompare(JEnumerable<JToken> jarray)
        {
            List<object> arrayToCompare = new List<object>();
            foreach (JValue item in jarray)
            {
                arrayToCompare.Add(item.ToString());
            }

            return arrayToCompare;
        }

        /// <summary>
        /// Parses the json string. Overrides the JObject.Parse method, since the default parsing removes all except the last value if duplicate keys are present
        /// If duplicate key is present, they are converted to an array. If duplicate arrays are present, all values are combined into single array.
        /// The final format should be:  $and : [{key:oldvalue}, {key:newvalue}] 
        /// </summary>
        /// <param name="reader">The json text reader.</param>
        /// <returns></returns>

        public static JToken ParseJsonString(JsonTextReader reader)
        {
            if (reader.TokenType == JsonToken.None)
            {
                reader.Read();
            }

            if (reader.TokenType == JsonToken.StartObject) //Any value enclosed in { }
            {
                reader.Read();

                JObject obj = new JObject();

                while (reader.TokenType != JsonToken.EndObject)
                {
                    string propName = (string)reader.Value; //Get the property name of the jobject
                    reader.Read();

                    JToken newValue = ParseJsonString(reader); //loop through to get the value to be filtered by - can either be an array/simple value/value with comparison operator 

                    //If an array has already been created for the duplicates, append to that array
                    JToken arrayWithDuplicates = null;
                    if (obj["$and"] != null)
                    {
                        if (obj["$and"] is JArray && obj["$and"][0] is JObject)
                        {
                            JToken firstTokenInArray = ((JObject)obj["$and"][0]).First;
                            var propertyName = firstTokenInArray != null ? ((JProperty)firstTokenInArray).Name : String.Empty;
                            if (propertyName == propName) arrayWithDuplicates = obj["$and"];
                        }
                    }

                    JToken oldValue = obj[propName] ?? arrayWithDuplicates;//check if the property already exists in the json query: either with the propName or an array with $and

                    if (oldValue == null)
                    {
                        obj.Add(new JProperty(propName, newValue));
                    }
                    else if (oldValue.Type == JTokenType.Array)
                    {
                        if (newValue.Type == JTokenType.Array) //If new value is array, add all items to the existing array
                        {
                            foreach (JToken child in newValue.Children())
                                ((JArray)oldValue).Add(child);
                        }
                        else
                        {
                            JObject newJObj = new JObject();
                            newJObj.Add(propName, newValue);
                            ((JArray)oldValue).Add(newJObj); //If the property already exists in query as an and array, add the new value to the existing array
                        }

                    }
                    else
                    {
                        // Convert existing simple value to an array and add the duplicate value to the array
                        JProperty combinedProperty = new JProperty("$and", null);
                        JArray combinedArray = new JArray();
                        combinedProperty.Value = combinedArray; //assign the array as the value to the key

                        JObject oldJObj = new JObject();
                        oldJObj.Add(propName, oldValue);
                        combinedArray.Add(oldJObj);

                        JObject newJObj = new JObject();//Add the duplicate value to the and array
                        newJObj.Add(propName, newValue);
                        combinedArray.Add(newJObj);

                        obj.Remove(propName);//Replace the existing single value with the and array
                        obj.Add(combinedProperty);
                    }

                    reader.Read();
                }
                return obj;
            }

            if (reader.TokenType == JsonToken.StartArray) //if value is an array
            {
                reader.Read();
                JArray array = new JArray();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    array.Add(ParseJsonString(reader));
                    reader.Read();
                }
                return array;
            }

            return new JValue(reader.Value);
        }

    }


}



