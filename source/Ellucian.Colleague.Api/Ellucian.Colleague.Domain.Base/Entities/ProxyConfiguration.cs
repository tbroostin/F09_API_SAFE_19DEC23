// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents proxy configuration data
    /// </summary>
    [Serializable]
    public class ProxyConfiguration
    {
        private bool _proxyEnabled;
        private string _disclosureReleaseDocumentId;
        private string _proxyEmailDocumentId;
        private bool _canAddOtherUsers;
        private bool _SSNIsFormatted;
        private readonly List<ProxyWorkflowGroup> _workflowGroups = new List<ProxyWorkflowGroup>();
        private readonly List<string> _relationshipTypeCodes = new List<string>();
        private readonly List<DemographicField> _demographicFields = new List<DemographicField>();

        /// <summary>
        /// Flag indicating whether or not proxy functionality is allowed
        /// </summary>
        public bool ProxyIsEnabled { get { return _proxyEnabled; } }

        /// <summary>
        /// Code for the disclosure release approval document
        /// </summary>
        public string DisclosureReleaseDocumentId { get { return _disclosureReleaseDocumentId; } }

        /// <summary>
        /// Disclosure release text
        /// </summary>
        public string DisclosureReleaseText { get; set; }

        /// <summary>
        /// Code for the proxy email document
        /// </summary>
        public string ProxyEmailDocumentId { get { return _proxyEmailDocumentId; } }

        /// <summary>
        /// Flag indicating whether or not users may add non-existent/unrelated users as proxies
        /// </summary>
        public bool CanAddOtherUsers { get { return _canAddOtherUsers; } }

        /// <summary>
        /// Text conveying information about the View/Add Third Party Access process
        /// </summary>
        public string ProxyFormHeaderText { get; set; }

        /// <summary>
        /// Text conveying information about the Add a Proxy workflow within the View/Add Third Party Access process
        /// </summary>
        public string AddProxyHeaderText { get; set; }

        /// <summary>
        /// Text conveying information about the user's need to reauthorize their granted access
        /// </summary>
        public string ReauthorizationText { get; set; }

        /// <summary>
        /// Code indicating the email address hierarchy to be used for proxy
        /// </summary>
        public string ProxyEmailAddressHierarchy { get; set; }

        /// <summary>
        /// Proxy workflow groups
        /// </summary>
        public ReadOnlyCollection<ProxyWorkflowGroup> WorkflowGroups { get; private set; }

        /// <summary>
        /// Codes of relationship types to which proxy access can be granted 
        /// </summary>
        public ReadOnlyCollection<string> RelationshipTypeCodes { get; private set; }

        /// <summary>
        /// Demographic information potentially collected when adding persons to Colleague via Proxy functionality
        /// </summary>
        public ReadOnlyCollection<DemographicField> DemographicFields { get; private set; }

        /// <summary>
        /// Flag indicating if the SSN is formatted 
        /// </summary>
        public bool SSNIsFormatted { get { return _SSNIsFormatted; } }

/// <summary>
/// Constructor for ProxyConfiguration
/// </summary>
/// <param name="isEnabled">Flag indicating whether or not proxy functionality is allowed</param>
/// <param name="disclosureDocId">Code for the disclosure release approval document</param>
/// <param name="emailDocId">Code for the proxy email document</param>
/// <param name="canAddOtherUsers">Flag indicating whether or not users may add non-existent/unrelated users as proxies</param>
public ProxyConfiguration(bool isEnabled, string disclosureDocId, string emailDocId, bool canAddOtherUsers, bool UsingFormattingSubroutine)
        {
            _proxyEnabled = isEnabled;
            _disclosureReleaseDocumentId = disclosureDocId;
            _proxyEmailDocumentId = emailDocId;
            _canAddOtherUsers = canAddOtherUsers;
            _SSNIsFormatted = UsingFormattingSubroutine;

            DisclosureReleaseText = string.Empty;
            WorkflowGroups = _workflowGroups.AsReadOnly();
            RelationshipTypeCodes = _relationshipTypeCodes.AsReadOnly();
            DemographicFields = _demographicFields.AsReadOnly();
        }

        /// <summary>
        /// Add a workflow group to the proxy configuration
        /// </summary>
        /// <param name="workflowGroup">Proxy workflow group to be added</param>
        public void AddWorkflowGroup(ProxyWorkflowGroup workflowGroup)
        {
            if (workflowGroup == null)
            {
                throw new ArgumentNullException("workflowGroup", "A proxy workflow group must be supplied.");
            }
            if (workflowGroup.Workflows.Count == 0)
            {
                throw new ArgumentException("A proxy workflow group must contain at least one proxy workflow.", "workflowGroup");
            }

            var currentWorkflowGroupIds = _workflowGroups.Select(w => w.Code).Distinct().ToList();
            if (!currentWorkflowGroupIds.Contains(workflowGroup.Code))
            {
                _workflowGroups.Add(workflowGroup);
            }
        }

        /// <summary>
        /// Add a relationship type code to the proxy configuration
        /// </summary>
        /// <param name="code">Relationship type code to be added</param>
        public void AddRelationshipTypeCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "A relationship type code must be supplied.");
            }
            if (!_relationshipTypeCodes.Contains(code))
            {
                _relationshipTypeCodes.Add(code);
            }
        }

        /// <summary>
        /// Add a demographic field to the proxy configuration
        /// </summary>
        /// <param name="code">Demographic field to be added</param>
        public void AddDemographicField(DemographicField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field", "A demographic field must be supplied.");
            }
            if (!_demographicFields.Contains(field))
            {
                _demographicFields.Add(field);
            }
        }
    }
}