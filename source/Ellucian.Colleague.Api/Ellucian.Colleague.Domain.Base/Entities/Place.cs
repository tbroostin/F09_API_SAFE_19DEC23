/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class Place
    {       
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
        /// No Argument constructor
        /// </summary>
        public Place()  {}    
    }
}
