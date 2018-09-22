using System;
// Copyright 2015 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The Session and Yearly Cycle restrictions specific to a location. Used in course restrictions
    /// </summary>
    [Serializable]
    public class LocationCycleRestriction
    {
        /// <summary>
        /// Location specific to this location cycle restriction (required)
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Session cycle this item is restricted to
        /// </summary>
        public string SessionCycle { get; set; }
        /// <summary>
        /// Yearly cycle that this item is restricted to
        /// </summary>
        public string YearlyCycle { get; set; }

        /// <summary>
        /// Constructor for the LocaitonCycleRestriction
        /// </summary>
        /// <param name="location">Location (required)</param>
        /// <param name="sessionCycle">Applicable session cycle. If null or empty there is no session restriction for this location</param>
        /// <param name="yearlyCycle">Applicable yearly cycle. If null or empty there is no yearly restriction for this location</param>
        public LocationCycleRestriction(string location, string sessionCycle, string yearlyCycle)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentNullException("location", "Location is required for a location cycle restriction.");
            }
            Location = location;
            SessionCycle = sessionCycle;
            YearlyCycle = yearlyCycle;
        }

        /// <summary>
        /// LocationCycleRestrictions are equal when the location, session cycle, and yearly cycle are the same 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            var lcr = obj as LocationCycleRestriction;
            // Location is required but session and yearly cycles can be null or empty and I am treating both the same.
            if ((lcr.Location == Location) && 
                ((lcr.SessionCycle == SessionCycle) || (string.IsNullOrEmpty(lcr.SessionCycle) && string.IsNullOrEmpty(SessionCycle))) && 
                ((lcr.YearlyCycle == YearlyCycle || (string.IsNullOrEmpty(lcr.YearlyCycle) && string.IsNullOrEmpty(YearlyCycle)))))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes the hashcode of this object based on all 3 pieces.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Location + SessionCycle + YearlyCycle).GetHashCode();

        }
    }
}
