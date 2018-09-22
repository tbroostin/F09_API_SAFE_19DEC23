//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// AcctStructureIntg
    /// </summary>
    [Serializable]
    public class AcctStructureIntg 
    {
        /// <summary>
        /// ID of ACCT.STRUCTURE.INTG record
        /// </summary>
        private string _id;
        public string Id { get { return _id; }  }

        /// <summary>
        /// GUID for ACCT.STRUCTURE.INTG
        /// </summary>
        private string _guid;
        public string Guid { get { return _guid; }  }

        /// <summary>
        /// The full name of the accounting string subcomponent.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// File containing the description of this component
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The character number within the GL account number where this subcomponent begins.
        /// </summary>
        public int? StartPosition { get; set; }

        /// <summary>
        /// The length (in characters) of this subcomponent.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// The type of subcomponent.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The accounting string subcomponent that is one level higher in the subcomponent hierarchy.
        /// </summary>
        public string ParentSubComponent { get; set; }

        public AcctStructureIntg(string guid, string Id, string title)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is a required field.AcctStructureIntg id: " + Id);
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "title is a required field.AcctStructureIntg id: " + Id);
            }
            this._guid = guid;
            this._id = Id;
            this.Title = title;
        }
    }
}