// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{

    [Serializable]
    public class Remark
    {

        private string _Guid;
        private string _Id;

        public string RemarksType { get; set; }
        public string RemarksCode { get; set; }
        public string RemarksDonorId { get; set; }
        public string RemarksAuthor { get; set; }
        public DateTime? RemarksDate { get; set; }
        public string RemarksText { get; set; }
        public ConfidentialityType RemarksPrivateType { get; set; }
        public string RemarksIntgEnteredBy { get; set; }

        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Remark Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the remark; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        public Remark(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Remark guid can not be null or empty");
            }
            _Guid = guid;
        }
    }
}