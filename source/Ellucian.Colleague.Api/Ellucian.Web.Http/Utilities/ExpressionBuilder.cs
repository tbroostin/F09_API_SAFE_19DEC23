// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.Utilities
{
    /// <summary>
    /// Helper class for building Expressions
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Builds the sort Expression
        /// </summary>
        /// <param name="modelType">Type of base class</param>
        /// <param name="sortField">sort field</param>
        /// <returns></returns>
        public static Expression<Func<object, object>> BuildSortExpression(Type modelType, string sortField)
        {
            Expression<Func<object, object>> final = null;
            var param = Expression.Parameter(typeof(object), "p"); //Create parameter p of type object
            try
            {
                Tuple<Expression, Type> tupleExpressionType = BuildBaseExpression(modelType, sortField, param);
                Expression body = Expression.Convert(tupleExpressionType.Item1, typeof(object));//Get the final sort expression
                final = Expression.Lambda<Func<object, object>>(body, param);
            }
            catch (Exception)
            {
                throw;
            }
            return final;
        }

        /// <summary>
        /// Builds the Basic Filter Expression
        /// String comparison is case sensitive
        /// </summary>
        /// <param name="filterField">Filter Field</param>
        /// <param name="comparisonOperator">Comparison Operator</param>
        /// <param name="filterValue">Filter Value</param>
        /// <param name="modelType">Type of base class</param>
        /// <returns></returns>
        public static Expression BuildFilterExpression(string filterField, string comparisonOperator, string filterValue, Type modelType, ParameterExpression param)
        {
            Expression body = null;
            try
            {
                Tuple<Expression, Type> tupleExpressionType = BuildBaseExpression(modelType, filterField, param);
                body = tupleExpressionType.Item1;
                body = Expression.Convert(body, tupleExpressionType.Item2);
                var convertedValue = CastDataAs(tupleExpressionType.Item2, filterValue); //casting to the correct data type toDo: body.Type will be object for nested objects

                ConstantExpression constant = Expression.Constant(convertedValue, tupleExpressionType.Item2);

                switch (comparisonOperator.ToLower())
                {
                    case "$ne":
                        body = Expression.Not(Expression.Equal(body, constant));
                        break;
                    case "$lt":
                        body = Expression.LessThan(body, constant);
                        break;
                    case "$lte":
                        body = Expression.LessThanOrEqual(body, constant);
                        break;
                    case "$gt":
                        body = Expression.GreaterThan(body, constant);
                        break;
                    case "$gte":
                        body = Expression.GreaterThanOrEqual(body, constant);
                        break;
                    case "$eq":
                    case "":
                        body = Expression.Equal(body, constant);
                        break;
                    default:
                        throw new ArgumentException("Invalid Request. Please check the comparison operator");
                }
            }
            catch (Exception)
            {
                throw;
            }
            return body;
        }

        /// <summary>
        /// Overload of BuildFilterExpression - to build the expression for filtering based on array
        /// Note: String comparison is case sensitive
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <param name="filterArrayValue">The filter array value.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="param">The parameter.</param>
        /// <returns></returns>
        public static Expression BuildFilterExpression(string filterField, string comparisonOperator, List<object> filterArrayValue, Type modelType, ParameterExpression param)
        {
            Expression body = null;

            try
            {
                Tuple<Expression, Type> tupleExpressionType = BuildBaseExpression(modelType, filterField, param);
                body = tupleExpressionType.Item1;

                Expression nullConstant = Expression.Constant(null);

                Type listItemType = tupleExpressionType.Item2.GetTypeInfo().GenericTypeArguments[0]; //The type of items in list to be filtered

                //Create a new list of the correct type and load the items from filterArrayValue 
                Type listType = typeof(List<>).MakeGenericType(listItemType);
                IList myList = (IList)Activator.CreateInstance(listType);
                foreach (var item in filterArrayValue)
                { myList.Add(CastDataAs(listItemType, item.ToString())); }

                ConstantExpression constantArrayToCompare = Expression.Constant(myList);

                switch (comparisonOperator.ToLower())
                {
                    case "$exact":

                        MethodInfo sequenceEqualMethodInfo = typeof(Enumerable).GetMember("SequenceEqual", MemberTypes.Method,
                                                                BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).OfType<MethodInfo>()
                                                                .Single(x => x.GetParameters().Length == 2)
                                                                .MakeGenericMethod(listItemType);

                        var expressionWithNullCheck = Expression.Condition(
                                                        Expression.NotEqual(body, nullConstant),  //If the dto's list property is not a null - compare it with the filter array, if not - return false
                                                         Expression.Convert(Expression.Call(sequenceEqualMethodInfo, body, constantArrayToCompare), typeof(bool)),
                                                         Expression.Default(typeof(bool)));

                        body = expressionWithNullCheck;

                        break;
                    case "$all":
                        // Expression should be in format:  !listToCompare.Except(x.ListFromDb).Any())

                        MethodInfo exceptMethodInfo = typeof(Enumerable).GetMember("Except", MemberTypes.Method,
                                                         BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).OfType<MethodInfo>()
                                                         .Single(x => x.GetParameters().Length == 2)
                                                         .MakeGenericMethod(listItemType);

                        var any_MethodInfo = typeof(Enumerable).GetMember("Any", MemberTypes.Method,
                                                BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).OfType<MethodInfo>()
                                                .Single(x => x.GetParameters().Length == 1)
                                                .MakeGenericMethod(listItemType);

                        var finalExpressionWithNullCheck = Expression.Condition(
                                                           Expression.NotEqual(body, nullConstant), //If the dto's list property is not a null, compare with filter array. If not- return false
                                                                Expression.Not(Expression.Convert(Expression.Call(any_MethodInfo, Expression.Call(exceptMethodInfo, constantArrayToCompare, tupleExpressionType.Item1)), typeof(bool))),
                                                                Expression.Default(typeof(bool)));

                        body = finalExpressionWithNullCheck;
                        break;

                    default:
                        throw new ArgumentException("Invalid Request. Please check the operator");
                }
            }
            catch (Exception)
            {
                throw;
            }
            return body;
        }

        /// <summary>
        /// Generate the expression for both nested and non-nested objects for sorting and filtering
        /// </summary>
        /// <param name="modelType">Type of base class</param>
        /// <param name="memberName">Member name [Ex:Parentclass.childclass.propertyname OR propertyname]</param>
        /// <param name="paramExpression">Parameter Expression</param>
        /// <returns>Tuple of Expression and Member Type</returns>
        private static Tuple<Expression, Type> BuildBaseExpression(Type modelType, string memberName, ParameterExpression paramExpression)
        {
            Expression finalExpression = null;
            Type expressionReturnType = null;
            try
            {
                Expression lambdaParameter = Expression.Convert(paramExpression, modelType); //Convert the parameter p from object type to the model type
                Expression nullConstant = Expression.Constant(null);
                List<Expression> nullCheckExpressions = new List<Expression>();

                // always split, this code handles all cases in one pass
                var splitMembers = memberName.Split('.');
                expressionReturnType = modelType;

                // 1. build the property expression and any null checks (when nested)
                for (int i = 0; i < splitMembers.Length; i++)
                {
                    var member = splitMembers[i];
                    Tuple<string, Type> memberInfo = GetMemberInfo(expressionReturnType, member);
                    expressionReturnType = memberInfo.Item2;

                    if (finalExpression != null)
                    {
                        // nested...
                        // null check exp is the previous property expression since we are building left to right
                        nullCheckExpressions.Add(Expression.NotEqual(finalExpression, nullConstant));

                        // add next member to property expression 
                        finalExpression = Expression.PropertyOrField(finalExpression, memberInfo.Item1);
                    }
                    else
                    {
                        // assume this is it, until we loop around on nested
                        finalExpression = Expression.PropertyOrField(lambdaParameter, memberInfo.Item1);
                    }
                }

                // 2. rebuild the final expression with null checks, if needed
                if (nullCheckExpressions != null && nullCheckExpressions.Any())
                {
                    Expression nullCheckExpression = null;
                    Expression defaultExpression = Expression.Default(expressionReturnType);

                    nullCheckExpressions.Reverse(); // null check has to be built inside-out
                    foreach (var nullCheck in nullCheckExpressions)
                    {
                        if (nullCheckExpression != null)
                        {
                            // use existing null check expression, nest within another null check
                            nullCheckExpression = Expression.Condition(nullCheck, nullCheckExpression, defaultExpression);
                        }
                        else
                        {
                            // first/inner most null check that contains the property accessor
                            nullCheckExpression = Expression.Condition(nullCheck, finalExpression, defaultExpression);
                        }
                    }
                    finalExpression = nullCheckExpression;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return new Tuple<Expression, Type>(finalExpression, expressionReturnType);
            // return body;

        }


        /// <summary>
        /// Gets the MemeberInfo for different types[property, field, datamember prop or field, Json prop or field]
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="memberName"></param>
        /// <returns>Tuple of Member Name and Type</returns>
        private static Tuple<string, Type> GetMemberInfo(Type modelType, string memberName)
        {
            Tuple<string, Type> tupleMemberNameType = null;
            try
            {
                //Check for Property
                PropertyInfo matchingProperty = modelType.GetProperty(memberName,
                                                             System.Reflection.BindingFlags.Public |
                                                             System.Reflection.BindingFlags.Instance |
                                                             System.Reflection.BindingFlags.IgnoreCase);
                if (matchingProperty != null)
                    tupleMemberNameType = new Tuple<string, Type>(matchingProperty.Name, matchingProperty.PropertyType);

                //Check for fieldname
                if (tupleMemberNameType == null)
                {
                    FieldInfo matchingField = modelType.GetField(memberName, System.Reflection.BindingFlags.Public |
                                                                                   System.Reflection.BindingFlags.Instance |
                                                                                   System.Reflection.BindingFlags.IgnoreCase);

                    if (matchingField != null)
                        tupleMemberNameType = new Tuple<string, Type>(matchingField.Name, matchingField.FieldType);
                }
                //Check if any of the properties in the obj has a datamember name=propertyName
                if (tupleMemberNameType == null)
                {
                    PropertyInfo dataMemberProperty = modelType.GetProperties()
                                                        .Where(p => Attribute.IsDefined(p, typeof(DataMemberAttribute)))
                                                        .SingleOrDefault(p => ((DataMemberAttribute)Attribute
                                                            .GetCustomAttribute(p, typeof(DataMemberAttribute))).Name == memberName);
                    if (dataMemberProperty != null)
                        tupleMemberNameType = new Tuple<string, Type>(dataMemberProperty.Name, dataMemberProperty.PropertyType);
                }
                //Check if any of the fields in the obj has a datamember name=fieldName
                if (tupleMemberNameType == null)
                {
                    FieldInfo dataMemberField = modelType.GetFields()
                                                .Where(p => Attribute.IsDefined(p, typeof(DataMemberAttribute)))
                                                .SingleOrDefault(p => ((DataMemberAttribute)Attribute
                                                    .GetCustomAttribute(p, typeof(DataMemberAttribute))).Name == memberName);
                    if (dataMemberField != null)
                        tupleMemberNameType = new Tuple<string, Type>(dataMemberField.Name, dataMemberField.FieldType);
                }
                //Check if any of the properties in the obj has a jsonProperty name=propertyName
                if (tupleMemberNameType == null)
                {
                    PropertyInfo jsonProperty = modelType.GetProperties()
                                                    .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyAttribute)))
                                                    .SingleOrDefault(p => ((JsonPropertyAttribute)Attribute
                                                        .GetCustomAttribute(p, typeof(JsonPropertyAttribute))).PropertyName == memberName);
                    if (jsonProperty != null)
                        tupleMemberNameType = new Tuple<string, Type>(jsonProperty.Name, jsonProperty.PropertyType);
                }

                //Check if any of the fields in the obj has a jsonProperty name=fieldName
                if (tupleMemberNameType == null)
                {
                    FieldInfo jsonField = modelType.GetFields()
                                            .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyAttribute)))
                                            .SingleOrDefault(p => ((JsonPropertyAttribute)Attribute
                                                .GetCustomAttribute(p, typeof(JsonPropertyAttribute))).PropertyName == memberName);
                    if (jsonField != null)
                        tupleMemberNameType = new Tuple<string, Type>(jsonField.Name, jsonField.FieldType);
                }

                //Check for Match found, if exists, check for nullable type if so return the base type
                //if no match found, throw Invalid request exception
                if (tupleMemberNameType == null)
                {
                    throw new ArgumentException("Invalid Request. Please check the query parameters");
                }

            }

            catch (Exception)
            {
                throw;
            }
            return tupleMemberNameType;
        }

        /// <summary>
        /// Convert the value to correct data type for filterng value
        /// </summary>
        /// <param name="propertyType">property type</param>
        /// <param name="value">value to convert</param>
        /// <returns></returns>
        private static object CastDataAs(Type propertyType, string value)
        {
            try
            {
                if (value == "null")
                    return null;

                if (Nullable.GetUnderlyingType(propertyType) != null)
                    propertyType = Nullable.GetUnderlyingType(propertyType);


                if (propertyType == typeof(bool))
                    return bool.Parse(value);

                if (propertyType == typeof(string))
                    return value;

                if (propertyType == typeof(int))
                    return int.Parse(value);

                if (propertyType == typeof(float))
                    return float.Parse(value);

                if (propertyType == typeof(decimal))
                    return decimal.Parse(value);

                if (propertyType == typeof(double))
                    return double.Parse(value);

                if (propertyType == typeof(long))
                    return long.Parse(value);

                if (propertyType == typeof(DateTime))
                    return DateTime.Parse(value);

                if (propertyType.IsEnum)
                    try
                    {
                        return Enum.Parse(propertyType, GetEnumValue(propertyType, value)); //If property is Enum with EnumMember, the incoming  value will be the Enum attribute value
                    }
                    catch (Exception)
                    { throw new ArgumentException("Invalid request. Please check the filter values"); }
            }
            catch (Exception) { throw; }
            return value;
        }

        /// <summary>
        /// Gets the enum value of the enum property type. If the value has an EnumMember attribute, the original value is returned for Enum.Parse
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="value">The value that has to be compared with</param>
        /// <returns></returns>
        private static string GetEnumValue(Type enumType, string value)
        {
            foreach (var name in Enum.GetNames(enumType)) //Loop through all values in enum
            {
                if (name == value) return value;

                //If the Enum has EnumMember attribute, the original value should be returned
                if (enumType.GetField(name).GetCustomAttributes().Any())
                {
                    var enumMemberAttribute = enumType.GetField(name).GetCustomAttribute(typeof(EnumMemberAttribute));
                    if (enumMemberAttribute != null && enumMemberAttribute is EnumMemberAttribute)
                    { 
                        var enumAttributeValue = ((EnumMemberAttribute)enumMemberAttribute).Value;
                        if (enumAttributeValue == value) return name;
                    }

                }
            }

            return value;
        }
    }

}
