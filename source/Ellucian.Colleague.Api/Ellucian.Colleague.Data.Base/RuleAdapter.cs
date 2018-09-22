// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Base class from which rule adapters should extend. Provides expression parsing.
    /// </summary>
    public abstract class RuleAdapter : IRuleAdapter
    {
        private static MethodInfo _collectionContainsMethod = typeof(Enumerable).GetMethods().Where(m => m.Name == "Contains").Where(m => m.IsStatic == true).FirstOrDefault();
        private static MethodInfo _stringContainsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        private static MethodInfo _stringEndsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        private static MethodInfo _stringStartsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static Type _enumerableStringCollection = typeof(IEnumerable<string>);

        /// <summary>
        /// Creates an object to represent the specified rule. If the rule can be executed in .NET,
        /// the rule's expression shall be populated.
        /// </summary>
        /// <param name="descriptor">the rule data from Colleague</param>
        /// <returns>
        /// a rule
        /// </returns>
        /// <exception cref="System.ArgumentNullException">descriptor</exception>
        /// <exception cref="System.ArgumentException"></exception>
        public Rule Create(RuleDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            if (descriptor.PrimaryView != ExpectedPrimaryView)
            {
                throw new ArgumentException(this.GetType().Name + " expected a primary view of " + ExpectedPrimaryView + ", got " + descriptor.PrimaryView);
            }

            string unsupported = descriptor.NotSupportedMessage;
            if (descriptor.Expressions.Count == 0)
            {
                unsupported = "No expressions";
            }
            var exprList = new List<Expression>();
            Expression finalExpression = null;
            var param = Expression.Parameter(ContextType, "c");
            for (int i = 0; i < descriptor.Expressions.Count && unsupported == null; i++)
            {
                var line = descriptor.Expressions[i];
                Expression lhs = CreateDataElementExpression(line.DataElement, param, out unsupported);

                // Set a flag if the property in the lhs expression is a collection of any type
                bool propertyIsGenericCollection = false;
                var propertyType = typeof(string);
                if (lhs != null)
                {
                    if (lhs is MemberExpression)
                    {
                        var memInfo = ((MemberExpression)lhs).Member;
                        propertyType = ((PropertyInfo)memInfo).PropertyType;

                        // Verify we are generic
                        if ((propertyType.IsGenericType))
                        {
                            // Verify the lhs property can be assigned to an enumerable string collection type
                            if (_enumerableStringCollection.IsAssignableFrom(propertyType))
                            {
                                // Verify it implements the ICollection interface
                                Type[] interfaceTypes = propertyType.GetInterfaces();
                                if (interfaceTypes.Contains(typeof(ICollection)))
                                {
                                    propertyIsGenericCollection = true;
                                }
                            }
                        }
                    }
                }

                // We're only expecting literals.. data elements on the RHS can come later
                if (!line.Literal.Contains("'") && !line.Literal.Contains("\""))
                {
                    unsupported = "Data elements on the RHS not supported yet";
                }

                // Only support ienumerable operation with Equal operator
                if (propertyIsGenericCollection && line.Operator != RuleOperators.Equal)
                {
                    unsupported = "Rule operator " + line.Operator + " not supported yet on List data elements";
                }

                if (unsupported != null)
                {
                    break;
                }

                // Add a comma to multiple quote-delimited values as needed to provide a splitting point
                line.Literal = line.Literal.Replace("\"\"", "\",\"");
                line.Literal = line.Literal.Replace("\'\'", "\',\'");

                // Split values based on the comma
                var literals = line.Literal.Split(',');

                // Remove quotes and extraneous spaces from each value 
                for (int j = 0; j < literals.Length; j++)
                {
                    // trim any spaces before/after quotes
                    literals[j] = literals[j].Trim();
                    // remove single quotes
                    literals[j] = literals[j].Trim('\'');
                    // remove double quotes
                    literals[j] = literals[j].Trim('\"');
                    // remove leading and trailing spaces within quotes
                    literals[j] = literals[j].Trim();
                }

                Expression theExpr = Expression.Constant(false);
                foreach (var literal in literals)
                {
                    Expression rhs;

                    try
                    {
                        // Build constant expression from the literal, parsed to the needed type (string/decimal/date)
                        rhs = BuildConstantExpression(literal, propertyType, descriptor.RuleConversionOptions);
                    }
                    catch (Exception ex)
                    {
                        unsupported = ex.Message;
                        break;
                    }

                    Expression expr = null;
                    switch (line.Operator)
                    {
                        case RuleOperators.Equal:
                            if (propertyIsGenericCollection)
                            {
                                // Get the generic method, then update with type string
                                var method = _collectionContainsMethod;
                                MethodInfo mi = method.MakeGenericMethod(new Type[] { typeof(string) });
                                expr = Expression.Call(mi, lhs, rhs);
                            }
                            else
                            {
                                expr = Expression.Equal(lhs, rhs);
                            }
                            break;
                        case RuleOperators.NotEqual:
                            expr = Expression.NotEqual(lhs, rhs);
                            break;
                        case RuleOperators.GreaterThan:
                            expr = BuildCompareExpression(lhs, rhs, propertyType);
                            expr = Expression.GreaterThan(expr, Expression.Constant(0));
                            break;
                        case RuleOperators.GreaterThanOrEqual:
                            expr = BuildCompareExpression(lhs, rhs, propertyType);
                            expr = Expression.GreaterThanOrEqual(expr, Expression.Constant(0));
                            break;
                        case RuleOperators.LessThan:
                            expr = BuildCompareExpression(lhs, rhs, propertyType);
                            expr = Expression.LessThan(expr, Expression.Constant(0));
                            break;
                        case RuleOperators.LessThanOrEqual:
                            expr = BuildCompareExpression(lhs, rhs, propertyType);
                            expr = Expression.LessThanOrEqual(expr, Expression.Constant(0));
                            break;
                        case RuleOperators.Like:
                            if (literal.StartsWith("...") && literal.EndsWith("..."))
                            {
                                rhs = Expression.Constant(literal.Substring(3, literal.Length - 6));
                                MethodInfo mi = _stringContainsMethod;
                                expr = Expression.Call(lhs, mi, rhs);
                            }
                            else if (literal.StartsWith("..."))
                            {
                                rhs = Expression.Constant(literal.Substring(3));
                                MethodInfo mi = _stringEndsWithMethod;
                                expr = Expression.Call(lhs, mi, rhs);
                            }
                            else if (literal.EndsWith("..."))
                            {
                                rhs = Expression.Constant(literal.Substring(0, literal.Length - 3));
                                MethodInfo mi = _stringStartsWithMethod;
                                expr = Expression.Call(lhs, mi, rhs);
                            }
                            else if (!literal.Contains("..."))
                            {
                                expr = Expression.Equal(lhs, rhs);
                            }
                            else
                            {
                                unsupported = "Wildcard syntax in place other than beginning or end not supported yet";
                            }
                            break;
                        case RuleOperators.Unlike:
                            if (literal.StartsWith("...") && literal.EndsWith("..."))
                            {
                                rhs = Expression.Constant(literal.Substring(3, literal.Length - 6));
                                MethodInfo mi = _stringContainsMethod;
                                expr = Expression.Call(lhs, mi, rhs);
                                expr = Expression.Not(expr);
                            }
                            else if (literal.StartsWith("..."))
                            {
                                rhs = Expression.Constant(literal.Substring(3));
                                MethodInfo mi = _stringEndsWithMethod;
                                expr = Expression.Call(lhs, mi, rhs);
                                expr = Expression.Not(expr);
                            }
                            else if (literal.EndsWith("..."))
                            {
                                rhs = Expression.Constant(literal.Substring(0, literal.Length - 3));
                                MethodInfo mi = _stringStartsWithMethod;
                                expr = Expression.Call(lhs, mi, rhs);
                                expr = Expression.Not(expr);
                            }
                            else if (!literal.Contains("..."))
                            {
                                expr = Expression.Equal(lhs, rhs);
                                expr = Expression.Not(expr);
                            }
                            else
                            {
                                unsupported = "Wildcard syntax in place other than beginning or end not supported yet";
                            }
                            break;
                        default:
                            unsupported = "Rule operator " + line.Operator + " not supported yet";
                            break;
                    }
                    if (unsupported != null)
                    {
                        break;
                    }
                    theExpr = Expression.Or(theExpr, expr);
                }
                if (theExpr != null)
                {
                    exprList.Add(theExpr);
                    //Console.WriteLine(expr);
                }
            }


            if (unsupported == null)
            {
                Expr current = null;

                // Assume one level of nesting
                // First pass
                List<Expr> exprs = new List<Expr>();
                for (int i = 0; i < exprList.Count && unsupported == null; i++)
                {
                    var connector = descriptor.Expressions[i].Connector;
                    switch (connector)
                    {
                        case RuleConnectors.OrWith:
                            if (current != null)
                            {
                                exprs.Add(current);
                                current = null;
                            }

                            current = new Expr() { Connective = connector };
                            current.Expression = Expression.Or(Expression.Constant(false), exprList[i]);
                            break;
                        case RuleConnectors.With:
                            if (current != null)
                            {
                                exprs.Add(current);
                            }
                            current = new Expr() { Connective = connector };
                            current.Expression = Expression.And(Expression.Constant(true), exprList[i]);
                            break;
                        case RuleConnectors.And:
                            if (current == null)
                            {
                                // Huh?
                                unsupported = "AND as first connector - not supported yet";
                            }
                            else
                            {
                                current.Expression = Expression.And(current.Expression, exprList[i]);
                            }
                            break;
                        case RuleConnectors.Or:
                            if (current == null)
                            {
                                // Huh?
                                unsupported = "OR as first connector - not supported yet";
                            }
                            else
                            {
                                current.Expression = Expression.Or(current.Expression, exprList[i]);
                            }
                            break;
                        default:
                            unsupported = "Rule connector " + connector + " not supported yet";
                            break;
                    }

                }
                if (current != null)
                {
                    exprs.Add(current);
                }

                // Second pass
                List<Expr> reduced = new List<Expr>();
                foreach (var expr in exprs)
                {
                    switch (expr.Connective)
                    {
                        case RuleConnectors.With:
                            if (finalExpression == null)
                            {
                                finalExpression = Expression.And(Expression.Constant(true), expr.Expression);
                            }
                            else
                            {
                                finalExpression = Expression.And(finalExpression, expr.Expression);
                            }
                            break;
                        case RuleConnectors.OrWith:
                            if (finalExpression == null)
                            {
                                finalExpression = Expression.Or(Expression.Constant(false), expr.Expression);
                            }
                            else
                            {
                                finalExpression = Expression.Or(finalExpression, expr.Expression);
                            }
                            break;
                        default:
                            // Odd...
                            unsupported = "Hit default case in second pass through expression";
                            break;
                    }
                }
            }

            var rule = CreateExpressionAndRule(descriptor.Id, finalExpression, param);
            if (unsupported != null)
            {
                rule.NotSupportedMessage = unsupported;
            }
            return rule;
        }

        private static Expression BuildCompareExpression(Expression expr1, Expression expr2, Type type)
        {
            if (type == null)
                type = typeof(string);
            if (type == typeof(decimal?))
            {
                MethodInfo mi = typeof(RuleAdapter).GetMethod("CompareNullableDecimals");
                return Expression.Call(
                             mi,
                               expr1, expr2 );
            }
            else
            {
                return Expression.Call(type,
                              "Compare",
                              null,
                              new[] { expr1, expr2 });
            }
        }

        private static Expression BuildConstantExpression(string literal, Type propertyType, RuleConversionOptions options)
        {
            if (propertyType == typeof(decimal))
            {
              return Expression.Constant(decimal.Parse(literal));
              
            }
            else if(propertyType == typeof(decimal?))
            {
                decimal tmpvalue;
                decimal? convertedLiteral = null;
                if (decimal.TryParse(literal, out tmpvalue))
                {
                    convertedLiteral = tmpvalue;
                }
               return Expression.Constant(convertedLiteral, typeof(decimal?));
            }
            else if (propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
            {
                string[] dateParts = literal.Split(options.DateDelimiter.ElementAt(0));
                DateTime dateTimeConst;
                switch (options.DateFormat)
                {
                    case "MDY":
                        dateTimeConst = new DateTime(ConvertYear(int.Parse(dateParts[2]), options.CenturyThreshhold), int.Parse(dateParts[0]), int.Parse(dateParts[1]));
                        break;
                    case "YMD":
                        dateTimeConst = new DateTime(ConvertYear(int.Parse(dateParts[0]), options.CenturyThreshhold), int.Parse(dateParts[1]), int.Parse(dateParts[2]));
                        break;
                    case "DMY":
                        dateTimeConst = new DateTime(ConvertYear(int.Parse(dateParts[2]), options.CenturyThreshhold), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                        break;
                    default:
                        throw new ArgumentException("Date format must be MDY or YMD for rule evaluation");
                }
                return Expression.Constant((DateTime)dateTimeConst);
            }
            else
            {
                return Expression.Constant(literal);
            }
        }

        // In case a year is defined on a rule with only 2 digits for the year, assume a 2000s year if the
        // value is less than 68, and a 1900 year if the year is 68 - 100. (This mirrors the two digit year assumptions in Unidata.)
        private static int ConvertYear(int year, int twoDigitYearThreshhold)
        {
            if (year < 100)
            {
                if (year < twoDigitYearThreshhold)
                {
                    year += 2000;
                }
                else
                {
                    year += 1900;
                }
            }
            return year;
        }

        /// <summary>
        /// Returns the record ID for the specified context object. This is used when mapping
        /// from a rule back to Colleague for execution in Envision.
        /// </summary>
        /// <param name="context">must be object of the context type that this adapter deals with</param>
        /// <returns></returns>
        public abstract string GetRecordId(object context);

        /// <summary>
        /// Returns the Colleague file suite instance of the specified context object. This is used for objects that are equivalent to
        /// Colleague file suite specific objects so that we can map a file suite rule 
        /// back to Colleague for execution in Envision. For non file suite type objects, this
        /// method should return an empty string.
        /// </summary>
        /// <param name="context">must be an object of the context type that this adapter deals with</param>
        /// <returns>The default implementation returns an empty string</returns>
        public virtual string GetFileSuiteInstance(object context)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the context type.
        /// </summary>
        public abstract Type ContextType { get; }

        /// <summary>
        /// Gets the expected primary view.
        /// </summary>
        public abstract string ExpectedPrimaryView { get; }

        /// <summary>
        /// Implementations must override this method to create a Rule object for the specified id,
        /// and a lambda expression object which can be compiled to execute the specified expression.
        /// </summary>
        /// <param name="ruleId">the rule ID</param>
        /// <param name="finalExpression">an expression for the rule, may be null if not supported</param>
        /// <param name="contextExpression">an expression for the context object, may be null</param>
        /// <returns>always returns a Rule object, even for unsupported rules</returns>
        protected abstract Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression);

        /// <summary>
        /// Implementations must override this method to create an expression for the specified data element.
        /// </summary>
        /// <param name="dataElement">the data element</param>
        /// <param name="param">a parameter expression representing the context that will be passed at runtime</param>
        /// <param name="unsupportedMessage">should be set if the data element is not supported</param>
        /// <returns>an expression for the left hand side of the rule</returns>
        protected abstract Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage);

        /// <summary>
        /// Internal class for temporary usage.
        /// </summary>
        private class Expr
        {
            public string Connective { get; set; }
            public Expression Expression { get; set; }
        }

        /// <summary>
        /// This is to compare Nullable decimal values
        /// </summary>
        /// <param name="expr1">First nullable decimal value</param>
        /// <param name="expr2">Second nullable decimal value</param>
        /// <returns></returns>
        public static int CompareNullableDecimals(decimal? expr1, decimal? expr2 )
        {
            decimal val1 = expr1 ?? -1m;
            decimal val2 = expr2 ?? -1m;
            return  val1 > val2 ? 1 : val1 < val2 ? -1 : 0;
        }
    
    }
}
