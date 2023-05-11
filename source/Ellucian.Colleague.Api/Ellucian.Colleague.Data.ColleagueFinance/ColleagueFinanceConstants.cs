// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance
{
    /// <summary>
    /// Defines constants for ColleagueFinance Domain
    /// </summary>
    public static class ColleagueFinanceConstants
    {

        /// <summary>
        /// The IncludedYearsFor1099mi indicates the comma seperated years for which 1099MI form can be generated
        /// </summary>
        public const string IncludedYearsFor1099mi = "2022, 2021, 2020, 2019, 2018, 2017, 2016, 2015, 2014, 2013, 2012, 2011, 2010, 2009, 2008";

        /// <summary>
        /// The IncludedYearsFor1099nec indicates the comma seperated years for which 1099NEC form can be generated
        /// </summary>
        public const string IncludedYearsFor1099nec = "2022, 2021, 2020";

        /// <summary>
        /// Requisition attachments Tagone prefix
        /// </summary>
        public const string RequisitionAttachmentTagOnePrefix = "REQ-";

        /// <summary>
        /// Purchase order attachments Tagone prefix
        /// </summary>
        public const string PurchaseOrderAttachmentTagOnePrefix = "PO-";
        
        /// <summary>
        /// Voucher attachments Tagone prefix
        /// </summary>
        public const string VoucherAttachmentTagOnePrefix = "VOU-";

        /// <summary>
        /// project attachments Tagone prefix
        /// </summary>
        public const string ProjectAttachmentTagOnePrefix = "PADF-";

        /// <summary>
        /// draft budget adjustment attachments Tagone prefix
        /// </summary>
        public const string DraftBudgetAdjustmentAttachmentTagOnePrefix = "DBE-";

        /// <summary>
        /// budget adjustment attachments Tagone prefix
        /// </summary>
        public const string BudgetAdjustmentAttachmentTagOnePrefix = "BE-";
    }
}
