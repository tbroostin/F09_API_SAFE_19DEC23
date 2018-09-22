// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// School codes used within an institutional hierarchy such as School of Nursing.
    /// </summary>
    [Serializable]
    public class School : GuidCodeItem
    {
        // Summary:
        //     The Globally Unique ID (GUID) for this code. Always in lowercase.
        private string _Guid;
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (_Guid == null)
                {
                    _Guid = value;
                }
                else
                {
                    throw new InvalidOperationException("Guid cannot be changed");
                }
            }
        }

        
        private string _Code;
        public string Code
        {
            get { return _Code; }
            set
            {
                if (_Code == null)
                {
                    _Code = value;
                }
                else
                {
                    throw new InvalidOperationException("Code cannot be changed");
                }
            }
        }
        /// <summary>
        /// Description
        /// <\summary>
        private readonly string _Description;
        public string Description { get { return _Description; } }
        /// <summary>
        /// School Locations
        /// <\summary>
        private readonly List<string> _LocationCodes = new List<string>();
        public IEnumerable<string> LocationCodes { get { return _LocationCodes; } }
        /// <summary>
        /// Divisions within the School
        /// </summary>
        private readonly List<String> _DivisionCodes = new List<string>();
        public IEnumerable<string> DivisionCodes { get { return _DivisionCodes; } }
        /// <summary>
        /// Departments within the School
        /// </summary>
        private readonly List<string> _DepartmentCodes = new List<string>();
        public IEnumerable<string> DepartmentCodes { get { return _DepartmentCodes; } }
        /// <summary>
        /// Academic Level for the School
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// Institutions Id if applicable
        /// </summary>
        public string InstitutionId { get; set; }


        public School(string guid, string code, string description)
                : base (guid, code, description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Code must not be null");
            }

            _Code = code;

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "Description must not be null");
            }

            _Description = description;

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid must not be null");
            }

            _Guid = guid;
        }



        public void AddDepartmentCode(string departmentCode)
        {
            if (string.IsNullOrEmpty(departmentCode))
            {
                throw new ArgumentNullException("departmentCode", "Department must not be null");
            }
            if (DepartmentCodes.Contains(departmentCode))
            {
                throw new ArgumentException("departmentCode", "Department Code is already in the list");
            }
            _DepartmentCodes.Add(departmentCode);
        }
        public void AddLocationCode(string locationCode)
        {
            if (string.IsNullOrEmpty(locationCode))
            {
                throw new ArgumentNullException("locationCode", "Location must not be null");
            }
            if (LocationCodes.Contains(locationCode))
            {
                throw new ArgumentException("locationCode", "Location Code is already in the list");
            }
            _LocationCodes.Add(locationCode);
        }
        public void AddDivisionCode(string divisionCode)
        {
            if (string.IsNullOrEmpty(divisionCode))
            {
                throw new ArgumentNullException("divisionCode", "Division must not be null");
            }
            if (DivisionCodes.Contains(divisionCode))
            {
                throw new ArgumentException("divisionCode", "Division Code is already in the list");
            }
            _DivisionCodes.Add(divisionCode);
        }
    }
}
