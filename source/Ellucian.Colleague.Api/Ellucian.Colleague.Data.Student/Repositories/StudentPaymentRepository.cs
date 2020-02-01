﻿// Copyright 2017-2019 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Text;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IStudentPaymentsRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentPaymentRepository : BaseColleagueRepository, IStudentPaymentRepository
    {

        const string AllStudentPaymentsCache = "AllStudentPayments";

        const int AllStudentPaymentsCacheTimeout = 20; // Clear from cache every 20 minutes

        /// <summary>
        /// Constructor to instantiate a student payments repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public StudentPaymentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Get the StudentPayments requested
        /// </summary>
        /// <param name="id">StudentPayments GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<StudentPayment> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the INTG.GL.POSTINGS record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "AR.PAY.ITEMS.INTG")
            {
                throw new KeyNotFoundException(string.Format("No student payment was found for guid '{0}'. ", id));
            }
            var intgStudentPayments = await DataReader.ReadRecordAsync<ArPayItemsIntg>(recordInfo.PrimaryKey);
            {
                if (intgStudentPayments == null)
                {
                    throw new KeyNotFoundException(string.Format("No student payment was found for guid '{0}'. ", id));
                }
            }

            var exception = new RepositoryException();
            StudentPayment studentPayment = null;
            try
            {
                studentPayment = await BuildStudentPayment(intgStudentPayments);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Global.Internal.Error", ex.Message));
                throw exception;
            }

            return studentPayment;
        }

        /// <summary>
        /// Get student payments for specific filters.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="personId">The person or student ID</param>
        /// <returns>A list of StudentPayment domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<StudentPayment>,int>> GetAsync(int offset, int limit, bool bypassCache, string personId = "", string term = "", string arCode = "", string paymentType = "")
        {
            var intgStudentPaymentsEntities = new List<StudentPayment>();
            var criteria = new StringBuilder();
            // Read the AR.PAY.ITEMS.INTG records
            if (!string.IsNullOrEmpty(personId))
            {
                criteria.AppendFormat("WITH ARP.INTG.PERSON.ID = '{0}'", personId);
            }
            if (!string.IsNullOrEmpty(term))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH ARP.INTG.TERM = '{0}'", term);
            }
            if (!string.IsNullOrEmpty(arCode))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH ARP.INTG.AR.CODE = '{0}'", arCode);
            }
            if (!string.IsNullOrEmpty(paymentType))
            {
                if (criteria.Length > 0)
                {
                    criteria.Append(" AND ");
                }
                criteria.AppendFormat("WITH ARP.INTG.PAYMENT.TYPE = '{0}'", paymentType.ToLowerInvariant());
            }
            string select = criteria.ToString();
            string[] intgStudentPaymentIds = await DataReader.SelectAsync("AR.PAY.ITEMS.INTG", select);
            var totalCount = intgStudentPaymentIds.Count();

            Array.Sort(intgStudentPaymentIds);

            var subList = intgStudentPaymentIds.Skip(offset).Take(limit).ToArray();
            var intgStudentPayments = await DataReader.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG", subList);
            {
                if (intgStudentPayments == null)
                {
                    throw new KeyNotFoundException("No records selected from AR.PAY.ITEMS.INTG in Colleague.");
                }
            }

            foreach (var intgStudentPaymentsEntity in intgStudentPayments)
            {
                intgStudentPaymentsEntities.Add(await BuildStudentPayment(intgStudentPaymentsEntity));
            }
            return new Tuple<IEnumerable<StudentPayment>,int>(intgStudentPaymentsEntities, totalCount);
        }

        /// <summary>
        /// Get student payments for specific filters. Previous version supported ar codes.
        /// This version adds Distribution method and AR type filters.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="personId">The person or student ID</param>
        /// <returns>A list of StudentPayment domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<StudentPayment>, int>> GetAsync2(int offset, int limit, bool bypassCache, string personId = "", string term = "", string distrCode = "", string paymentType = "", string arType = "", string usage = "")
        {
            var intgStudentPaymentsEntities = new List<StudentPayment>();

            int totalCount = 0;
            string[] subList = null;

            string studentPaymentsCacheKey = CacheSupport.BuildCacheKey(AllStudentPaymentsCache, personId, term, distrCode, paymentType, arType, usage);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
               this,
               ContainsKey,
               GetOrAddToCacheAsync,
               AddOrUpdateCacheAsync,
               transactionInvoker,
               studentPaymentsCacheKey,
               "AR.PAY.ITEMS.INTG",
               offset,
               limit,
               AllStudentPaymentsCacheTimeout,
               async () =>
               {
                   var criteria = new StringBuilder();
                   // Read the AR.PAY.ITEMS.INTG records
                   if (!string.IsNullOrEmpty(personId))
                   {
                       criteria.AppendFormat("WITH ARP.INTG.PERSON.ID = '{0}'", personId);
                   }
                   if (!string.IsNullOrEmpty(term))
                   {
                       if (criteria.Length > 0)
                       {
                           criteria.Append(" AND ");
                       }
                       criteria.AppendFormat("WITH ARP.INTG.TERM = '{0}'", term);
                   }
                   if (!string.IsNullOrEmpty(distrCode))
                   {
                       if (criteria.Length > 0)
                       {
                           criteria.Append(" AND ");
                       }
                       criteria.AppendFormat("WITH ARP.INTG.DISTR.MTHD = '{0}'", distrCode);

                       //// If filter value is the default value, check for null payment type too.
                       var ldmDefaults = DataReader.ReadRecord<Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
                       var defaultDistrCode = ldmDefaults != null ? ldmDefaults.LdmdDefaultDistr : string.Empty;
                       if (defaultDistrCode == distrCode)
                       {
                           criteria.AppendFormat("''");
                       }
                   }
                   if (!string.IsNullOrEmpty(paymentType))
                   {
                       if (criteria.Length > 0)
                       {
                           criteria.Append(" AND ");
                       }
                       criteria.AppendFormat("WITH ARP.INTG.PAYMENT.TYPE = '{0}'", paymentType.ToLowerInvariant());
                   }
                   if (!string.IsNullOrEmpty(arType))
                   {
                       if (criteria.Length > 0)
                       {
                           criteria.Append(" AND ");
                       }
                       criteria.AppendFormat("WITH ARP.INTG.AR.TYPE = '{0}'", arType);
                   }
                   if (!string.IsNullOrEmpty(usage))
                   {
                       if (criteria.Length > 0)
                       {
                           criteria.Append(" AND ");
                       }
                       criteria.AppendFormat("WITH ARP.INTG.USAGE = '{0}'", usage);
                   }
                   CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                   {
                      criteria = criteria.ToString(),
                   };
                   return requirements;
               });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<StudentPayment>, int>(intgStudentPaymentsEntities, 0);
            }

            
            subList = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;

            var intgStudentPayments = await DataReader.BulkReadRecordAsync<ArPayItemsIntg>("AR.PAY.ITEMS.INTG", subList);
            {
                if (intgStudentPayments == null)
                {
                    throw new KeyNotFoundException("No records selected from AR.PAY.ITEMS.INTG in Colleague.");
                }
            }

            var exception = new RepositoryException();
            foreach (var intgStudentPaymentsEntity in intgStudentPayments)
            {
                try
                {
                    intgStudentPaymentsEntities.Add(await BuildStudentPayment(intgStudentPaymentsEntity));
                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("Global.Internal.Error", ex.Message));
                }
            }

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<StudentPayment>, int>(intgStudentPaymentsEntities, totalCount);
        }

        /// <summary>
        /// Update a single student payment for the data model version 6
        /// </summary>
        /// <param name="id">The GUID to the student payment entity</param>
        /// <param name="studentPayment">Student Payment to update</param>
        /// <returns>A single StudentPayment entity</returns>
        public async Task<StudentPayment> UpdateAsync(string id, StudentPayment studentPayment)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }  
            ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
            if (!studentPayment.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var recordInfo = await GetRecordInfoFromGuidAsync(id);
                if (recordInfo != null)
                {
                    throw new InvalidOperationException(string.Format("AR Payment Items Integration record {0} already exists.", id));
                }
            }
            return await CreateStudentPayments(studentPayment);
        }

        /// <summary>
        /// Create a single student payments entity for the data model version 6
        /// </summary>
        /// <param name="studentPayment">StudentPayment to create</param>
        /// <returns>A single StudentPayment</returns>
        public async Task<StudentPayment> CreateAsync(StudentPayment studentPayment)
        {
            if (!string.IsNullOrEmpty(studentPayment.Guid))
            {
                ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
                if (!studentPayment.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(studentPayment.Guid);
                    if (recordInfo != null)
                    {
                        throw new InvalidOperationException(string.Format("AR Payment Items Integration record {0} already exists.", studentPayment.Guid));
                    }
                }
            }
            return await CreateStudentPayments(studentPayment);
        }

        /// <summary>
        /// Create a single student payments entity for the data model version 6
        /// </summary>
        /// <param name="studentPayment">StudentPayment to create</param>
        /// <returns>A single StudentPayment</returns>
        public async Task<StudentPayment> CreateAsync2(StudentPayment studentPayment)
        {
            if (!string.IsNullOrEmpty(studentPayment.Guid))
            {
                ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
                if (!studentPayment.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(studentPayment.Guid);
                    if (recordInfo != null)
                    {
                        throw new InvalidOperationException(string.Format("AR Payment Items Integration record {0} already exists.", studentPayment.Guid));
                    }
                }
            }
            return await CreateStudentPayments2(studentPayment);
        }

        /// <summary>
        /// Delete a single student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns></returns>
        public async Task<StudentPayment> DeleteAsync(string id)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "AR.PAY.ITEMS.INTG")
            {
                throw new KeyNotFoundException(string.Format("AR Invoice Items Integration record {0} does not exist.", id));
            }
            var request = new DeleteStudentPaymentRequest()
            {
                ArPayItemsIntgId = recordInfo.PrimaryKey,
                Guid = id
            };

            ////Delete
            var response = await transactionInvoker.ExecuteAsync<DeleteStudentPaymentRequest, DeleteStudentPaymentResponse>(request);

            ////if there are any errors throw
            if (response.DeleteStudentPaymentErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting student-payments: " + id);
                response.DeleteStudentPaymentErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCode) ? "" : e.ErrorCode, e.ErrorMsg)));
                throw exception;
            }
            return null;
        }

        private async Task<StudentPayment> BuildStudentPayment(ArPayItemsIntg intgStudentPayment)
        {
            var studentPayment = new StudentPayment(intgStudentPayment.ArpIntgPersonId,
                intgStudentPayment.ArpIntgPaymentType, intgStudentPayment.ArpIntgPaymentDate)
            {
                RecordKey = intgStudentPayment.Recordkey,
                AccountsReceivableCode = intgStudentPayment.ArpIntgArCode,
                AccountsReceivableTypeCode = intgStudentPayment.ArpIntgArType,
                PaymentAmount = intgStudentPayment.ArpIntgAmt,
                PaymentCurrency = intgStudentPayment.ArpIntgAmtCurrency,
                Comments = !string.IsNullOrEmpty(intgStudentPayment.ArpIntgComments) ? new List<string> { intgStudentPayment.ArpIntgComments } : null,
                Guid = intgStudentPayment.RecordGuid,
                PaymentID = intgStudentPayment.ArpIntgArPaymentsIds.Any() ? intgStudentPayment.ArpIntgArPaymentsIds.ElementAt(0) : string.Empty,
                Term = intgStudentPayment.ArpIntgTerm,
                Usage = intgStudentPayment.ArpIntgUsage,
                OriginatedOn = intgStudentPayment.ArpIntgOriginatedOn,
                OverrideDescription = intgStudentPayment.ArpIntgOverrideDesc
            };
            // Derive the distribution method. This is a required field so We'll insure we get a value
            if (!string.IsNullOrWhiteSpace(intgStudentPayment.ArpIntgDistrMthd))
            {
                //if it stored in the INTG table then get it from there
                studentPayment.DistributionCode = intgStudentPayment.ArpIntgDistrMthd;
            }
            else
            {
                CashRcpts cashRcpts = null;
                if (intgStudentPayment.ArpIntgArPaymentsIds.Any() && intgStudentPayment.ArpIntgArPaymentsIds != null)
                {
                    var arPaymentsId = intgStudentPayment.ArpIntgArPaymentsIds[0];
                    var arPaymentsColumns = new string[] { "ARP.CASH.RCPT" };
                    var arPayments = await DataReader.ReadRecordColumnsAsync("AR.PAYMENTS", arPaymentsId, arPaymentsColumns);
                    if (arPayments != null && arPayments.Any())
                    {
                        var cashRcptsId = arPayments.ElementAt(0).Value;
                        if (!string.IsNullOrEmpty(cashRcptsId))
                            cashRcpts = await DataReader.ReadRecordAsync<CashRcpts>(cashRcptsId);
                    }
                }

                // Derive the distribution method. This is a required field so We'll insure we get a value
                if (cashRcpts != null && !string.IsNullOrWhiteSpace(cashRcpts.RcptTenderGlDistrCode))
                {
                    //If we have a cashRcpt that has it then get it form there.
                    studentPayment.DistributionCode = cashRcpts.RcptTenderGlDistrCode;
                }
                else
                {
                    //If all else fails get the default record. as this is a required field for V11.
                    //it could mean this record was created by a previous version which didn't have the ArpIntgDistrMthd
                    //populated yet. This should only be a case for Sponsor types.
                    var ldmDefaults = DataReader.ReadRecord<Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
                    studentPayment.DistributionCode = ldmDefaults.LdmdDefaultDistr;
                }
            }

            return studentPayment;
        }
 
        private async Task<StudentPayment> CreateStudentPayments(StudentPayment studentPayment)
        {
            var comments = new StringBuilder();
            if (studentPayment.Comments != null)
            {
                foreach (var com in studentPayment.Comments)
                {
                    if (comments.Length > 0)
                    {
                        comments.Append(" ");
                    }
                    comments.Append(com);
                }
            }
            var request = new PostStudentPaymentsRequest()
            {
                ArpIntgAmt = studentPayment.PaymentAmount,
                ArpIntgAmtCurrency = studentPayment.PaymentCurrency,
                ArpIntgArCode = studentPayment.AccountsReceivableCode,
                ArpIntgArType = studentPayment.AccountsReceivableTypeCode,
                ArpIntgPaymentType = studentPayment.PaymentType,
                ArpIntgComments = comments.ToString(),
                ArpIntgPaymentDate = studentPayment.PaymentDate,
                ArpIntgGuid = studentPayment.Guid,
                ArpIntgPersonId = studentPayment.PersonId,
                ArpIntgTerm = studentPayment.Term,
                ElevateFlag = studentPayment.ChargeFromElevate,
                //ArpGlPosted = studentPayment.GlPosted.HasValue == true ? studentPayment.GlPosted.Value:false,
                ArpIntgDistrMthd = studentPayment.DistributionCode
            };

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (request.ArpIntgGuid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                request.ArpIntgGuid = string.Empty;
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<PostStudentPaymentsRequest, PostStudentPaymentsResponse>(request);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.Error))
            {
                var errorMessage = string.Format("Error(s) occurred updating student-payments for id: '{0}'.", request.ArpIntgGuid);
                var exception = new RepositoryException(errorMessage);
                foreach (var errMsg in updateResponse.StudentPaymentErrors)
                {
                    exception.AddError(new RepositoryError(string.IsNullOrEmpty(errMsg.ErrorCodes) ? "" : errMsg.ErrorCodes, errMsg.ErrorMessages));
                    errorMessage += string.Join(Environment.NewLine, errMsg.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
                throw exception;
            }

            return await GetByIdAsync(updateResponse.ArpIntgGuid);
        }

        private async Task<StudentPayment> CreateStudentPayments2(StudentPayment studentPayment)
        {
            var comments = new StringBuilder();
            if (studentPayment.Comments != null)
            {
                foreach (var com in studentPayment.Comments)
                {
                    if (comments.Length > 0)
                    {
                        comments.Append(" ");
                    }
                    comments.Append(com);
                }
            }
            var request = new PostStudentPaymentsRequest()
            {
                ArpIntgAmt = studentPayment.PaymentAmount,
                ArpIntgAmtCurrency = studentPayment.PaymentCurrency,
                ArpIntgArCode = studentPayment.AccountsReceivableCode,
                ArpIntgArType = studentPayment.AccountsReceivableTypeCode,
                ArpIntgPaymentType = studentPayment.PaymentType,
                ArpIntgComments = comments.ToString(),
                ArpIntgPaymentDate = studentPayment.PaymentDate,
                ArpIntgGuid = studentPayment.Guid,
                ArpIntgPersonId = studentPayment.PersonId,
                ArpIntgTerm = studentPayment.Term,
                ElevateFlag = studentPayment.ChargeFromElevate,
                ArpIntgDistrMthd = studentPayment.DistributionCode,
                ArpIntgUsage = studentPayment.Usage,
                ArpIntgOriginatedOn = studentPayment.OriginatedOn,
                ArpIntgOverrideDesc = studentPayment.OverrideDescription
            };

            //Since Sponsor types require a AR code and that has been removed from the payload we need to grab
            //a default value to pass to the subroutine.
            var ldmDefaults = DataReader.ReadRecord<Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
            request.ArpIntgArCode = ldmDefaults != null ? ldmDefaults.LdmdSponsorArCode : string.Empty;

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (request.ArpIntgGuid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                request.ArpIntgGuid = string.Empty;
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<PostStudentPaymentsRequest, PostStudentPaymentsResponse>(request);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.Error))
            {
                var errorMessage = string.Format("Error(s) occurred updating student-payments for id: '{0}'.", request.ArpIntgGuid);
                var exception = new RepositoryException(errorMessage);
                foreach (var errMsg in updateResponse.StudentPaymentErrors)
                {
                    exception.AddError(new RepositoryError(string.IsNullOrEmpty(errMsg.ErrorCodes) ? "" : errMsg.ErrorCodes, errMsg.ErrorMessages));
                    errorMessage += string.Join(Environment.NewLine, errMsg.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
                throw exception;
            }

            return await GetByIdAsync(updateResponse.ArpIntgGuid);
        }
    }
}
