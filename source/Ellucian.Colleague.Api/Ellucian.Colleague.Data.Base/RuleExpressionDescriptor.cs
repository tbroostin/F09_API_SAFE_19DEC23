// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Intermediate description of a single rule "line" from Colleague
    /// </summary>
    public class RuleExpressionDescriptor
    {
        /// <summary>
        /// Gets or sets the connector.
        /// </summary>
        /// <value>
        /// The connector.
        /// </value>
        public string Connector { get; set; }

        /// <summary>
        /// Gets or sets the data element.
        /// </summary>
        /// <value>
        /// The data element.
        /// </value>
        public RuleDataElement DataElement { get; set; }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the literal.
        /// </summary>
        /// <value>
        /// The literal.
        /// </value>
        public string Literal { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Connector + " " + DataElement.Id + " " + Operator + " " + Literal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleExpressionDescriptor"/> class.
        /// </summary>
        public RuleExpressionDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleExpressionDescriptor"/> class.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <param name="dataElementId">The data element identifier.</param>
        /// <param name="op">The op.</param>
        /// <param name="literal">The literal.</param>
        public RuleExpressionDescriptor(string connector, string dataElementId, string op, string literal)
        {
            this.Connector = connector;
            this.DataElement = new RuleDataElement() { Id = dataElementId };
            this.Operator = op;
            this.Literal = literal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleExpressionDescriptor"/> class.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <param name="dataElement">The data element.</param>
        /// <param name="op">The op.</param>
        /// <param name="literal">The literal.</param>
        public RuleExpressionDescriptor(string connector, RuleDataElement dataElement, string op, string literal)
        {
            this.Connector = connector;
            this.DataElement = dataElement;
            this.Operator = op;
            this.Literal = literal;
        }

        /// <summary>
        /// Return the expression based in the provided left-hand-side and right-hand-side expressions.
        /// </summary>
        /// <param name="lhs">The left-hand-side expression.</param>
        /// <param name="rhs">The right-hand-side expression.</param>
        /// <returns></returns>
        public Expression ToExpression(Expression lhs, Expression rhs)
        {
            switch (Operator)
            {
                case RuleOperators.Equal:
                    return Expression.Equal(lhs, rhs);
                case RuleOperators.GreaterThan:
                    return Expression.GreaterThan(lhs, rhs);
                case RuleOperators.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(lhs, rhs);
                case RuleOperators.LessThan:
                    return Expression.LessThan(lhs, rhs);
                case RuleOperators.LessThanOrEqual:
                    return Expression.LessThanOrEqual(lhs, rhs);
                case RuleOperators.Like:
                    return null;
                case RuleOperators.NotEqual:
                    return Expression.NotEqual(lhs, rhs);
                default:
                    return null;
            }
        } 
    }
}
