// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Mapping between a business event, resource, version, and path segment
    /// </summary>
    [Serializable]
    public class ResourceBusinessEventMapping
    {
        private string _resourceName;
        private string _resourceVersion;
        private string _pathSegment;
        private string _businessEvent;

        /// <summary>
        /// Name of the mapped resource
        /// </summary>
        public string ResourceName { get { return _resourceName; } }

        /// <summary>
        /// Version number of the mapped resource
        /// </summary>
        public string ResourceVersion { get { return _resourceVersion; } }

        /// <summary>
        /// Segment of the API endpoint for the mapped resource
        /// </summary>
        public string PathSegment { get { return _pathSegment; } }

        /// <summary>
        /// Name of the business event to which the resource is mapped
        /// </summary>
        public string BusinessEvent { get { return _businessEvent; } }

        /// <summary>
        /// Constructor for ResourceBusinessEventMapping
        /// </summary>
        /// <param name="resourceName">Name of the mapped resource</param>
        /// <param name="resourceVersion">JSON schema version for the mapped resource</param>
        /// <param name="pathSegment">Mapped resource path segment</param>
        /// <param name="businessEvent">Business event to which resource is mapped</param>
        public ResourceBusinessEventMapping(string resourceName, string resourceVersion, string pathSegment, string businessEvent)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException("resourceName", "A resource name must be provided.");
            }
            if (!string.IsNullOrEmpty(resourceVersion))
            {
                if (!(resourceVersion.All(c => Char.IsDigit(c) || c == '.')))
                {
                    throw new ArgumentException("resourceVersion", "A resource version can only contain some combination of these characters '.0123456789'.");
                }
            }
            if (string.IsNullOrEmpty(pathSegment))
            {
                throw new ArgumentNullException("pathSegment", "A path segment must be provided.");
            }

            _resourceName = resourceName;
            _resourceVersion = resourceVersion;
            _pathSegment = pathSegment;
            _businessEvent = businessEvent;
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
            if (obj == null)
            {
                return false;
            }

            ResourceBusinessEventMapping other = obj as ResourceBusinessEventMapping;
            if (other == null)
            {
                return false;
            }

            return other.ResourceName.Equals(ResourceName) && other.ResourceVersion.Equals(ResourceVersion);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ResourceName.GetHashCode();
        }
    }
}
