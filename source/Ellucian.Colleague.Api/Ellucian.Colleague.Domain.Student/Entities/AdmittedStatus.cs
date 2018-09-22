// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmittedStatus : CodeItem
    {
        /// <summary>
        /// Transfer Flag to indicate if admit status indicates a transfer status
        /// </summary>
        public string TransferFlag { get; set; }
 
        public AdmittedStatus(string code, string description, string transferFlag)
            : base(code, description)
        {
            TransferFlag = transferFlag;
        }
    }
}