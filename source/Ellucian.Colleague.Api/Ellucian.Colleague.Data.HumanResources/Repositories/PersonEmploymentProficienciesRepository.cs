/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates */

using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// This class implements the PersonEmploymentProficiencies
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonEmploymentProficienciesRepository : BaseColleagueRepository, IPersonEmploymentProficienciesRepository
    {

        private readonly int _bulkReadSize;
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;

        public PersonEmploymentProficienciesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get all HR.IND.SKILL records by paging
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<PersonEmploymentProficiency>, int>> GetPersonEmploymentProficienciesAsync(int offset, int limit, bool bypassCache = false)
        {
            try
            {
                var hrIndSkillIds = await DataReader.SelectAsync("HR.IND.SKILL", "");
                int totalCount = hrIndSkillIds.Count();
                Array.Sort(hrIndSkillIds);
                var subList = hrIndSkillIds.Skip(offset).Take(limit).ToArray();

                var hrIndSkillData = await DataReader.BulkReadRecordAsync<DataContracts.HrIndSkill>("HR.IND.SKILL", subList);

                if (hrIndSkillData == null)
                    throw new KeyNotFoundException("No records selected from HR.IND.SKILL entity in colleague");

                List<PersonEmploymentProficiency> allPep = new List<PersonEmploymentProficiency>();
                foreach (var hrIndSkill in hrIndSkillData)
                {
                    allPep.Add(BuildPEP(hrIndSkill));
                }

                return new Tuple<IEnumerable<PersonEmploymentProficiency>, int>(allPep, totalCount);
            }
            catch (RepositoryException REX)
            {
                throw REX;
            } catch (Exception EX)
            {
                RepositoryException REX = new RepositoryException();
                REX.AddError(new RepositoryError("PersonEmploymentProficiencies.Repository", EX.Message));
                throw REX;
            }
            
        }

        /// <summary>
        /// Get the HR.IND.SKILL by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<PersonEmploymentProficiency> GetPersonEmploymentProficiency(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                throw new ArgumentNullException("guid");

            var id = await GetRecordKeyFromGuidAsync(guid);

            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("Could not find id with GUID " + guid);
            }

            var hrIndSkill = await DataReader.ReadRecordAsync<HrIndSkill>(id);

            if (hrIndSkill == null)
            {
                throw new KeyNotFoundException("Could not find record for guid " + guid);
            }
            
            return BuildPEP(hrIndSkill);
        }

        /// <summary>
        /// Get the GUID for an Entity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> GetGuidFromID(string key, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, key);
            }
            catch (RepositoryException REX)
            {
                REX.AddError(new RepositoryError(entity + ".guid.NotFound", "GUID not found for " + entity + "id " + key));
                throw REX;
            }
            
        }

        /// <summary>
        /// Build the proficiency Entity with source coming from HR.IND.SKILL dataContract
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private PersonEmploymentProficiency BuildPEP(HrIndSkill source)
        {
            PersonEmploymentProficiency PEP = new PersonEmploymentProficiency();

            PEP.RecordKey = source.Recordkey;
            PEP.Guid = source.RecordGuid;
            PEP.PersonId = source.HskHrperId;
            PEP.ProficiencyId = source.HskJobskillId;
            PEP.StartOn = source.HskLicenseDate;
            PEP.EndOn = source.HskLicenseExpireDate;
            PEP.Comment = source.HskComment;

            return PEP;
        }

    }
}
