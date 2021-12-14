//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// FixedAssets
    /// </summary>
    [Serializable]
    public class FixedAssets
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedAssets"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public FixedAssets(string guid, string id, string description, string capitalizationStatus, string acquisitionMethod, string fixPropertyTag)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Concat("Guid is required. Record ID: ", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Id is required.");
            }
            if (string.IsNullOrEmpty(capitalizationStatus))
            {
                throw new ArgumentNullException(string.Concat("Capitalization status is required. Record ID: ", id));
            }
            if (string.IsNullOrEmpty(acquisitionMethod))
            {
                throw new ArgumentNullException(string.Concat("Acquisition method is required. Record ID: ", id));
            }
            if (string.IsNullOrEmpty(fixPropertyTag))
            {
                throw new ArgumentNullException(string.Concat("Fix property tag is required. Record ID: ", id));
            }

            _guid = guid;
            _id = id;
            _description = description;
            _capitalizationStatus = capitalizationStatus;
            _acquisitionMethod = acquisitionMethod;
            _fixPropertyTag = fixPropertyTag;
        }

        private string _guid;
        public string RecordGuid
        {
            get { return _guid; }
            private set { _guid = value; }
        }

        private string _id;
        public string RecordKey
        {
            get { return _id; }
            private set { _id = value; }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            private set { _description = value; }
        }

        private string _capitalizationStatus;
        public string CapitalizationStatus
        {
            get { return _capitalizationStatus; }
            private set { _capitalizationStatus = value; }
        }

        private string _acquisitionMethod;
        public string AcquisitionMethod
        {
            get { return _acquisitionMethod; }
            private set { _acquisitionMethod = value; }
        }

        private string _fixPropertyTag;

        public string FixPropertyTag
        {
            get { return _fixPropertyTag; }
            set { _fixPropertyTag = value; }
        }

        public string FixAssetType { get; set; }
        public string FixAssetCategory { get; set; }
        public DateTime? FixDisposalDate { get; set; }
        public string FixInvoiceCondition { get; set; }
        public string FixLocation { get; set; }
        public string FixBuilding { get; set; }
        public string FixRoom { get; set; }
        public decimal? InsuranceAmountCoverage { get; set; }
        public decimal? FixValueAmount { get; set; }
        public decimal? FixAcqisitionCost { get; set; }
        public decimal? FixAllowAmount { get; set; }
        public string FixCalculationMethod { get; set; }
        public decimal? FixSalvageValue { get; set; }
        public int? FixUsefulLife { get; set; }
        public string FixCalcAccount { get; set; }
        public decimal? FixRenewalAmount { get; set; }
        public string FixStewerdId { get; set; }
    }
}