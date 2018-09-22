// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Serialize.Linq.Serializers;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Base class representing a Colleague rule.
    /// </summary>
    [Serializable]
    public abstract class Rule
    {
        private readonly string _id;

        /// <summary>
        /// Rule identifier, for example DA.ENGL
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Gets or sets the not supported message.
        /// </summary>
        /// <value>
        /// The not supported message.
        /// </value>
        public string NotSupportedMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.ArgumentNullException">id</exception>
        protected Rule(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            _id = id;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Rule))
            {
                return false;
            }
            var other = obj as Rule;
            return other._id.Equals(_id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _id;
        }
    }

    /// <summary>
    /// Provides a boolean answer to an expression evaluated against a context.
    /// </summary>
    [Serializable]
    public class Rule<T> : Rule
    {
        [NonSerialized]
        private Func<T, bool> _compiledDelegate;

        private readonly byte[] _serializedLambdaExpression;
        private static ExpressionSerializer _expressionSerializer = new ExpressionSerializer(new BinarySerializer());

        /// <summary>
        /// Initializes a new instance of the Rule class.
        /// </summary>
        /// <param name="id">rule ID, must not be null or empty</param>
        /// <param name="lambdaExpression">The lambda expression.</param>
        public Rule(string id, Expression<Func<T, bool>> lambdaExpression = null)
            : base(id)
        {
            if (lambdaExpression != null)
            {
                _serializedLambdaExpression = _expressionSerializer.SerializeBinary(lambdaExpression);
            }
        }

        /// <summary>
        /// Answers if the rule passes given the specified context (aka, the record).
        /// This method should only be called if this rule instance has a delegate populated.
        /// </summary>
        /// <param name="context">the data to evaluate against</param>
        /// <returns>true if the rule passes</returns>
        public bool Passes(T context)
        {
            if (_serializedLambdaExpression == null)
            {
                throw new InvalidOperationException("Rule cannot be evaluated in .NET");
            }

            // Check to see if the compiled delegate already exists in memory...
            //   If so, use it
            //   If not, deserialize the serialized lambda expression object and compile it
            if (_compiledDelegate == null)
            {
                // Lambda expression object stored; need to deserialize to compile the function, and then invoke it
                Expression<Func<T, bool>> lambdaExpression = (Expression<Func<T, bool>>)_expressionSerializer.DeserializeBinary(_serializedLambdaExpression);
                _compiledDelegate = lambdaExpression.Compile();
            }

            return _compiledDelegate.Invoke(context);
        }

        /// <summary>
        /// Answers true if this rule has an expression tree associated to it, which means it can be executed in .NET.
        /// </summary>
        public bool HasExpression
        {
            get
            {
                if (_compiledDelegate != null)
                {
                    return true;
                }
                else if (_serializedLambdaExpression != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
