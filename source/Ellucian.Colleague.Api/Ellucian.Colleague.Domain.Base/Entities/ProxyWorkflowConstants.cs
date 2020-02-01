// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Contains static properties with known proxy workflow codes for checking proxy workflow access
    /// </summary>
    [Serializable]
    public class ProxyWorkflowConstants
    {
        public string Value { get; private set; }
        private ProxyWorkflowConstants(string workflowCode)
        {
            Value = workflowCode;
        }

        #region CORE General

        public static ProxyWorkflowConstants CoreNotifications
        {
            get { return new ProxyWorkflowConstants("CONO"); }
        }

        public static ProxyWorkflowConstants CoreRequiredDocuments
        {
            get { return new ProxyWorkflowConstants("CORD"); }
        }

        #endregion

        #region EM Employee

        public static ProxyWorkflowConstants TimeManagementTimeApproval
        {
            get { return new ProxyWorkflowConstants("TMTA"); }
        }

        #endregion

        #region ST Academics

        public static ProxyWorkflowConstants AcademicsGrades
        {
            get { return new ProxyWorkflowConstants("STGR"); }
        }

        #endregion

        #region SF Student Finance

        public static ProxyWorkflowConstants StudentFinanceAccountActivity
        {
            get { return new ProxyWorkflowConstants("SFAA"); }
        }

        public static ProxyWorkflowConstants StudentFinanceAccountSummary
        {
            get { return new ProxyWorkflowConstants("SFAS"); }
        }

        public static ProxyWorkflowConstants StudentFinanceMakeAPayment
        {
            get { return new ProxyWorkflowConstants("SFMAP"); }
        }

        #endregion

        #region FA Financial Aid

        public static ProxyWorkflowConstants FinancialAidAwardLetter
        {
            get { return new ProxyWorkflowConstants("FAAL"); }
        }

        public static ProxyWorkflowConstants FinancialAidHome
        {
            get { return new ProxyWorkflowConstants("FACL"); }
        }

        public static ProxyWorkflowConstants FinancialAidCorrespondenceOption
        {
            get { return new ProxyWorkflowConstants("FACO"); }
        }

        public static ProxyWorkflowConstants FinancialAidLoanRequest
        {
            get { return new ProxyWorkflowConstants("FALR"); }
        }

        public static ProxyWorkflowConstants FinancialAidMyAwards
        {
            get { return new ProxyWorkflowConstants("FAMA"); }
        }

        public static ProxyWorkflowConstants FinancialAidOutsideAwards
        {
            get { return new ProxyWorkflowConstants("FAOA"); }
        }

        public static ProxyWorkflowConstants FinancialAidRequiredDocuments
        {
            get { return new ProxyWorkflowConstants("FARD"); }
        }

        public static ProxyWorkflowConstants FinancialAidSatisfactoryAcademicProgress
        {
            get { return new ProxyWorkflowConstants("FASAP"); }
        }

        public static ProxyWorkflowConstants FinancialAidFederalShoppingSheet
        {
            get { return new ProxyWorkflowConstants("FASS"); }
        }

        #endregion

        #region TAX Tax Information
        public static ProxyWorkflowConstants TaxInformation
        {
            get { return new ProxyWorkflowConstants("TI"); }
        }

        #endregion

    }
}
