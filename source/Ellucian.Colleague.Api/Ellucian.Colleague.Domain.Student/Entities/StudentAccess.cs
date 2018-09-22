// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Provides student information needed to determine a user's access to student data.
    /// </summary>
    [Serializable]
    public class StudentAccess
    {
        /// <summary>
        /// Id of the student
        /// </summary>
        public string Id { get { return _id; } }
        private string _id;

        /// <summary>
        /// List of advisements for the student
        /// </summary>
        public ReadOnlyCollection<Advisement> Advisements { get; set; }
        private readonly List<Advisement> _advisements = new List<Advisement>();

        /// <summary>
        /// List of Advisor ids represented in Advisements
        /// </summary>
        public ReadOnlyCollection<string> AdvisorIds
        {
            get
            {
                return Advisements.Select(a => a.AdvisorId).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Build a student with information needed to determine if the current user can access
        /// the student's data.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="advisements"></param>
        public StudentAccess(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id must be provided.");
            }
            _id = id;
            Advisements = _advisements.AsReadOnly();
        }

        public void AddAdvisement(string advisorId, DateTime? startDate, DateTime? endDate, string advisorType)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId", "Advisor Id must be specified");
            }
            // Since we are only currently putting active advisements in this list and since a student can actually only have one active advisement
            // per advisor (and therefore only 1 type) ensure that the advisor is not already in the list of advisements.
            if (Advisements.Where(a => a.AdvisorId.Equals(advisorId)).Count() == 0)
            {
                Advisement advisor = new Advisement(advisorId, startDate) { AdvisorType = advisorType, EndDate = endDate };
                _advisements.Add(advisor);
            }
        }
    }
}
