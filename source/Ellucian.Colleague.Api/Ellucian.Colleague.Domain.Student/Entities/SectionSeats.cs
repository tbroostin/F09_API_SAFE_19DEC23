// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section seats information
    /// </summary>
    [Serializable]
    public class SectionSeats
    {
        #region Constructor

        /// <summary>
        /// Section Constructor. Section inherits course name (Course.Subject.Code + Course.Number)
        /// </summary>
        /// <param name="id">Section ID</param>
        public SectionSeats(string id )
        {
      
            _Id = id;
        }

        #endregion

        #region Required fields

        private string _Id;
        /// <summary>
        /// Section ID
        /// </summary>
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Section Id cannot be changed");
                }
            }
        }

        #endregion

        #region Optional Fields

        private string _Guid;
        /// <summary>
        /// GUID for the section; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

       
     
        /// <summary>
        /// Active Students in section, not dropped or deleted
        /// </summary>
        public List<string> ActiveStudentIds = new List<string>();

        

        private int? _NumberOnWaitlist;
        /// <summary>
        /// Number of students actively on the waitlist for this specific section (includes active and permitted to register).
        /// </summary>
        public int? NumberOnWaitlist
        {
            get { return _NumberOnWaitlist; }
            set
            {
                if (value != null)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Number On Waitlist may not be negative");
                    }
                    _NumberOnWaitlist = value;
                }
            }
        }

        private int? _PermittedToRegisterOnWaitlist;
        /// <summary>
        /// Number of students on the waitlist who've been given permission to register. (Used to reduce availability.)
        /// Subset of ActiveOnWaitlist above!
        /// </summary>
        public int? PermittedToRegisterOnWaitlist 
        { 
            get { return _PermittedToRegisterOnWaitlist; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Number of students permitted to register cannot be negative.");
                }
                _PermittedToRegisterOnWaitlist = value;
            }
        }

        private int? _ReservedSeats;
        /// <summary>
        /// Number of seats being held in reserve by web processes for a particular section.
        /// </summary>
        public int? ReservedSeats
        {
            get { return _ReservedSeats; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Reserved Seats cannot be negative.");
                }
                _ReservedSeats = value;
            }
        }

        private int? _SectionCapacity;
        /// <summary>
        /// Capacity set for this specific section. If null it means there is no limit.
        /// </summary>
        public int? SectionCapacity 
        {
            get { return _SectionCapacity; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Section capacity may not be negative");
                }
                _SectionCapacity = value;
            }
        }

       

        private int? _GlobalCapacity;
        /// <summary>
        /// Capacity set for all cross listed sections. Null if section is NOT cross listed.
        /// NOTE: If the section IS cross listed both the global and the local capacities have to be null for it to be truly unlimited.
        /// </summary>
        public int? GlobalCapacity 
        { 
            get { return _GlobalCapacity; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Global capacity may not be negative");
                }
                _GlobalCapacity = value;
            }
        }

       
        /// <summary>
        /// Indicates whether cross-listed sections are to operate with effectively 1 waitlist.  If true, then if a waitlist is started for any section 
        /// then the waitlist starts for ALL sections.  If this is false, sections with availability will show their availability even
        /// if other cross-listed sections are wait-listed. 
        /// section.
        /// </summary>
        public bool CombineCrosslistWaitlists { get; set; }

        public  List<SectionSeats> CrossListedSections = new List<SectionSeats>();
        /// <summary>
        /// Sections that are cross listed with this section
        /// </summary>
     //   public ReadOnlyCollection<SectionSeats> CrossListedSections { get; private set; }

     

        #endregion

        #region Calculated properties

        /// <summary>
        /// CALCULATED: If global capacity is not null it will return global capacity, otherwise it returns the section capacity
        /// </summary>
        public int? Capacity
        {
            get
            {
                return _GlobalCapacity != null ? _GlobalCapacity : _SectionCapacity;
            }
        }

        /// <summary>
        /// CALCULATED: Number of available slots still available for registration. If there is no capacity this is null. For cross-listed
        /// sections, this will which ever is less - global available or section available.  NOTE: If CombineCrosslistWaitlist is true, and a waitlist
        /// exists for any of the cross-listed sections, then available is zero.
        /// </summary>
        public int? Available
        {
            get
            {
                // NOTES:
                // Uses Global Available if it is not null AND it is less then the Section Available.
                // If there IS a global capacity but no section capacity (it's null - meaning unlimited), then it return null, same as Colleague.
                // If the CombineCrosslistWaitlists is true and anyone is waitlisted for the section or any of its cross-listed sections, then availability is zero. End of story.
                if (CombineCrosslistWaitlists == true )
                {
                    int TotalOnWaitlists = CrossListedSections.Sum(sc => sc.NumberOnWaitlist.GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                    if(TotalOnWaitlists > 0)
                    {
                        return 0;
                    }
                }
                var numberEnrolledLocal = ActiveStudentIds.Count() + _ReservedSeats.GetValueOrDefault(0) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                int crossListedActiveStudentCount = CrossListedSections.Sum(cl => cl.ActiveStudentIds.Count());
                int crossListedReservedCount = CrossListedSections.Sum(cls => cls._ReservedSeats.GetValueOrDefault(0));
                int crossListedEnrolled = crossListedActiveStudentCount + crossListedReservedCount;
                int crossListedPermissionToRegister = (CrossListedSections.Sum(c => c._PermittedToRegisterOnWaitlist.GetValueOrDefault(0))) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                var numberEnrolledGlobal = numberEnrolledLocal + crossListedEnrolled;
                int? numberAvailableLocal = null;
                if (_SectionCapacity != null)
                {
                    if (_SectionCapacity - numberEnrolledLocal <= 0)
                    {
                        numberAvailableLocal = 0;
                    }
                    else
                    {
                        numberAvailableLocal = _SectionCapacity - numberEnrolledLocal;
                    }
                }
                int? numberAvailableGlobal = null;
                if (_GlobalCapacity != null)
                {
                    if (_GlobalCapacity - numberEnrolledGlobal <= 0)
                    {
                        numberAvailableGlobal = 0;
                    }
                    else
                    {
                        numberAvailableGlobal = _GlobalCapacity - numberEnrolledGlobal - crossListedPermissionToRegister;
                    }
                }
                if (numberAvailableGlobal != null && (numberAvailableGlobal <= numberAvailableLocal))
                {
                    return numberAvailableGlobal;
                }
                return numberAvailableLocal;
            }
        }

        /// <summary>
        /// CALCULATED: This is the number of active people on the waitlist for the section.
        /// If the section is cross listed, and if "CombinedCrosslistWaitlist" is true, then the number waitlisted will be
        /// the number on any of the cross listed section's waitlists.
        /// </summary>
        public int Waitlisted
        {
            get
            {
                int TotalOnWaitlists = CrossListedSections.Sum(sc => sc.NumberOnWaitlist.GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                if (CombineCrosslistWaitlists == true && TotalOnWaitlists > 0)
                {
                    return TotalOnWaitlists;
                }
                return NumberOnWaitlist.GetValueOrDefault(0);
            }
        }


        #endregion


        #region Object override methods

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            SectionSeats other = obj as SectionSeats;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

    }
}
