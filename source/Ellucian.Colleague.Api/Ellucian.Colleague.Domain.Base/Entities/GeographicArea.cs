//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// GeographicArea
    /// </summary>
    [Serializable]
    public class GeographicArea
    {
        /// <summary>
        /// The global identifier for the employee
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// The JOB Id
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The TYPE Id
        /// </summary>
        public string Type { get; private set; }


        /// <summary>
        /// The RATING Id
        /// </summary>
        public List<string> IncludedAreas { get; set; }


		/// <summary>
        /// Initializes a new instance of the <see cref="GeographicArea"/> class.
        /// </summary>
        /// <param name="guid">The global identifier for the employee record</param>
        /// <param name="personId">The Colleague PERSON id of the person</param>
        /// <param name="perposId">The Colleague JOB id of the person</param>
        /// <param name="completedDate">The Colleague complete date of the review</param>
        /// <param name="ratingCycleCode">The Colleague TYPE id of the review</param>
        /// <param name="ratingCode">The Colleague RATING id of the review</param>
        public GeographicArea(string guid, string code, string description, string type)
        { 
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id", string.Format("Id missing for Geographic Area. Code: '{0}', Description: '{1}', Type: '{2}'. ", code, description, type));
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", string.Format("Code missing for Geographic Area. Id: '{0}', Description: '{1}', Type: '{2}'. ", guid, description, type));
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", string.Format("Description missing for Geographic Area. Id: '{0}', Code: '{1}', Type: '{2}'. ", guid, code, type));
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", string.Format("Type missing for Geographic Area. Id: '{0}', Code '{1}', Description: '{2}'. ", guid, code, description));
            }

            Guid = guid;
            Code = code;
            Description = description;
            Type = type;
        }

        /// <summary>
        /// Two reviews are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var geographicArea = obj as GeographicArea;

            return geographicArea.Guid == Guid;
        }

        /// <summary>
        /// Hashcode representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        /// <summary>
        /// String representation of object (Id)
        /// </summary>
        /// <returns>Global Identifier</returns>
        public override string ToString()
        {
            return Guid;
        }
    }
}