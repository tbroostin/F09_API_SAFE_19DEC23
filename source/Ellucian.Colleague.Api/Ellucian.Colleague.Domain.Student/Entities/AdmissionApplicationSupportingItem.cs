// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionApplicationSupportingItem
    {
        /// <summary>
        /// Unique Guid based on MAILING index of code, assign date, and instance
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// Key to the Mailing Record
        /// </summary>
        public string MailingId { get; private set; }

        /// <summary>
        /// The Application that is attached to this communication
        /// </summary>
        public string ApplicationId { get; private set; }

        /// <summary>
        /// Correspondece Received (CC.CODES) from MAILING.CORR.RECEIVED
        /// </summary>
        public string ReceivedCode { get; private set; }

        /// <summary>
        /// Correspondence Received Assigned Date from MAILING.CORR.RCVD.ASGN.DT
        /// </summary>
        public DateTime? AssignedDate { get; private set; }

        /// <summary>
        /// Correspondence Received Instance from MAILING.CORR.RCVD.INSTANCE
        /// </summary>
        public string Instance { get; private set; }

        /// <summary>
        /// Correspondence Received Date from MAILING.CORR.RECEIVED.DATE
        /// </summary>
        public DateTime? ReceivedDate { get; set; }

        /// <summary>
        /// Correspondence Received Action Date from MAILING.CORR.RCVD.ACT.DT
        /// </summary>
        public DateTime? ActionDate { get; set; }

        /// <summary>
        /// Correspondence Received Status from MAILING.CORR.RCVD.STATUS
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Correspondence Received Status from MAILING.CORR.RCVD.STATUS
        /// </summary>
        public string StatusAction { get; set; }

        /// <summary>
        /// Correspondence Received Comment from MAILING.CORR.RCVD.COMMENT
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// From the Coreq record, if required on any, then set to required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Admission Application Supporting Item from MAILING associatoin CH.CORR
        /// </summary>
        public AdmissionApplicationSupportingItem(string guid, string personId, string applicationId, string receivedCode, string instance, DateTime? assignDate, string status)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is required for AdmissionsApplicationSupportingItem. ");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId is required for AdmissionsApplicationSupportingItem. ");
            }
            if (string.IsNullOrEmpty(applicationId))
            {
                throw new ArgumentNullException("applicationId", "applicationId is required for AdmissionsApplicationSupportingItem. ");
            }
            if (string.IsNullOrEmpty(receivedCode))
            {
                throw new ArgumentNullException("receivedCode", "receivedCode is required for AdmissionsApplicationSupportingItem. ");
            }
            if (assignDate == null)
            {
                throw new ArgumentNullException("assignDate", "assignDate is required for AdmissionsApplicationSupportingItem. ");
            }
            Guid = guid;
            MailingId = personId;
            ApplicationId = applicationId;
            ReceivedCode = receivedCode;
            AssignedDate = assignDate;
            Instance = instance;
            Status = status;
        }
    }
}