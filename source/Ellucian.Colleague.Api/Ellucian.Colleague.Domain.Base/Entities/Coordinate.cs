// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of coordinates
    /// </summary>
    [Serializable]
    public class Coordinate 
    {
        private readonly bool _OfficeUseOnly;
        private readonly decimal? _Latitude;
        private readonly decimal? _Longitude;

        /// <summary>
        /// Gets a value indicating whether [office use only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [office use only]; otherwise, <c>false</c>.
        /// </value>
        public bool OfficeUseOnly { get { return _OfficeUseOnly; } }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public decimal? Latitude { get { return _Latitude; } }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public decimal? Longitude { get { return _Longitude; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coordinate"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="officeUseOnly">if set to <c>true</c> [office use only].</param>
        public Coordinate(Decimal? latitude, Decimal? longitude, bool officeUseOnly) 
        {
            // without latitude or longitude, force OfficeUseOnly to true
            // with latitude and longitude, respect the parameter officeUseOnly
            if ((latitude == null) || (longitude == null)) {
                _Latitude = null;
                _Longitude = null;
                _OfficeUseOnly = true;
            } else {
                _Latitude = latitude;
                _Longitude = longitude;
                _OfficeUseOnly = officeUseOnly;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            Coordinate other = obj as Coordinate;
            if (other == null) {
                return false;
            }
            if (other.OfficeUseOnly.Equals(_OfficeUseOnly) &&
                (_Latitude.HasValue && other.Latitude.HasValue && other.Latitude.Equals(_Latitude)) &&
                (_Longitude.HasValue && other.Longitude.HasValue && other.Longitude.Equals(_Longitude))) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() {
            string sLat = "";
            if (_Latitude.HasValue) { sLat = String.Format("{0:0.000000}", _Latitude); }
            string sLong = "";
            if (_Longitude.HasValue) { sLong = String.Format("{0:0.000000}", _Longitude); }
            string sOFf = (_OfficeUseOnly ? "Y" : "N");
            string all = sLat + sLong + sOFf;
            return all.GetHashCode();
        }
    }
}
