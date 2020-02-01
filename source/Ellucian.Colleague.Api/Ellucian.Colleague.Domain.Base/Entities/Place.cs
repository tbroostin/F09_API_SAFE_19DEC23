/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class Place
    {
        private string _guid { get; set; }

        /// <summary>
        /// GUID for the remark; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (string.IsNullOrEmpty(_guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        /// <summary>
        /// 3166-1 alpha-3 country code 
        /// Note: this code will also be stored on the COUNTRIES code file in CTRY.ISO.ALPHA3.CODE
        /// </summary>
        public string PlacesCountry { get; set; }

        /// <summary>
        /// Code describing a region in a particular country
        /// </summary>
        public string PlacesRegion { get; set; }

        /// <summary>
        /// Code describing a sub-region in a particular region of a particular country
        /// </summary>
        public string PlacesSubRegion { get; set; }

        /// <summary>
        /// Description of a country, region, or sub-region 
        /// </summary>
        public string PlacesDesc { get; set; }

        /// <summary>
        /// An indicator as to whether the ISO code is currently in use or was previously used to represent a country.
        /// </summary>
        public string PlacesInactive { get; set; }


        /// <summary>
        /// No Argument constructor
        /// </summary>
        public Place()  {}


        /// <summary>
        /// No Argument constructor
        /// </summary>
        public Place(string guid)
        {
            _guid = guid;
        }
    }
}
