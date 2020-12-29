// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for Student tax forms
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTaxFormStatementRepository : BaseColleagueRepository, IStudentTaxFormStatementRepository
    {
        private readonly string colleagueTimeZone;

        /// <summary>
        /// Student Tax Form Statement repository constructor.
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public StudentTaxFormStatementRepository(ApiSettings settings, ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Retrieve set of tax form statements assigned to the specified person for the tax form type.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<TaxFormStatement3>> Get2Async(string personId, string taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            var statements = new List<TaxFormStatement3>();

            // Based on the type of tax form, we will obtain the statements from different entities.
            switch (taxForm)
            {
                case TaxFormTypes.Form1098:
                    statements = (await Get1098TaxStatements2Async(personId)).ToList();
                    break;
                case TaxFormTypes.FormT2202A:
                    statements = (await GetT2202aTaxStatements2Async(personId)).ToList();
                    break;
                default:
                    throw new ArgumentException(taxForm.ToString() + " is not accessible within the ST module.", "taxForm");
            }

            return statements;
        }

        private async Task<IEnumerable<TaxFormStatement3>> Get1098TaxStatements2Async(string personId)
        {
            var statements = new List<TaxFormStatement3>();

            var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
            if (parm1098Contract == null)
            {
                throw new NullReferenceException("PARM.1098 cannot be null.");
            }
            // Get all the 1098 records for the person ID including 1098Ts and 1098Es.
            var form1098Criteria = "TF98F.STUDENT EQ '" + personId + "' AND WITH TF98F.TAX.FORM EQ '" + parm1098Contract.P1098TTaxForm +
                "' OR TF98F.TAX.FORM EQ '" + parm1098Contract.P1098ETaxForm + "'";
            var form1098StatementRecords = await DataReader.BulkReadRecordAsync<TaxForm1098Forms>(form1098Criteria);

            if (form1098StatementRecords != null)
            {
                // Sort the statements.
                var sorted1098StatementRecords = form1098StatementRecords.Where(x => x != null).OrderByDescending(x => x.TaxForm1098FormsAdddate)
                    .ThenByDescending(x => x.TaxForm1098FormsAddtime);

                // Loop through each tax year. We do not need to remove duplicate years because there may be multiple institutions issuing 1098s.
                foreach (var form1098Statement in sorted1098StatementRecords)
                {
                    try
                    {
                        if (form1098Statement.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        if (!string.IsNullOrEmpty(form1098Statement.Tf98fStudent) && form1098Statement.Tf98fTaxYear.HasValue)
                        {
                            var statement1098 = new TaxFormStatement3(form1098Statement.Tf98fStudent,
                                form1098Statement.Tf98fTaxYear.ToString(), GetCurrent1098TaxFormFromRecord2(form1098Statement.Tf98fTaxForm, parm1098Contract), form1098Statement.Recordkey);

                            // Add the statement to the list
                            statements.Add(statement1098);
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                    }
                }
            }
            return statements;
        }

        private async Task<IEnumerable<TaxFormStatement3>> GetT2202aTaxStatements2Async(string personId)
        {
            var statements = new List<TaxFormStatement3>();

            var parmT2202aContract = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
            if (parmT2202aContract == null)
            {
                throw new NullReferenceException("CNST.RPT.PARMS cannot be null.");
            }

            // Get all the T2202A records for the person ID.
            var formT2202aCriteria = "T2.REPOS.STUDENT EQ '" + personId + "'";
            var formT2202aStatementRecords = await DataReader.BulkReadRecordAsync<CnstT2202aRepos>(formT2202aCriteria);

            if (formT2202aStatementRecords != null)
            {
                // Sort the statements.
                var sortedT2202aStatementRecords = formT2202aStatementRecords.Where(x => x != null).OrderByDescending(x => x.T2ReposYear);

                // Loop through each tax year. We do not need to remove duplicate years because there may be multiple institutions issuing T2202s.
                foreach (var formT2202aStatement in sortedT2202aStatementRecords)
                {
                    try
                    {
                        if (formT2202aStatement.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        if (!string.IsNullOrEmpty(formT2202aStatement.T2ReposStudent) && formT2202aStatement.T2ReposYear.HasValue)
                        {
                            var statementT2202a = new TaxFormStatement3(formT2202aStatement.T2ReposStudent,
                                formT2202aStatement.T2ReposYear.ToString(), TaxFormTypes.FormT2202A, formT2202aStatement.Recordkey);

                            // Set notation as cancelled if the status flag is "C" or if the cancelled flag is "Y".
                            if ((formT2202aStatement.T2CancelFlag != null && formT2202aStatement.T2CancelFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                                || (formT2202aStatement.T2Status != null && formT2202aStatement.T2Status.Equals("C", StringComparison.InvariantCultureIgnoreCase) && !(formT2202aStatement.T2AmendedFlag != null && formT2202aStatement.T2AmendedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase))))
                            {
                                statementT2202a.Notation = TaxFormNotations.Cancelled;
                            }

                            // Add the statement to the list
                            statements.Add(statementT2202a);
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                    }
                }
            }
            return statements;
        }

        private string GetCurrent1098TaxFormFromRecord2(string taxForm, Parm1098 parm1098)
        {
            string currentTaxForm = TaxFormTypes.Form1098;
            if (taxForm == parm1098.P1098TTaxForm)
            {
                currentTaxForm = TaxFormTypes.Form1098T;
            }
            else if (taxForm == parm1098.P1098ETaxForm)
            {
                currentTaxForm = TaxFormTypes.Form1098E;
            }
            return currentTaxForm;
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Retrieve set of tax form statements assigned to the specified person for the tax form type.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        public async Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            var statements = new List<TaxFormStatement2>();

            // Based on the type of tax form, we will obtain the statements from different entities.
            switch (taxForm)
            {
                case TaxForms.Form1098:
                    statements = (await Get1098TaxStatements(personId)).ToList();
                    break;
                case TaxForms.FormT2202A:
                    statements = (await GetT2202aTaxStatements(personId)).ToList();
                    break;
                default:
                    throw new ArgumentException(taxForm.ToString() + " is not accessible within the ST module.", "taxForm");
            }

            return statements;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use Get1098TaxStatements2Async instead.")]
        private async Task<IEnumerable<TaxFormStatement2>> Get1098TaxStatements(string personId)
        {
            var statements = new List<TaxFormStatement2>();

            var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
            if (parm1098Contract == null)
            {
                throw new NullReferenceException("PARM.1098 cannot be null.");
            }
            // Get all the 1098 records for the person ID including 1098Ts and 1098Es.
            var form1098Criteria = "TF98F.STUDENT EQ '" + personId + "' AND WITH TF98F.TAX.FORM EQ '" + parm1098Contract.P1098TTaxForm +
                "' OR TF98F.TAX.FORM EQ '" + parm1098Contract.P1098ETaxForm + "'";
            var form1098StatementRecords = await DataReader.BulkReadRecordAsync<TaxForm1098Forms>(form1098Criteria);

            if (form1098StatementRecords != null)
            {
                // Sort the statements.
                var sorted1098StatementRecords = form1098StatementRecords.Where(x => x != null).OrderByDescending(x => x.TaxForm1098FormsAdddate)
                    .ThenByDescending(x => x.TaxForm1098FormsAddtime);

                // Loop through each tax year. We do not need to remove duplicate years because there may be multiple institutions issuing 1098s.
                foreach (var form1098Statement in sorted1098StatementRecords)
                {
                    try
                    {
                        if (form1098Statement.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        if (!string.IsNullOrEmpty(form1098Statement.Tf98fStudent) && form1098Statement.Tf98fTaxYear.HasValue)
                        {
                            var statement1098 = new TaxFormStatement2(form1098Statement.Tf98fStudent,
                                form1098Statement.Tf98fTaxYear.ToString(), GetCurrent1098TaxFormFromRecord(form1098Statement.Tf98fTaxForm, parm1098Contract), form1098Statement.Recordkey);

                            // Add the statement to the list
                            statements.Add(statement1098);
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                    }
                }
            }
            return statements;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use GetT2202aTaxStatements2Async instead.")]
        private async Task<IEnumerable<TaxFormStatement2>> GetT2202aTaxStatements(string personId)
        {
            var statements = new List<TaxFormStatement2>();

            var parmT2202aContract = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
            if (parmT2202aContract == null)
            {
                throw new NullReferenceException("CNST.RPT.PARMS cannot be null.");
            }

            // Get all the T2202A records for the person ID.
            var formT2202aCriteria = "T2.REPOS.STUDENT EQ '" + personId + "'";
            var formT2202aStatementRecords = await DataReader.BulkReadRecordAsync<CnstT2202aRepos>(formT2202aCriteria);

            if (formT2202aStatementRecords != null)
            {
                // Sort the statements.
                var sortedT2202aStatementRecords = formT2202aStatementRecords.Where(x => x != null).OrderByDescending(x => x.T2ReposYear);

                // Loop through each tax year. We do not need to remove duplicate years because there may be multiple institutions issuing T2202s.
                foreach (var formT2202aStatement in sortedT2202aStatementRecords)
                {
                    try
                    {
                        if (formT2202aStatement.Recordkey == null)
                        {
                            throw new NullReferenceException("RecordKey is a required field");
                        }

                        if (!string.IsNullOrEmpty(formT2202aStatement.T2ReposStudent) && formT2202aStatement.T2ReposYear.HasValue)
                        {
                            var statementT2202a = new TaxFormStatement2(formT2202aStatement.T2ReposStudent,
                                formT2202aStatement.T2ReposYear.ToString(), TaxForms.FormT2202A, formT2202aStatement.Recordkey);

                            // Set notation as cancelled if the status flag is "C" or if the cancelled flag is "Y".
                            if ((formT2202aStatement.T2CancelFlag != null && formT2202aStatement.T2CancelFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                                || (formT2202aStatement.T2Status != null && formT2202aStatement.T2Status.Equals("C", StringComparison.InvariantCultureIgnoreCase) && !(formT2202aStatement.T2AmendedFlag != null && formT2202aStatement.T2AmendedFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase))))
                            {
                                statementT2202a.Notation = TaxFormNotations.Cancelled;
                            }

                            // Add the statement to the list
                            statements.Add(statementT2202a);
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                    }
                }
            }
            return statements;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use GetCurrent1098TaxFormFromRecord2 instead.")]
        private TaxForms GetCurrent1098TaxFormFromRecord(string taxForm, Parm1098 parm1098)
        {
            TaxForms currentTaxForm = TaxForms.Form1098;
            if (taxForm == parm1098.P1098TTaxForm)
            {
                currentTaxForm = TaxForms.Form1098T;
            }
            else if (taxForm == parm1098.P1098ETaxForm)
            {
                currentTaxForm = TaxForms.Form1098E;
            }
            return currentTaxForm;
        }

        #endregion
    }
}


