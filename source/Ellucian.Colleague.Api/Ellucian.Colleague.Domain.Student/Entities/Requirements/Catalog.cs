// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
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

       
    }
}
