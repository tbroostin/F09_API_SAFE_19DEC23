// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Summary of Groups of Retention Alert Cases
    /// </summary>
    [Serializable]
    public class RetentionAlertGroupOfCasesSummary
    {
        private List<RetentionAlertGroupOfCases> _RoleCases;
        private List<RetentionAlertGroupOfCases> _EntityCases;

        /// <summary>
        /// Summary description of the Groups of Cases
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// List of Retention Alert Cases owned by a Role
        /// </summary>
        public List<RetentionAlertGroupOfCases> RoleCases { get { return _RoleCases;  } }

        /// <summary>
        /// List of Retention Alert Cases owned by individual Entitiles
        /// </summary>
        public List<RetentionAlertGroupOfCases> EntityCases { get { return _EntityCases;  } }

        /// <summary>
        /// Total number of Retention Alert Cases from the RoleCases and the EntityCases lists.
        /// </summary>
        public int TotalCases
        {
            get
            {
                var roleCaseIds = RoleCases.Where(rc => rc != null).SelectMany(rc => rc.CaseIds).Distinct().ToList();
                var entityCaseIds = EntityCases.Where(rc => rc != null).SelectMany(rc => rc.CaseIds).Distinct().ToList();
                var totalCases = roleCaseIds.Union(entityCaseIds).ToList().Count();
                return totalCases;
            }
        }

        /// <summary>
        /// Add a Group of Retention Alert Cases that are owned by a Role.
        /// </summary>
        /// <param name="roleCase">Group of Retention Alert Cases owned by a Role</param>
        public void AddRoleCase(RetentionAlertGroupOfCases groupOfCases)
        {
            _RoleCases.Add(groupOfCases);
        }

        /// <summary>
        /// Add a Group of Retention Alert Cases that are owned by an Entity.
        /// </summary>
        /// <param name="entityCase">Group of Retention Alert Cases owned by an Entity</param>
        public void AddEntityCase(RetentionAlertGroupOfCases groupOfCases)
        {
            _EntityCases.Add(groupOfCases);
        }

        public RetentionAlertGroupOfCasesSummary()
        {
            _RoleCases = new List<RetentionAlertGroupOfCases>();
            _EntityCases = new List<RetentionAlertGroupOfCases>();
        }
    }
}
