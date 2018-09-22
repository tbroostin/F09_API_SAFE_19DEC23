/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Exceptions;
using System.Text;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ExternalEmploymentsRepository : BaseColleagueRepository, IExternalEmploymentsRepository
    {
        private readonly int _readSize;

        public ExternalEmploymentsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {

            CacheTimeout = Level1CacheTimeoutValue;

            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;

        }

        /// <summary>
        ///  Get all external employments
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns>Collection of ExternalEmployments domain entities</returns>
        public async Task<Tuple<IEnumerable<ExternalEmployments>, int>> GetExternalEmploymentsAsync(int offset, int limit)
        {
            var externalEmploymentsEntities = new List<ExternalEmployments>();
            var criteria = new StringBuilder();

            string select = criteria.ToString();
            string[] externalEmploymentsIds = await DataReader.SelectAsync("EMPLOYMT", select);
            var totalCount = externalEmploymentsIds.Count();

            Array.Sort(externalEmploymentsIds);

            var subList = externalEmploymentsIds.Skip(offset).Take(limit).ToArray();
            var externalEmployments = await DataReader.BulkReadRecordAsync<DataContracts.Employmt>("EMPLOYMT", subList);
            {
                if (externalEmployments == null)
                {
                    throw new KeyNotFoundException("No records selected from EMPLOYMT in Colleague.");
                }
            }

            foreach (var externalEmplyEntity in externalEmployments)
            {
                externalEmploymentsEntities.Add(await BuildExternalEmploymentsAsync(externalEmplyEntity));
            }
            return new Tuple<IEnumerable<ExternalEmployments>, int>(externalEmploymentsEntities, totalCount);
        }


        /// <summary>
        /// Get external employments by a guid
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns>External Employment entity</returns>
        public async Task<ExternalEmployments> GetExternalEmploymentsByGuidAsync(string guid)
        {
            if (!(string.IsNullOrEmpty(guid)))
            {
                var id = await GetExternalEmploymentsIdFromGuidAsync(guid);

                try
                {
                    if (!(string.IsNullOrEmpty(id)))
                    {
                        return await GetExternalEmploymentsByIdAsync(id);
                    }
                }
                catch
                    (Exception e)
                {
                    logger.Error(string.Format("Could not build external-employments for guid {0}", guid));
                    logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);

                }
            }
            return null;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key for EMPLOYMT</returns>
        public async Task<string> GetExternalEmploymentsIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Employmt GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Employmt GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "EMPLOYMT")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, EMPLOYMT");
            }

            return foundEntry.Value.PrimaryKey;
        }


        /// <summary>
        /// Get a single external employment with EMPLOYMT ID
        /// </summary>
        /// <param name="id">external employment id</param>
        /// <returns>External employment entity object</returns>
        public async Task<ExternalEmployments> GetExternalEmploymentsByIdAsync(string id)
        {
            ExternalEmployments employmt = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get an employmt record.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Employmt>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or EMPLOYMT with ID '", id, "' is invalid."));
            }
            employmt = await BuildExternalEmploymentsAsync(record);
            return employmt;
        }

        /// <summary>
        /// Helper to build external eomployment object
        /// </summary>
        /// <param name="source">the employmt db record</param>
        /// <returns>external employment Entity object</returns>
        public async Task<ExternalEmployments> BuildExternalEmploymentsAsync(Employmt source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build employmt entity.");
            }
            ExternalEmployments externalEmployments = null;
            
            externalEmployments = new ExternalEmployments(source.RecordGuid, source.Recordkey, source.EmpEmployee, source.EmpTitle, source.EmpStatus)
            {
                OrganizationId = source.EmpEmployer,
                PositionId = source.EmpPosition,
                StartDate = source.EmpStartDate,
                EndDate = source.EmpEndDate,
                PrincipalEmployment = source.EmpPrincipalEmploymtInd,
                Status = source.EmpStatus,
                HoursWorked = source.EmpHoursPerWeek,
                Vocations = source.EmpVocations,
                comments = source.EmpComments,
                selfEmployed = source.EmpSelfEmployedFlag,
                unknownEmployer = source.EmpUnknownEmployerFlag
            };
            // get org name
            if (!string.IsNullOrEmpty(externalEmployments.OrganizationId))
            {
                var corpContract = await DataReader.ReadRecordAsync<Corp>("PERSON", externalEmployments.OrganizationId);
                externalEmployments.OrgName = String.Join(" ", corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x)));

            }
            //get supervisors
            if (source.EmpSpvsrEntityAssociation != null && source.EmpSpvsrEntityAssociation.Any())
            {
                var supervisors = new List<ExternalEmploymentSupervisors>();
                foreach (var super in source.EmpSpvsrEntityAssociation)
                {
                    var supervisor = new ExternalEmploymentSupervisors(super.EmpSpvsrFirstNameAssocMember, super.EmpSpvsrLastNameAssocMember, super.EmpSpvsrPhoneAssocMember, super.EmpSpvsrEmailAssocMember);
                    supervisors.Add(supervisor);
                }
                externalEmployments.Supervisors = supervisors;
            }
            return externalEmployments;
        }
    }
        
    
}