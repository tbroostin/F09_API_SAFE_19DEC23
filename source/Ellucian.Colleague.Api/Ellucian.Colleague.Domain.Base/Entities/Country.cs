// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Country codes.
    /// </summary>
    [Serializable]
     public class Country : CodeItem
    {
          private readonly string _isoCode;
          /// <summary>
          /// Gets the ISO code.
          /// </summary>
          /// <value>
          /// The ISO code.
          /// </value>
          public string IsoCode { get { return _isoCode; } }


          private readonly string _iso3Code;
          /// <summary>
          /// Gets the ISO Alpha 3 code.
          /// </summary>
          /// <value>
          /// The ISO code.
          /// </value>
          public string Iso3Code { get { return _iso3Code; } }

        private readonly string _guid;
        /// <summary>
        /// Gets the Guid.
        /// </summary>
        /// <value>
        /// The Guid.
        /// </value>
        public string Guid { get { return _guid; } }

        private readonly bool _isNotInUse;
          /// <summary>
          /// Indicates if this country no longer exist as a mailable country. If true the country no longer exists.
          /// </summary>
          public bool IsNotInUse { get { return _isNotInUse; } }

         /// <summary>
          /// Iso Alpha3 Code
         /// </summary>
          public string IsoAlpha3Code { get; set; }


        /// <summary>
        /// Initializes a new instance of the<see cref="Country"/> class.
        /// </summary>
        /// <param name = "code" > The code.</param>
        /// <param name = "description" > The description.</param>
        /// <param name = "isoCode" > The ISO code.</param>
        /// <param name = "isNotInUse" > Indicates whether the country still exists.</param>
        public Country(string code, string description, string isoCode, bool isNotInUse = false)
             : base(code, description)
        {
            //If no ISO code is provided, assume the country code is the ISO code
            if (String.IsNullOrWhiteSpace(isoCode))
            {
                code = code.Trim();
                _isoCode = code.Length >= 2 ? code.Substring(0, 2) : code;
            }
            else
            {
                isoCode = isoCode.Trim();
                _isoCode = (isoCode.Length >= 2) ? isoCode.Substring(0, 2) : isoCode;
            }
            _isNotInUse = isNotInUse;
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="Country"/> class.
        /// </summary>
        /// <param name = "code" > The code.</param>
        /// <param name = "description" > The description.</param>
        /// <param name = "isoCode" > The ISO code.</param>
        /// <param name = "iso3Code" > The ISO Alpha 3 code.</param>
        /// <param name = "isNotInUse" > Indicates whether the country still exists.</param>
        public Country(string code, string description, string isoCode, string iso3Code, bool isNotInUse = false)
            : base(code, description)
        {
            //If no ISO code is provided, assume the country code is the ISO code
            if (String.IsNullOrWhiteSpace(isoCode))
            {
                code = code.Trim();
                _isoCode = code.Length > 2 ? code.Substring(0, 2) : code;
            }
            else
            {
                isoCode = isoCode.Trim();
                _isoCode = (isoCode.Length > 2) ? isoCode.Substring(0, 2) : isoCode;
            }
            if (String.IsNullOrWhiteSpace(iso3Code))
            {
                code = code.Trim();
                _iso3Code = code.Length > 3 ? code.Substring(0, 3) : code;
            }
            else
            {
                iso3Code = iso3Code.Trim();
                _iso3Code = (iso3Code.Length > 3) ? iso3Code.Substring(0, 3) : iso3Code;
            }
            IsoAlpha3Code = iso3Code;
            _isNotInUse = isNotInUse;
        }


        ///// <summary>
        ///// Initializes a new instance of the <see cref="Country"/> class.
        ///// </summary>
        ///// <param name="guid">The guid.</param>
        ///// <param name="code">The code.</param>
        ///// <param name="description">The description.</param>
        ///// <param name="isoCode">The ISO code.</param>
        ///// <param name="iso3Code">The ISO Alpha 3 code.</param>
        ///// <param name="isNotInUse">Indicates whether the country still exists.</param>
        public Country(string guid, string code, string description, string isoCode, string iso3Code, bool isNotInUse = false)
             : base(code, description)
        {
            //If no ISO code is provided, assume the country code is the ISO code
            if (String.IsNullOrWhiteSpace(isoCode))
            {
                code = code.Trim();
                _isoCode = code.Length > 2 ? code.Substring(0, 2) : code;
            }
            else
            {
                isoCode = isoCode.Trim();
                _isoCode = (isoCode.Length > 2) ? isoCode.Substring(0, 2) : isoCode;
            }
            if (String.IsNullOrWhiteSpace(iso3Code))
            {
                code = code.Trim();
                _iso3Code = code.Length > 3 ? code.Substring(0, 3) : code;
            }
            else
            {
                iso3Code = iso3Code.Trim();
                _iso3Code = (iso3Code.Length > 3) ? iso3Code.Substring(0, 3) : iso3Code;
            }
            IsoAlpha3Code = iso3Code;
            _isNotInUse = isNotInUse;
            _guid = guid;
        }

    }
}
