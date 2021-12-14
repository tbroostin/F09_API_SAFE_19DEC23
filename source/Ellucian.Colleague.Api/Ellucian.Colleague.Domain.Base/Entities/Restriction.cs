// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Restriction codes
    /// </summary>
    [Serializable]
    public class Restriction : GuidCodeItem
    {
        private readonly int? _Severity;
        private readonly bool _OfficeUseOnly;
        private readonly string _Title;
        private readonly string _Details;
        private readonly string _FollowUpApplication;
        private readonly string _FollowUpLinkDefinition;
        private readonly string _FollowUpWebAdvisorForm;
        private readonly bool _MiscellaneousTextFlag;
        private string _FollowUpLabel;
        private string _Hyperlink;
        private RestrictionCategoryType _restIntgCategory;

        /// <summary>
        /// Gets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public int? Severity { get { return _Severity; } }

        /// <summary>
        /// Gets a value indicating whether [office use only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [office use only]; otherwise, <c>false</c>.
        /// </value>
        public bool OfficeUseOnly { get { return _OfficeUseOnly; } }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get { return _Title; } }

        /// <summary>
        /// Gets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details { get { return _Details; } }

        /// <summary>
        /// Gets the follow up application.
        /// </summary>
        /// <value>
        /// The follow up application.
        /// </value>
        public string FollowUpApplication { get { return _FollowUpApplication; } }

        /// <summary>
        /// Gets the follow up link definition.
        /// </summary>
        /// <value>
        /// The follow up link definition.
        /// </value>
        public string FollowUpLinkDefinition { get { return _FollowUpLinkDefinition; } }

        /// <summary>
        /// Gets the follow up web advisor form.
        /// </summary>
        /// <value>
        /// The follow up web advisor form.
        /// </value>
        public string FollowUpWebAdvisorForm { get { return _FollowUpWebAdvisorForm; } }

        /// <summary>
        /// Gets a value indicating whether [miscellaneous text flag].
        /// </summary>
        /// <value>
        /// <c>true</c> if [miscellaneous text flag]; otherwise, <c>false</c>.
        /// </value>
        public bool MiscellaneousTextFlag { get { return _MiscellaneousTextFlag; } }

        /// <summary>
        /// Gets or sets the follow up label.
        /// </summary>
        /// <value>
        /// The follow up label.
        /// </value>
        public string FollowUpLabel 
        { 
            get { return _FollowUpLabel; }
            set
            {
                _FollowUpLabel = value;
            }
        }

        /// <summary>
        /// Gets or sets the hyperlink.
        /// </summary>
        /// <value>
        /// The hyperlink.
        /// </value>
        public string Hyperlink 
        { 
            get { return _Hyperlink; }
            set
            {
                _Hyperlink = value;
            }
        }

        /// <summary>
        /// A global category of student hold types
        /// </summary>
        public RestrictionCategoryType RestIntgCategory
        {
            get { return _restIntgCategory; }
            set
            {
                _restIntgCategory = value;
            }
        }

        public string RestPrtlDisplayFlag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Restriction"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="visibleToUsers">The visible to users.</param>
        /// <param name="title">The title.</param>
        /// <param name="details">The details.</param>
        /// <param name="followUpApp">The follow up application.</param>
        /// <param name="followUpLinkDef">The follow up link definition.</param>
        /// <param name="followUpWAForm">The follow up Web Advisor form.</param>
        /// <param name="followUpLabel">The follow up label.</param>
        /// <param name="followUpIsMiscText">The follow up is miscellaneous text.</param>
        public Restriction(string guid, string code, string description, int? severity, string visibleToUsers, string title, string details, 
            string followUpApp, string followUpLinkDef, string followUpWAForm, string followUpLabel, string followUpIsMiscText) 
            : base(guid, code, description)
        {
            _Severity = severity;
            _OfficeUseOnly = true;
            if (!string.IsNullOrEmpty(visibleToUsers) && visibleToUsers.Equals("Y"))
            {
                _OfficeUseOnly = false;
            }
            _Title = title;
            if (string.IsNullOrEmpty(title)) { _Title = description; }
            _Details = details;
            _FollowUpApplication = followUpApp;
            _FollowUpLinkDefinition = followUpLinkDef;
            _FollowUpWebAdvisorForm = followUpWAForm;
            _FollowUpLabel = followUpLabel;
            _MiscellaneousTextFlag = false;
            if (!string.IsNullOrEmpty(followUpIsMiscText) && followUpIsMiscText.Equals("Y"))
            {
                    _MiscellaneousTextFlag = true;
            }
            _Hyperlink = null;
        }
    }
}
