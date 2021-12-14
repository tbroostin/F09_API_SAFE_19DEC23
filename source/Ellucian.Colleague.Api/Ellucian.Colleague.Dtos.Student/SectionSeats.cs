// Copyright 2021 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Section seats information
    /// </summary>
    public class SectionSeats
    {
        /// <summary>
        /// Section Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// GUID for the section; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Number of students actively on the waitlist for this specific section 
        /// This will include active and permitted to register.
        /// </summary>
        public int? NumberOnWaitlist { get; set; }

        /// <summary>
        /// Number of students on the waitlist who have been given permission to register. 
        /// This is used to reduce availability and is a subset of the above NumberOnWaitlist property.
        /// </summary>
        public int? PermittedToRegisterOnWaitlist { get; set; }

        /// <summary>
        /// Number of seats being held in reserve by web processes for a particular section.
        /// </summary>
        public int? ReservedSeats { get; set; }

        /// <summary>
        /// Capacity set for this specific section. When null there is no limit on seats available.
        /// </summary>
        public int? SectionCapacity { get; set; }

        /// <summary>
        /// Capacity set for all cross listed sections. Null if section is NOT cross listed.
        /// NOTE: If the section IS cross listed both the global and the local capacities have to be null for it to be truly unlimited.
        /// </summary>
        public int? GlobalCapacity { get; set; }

        /// <summary>
        /// Indicates whether cross-listed sections are to operate with effectively 1 waitlist.  
        /// When true, if a waitlist is started for any section then the waitlist starts for ALL sections.  
        /// When false, sections with availability will show their availability even if other cross-listed sections are wait-listed.
        /// </summary>
        public bool CombineCrosslistWaitlists { get; set; }

        /// <summary>
        /// CALCULATED: If global capacity is not null it will return global capacity, otherwise it returns the section capacity.
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// CALCULATED: Number of available slots still available for registration. Null when there is no capacity.
        /// For cross-listed sections, this will which ever is less - global available or section available.  
        /// NOTE: If CombineCrosslistWaitlist is true, and a waitlist exists for any of the cross-listed sections, then available is zero.
        /// </summary>
        public int? Available { get; set; }

        /// <summary>
        /// CALCULATED: This is the number of active people on the waitlist for the section.
        /// If the section is cross listed, and if "CombinedCrosslistWaitlist" is true, then the number waitlisted will be
        /// the number on any of the cross listed section's waitlists.
        /// </summary>
        public int Waitlisted { get; set; }
    }
}
