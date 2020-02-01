// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PlanningStudent : Person
    {
        #region private members
        private readonly int? _DegreePlanId;
        private readonly List<string> _ProgramIds;
        private readonly List<string> _RegistrationPriorityIds = new List<string>();
        private readonly List<Advisement> _Advisements = new List<Advisement>();
        private readonly List<CompletedAdvisement> _CompletedAdvisements = new List<CompletedAdvisement>();

        #endregion

        #region properties

        /// <summary>
        /// Gets the student's Degree Plan Id if the student has a plan or null.
        /// </summary>
        public int? DegreePlanId { get { return this._DegreePlanId; } }

        /// <summary>
        /// Gets a list of the student's Academic Program Ids.
        /// </summary>
        public List<string> ProgramIds { get { return this._ProgramIds; } }
        /// <summary>
        /// Phonetypes hierarchy for student profile
        /// </summary>
        public List<string> PhoneTypesHierarchy { get; set; }

        /// <summary>
        /// Id numbers of the active advisors who are currently assigned to this student (if applicable).
        /// Former advisors (those with an end date prior to today) are not included in this list.
        /// </summary>
        public ReadOnlyCollection<string> AdvisorIds
        {
            get
            {
                return Advisements.Select(a => a.AdvisorId).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Student's stated educational goal, useful for advising. i.e. BA degree, Certification, New Career (institutionally defined)
        /// </summary>
        public string EducationalGoal { get; set; }

        /// <summary>
        /// Gets a list of student's RegistrationPriorityIds
        /// </summary>
        public ReadOnlyCollection<string> RegistrationPriorityIds { get; private set; }

        /// <summary>
        /// Flag to identify if the student has an active advisor assigned
        /// </summary>
        public bool HasAdvisor
        {
            get
            {
                if (Advisements.Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// List of Active Advisements for the student. Former advisements are not included in this list.
        /// </summary>
        public ReadOnlyCollection<Advisement> Advisements { get; set; }

        //to be removed later after integration
        public Collection<string> AcademicCreditIds { get; set; }

        /// <summary>
        /// List of completed advisements for the student for a given date and time by a given advisor
        /// </summary>
        public ReadOnlyCollection<CompletedAdvisement> CompletedAdvisements { get; set; }

        #endregion

        /// <summary>
        /// Create a Student domain object
        /// </summary>
        /// <param name="id">Student's ID</param>
        /// <param name="lastName">Student's last name</param>
        /// <param name="degreePlanId">Degree plan ID</param>
        /// <param name="programIds">List of program IDs</param>
        public PlanningStudent(string id, string lastName, int? degreePlanId, List<string> programIds, string privacyStatusCode = null)
            : base(id, lastName, privacyStatusCode)
        {
            if (degreePlanId.HasValue)
            {
                if (degreePlanId.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException("id", degreePlanId.Value, "id may only be null or a positive number");
                }
            }

            this._DegreePlanId = degreePlanId;
            this._ProgramIds = programIds;
            Advisements = _Advisements.AsReadOnly();
            CompletedAdvisements = _CompletedAdvisements.AsReadOnly();
            RegistrationPriorityIds = _RegistrationPriorityIds.AsReadOnly();
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
                _Advisements.Add(advisor);
            }
        }

        /// <summary>
        /// Adds a completed advisement to the student's list of completed advisements
        /// </summary>
        /// <param name="advisorId">Date on which an advisor is marking the student's advisement complete</param>
        /// <param name="completionDate">Time of day at which an advisor is marking the student's advisement complete</param>
        /// <param name="completionTime">ID of the advisor who is marking the student's advisement complete</param>
        public void AddCompletedAdvisement(string advisorId, DateTime completionDate, DateTimeOffset completionTime)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId", "An advisor ID is required when creating a completed advisement.");
            }
            CompletedAdvisement completedAdvisement = new CompletedAdvisement(completionDate, completionTime, advisorId);
            _CompletedAdvisements.Add(completedAdvisement);
        }

        public void AddRegistrationPriority(string registrationPriorityId)
        {
            if (string.IsNullOrEmpty(registrationPriorityId))
            {
                throw new ArgumentNullException("registrationPriorityId", "Registration Priority ID must be specified");
            }
            if (RegistrationPriorityIds.Where(r => r.Equals(registrationPriorityId)).Count() == 0)
            {
                _RegistrationPriorityIds.Add(registrationPriorityId);
            }
        }

        public StudentAccess ConvertToStudentAccess()
        {
            var studentAccess = new StudentAccess(this.Id);
            foreach (var advisement in this.Advisements)
            {
                studentAccess.AddAdvisement(advisement.AdvisorId, advisement.StartDate, advisement.EndDate, advisement.AdvisorType);
            }
            return studentAccess;
        }

        #region public overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PlanningStudent))
            {
                return false;
            }
            return (obj as PlanningStudent).Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
        #endregion
    }
}
