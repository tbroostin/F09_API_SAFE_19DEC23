// Copyright 2016 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IStudentChargesRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentChargeRepository : BaseColleagueRepository, IStudentChargeRepository
    {
        /// <summary>
        /// Constructor to instantiate a student charges repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public StudentChargeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the StudentCharges requested
        /// </summary>
        /// <param name="id">StudentCharges GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<StudentCharge> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the INTG.GL.POSTINGS record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "AR.INV.ITEMS.INTG")
            {
                throw new KeyNotFoundException(string.Format("AR Invoice Items Integration record {0} does not exist.", id));
            }
            var intgStudentCharges = await DataReader.ReadRecordAsync<ArInvItemsIntg>(recordInfo.PrimaryKey);
            {
                if (intgStudentCharges == null)
                {
                    throw new KeyNotFoundException(string.Format("AR Invoice Items Integration record {0} does not exist.", id));
                }
            }

            return BuildStudentCharge(intgStudentCharges);
        }

        /// <summary>
        /// Get student charges for specific filters.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="personId">The person or student ID</param>
        /// <returns>A list of StudentCharge domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<StudentCharge>, int>> GetAsync(int offset, int limit, bool bypassCache, string personId = "", string term = "", string arCode = "", string arType = "", string chargeType = "", string usage = "")
        {
            var intgStudentChargesEntities = new List<StudentCharge>();
            var criteria = new StringBuilder();
            // Read the AR.INV.ITEMS.INTG records
            if (!string.IsNullOrEmpty(personId))
            {
                criteria.AppendFormat("WITH INVI.INTG.PERSON.ID = '{0}'", personId);
            }
            if (!string.IsNullOrEmpty(term))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH INVI.INTG.TERM = '{0}'", term);
            }
            if (!string.IsNullOrEmpty(arCode))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH INVI.INTG.AR.CODE = '{0}'", arCode);
            }
            if (!string.IsNullOrEmpty(arType))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH INVI.INTG.AR.TYPE = '{0}'", arType);
            }
            if (!string.IsNullOrEmpty(chargeType))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH INVI.INTG.CHARGE.TYPE = '{0}'", chargeType.ToLowerInvariant());
            }
            if (!string.IsNullOrEmpty(usage))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH INVI.INTG.USAGE = '{0}'", usage);
            }
            string select = criteria.ToString();
            string[] intgStudentChargeIds = await DataReader.SelectAsync("AR.INV.ITEMS.INTG", select);
            var totalCount = intgStudentChargeIds.Count();

            Array.Sort(intgStudentChargeIds);

            var subList = intgStudentChargeIds.Skip(offset).Take(limit).ToArray();
            var intgStudentCharges = await DataReader.BulkReadRecordAsync<ArInvItemsIntg>("AR.INV.ITEMS.INTG", subList);
            {
                if (intgStudentCharges == null)
                {
                    return new Tuple<IEnumerable<StudentCharge>, int>(new List<StudentCharge>(), 0);
                    //throw new KeyNotFoundException("No records selected from AR.INV.ITEMS.INTG in Colleague.");
                }
            }

            foreach (var intgStudentChargesEntity in intgStudentCharges)
            {
                intgStudentChargesEntities.Add(BuildStudentCharge(intgStudentChargesEntity));
            }
            return new Tuple<IEnumerable<StudentCharge>, int>(intgStudentChargesEntities, totalCount);
        }

        /// <summary>
        /// Update a single student charge entity for the data model version 6
        /// </summary>
        /// <param name="id">The GUID for the student charge entity</param>
        /// <param name="studentCharge">Student Charge to update</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<StudentCharge> UpdateAsync(string id, StudentCharge studentCharge)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
            if (!studentCharge.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var recordInfo = await GetRecordInfoFromGuidAsync(id);
                if (recordInfo != null)
                {
                    throw new InvalidOperationException(string.Format("AR Invoice Items Integration record {0} already exists.", id));
                }
            }
            return await CreateStudentCharges(studentCharge);
        }

        /// <summary>
        /// Create a single student charges entity for the data model version 6
        /// </summary>
        /// <param name="studentCharge">StudentCharge to create</param>
        /// <returns>A single StudentCharge</returns>
        public async Task<StudentCharge> CreateAsync(StudentCharge studentCharge)
        {
            if (!string.IsNullOrEmpty(studentCharge.Guid))
            {
                ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
                if (!studentCharge.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(studentCharge.Guid);
                    if (recordInfo != null)
                    {
                        throw new InvalidOperationException(string.Format("AR Invoice Items Integration record {0} already exists.", studentCharge.Guid));
                    }
                }
            }
            return await CreateStudentCharges(studentCharge);
        }

        /// <summary>
        /// Delete a single student charges for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charges GUID</param>
        /// <returns></returns>
        public async Task<StudentCharge> DeleteAsync(string id)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "AR.INV.ITEMS.INTG")
            {
                throw new KeyNotFoundException(string.Format("AR Invoice Items Integration record {0} does not exist.", id));
            }
            var request = new DeleteStudentChargeRequest()
            {
                ArInvItemsIntgId = recordInfo.PrimaryKey,
                Guid = id
            };

            ////Delete
            var response = await transactionInvoker.ExecuteAsync<DeleteStudentChargeRequest, DeleteStudentChargeResponse>(request);

            ////if there are any errors throw
            if (response.DeleteIntgGlPostingErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting student-charges: " + id);
                response.DeleteIntgGlPostingErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCode) ? "" : e.ErrorCode, e.ErrorMsg)));
                throw exception;
            }
            return null;
        }

        private StudentCharge BuildStudentCharge(ArInvItemsIntg integStudentCharge)
        {
            var studentCharge = new StudentCharge(integStudentCharge.InviIntgPersonId, integStudentCharge.InviIntgDueDate)
            {
                ChargeType = integStudentCharge.InviIntgChargeType,
                AccountsReceivableCode = integStudentCharge.InviIntgArCode,
                AccountsReceivableTypeCode = integStudentCharge.InviIntgArType,
                ChargeAmount = integStudentCharge.InviIntgAmt,
                ChargeCurrency = integStudentCharge.InviIntgAmtCurrency,
                Comments = !string.IsNullOrEmpty(integStudentCharge.InviIntgComments) ? new List<string> { integStudentCharge.InviIntgComments } : null,
                Guid = integStudentCharge.RecordGuid,
                InvoiceItemID = integStudentCharge.InviIntgArInvItem,
                Term = integStudentCharge.InviIntgTerm,
                UnitCost = integStudentCharge.InviIntgUnitCost,
                UnitCurrency = integStudentCharge.InviIntgUnitCurrency,
                UnitQuantity = integStudentCharge.InviIntgUnitQty,
                Usage = integStudentCharge.InviIntgUsage,
                OriginatedOn = integStudentCharge.InviIntgOriginatedOn,
                OverrideDescription = integStudentCharge.InviIntgOverrideDesc
            };
            return studentCharge;
        }

        private async Task<StudentCharge> CreateStudentCharges(StudentCharge studentCharge)
        {
            var comments = new StringBuilder();
            if (studentCharge.Comments != null)
            {
                foreach (var com in studentCharge.Comments)
                {
                    if (comments.Length > 0)
                    {
                        comments.Append(" ");
                    }
                    comments.Append(com);
                }
            }
            var request = new PostStudentChargesRequest()
            {
                InviIntgAmt = studentCharge.ChargeAmount,
                InviIntgAmtCurrency = studentCharge.ChargeCurrency,
                InviIntgArCode = studentCharge.AccountsReceivableCode,
                InviIntgArInvItem = studentCharge.InvoiceItemID,
                InviIntgArType = studentCharge.AccountsReceivableTypeCode,
                InviIntgChargeType = studentCharge.ChargeType,
                InviIntgComments = comments.ToString(),
                InviIntgDueDate = studentCharge.ChargeDate,
                InviIntgGuid = studentCharge.Guid,
                InviIntgPersonId = studentCharge.PersonId,
                InviIntgTerm = studentCharge.Term,
                InviIntgUnitCost = studentCharge.UnitCost,
                InviIntgUnitCurrency = studentCharge.UnitCurrency,
                InviIntgUnitQty = studentCharge.UnitQuantity,
                ElevateFlag = studentCharge.ChargeFromElevate,
                InviIntgUsage = studentCharge.Usage,
                InviIntgOriginatedOn = studentCharge.OriginatedOn,
                InviIntgOverrideDesc = studentCharge.OverrideDescription
            };

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (request.InviIntgGuid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                request.InviIntgGuid = string.Empty;
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<PostStudentChargesRequest, PostStudentChargesResponse>(request);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.Error))
            {
                var errorMessage = string.Format("Error(s) occurred updating student-charges for id: '{0}'.", request.InviIntgGuid);
                var exception = new RepositoryException(errorMessage);
                foreach (var errMsg in updateResponse.StudentChargeErrors)
                {
                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(errMsg.ErrorCodes, ": ", errMsg.ErrorMessages)));
                    errorMessage += string.Join(Environment.NewLine, errMsg.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
                throw exception;
            }

            return await GetByIdAsync(updateResponse.InviIntgGuid);
        }
    }
}
