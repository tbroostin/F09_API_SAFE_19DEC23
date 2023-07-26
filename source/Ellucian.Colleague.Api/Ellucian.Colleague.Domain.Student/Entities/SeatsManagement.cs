﻿// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A Base class that contains properties regarding section's seats management
    /// </summary>
    [Serializable]
    public abstract class SeatsManagement
    {
        /// <summary>
        /// An abstract method that needs to be overridden in derived class to return cross listed sections
        /// </summary>
        /// <returns></returns>
        protected abstract List<SeatsManagement> GetCrossListedSections();

        protected List<string> _ActiveStudentIds = new List<string>();
        /// <summary>
        /// Active Students in section, not dropped or deleted
        /// </summary>
        public ReadOnlyCollection<string> ActiveStudentIds { get; protected set; }

        protected List<SectionStatusItem> _Statuses = new List<SectionStatusItem>();
        /// <summary>
        /// Section status information
        /// </summary>
        public ReadOnlyCollection<SectionStatusItem> Statuses { get; protected set; }

        /// </summary>
        /// Current status for this section
        /// </summary>
        public SectionStatus CurrentStatus { get { return Statuses.Any() ? Statuses[0].Status : SectionStatus.NotSet; } }

        /// <summary>
        /// Indicates whether this section is active
        /// </summary>
        public bool IsActive { get { return CurrentStatus == SectionStatus.Active; } }

        protected bool _AllowWaitlist;
        /// <summary>
        /// Indicates whether students are allowed to go on the waitlist for this section
        /// </summary>
        public bool AllowWaitlist { get { return _AllowWaitlist; } }

        protected bool _WaitlistClosed;
        /// <summary>
        /// Indicates whether the waitlist is closed for this section
        /// </summary>
        public bool WaitlistClosed { get { return _WaitlistClosed; } }

        /// <summary>
        /// Indicates whether cross-listed sections are to operate with effectively 1 waitlist.  When true, then if a waitlist is started for any section 
        /// then the waitlist starts for ALL sections.  When this is false, sections with availability will show their availability even
        /// if other cross-listed sections are wait-listed. 
        /// </summary>
        public bool CombineCrosslistWaitlists { get; set; }

        protected int? _WaitlistMaximum;
        /// <summary>
        /// Maximum number of students that can be placed on this section's waitlist.
        /// </summary>
        public int? WaitlistMaximum
        {
            get { return _WaitlistMaximum; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Waitlist Maximum cannot be negative.");
                }
                _WaitlistMaximum = value;
            }
        }

        protected int? _GlobalWaitlistMaximum;
        /// <summary>
        /// Maximum to be place on the waitlist for any cross listed sections. Null if the section is NOT cross listed.
        /// </summary>
        public int? GlobalWaitlistMaximum
        {
            get { return _GlobalWaitlistMaximum; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Global Waitlist Maximum cannot be negative.");
                }
                _GlobalWaitlistMaximum = value;
            }
        }

        protected int? _NumberOnWaitlist;
        /// <summary>
        /// Number of students actively on the waitlist for this specific section 
        ///  - includes active and permitted to register
        ///  - does not include any cross-listed sections waitlist counts (individual section waitlist count)
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

        protected int? _GlobalActiveOnWaitlist;
        /// <summary>
        /// CALCULATED: Only when SeatServiceIsEnabled is false OR UseSeatServiceWhenEnabled is false
        /// Global count of waitlisted students for all cross listed sections.
        /// NOTE: If the section IS cross listed both the global and the local capacities have to be null for it to be truly unlimited.
        /// EXTERNAL SERVICE: When SeatServiceIsEnabled is true AND UseSeatServiceWhenEnabled is true 
        /// Returns the number of waitlisted students from the external seat service
        /// </summary>
        public int? GlobalActiveOnWaitlist
        {
            get
            {
                if (IsSeatServiceEnabled && UseSeatServiceWhenEnabled)
                {
                    return _GlobalActiveOnWaitlist;
                }

                if (_GlobalWaitlistMaximum != null)
                {
                    return (GetCrossListedSections().Sum(c => c.NumberOnWaitlist).GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                }
                else
                {
                    // No global maximum so only the section waitlist max matters.
                    return null;
                }
            }
            set
            {
                _GlobalActiveOnWaitlist = value;
            }
        }

        protected int? _PermittedToRegisterOnWaitlist;
        /// <summary>
        /// Number of students on the waitlist who've been given permission to register. (Used to reduce availability.)
        /// Subset of ActiveOnWaitlist (the NumberOnWaitlist property above)
        /// Null when SeatServiceIsEnabled is true AND UseSeatServiceWhenEnabled is true
        /// Currently not used by Self-Service, has been exposed in the SectionSeats descendant DTO
        /// EXTERNAL SERVICE: this is not used when both SeatServiceIsEnabled AND UseSeatServiceWhenEnabled are true
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

        protected int? _SectionCapacity;
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

        protected int? _GlobalCapacity;
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

        protected int? _ReservedSeats;
        /// <summary>
        /// Number of seats being held in reserve by web processes for a particular section.
        /// Null (not used) when SeatServiceIsEnabled is true AND UseSeatServiceWhenEnabled is true
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

        //protected int? _Capacity;
        /// <summary>
        /// CALCULATED: Returns global capacity when not null, otherwise returns the section capacity
        /// </summary>
        public int? Capacity
        {
            get
            {
                return _GlobalCapacity != null ? _GlobalCapacity : _SectionCapacity;
            }
        }

        protected int? _Available;
        /// <summary>
        /// CALCULATED: Number of available slots still available for registration. If there is no capacity this is null. 
        /// For cross-listed sections, this will which ever is less - global available or section available.  
        /// NOTE: If CombineCrosslistWaitlist is true, and a waitlist exists for any of the cross-listed sections, then available is zero.
        /// EXTERNAL SERVICE: When SeatServiceIsEnabled is true AND UseSeatServiceWhenEnabled is true 
        /// Returns the available value from the external seat service
        /// </summary>
        public int? Available
        {
            get
            {
                if (IsSeatServiceEnabled && UseSeatServiceWhenEnabled)
                {
                    return _Available;
                }
                else
                {
                    List<SeatsManagement> crossListedSections = GetCrossListedSections();
                    // NOTES:
                    // Uses Global Available if it is not null AND it is less then the Section Available.
                    // If there IS a global capacity but no section capacity (it's null - meaning unlimited), then it return null, same as Colleague.
                    // If the CombineCrosslistWaitlists is true and anyone is waitlisted for the section or any of its cross-listed sections, then availability is zero. End of story.
                    if (CombineCrosslistWaitlists == true)
                    {
                        int TotalOnWaitlists = crossListedSections.Sum(sc => sc.NumberOnWaitlist.GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                        if (TotalOnWaitlists > 0)
                        {
                            return 0;
                        }
                    }
                    var numberEnrolledLocal = ActiveStudentIds.Count() + _ReservedSeats.GetValueOrDefault(0) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                    int crossListedActiveStudentCount = crossListedSections.Sum(cl => cl.ActiveStudentIds.Count());
                    int crossListedReservedCount = crossListedSections.Sum(cls => cls._ReservedSeats.GetValueOrDefault(0));
                    int crossListedEnrolled = crossListedActiveStudentCount + crossListedReservedCount;
                    int crossListedPermissionToRegister = (crossListedSections.Sum(c => c._PermittedToRegisterOnWaitlist.GetValueOrDefault(0))) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
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
            set
            {
                _Available = value;
            }
        }

        protected int? _Enrolled;
        /// <summary>
        /// CALCULATED: Number of enrolled students for the course section.
        /// <remarks>If there is no global capacity, this is the sum of:
        /// all active students 
        /// + reserved seats 
        /// + waitlisted students with permission to register</remarks>
        /// <remarks>If there is a global capacity, this is the sum of:
        /// all active students (for the section and any crosslisted sections) 
        /// + reserved seats (for the section and any crosslisted sections) 
        /// + waitlisted students with permission to register (for the section and any crosslisted sections)</remarks>
        /// EXTERNAL SERVICE: When SeatServiceIsEnabled is true AND UseSeatServiceWhenEnabled is true 
        /// Returns the number of seats used from the external seat service
        /// </summary>
        public int? Enrolled
        {
            get
            {
                if (IsSeatServiceEnabled && UseSeatServiceWhenEnabled)
                {
                    return _Enrolled;
                }
                else
                {
                    List<SeatsManagement> crossListedSections = GetCrossListedSections();
                    var numberEnrolledLocal = ActiveStudentIds.Count() + _ReservedSeats.GetValueOrDefault(0) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                    if (_GlobalCapacity == null)
                    {
                        return numberEnrolledLocal;
                    }
                    else
                    {
                        int crossListedActiveStudentCount = crossListedSections.Sum(cl => cl.ActiveStudentIds.Count());
                        int crossListedReservedCount = crossListedSections.Sum(cls => cls._ReservedSeats.GetValueOrDefault(0));
                        int crossListedPermissionToRegister = (crossListedSections.Sum(c => c._PermittedToRegisterOnWaitlist.GetValueOrDefault(0))) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                        var numberEnrolledGlobal = numberEnrolledLocal + crossListedActiveStudentCount + crossListedReservedCount + crossListedPermissionToRegister;
                        return numberEnrolledGlobal;
                    }
                }
            }
            set
            {
                _Enrolled = value;
            }
        }

        /// <summary>
        /// CALCULATED: Indicates when the ability to add oneself to a waitlist for this section is currently available.
        /// This is only true if the section is active, it allows a waitlist, the waitlist isn't closed and if waitlist maximums have not been reached
        /// </summary>
        public bool WaitlistAvailable
        {
            get
            {
                if (IsActive == false || AllowWaitlist == false || WaitlistClosed == true)
                {
                    return false;
                }
                if (_WaitlistMaximum == null && _GlobalWaitlistMaximum == null)
                {
                    // A waitlist is available and no waitlist maximums have been set.
                    return true;
                }
                if (_GlobalWaitlistMaximum == null)
                {
                    // No global maximum so only the section waitlist max matters.
                    if (_WaitlistMaximum > 0 && NumberOnWaitlist.GetValueOrDefault(0) < _WaitlistMaximum)
                    {
                        return true;
                    }
                    return false;
                }
                if (_WaitlistMaximum == null)
                {
                    // No maximum on the section's local waitlist - just need to see if the global max has been reached yet.
                    if (GlobalActiveOnWaitlist.GetValueOrDefault(0) < _GlobalWaitlistMaximum)
                    {
                        return true;
                    }
                    return false;
                }
                // If we get her this means there is both a section waitlist max AND a global waitlist max.  Both must be unmet.
                if (GlobalActiveOnWaitlist.GetValueOrDefault(0) < _GlobalWaitlistMaximum && NumberOnWaitlist.GetValueOrDefault(0) < _WaitlistMaximum)
                {
                    return true;
                }
                return false;
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
                if (CombineCrosslistWaitlists == true && GlobalActiveOnWaitlist.GetValueOrDefault(0) > 0)
                {
                    return GlobalActiveOnWaitlist.GetValueOrDefault(0);
                }
                return NumberOnWaitlist.GetValueOrDefault(0);
            }
        }

        /// <summary>
        /// CALCULATED: This determines the status of the section - Open, Waitlisted or Closed
        /// </summary>
        public SectionAvailabilityStatusType AvailabilityStatus
        {
            get

            {
                //When the Capacity is null then it means there are unlimited seats available for the section. In that case Section is Open.
                if (this.Capacity == null)
                {
                    return SectionAvailabilityStatusType.Open;
                }
                //Section is open when seats are still available for registration and also there are no students in waitlist.
                //There is a scenario where seats could be available for registration if a student drops when there are students already in waitlist, 
                //technically in that situation students already in waitlist should be available to register and not others (though in certain conditions it might allow others like overrides or authorization)
                //but section will still show the waitlisted status because when a section moves from open to waitlisted status, it doesn't go back to open.
                //Therefore we are also checking on waitlisted count. It implies that section hasn't moved in swim lane from open to waitlist yet and is still open for regular registrations.
                if (Waitlisted == 0 && (Available == null || Available > 0))
                {
                    return SectionAvailabilityStatusType.Open;
                }

                //When there are no registration seats and Wailist availability is false
                if (this.Available.HasValue && this.Available.Value <= 0 && WaitlistAvailable == false)
                {
                    return SectionAvailabilityStatusType.Closed;
                }
                //When there are seats available for waitlist or waitlist is allowed
                if (this.WaitlistAvailable == true || AllowWaitlist == true)
                {
                    return SectionAvailabilityStatusType.Waitlisted;
                }
                return SectionAvailabilityStatusType.Open;
            }
        }

        /// <summary>
        /// Flag indicating whether or not the seat count information was retrieved from the external seat service API rather than the local database
        /// </summary>
        protected bool IsSeatServiceEnabled { get; set; }

        /// <summary>
        /// When a SeatsManagement entity object is created from a call to an Ethos API the data returned must come from the local Colleague database
        /// External services, like the Experience Seat Service, are listening for section change notifications and 
        /// will use the Ethos APIs to retrieve changes to Colleague Section data in the local database
        /// Flag indicating whether to return seat count data from the external seat service or the local Colleague database
        /// Will be false when the SeatsManagement entity object is created by call to an Ethos endpoint
        /// </summary>
        protected bool UseSeatServiceWhenEnabled { get; set; }

        /// <summary>
        /// Flag indicating whether or not the section seat count information is available
        ///  - this will be false when the Seat Service API is enabled and UseSeatServiceWhenEnabled and the call to the External Seat Service API failed
        /// </summary>
        public bool AreSeatCountsAvailable { get; set; }
    }
}
