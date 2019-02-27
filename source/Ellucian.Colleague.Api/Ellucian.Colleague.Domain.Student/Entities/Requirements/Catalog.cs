// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Academic Catalog
    /// </summary>
    [Serializable]
    public class Catalog
    {
        private readonly string _Guid;
        public string Guid { get { return _Guid; } }
        /// <summary>
        /// Catalog Code such as 2010 (required)
        /// </summary>
        private string _Code;
        public string Code
        {
            get { return _Code; }
            set
            {
                if (_Code == "")
                {
                    _Code = value;
                }
                else
                {
                    throw new InvalidOperationException("Catalog Code cannot be changed");
                }
            }
        }

        private readonly string _Description;
        public string Description { get { return _Description; } }

        /// <summary>
        /// Start date for the catalog (required)
        /// </summary>
        private readonly DateTime _StartDate;
        public DateTime StartDate { get { return _StartDate; } }

        /// <summary>
        /// End  date for the catalog
        /// </summary>
        public DateTime? EndDate { get; set; }


        /// <summary>
        /// Is Catalog Active?
        /// </summary>
        public bool IsActive
        {
            // If the endOn date contains a value and it is less than today's date then set this to "Inactive", 
            // otherwise set this to "Active"
            get
            {
                if (EndDate == null)
                    return true;
                else
                    return (EndDate < DateTime.Now) ? false : true;
            }
        }

        /// <summary>
        ///  Academic Programs
        /// </summary>
        public List<string> AcadPrograms { get; set; }

        /// <summary>
        /// Creates a new <see cref="Catalog"/> object.
        /// </summary>
        /// <param name="code">Catalog code</param>
        /// <param name="startDate">Date on which catalog begins</param>
        public Catalog(string code, DateTime startDate)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code", "Catalog code is required");
            }

            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate", "Start Date is required");
            }
            _Code = code;
            _StartDate = startDate;
        }

        /// <summary>
        /// Creates a new <see cref="Catalog"/> object.
        /// </summary>
        /// <param name="guid">Unique identifier</param>
        /// <param name="code">Catalog code</param>
        /// <param name="description">Description of the catalog</param>
        /// <param name="startDate">Date on which catalog begins</param>
        public Catalog(string guid, string code, string description, DateTime startDate)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code", "Catalog code is required");
            }

            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate", "Start Date is required");
            }
            _Code = code;
            _StartDate = startDate;
            _Guid = guid;
            _Description = description;
        }

        /// <summary>
        /// Additional constructor for What if
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        /// <param name="startDate">startDate</param>
        /// <param name="hideInWhatIf">hideInWhatIf</param>
        public Catalog(string guid, string code, string description, DateTime startDate, string hideInWhatIf)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code", "Catalog code is required");
            }

            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate", "Start Date is required");
            }
            _Code = code;
            _StartDate = startDate;
            _Guid = guid;
            _Description = description;
            // Hide catalog if hideInWhatIf is null or if hideInWhatIf is Y
            _HideInWhatIf = (string.IsNullOrEmpty(hideInWhatIf) || hideInWhatIf.ToUpperInvariant() == "Y");
        }

        /// <summary>
        /// Additional constructor for Catalog
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="startDate">start date</param>
        public Catalog(string guid, string code, DateTime startDate)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code", "Catalog code is required");
            }

            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate", "Start Date is required");
            }
            _Code = code;
            _StartDate = startDate;
            _Guid = guid;
        }

        /// <summary>
        /// Returns the catalog item, from a list of catalog items, that is the "current" catalog (can be null)
        /// The current catalog must be the one with the latest start date (that is not in the future)
        /// and without an end date or that has an end date in the future. The result may be null since none may match this criteria.
        /// </summary>
        /// <param name="catalogList"></param>
        /// <returns></returns>
        public Catalog GetCurentCatalog(ICollection<Catalog> catalogList)
        {
            return catalogList.Where(c => c.EndDate == null || c.EndDate > DateTime.Now).Where(cx => cx.StartDate <= DateTime.Now).OrderByDescending(s => s.StartDate).FirstOrDefault();
        }

        private readonly bool _HideInWhatIf;
        /// <summary>
        /// Flag indicating if this catalog year should be hidden in What If
        /// </summary>
        public bool HideInWhatIf { get { return _HideInWhatIf; } }
    }
}
