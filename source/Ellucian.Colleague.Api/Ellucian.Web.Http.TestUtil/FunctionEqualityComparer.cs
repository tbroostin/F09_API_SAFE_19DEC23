/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Ellucian.Web.Http.TestUtil
{
    /// <summary>
    /// This EqualityComparer uses a function to compare two objects. Useful if there is
    /// no Equals override on the objects (such as DTOs), or if you need additional equality control.    
    /// </summary>
    /// <typeparam name="T">The Type of the objects being compared </typeparam>
    public class FunctionEqualityComparer<T> : IEqualityComparer<T>, System.Collections.IComparer
        where T : class
    {
        /// <summary>
        /// Equality function
        /// </summary>
        private readonly Func<T, T, bool> _equals;

        /// <summary>
        /// Hash function
        /// </summary>
        private readonly Func<T, int> _hash;

        /// <summary>
        /// Comparer function
        /// </summary>
        private readonly Func<T, T, int> _comparer;

        /// <summary>
        /// Instantiate the FunctionEqualityComparer class with an equality function and a hash function. 
        /// This uses a default comparison function, which uses the hash function to determine less than or greater than
        /// The hash function improves performance in Linq methods.
        /// </summary>
        /// <param name="equals">The equality function returns a boolean whether the two objects are equal or not</param>
        /// <param name="hash">The hash function returns the HashCode of an object </param>
        public FunctionEqualityComparer(Func<T, T, bool> equals, Func<T, int> hash)
        {
            _equals = equals;
            _hash = hash;
        }

        /// <summary>
        /// Instantiate the FunctionEqualityComparer class with an equality function, a hash function and a custom comparison function.        
        /// </summary>
        /// <param name="equals">The equality function returns a boolean whether the two objects are equal or not</param>
        /// <param name="hash">The hash function returns the HashCode of an object</param>
        /// <param name="comparer">The comparer function compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</param>        
        public FunctionEqualityComparer(Func<T, T, bool> equals, Func<T, int> hash, Func<T, T, int> comparer)
        {
            _equals = equals;
            _hash = hash;
            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hash(obj);
        }

        public int Compare(object x, object y)
        {
            if (x == null) throw new ArgumentException("Cannot handle comparisons between null objects", "x");
            if (y == null) throw new ArgumentException("Cannot handle comparisons between null objects", "y");
            if (x.GetType() != y.GetType()) throw new ArgumentException("Cannot handle comparisons between objects of different types");
            if (x.GetType() != typeof(T)) throw new ArgumentException(string.Format("Cannot handle comparisons of objects not of type {0}", typeof(T)), "x");
            if (y.GetType() != typeof(T)) throw new ArgumentException(string.Format("Cannot handle comparisons of objects not of type {0}", typeof(T)), "y");

            var tOfX = x as T;
            var tOfY = y as T;

            if (_comparer != null) return _comparer(tOfX, tOfY);

            if (this.Equals(tOfX, tOfY)) return 0;
            else if (this.GetHashCode(tOfX) > this.GetHashCode(tOfY)) return 1;
            else return -1;
        }
    }
}
