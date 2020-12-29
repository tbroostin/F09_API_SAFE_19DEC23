// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.TransferWork
{
    /// <summary>
    /// Transfer Equivalency Work
    /// </summary>
    [Serializable]
    public class TransferEquivalencies
    {
        private string _institutionId;
        private readonly List<Equivalency> _equivalencies;

        /// <summary>
        /// Id of the Institution the Transfer Work is from
        /// </summary>
        public string InstitutionId { get { return _institutionId; } }

        /// <summary>
        /// List of transfer equivalencies
        /// </summary>
        public ReadOnlyCollection<Equivalency> Equivalencies { get; private set; }

        /// <summary>
        /// Total number of credits transfered for these equivalencies
        /// </summary>
        public decimal TotalTransferCredits { get
            {
                decimal totalTransferCredits = 0;
                foreach (var equiv in Equivalencies)
                {
                    totalTransferCredits += equiv.ExternalCourseWork.Any() ? equiv.ExternalCourseWork.Sum(tw => tw.Credits) : 0;                    
                }
                return totalTransferCredits;
            } 
        }

        /// <summary>
        /// Total number of institutional credits for these equivalencies
        /// </summary>
        public decimal TotalEquivalentCredits { get
            {
                decimal totalEquivalentCredits = 0;
                foreach (var equiv in Equivalencies)
                {
                    totalEquivalentCredits += equiv.EquivalentCourseCredits.Any() ? equiv.EquivalentCourseCredits.Sum(eq => eq.Credits) : 0;
                    totalEquivalentCredits += equiv.EquivalentGeneralCredits.Any() ? equiv.EquivalentGeneralCredits.Sum(eq => eq.Credits) : 0;
                }
                return totalEquivalentCredits;
            } 
        }

        public TransferEquivalencies()
        {
            _equivalencies = new List<Equivalency>();
            Equivalencies = _equivalencies.AsReadOnly();
        }

        public TransferEquivalencies(string institutionId) : this()
        {            
            _institutionId = institutionId;
        }

        public TransferEquivalencies(string institutionId, List<Equivalency> equivalencies) : this(institutionId)
        {
            if (equivalencies == null)
            {
                throw new ArgumentNullException("equivalencies is required.");
            }
            foreach (var equivalency in equivalencies) {
                AddTransferCourseWork(equivalency);
            };
        }

        public void AddTransferCourseWork(Equivalency equivalancy)
        {
            if (equivalancy == null)
            {
                throw new ArgumentNullException("equivalancy is required.");
            }
            _equivalencies.Add(equivalancy);
        }
    }
}
