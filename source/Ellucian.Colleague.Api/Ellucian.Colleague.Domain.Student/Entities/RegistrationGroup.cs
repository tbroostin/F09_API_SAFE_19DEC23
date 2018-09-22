// Copyright 2015 Ellucian Company L.P. and its affiliates.using System;
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Information that describes special registration permissions and overrides for a particular group of people during registration.
    /// </summary>
    [Serializable]
    public class RegistrationGroup
    {
        /// <summary>
        /// Gets or sets the identifier - registration Group Id (for example, WEBREG)
        /// </summary>
        public string Id {get {return _id;}}
        private string _id;

        /// <summary>
        /// Specific Staff Assignments in the group, if a person does not have an active, special assignment in any group then they will
        /// fall into one of the parameterized registration group definitions.  If all else fails the process will put them in the NAMELESS group.
        /// </summary>
        public ReadOnlyCollection<StaffAssignment> StaffAssignments { get; private set; }
        private List<StaffAssignment> _staffAssignments = new List<StaffAssignment>();

        ///// <summary>
        ///// Contains all section registration date overrides for this registration group. 
        ///// </summary>
        public ReadOnlyCollection<SectionRegistrationDate> SectionRegistrationDates { get; private set; }
        private List<SectionRegistrationDate> _sectionRegistrationDates = new List<SectionRegistrationDate>();

        ///// <summary>
        ///// Registration date overrides for this registration group in particular terms. These items will have no locations
        ///// </summary>
        public ReadOnlyCollection<TermRegistrationDate> TermRegistrationDates { get; private set; }
        private readonly List<TermRegistrationDate> _termRegistrationDates = new List<TermRegistrationDate>();

        ///// <summary>
        ///// Contains all term/location registration date overrides for this registration group
        ///// </summary>
        public ReadOnlyCollection<TermRegistrationDate> TermLocationRegistrationDates { get; private set; }
        private readonly List<TermRegistrationDate> _termLocationRegistrationDates = new List<TermRegistrationDate>();
        
        /// <summary>
        /// Constructor for the Registration Group
        /// </summary>
        /// <param name="groupId">Group Id</param>
        public RegistrationGroup(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Id is required for a new registration group.");
            }
            _id = id;
            StaffAssignments = _staffAssignments.AsReadOnly();
            SectionRegistrationDates = _sectionRegistrationDates.AsReadOnly();
            TermRegistrationDates = _termRegistrationDates.AsReadOnly();
            TermLocationRegistrationDates = _termLocationRegistrationDates.AsReadOnly();
        }
        /// <summary>
        /// Used to add a staff assignment to the group
        /// </summary>
        /// <param name="staffAssignment">Staff Assignment to add</param>
        public void AddStaffAssignment(StaffAssignment staffAssignment)
        {
            if (staffAssignment == null)
            {
                throw new ArgumentNullException("sectionRegistrationDate", "Section registration date object must be provided.");
            }
            if (_staffAssignments.Where(s => s.StaffId == staffAssignment.StaffId && s.StartDate == staffAssignment.StartDate && s.EndDate == staffAssignment.EndDate).Count() == 0)
            {
                _staffAssignments.Add(staffAssignment);
            }
        }

        /// <summary>
        /// Used to add new TermRegistrationDate objects - this routine will determine which list to put it in.
        /// </summary>
        /// <param name="termRegistrationDate">The TermRegistrationDate object to be added. </param>
        public void AddTermRegistrationDate(TermRegistrationDate termRegistrationDate)
        { 
            if (termRegistrationDate == null)
            {
                throw new ArgumentNullException("termRegistrationDate", "Term registration date must be provided.");
            }
            if (string.IsNullOrEmpty(termRegistrationDate.Location))
            {
                // Make sure this term has not already been added to the list. 
                if (_termRegistrationDates.Where(t => t.TermId == termRegistrationDate.TermId).Count() == 0)
                {
                    _termRegistrationDates.Add(termRegistrationDate);
                }
            }
            else
            {
                // Make sure this term/location combination has not already been added to the list
                if (_termLocationRegistrationDates.Where(t => t.TermId == termRegistrationDate.TermId && t.Location == termRegistrationDate.Location).Count() == 0)
                {
                    _termLocationRegistrationDates.Add(termRegistrationDate);
                }
                
            }
            
        }
        /// <summary>
        /// Method used to add a SectionRegistrationDate object 
        /// </summary>
        /// <param name="sectionRegistrationDate">The SectionRegistrationDate object to add</param>
        public void AddSectionRegistrationDate(SectionRegistrationDate sectionRegistrationDate)
        {
            if (sectionRegistrationDate == null) 
            {
                throw new ArgumentNullException("sectionRegistrationDate", "Section registration date object must be provided.");
            }
            if (_sectionRegistrationDates.Where(s => s.SectionId == sectionRegistrationDate.SectionId).Count() > 0)
            {
                    // Maybe just log the duplicate instead of failing?
                    throw new ArgumentException("Section Registration Date already included.");
            }
            _sectionRegistrationDates.Add(sectionRegistrationDate);
        }
    }
}
