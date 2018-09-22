// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// IpedsInstitution class helps translate OPE ids into the Name of the institution.
    /// </summary>
    [Serializable]
    public class IpedsInstitution
    {
        /// <summary>
        /// Unique identifier of IpedsInstitution as known to the Colleague database
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Unique identification number of the institution as known to IPEDS dataset
        /// </summary>
        public string UnitId { get { return unitId; } }
        private readonly string unitId;

        /// <summary>
        /// Institution name
        /// </summary>
        public string Name { get { return name; } }
        private readonly string name;

        /// <summary>
        /// Office of Postsecondary Education (OPE) ID Number
        /// </summary>        
        public string OpeId { get { return opeId; } }
        private readonly string opeId;

        /// <summary>
        /// The date the this IpedsInstitution object was last modified in the Colleague database.
        /// </summary>
        public DateTime LastModifiedDate { get { return lastModifiedDate; } }
        private readonly DateTime lastModifiedDate;

        /// <summary>
        /// Build an IpedsInstitution object
        /// </summary>
        /// <param name="id">Unique identifier of IpedsInstitution as known to the Colleague database. If empty, object data did not come from Colleague database.</param>
        /// <param name="unitId">Required: Unique identification number of the institution as known to IPEDS dataset</param>
        /// <param name="name">Institution name - Optional: Can be empty</param>
        /// <param name="opeId">Required: Office of Postsecondary Education (OPE) ID Number</param>
        /// <param name="lastModifiedDate">Required: The date the this IpedsInstitution object was last modified in the Colleague database.</param>        
        /// <exception cref="ArgumentNullException">Thrown if any of the required arguments are null or empty</exception>
        public IpedsInstitution(string id, string unitId, string name, string opeId, DateTime lastModifiedDate)
        {
            if (string.IsNullOrEmpty(unitId)) { throw new ArgumentNullException("unitId"); }
            if (string.IsNullOrEmpty(opeId)) { throw new ArgumentNullException("opeId"); }

            this.id = id;
            this.unitId = unitId;
            this.opeId = opeId;
            this.lastModifiedDate = lastModifiedDate;
            this.name = name;
        }

        /// <summary>
        /// Two IpedsInstitutions are equal when their UnitIds are equal.
        /// </summary>
        /// <param name="obj">IpedsInstitution object to compare to this</param>
        /// <returns>True if the two object's UnitIds are equal. False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            var ipedsInstitution = obj as IpedsInstitution;

            if (ipedsInstitution.unitId == this.unitId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the HashCode of this IpedsInstitution object
        /// </summary>
        /// <returns>HashCode based on this IpedsInstitution object's UnitId</returns>
        public override int GetHashCode()
        {
            return this.unitId.GetHashCode();
        }

        /// <summary>
        /// Get a string representation of this IpedsInstitution object.
        /// </summary>
        /// <returns>The IpedsInstitution object's Name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
