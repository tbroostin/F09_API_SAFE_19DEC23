// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository for W-2 statements
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class HumanResourcesTaxFormStatementRepository : BaseColleagueRepository, IHumanResourcesTaxFormStatementRepository
    {
        private readonly string colleagueTimeZone;

        /// <summary>
        /// Tax Form Statement repository constructor.
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public HumanResourcesTaxFormStatementRepository(ApiSettings settings, ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Retrieve set of tax form statements assigned to the specified person for the tax form type.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<TaxFormStatement2>> GetAsync(string personId, TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            var statements = new List<TaxFormStatement2>();

            // Based on the type of tax form, we will obtain the statements from different entities.
            switch (taxForm)
            {
                case TaxForms.FormW2:

                    // W2C read
                    var criteriaW2C = "WW2CO.EMPLOYEE.ID EQ '" + personId + "' AND WITH WW2CO.CORRECTION.YEAR GE '2010'";
                     var w2CStatementRecords = await DataReader.BulkReadRecordAsync<WebW2cOnline>(criteriaW2C);

                    if (w2CStatementRecords != null)
                    {
                        var criteriaTaxW2C = "WITH TW2C.EMPLOYEE.ID EQ '" + personId + "' AND WITH TW2C.STATUS EQ 'EXPORTED'";
                        var exportedTaxW2Ckeys = await DataReader.SelectAsync("TAX.W2C", criteriaTaxW2C);

                        //sets empty array if the DataReader returns a null array
                        if (exportedTaxW2Ckeys == null)
                        {
                            exportedTaxW2Ckeys = new string[0];
                        }

                        // Filters w2c statement records by whether or not they have been exported
                        var filteredW2CStatementRecords = w2CStatementRecords.Where(x => exportedTaxW2Ckeys.ToList().Contains(x.Ww2coSourceTaxW2cId)).ToList();

                        // Gets the shared Ww2coSourceTaxW2cIds between multiform records
                        var multipleFormW2cRecordKeys = filteredW2CStatementRecords.GroupBy(x => x.Ww2coSourceTaxW2cId).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

                        foreach (var contract in filteredW2CStatementRecords)
                        {
                            if (contract != null && !string.IsNullOrEmpty(contract.Ww2coEmployeeId) && !string.IsNullOrEmpty(contract.Ww2coCorrectionYear))
                            {
                                try
                                {
                                    //If it is in the multipleFormW2cRecordKey list, it is a multiple form w2c - otherwise, it receives a type of correction
                                    if (!string.IsNullOrEmpty(contract.Ww2coSourceTaxW2cId) && multipleFormW2cRecordKeys.Contains(contract.Ww2coSourceTaxW2cId))
                                    {
                                        var correctionStatement = new TaxFormStatement2(contract.Ww2coEmployeeId, contract.Ww2coCorrectionYear, TaxForms.FormW2C, contract.Recordkey, contract.WebW2cOnlineAdddate);
                                        correctionStatement.Notation = TaxFormNotations.MultipleForms;
                                        statements.Add(correctionStatement);
                                    }
                                    else
                                    {
                                        var correctionStatement = new TaxFormStatement2(contract.Ww2coEmployeeId, contract.Ww2coCorrectionYear, TaxForms.FormW2C, contract.Recordkey, contract.WebW2cOnlineAdddate);
                                        correctionStatement.Notation = TaxFormNotations.Correction;
                                        statements.Add(correctionStatement);
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                                }
                            }
                        }
                    }

                    // W2 read
                    var criteria = "WW2O.EMPLOYEE.ID EQ '" + personId + "' AND WITH WW2O.YEAR GE '2010'";
                    var w2StatementRecords = await DataReader.BulkReadRecordAsync<WebW2Online>(criteria);

                    if (w2StatementRecords != null)
                    {
                        foreach (var contract in w2StatementRecords)
                        {
                            if (contract != null && !string.IsNullOrEmpty(contract.Ww2oEmployeeId) && !string.IsNullOrEmpty(contract.Ww2oYear))
                            {
                                try
                                {
                                    // If the record we're processing is a correction and has no W2c record then remove all forms for the same year
                                    // from the statements list then add the correction. Otherwise, only insert the non-correction
                                    // record if no corrections exist for the given year.
                                    if (!string.IsNullOrEmpty(contract.Ww2oW2cFlag)
                                        && contract.Ww2oW2cFlag == "Y"
                                        && statements.Where(x => x.TaxYear == contract.Ww2oYear && x.TaxForm == TaxForms.FormW2C).ToList().Count == 0)
                                    {
                                        // W-2's can only have a single correction.
                                        statements.RemoveAll(x => x.TaxYear == contract.Ww2oYear);
                                        var statement = new TaxFormStatement2(contract.Ww2oEmployeeId, contract.Ww2oYear, TaxForms.FormW2, contract.Recordkey);
                                        statement.Notation = TaxFormNotations.Correction;
                                        statements.Add(statement);
                                    }
                                    else
                                    {
                                        if (statements.Where(x => (x.TaxForm == TaxForms.FormW2 && x.Notation == TaxFormNotations.Correction)
                                            && x.TaxYear == contract.Ww2oYear).ToList().Count == 0)
                                        {
                                            // Add two statements if this record has overflow data. Otherwise just add
                                            // a single record with no notation.
                                            if (!string.IsNullOrEmpty(contract.Ww2oCodeBoxCodeE)
                                                || !string.IsNullOrEmpty(contract.Ww2oOtherBoxCodeE)
                                                || !string.IsNullOrEmpty(contract.Ww2oStateCodeC)
                                                || !string.IsNullOrEmpty(contract.Ww2oLocalNameC))
                                            {
                                                // First add the statement record
                                                var statement = new TaxFormStatement2(contract.Ww2oEmployeeId, contract.Ww2oYear, TaxForms.FormW2, contract.Recordkey);
                                                statement.Notation = TaxFormNotations.HasOverflowData;
                                                statements.Add(statement);

                                                // Next add the extra overflow record
                                                var overflowStatement = new TaxFormStatement2(contract.Ww2oEmployeeId, contract.Ww2oYear, TaxForms.FormW2, contract.Recordkey);
                                                overflowStatement.Notation = TaxFormNotations.IsOverflow;
                                                statements.Add(overflowStatement);
                                            }
                                            else
                                            {
                                                statements.Add(new TaxFormStatement2(contract.Ww2oEmployeeId, contract.Ww2oYear, TaxForms.FormW2, contract.Recordkey));
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                                }
                            }
                        }
                  }
                    // Lastly, search for years that have multiple statements and mark those statements as such.
                    var statementYears = statements.Select(x => x.TaxYear).Distinct();
                    foreach (var year in statementYears)
                    {
                        var multipleStatements = statements.Where(x => x.TaxYear == year && x.Notation == TaxFormNotations.None).ToList();
                        if (multipleStatements.Count > 1)
                        {
                            foreach (var statementToUpdate in multipleStatements)
                            {
                                statementToUpdate.Notation = TaxFormNotations.MultipleForms;
                            }
                        }
                    }
                    break;

                case TaxForms.Form1095C:

                    // Get all the 1095-C records for the person id that have a status of submitted or corrected
                    var form1095cCriteria = "TFCWH.HRPER.ID EQ '" + personId + "' AND WITH TFCWH.STATUS EQ 'FRO' 'SUB' 'COR' 'GFE'";
                    var form1095cStatementRecords = await DataReader.BulkReadRecordAsync<TaxForm1095cWhist>(form1095cCriteria);

                    if (form1095cStatementRecords != null)
                    {
                        // From all the selected records, get a list of unique tax years for which there are statements
                        // In case we get a null record, or the record does not have a tax year, check for a null tax year first.
                        var taxYears = form1095cStatementRecords.Where(y => y != null).Select(x => x.TfcwhTaxYear).Distinct().ToList();

                        // Loop through each tax year
                        foreach (var year in taxYears)
                        {
                            // Get the statement for the year that is the most recent based on the record add date.
                            // If there are no corrections, it should be just the one submitted.
                            // If there are corrections, it should be the last statement that was corrected.
                            // In case we get a null record, or the record does not have a tax year, check for a null tax year first.
                            // If the record has no add date or add time, we still want to process it and show the statement if appropriate.

                            try
                            {
                                // Catch any exception that .First may throw if the data is corrupted.
                                var mostRecentStatement = form1095cStatementRecords.Where(x => x != null && x.TfcwhTaxYear == year).OrderByDescending(x => x.TaxForm1095cWhistAdddate).ThenByDescending(t => t.TaxForm1095cWhistAddtime).First();

                                if (!string.IsNullOrEmpty(mostRecentStatement.TfcwhHrperId) && !string.IsNullOrEmpty(mostRecentStatement.TfcwhTaxYear))
                                {
                                    var statement1095c = new TaxFormStatement2(mostRecentStatement.TfcwhHrperId, mostRecentStatement.TfcwhTaxYear, TaxForms.Form1095C, mostRecentStatement.Recordkey);

                                    // Add the statement to the entity
                                    statements.Add(statement1095c);
                                }
                            }
                            catch (Exception e)
                            {
                                LogDataError("TaxFormStatement", personId, new Object(), e, e.Message);
                            }
                        }
                    }
                    break;

                case TaxForms.FormT4:
                    // Get all the T4 records for the person id that have a status of submitted or corrected
                    var formT4Criteria = "WT4O.EMPLOYEE.ID EQ '" + personId + "' AND WITH WT4O.YEAR GE '2010'";
                    var formT4StatementRecords = await DataReader.BulkReadRecordAsync<WebT4Online>(formT4Criteria);

                    if (formT4StatementRecords != null)
                    {
                        var formT4StatementRecordsList = formT4StatementRecords.Where(v => v != null).OrderByDescending(w => w.Wt4oYear).ThenByDescending(x => x.WebT4OnlineAdddate).ThenByDescending(y => y.WebT4OnlineAddtime).ThenByDescending(z => z.Recordkey).ToList();
                        foreach (var statement in formT4StatementRecordsList)
                        {
                            try
                            {
                                var firstStatement = new TaxFormStatement2(statement.Wt4oEmployeeId, statement.Wt4oYear, TaxForms.FormT4, statement.Recordkey);
                                var numberOfSlips = 0;
                                if (statement.Wt4oOtherInfoFlags != null)
                                {
                                    var numberOfBoxes = statement.Wt4oOtherInfoFlags.Where(x => x.ToUpper() == "Y").Count();
                                    numberOfSlips = numberOfBoxes / 6;

                                    if (numberOfSlips > 0)
                                    {
                                        firstStatement.Notation = TaxFormNotations.HasOverflowData;
                                    }
                                }
                                statements.Add(firstStatement);
                                for (var i = 0; i < numberOfSlips; i++)
                                {
                                    statements.Add(new TaxFormStatement2(statement.Wt4oEmployeeId, statement.Wt4oYear, TaxForms.FormT4, statement.Recordkey) { Notation = TaxFormNotations.IsOverflow });
                                }
                            }
                            catch (Exception e)
                            {
                                LogDataError("TaxFormStatement", personId, new object(), e, e.Message);
                            }
                        }
                        statements = statements.OrderByDescending(x => x.TaxYear).ToList();
                    }


                    break;
            }

            return statements;
        }
    }
}
