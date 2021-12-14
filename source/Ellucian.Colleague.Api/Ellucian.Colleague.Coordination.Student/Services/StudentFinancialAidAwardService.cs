// Copyright 2017-2020 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Implements the IStudentFinancialAidAwardService
    /// </summary>
    [RegisterType]
    public class StudentFinancialAidAwardService : BaseCoordinationService, IStudentFinancialAidAwardService
    {
        private readonly IStudentFinancialAidAwardRepository _studentFinancialAidAwardRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IFinancialAidFundRepository _financialAidFundRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IStudentFinancialAidOfficeRepository _studentFinancialAidOfficeRepository;
        private Dictionary<string, string> studentIdDict = null;


        /// <summary>
        /// Constructor used by injection framework
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentFinancialAidAwardRepository"></param>
        /// <param name="financialAidFundRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="financialAidReferenceDataRepository"></param>
        /// <param name="studentFinancialAidOfficeRepository"></param>
        /// <param name="communicationRepository"></param>
        /// <param name="termRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public StudentFinancialAidAwardService(IAdapterRegistry adapterRegistry,
            IStudentFinancialAidAwardRepository studentFinancialAidAwardRepository,
            IReferenceDataRepository referenceDataRepository,
            IFinancialAidFundRepository financialAidFundRepository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository financialAidReferenceDataRepository,
            IStudentFinancialAidOfficeRepository studentFinancialAidOfficeRepository,
            ICommunicationRepository communicationRepository,
            ITermRepository termRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentFinancialAidAwardRepository = studentFinancialAidAwardRepository;
            _referenceDataRepository = referenceDataRepository;
            _financialAidFundRepository = financialAidFundRepository;
            _personRepository = personRepository;
            _studentReferenceDataRepository = financialAidReferenceDataRepository;
            _studentFinancialAidOfficeRepository = studentFinancialAidOfficeRepository;
            _communicationRepository = communicationRepository;
            _termRepository = termRepository;
            _configurationRepository = configurationRepository;
        }

        private IEnumerable<Domain.Student.Entities.AwardStatus> _awardStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.AwardStatus>> GetAwardStatusesAsync(bool bypassCache = false)
        {
            if (_awardStatuses == null)
            {
                _awardStatuses = await _studentReferenceDataRepository.AwardStatusesAsync(bypassCache);
            }
            return _awardStatuses;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidAwardPeriod> _financialAidAwardPeriods = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool bypassCache = false)
        {
            if (_financialAidAwardPeriods == null)
            {
                _financialAidAwardPeriods = await _studentReferenceDataRepository.GetFinancialAidAwardPeriodsAsync(bypassCache);
            }
            return _financialAidAwardPeriods;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidFund> _financialAidFunds = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidFund>> GetFinancialAidFundsAsync(bool bypassCache = false)
        {
            if (_financialAidFunds == null)
            {
                _financialAidFunds = await _financialAidFundRepository.GetFinancialAidFundsAsync(bypassCache);
            }
            return _financialAidFunds;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidYear> _financialAidYears = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidYear>> GetFinancialAidYearsAsync(bool bypassCache = false)
        {
            if (_financialAidYears == null)
            {
                _financialAidYears = await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache);
            }
            return _financialAidYears;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidFundCategory> _financialAidFundCategories = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool bypassCache = false)
        {
            if (_financialAidFundCategories == null)
            {
                _financialAidFundCategories = await _studentReferenceDataRepository.GetFinancialAidFundCategoriesAsync(bypassCache);
            }
            return _financialAidFundCategories;
        }

        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync(bool bypassCache = false)
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        private IEnumerable<string> _notAwardedCategories = null;
        private async Task<IEnumerable<string>> GetNotAwardedCategoriesAsync()
        {
            if (_notAwardedCategories == null)
            {
                _notAwardedCategories = await _studentFinancialAidAwardRepository.GetNotAwardedCategoriesAsync(); ;
            }
            return _notAwardedCategories;
        }


        /// <summary>
        /// Returns the DTO for the specified student financial aid award v7
        /// </summary>
        /// <param name="id">Guid for Student Financial Aid Award</param>
        /// <param name="restricted">True if you are allowed to view restricted awards.</param>
        /// <returns>Student Financial Aid Award DTO</returns>
        public async Task<Dtos.StudentFinancialAidAward> GetByIdAsync(string id, bool restricted)
        {
            
            // Get the student financial aid awards domain entity from the repository
            var studentFinancialAidAwardDomainEntity = await _studentFinancialAidAwardRepository.GetByIdAsync(id);

            if (studentFinancialAidAwardDomainEntity == null)
            {
                throw new ArgumentNullException("StudentFinancialAidAwardDomainEntity", "StudentFinancialAidAwardDomainEntity cannot be null. ");
            }

            // Find a list of all non-securred financial aid funds
            var unrestrictedFundCategories = (await GetFinancialAidFundCategoriesAsync()).Where(fc => fc.restrictedFlag == false).Select(fc => fc.Code).ToList();
            var unrestrictedFunds = (await GetFinancialAidFundsAsync()).Where(fa => (unrestrictedFundCategories.Contains(fa.CategoryCode)) || (fa.CategoryCode == string.Empty) || (fa.CategoryCode == null)).Select(fa => fa.Code).ToList();

            if (restricted == true)
            {
                // Running for restricted only so error if its unrestricted.
                if (unrestrictedFunds.Contains(studentFinancialAidAwardDomainEntity.AwardFundId))
                {
                    throw new PermissionsException(string.Format("Unrestricted FA award is not permitted for guid {0}", id));
                }
            }
            else
            {
                // Running for unrestricted so error if its restricted.
                if (!unrestrictedFunds.Contains(studentFinancialAidAwardDomainEntity.AwardFundId))
                {
                    throw new PermissionsException(string.Format("Restricted FA award is not permitted for guid {0}", id));
                }
            }

            // Convert the student financial aid award object into DTO.
            return await BuildStudentFinancialAidAwardDtoAsync(studentFinancialAidAwardDomainEntity);
        }

        /// <summary>
        /// Returns the DTO for the specified student financial aid award v11
        /// </summary>
        /// <param name="id">Guid for Student Financial Aid Award</param>
        /// <param name="restricted">True if you are allowed to view restricted awards.</param>
        /// <returns>Student Financial Aid Award DTO</returns>
        public async Task<Dtos.StudentFinancialAidAward2> GetById2Async(string id, bool restricted, bool bypassCache = false)
        {
           
            if (string.IsNullOrEmpty(id))
            {
                IntegrationApiExceptionAddError("Requested Id is missing.", "Missing.Request.ID", id);
                throw IntegrationApiException;
            }

            Domain.Student.Entities.StudentFinancialAidAward studentFinancialAidAwardDomainEntity = null;
            try
            {
                // Get the student financial aid awards domain entity from the repository            
                studentFinancialAidAwardDomainEntity = await _studentFinancialAidAwardRepository.GetByIdAsync(id);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: id);
                throw IntegrationApiException;
            }

            if (studentFinancialAidAwardDomainEntity == null)
            {
                IntegrationApiExceptionAddError("StudentFinancialAidAwardDomainEntity cannot be null.", "Bad.Data", id);
                throw IntegrationApiException;
            }

            // Find a list of all non-securred financial aid funds
            var unrestrictedFundCategories = (await GetFinancialAidFundCategoriesAsync(bypassCache)).Where(fc => fc.restrictedFlag == false).Select(fc => fc.Code).ToList();
            var unrestrictedFunds = (await GetFinancialAidFundsAsync(bypassCache)).Where(fa => (unrestrictedFundCategories.Contains(fa.CategoryCode)) || (fa.CategoryCode == string.Empty) || (fa.CategoryCode == null)).Select(fa => fa.Code).ToList();

            if (restricted == true)
            {
                // Running for restricted only so error if its unrestricted.
                if (unrestrictedFunds.Contains(studentFinancialAidAwardDomainEntity.AwardFundId))
                {
                    IntegrationApiExceptionAddError(string.Format("Unrestricted FA award is not permitted for guid '{0}'.", id), "Access.Denied", id, httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                    throw IntegrationApiException;
                }
            }
            else
            {
                // Running for unrestricted so error if its restricted.
                if (!unrestrictedFunds.Contains(studentFinancialAidAwardDomainEntity.AwardFundId))
                {
                    IntegrationApiExceptionAddError(string.Format("Restricted FA award is not permitted for guid '{0}'.", id), "Access.Denied", id, httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                    throw IntegrationApiException;
                }
            }

            //Collect Student Ids
            var studentId = studentFinancialAidAwardDomainEntity.StudentId;
            studentIdDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { studentId });

            // Convert the student financial aid award object into DTO.
            var studentFinancialAidAwardDto = await BuildStudentFinancialAidAward2DtoAsync(studentFinancialAidAwardDomainEntity);
            
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return studentFinancialAidAwardDto;
        }

        /// <summary>
        /// Returns all student charges for the data model version 7
        /// </summary>
        /// <param name="offset">Offset used for paging</param>
        /// <param name="limit">Limit of items per page</param>
        /// <param name="bypassCache">Flag to read from disk instead of cache</param>
        /// <param name="restricted">True if you are allowed to see restricted awards</param>
        /// <returns>Collection of StudentFinancialAidAwards</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>> GetAsync(int offset, int limit, bool bypassCache, bool restricted)
        {
          
            var studentFinancialAidAwardDtos = new List<Dtos.StudentFinancialAidAward>();
            var aidYearsEntities = await GetFinancialAidYearsAsync(bypassCache);
            var aidYears = aidYearsEntities.Where(ay => ay.status != "D").Select(ay => ay.Code).Distinct().ToList();
            List<string> unrestrictedFunds = new List<string>();

            // Find a list of all non-securred financial aid funds
            var unrestrictedFundCategories = (await GetFinancialAidFundCategoriesAsync(bypassCache)).Where(fc => fc.restrictedFlag == false).Select(fc => fc.Code).ToList();
            unrestrictedFunds = (await GetFinancialAidFundsAsync(bypassCache)).Where(fa => (unrestrictedFundCategories.Contains(fa.CategoryCode)) || (fa.CategoryCode == string.Empty) || (fa.CategoryCode == null)).Select(fa => fa.Code).ToList();

            // Get the student financial aid awards domain entity from the repository
            var studentFinancialAidAwardDomainTuple = await _studentFinancialAidAwardRepository.GetAsync(offset, limit, bypassCache, restricted, unrestrictedFunds, aidYears);
            var studentFinancialAidAwardDomainEntities = studentFinancialAidAwardDomainTuple.Item1;
            var totalRecords = studentFinancialAidAwardDomainTuple.Item2;

            if (studentFinancialAidAwardDomainEntities == null)
            {
                throw new ArgumentNullException("StudentFinancialAidAwardDomainEntity", "StudentFinancialAidAwardDomainEntity cannot be null. ");
            }

            // Convert the student financial aid awards and all its child objects into DTOs.
            foreach (var entity in studentFinancialAidAwardDomainEntities)
            {
                if (entity != null)
                {
                    var FinancialAidAwardDto = await BuildStudentFinancialAidAwardDtoAsync(entity, bypassCache);
                    studentFinancialAidAwardDtos.Add(FinancialAidAwardDto);
                }
            }
            return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward>, int>(studentFinancialAidAwardDtos, totalRecords);
        }

        /// <summary>
        /// Returns all student charges for the data model version 11
        /// </summary>
        /// <param name="offset">Offset used for paging</param>
        /// <param name="limit">Limit of items per page</param>
        /// <param name="bypassCache">Flag to read from disk instead of cache</param>
        /// <param name="restricted">True if you are allowed to see restricted awards</param>
        /// <returns>Collection of StudentFinancialAidAwards</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>> Get2Async(int offset, int limit, Dtos.StudentFinancialAidAward2 criteria, string personFilter = "", bool bypassCache = false, 
            bool restricted = false)
        {
             //CheckViewStudentFinancialAidAwardsPermission2();
            
            Domain.Student.Entities.StudentFinancialAidAward criteriaEntity = null;
            var aidYearsEntities = await GetFinancialAidYearsAsync(bypassCache);
            List<string> aidYears = null;
            aidYears = aidYearsEntities.Where(ay => ay.status != "D").Select(ay => ay.Code).Distinct().ToList();
            var finAidFunds = await GetFinancialAidFundsAsync(bypassCache);

            if (criteria != null)
            {
                string studentId = string.Empty;
                string awardFundId = string.Empty;
                string aidYearId = string.Empty;

                //student
                if (criteria.Student != null && !string.IsNullOrEmpty(criteria.Student.Id))
                {
                    try
                    {
                        studentId = await _personRepository.GetPersonIdFromGuidAsync(criteria.Student.Id);
                        if (string.IsNullOrEmpty(studentId))
                        {
                            return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
                        }
                    }
                    catch
                    {
                        return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
                    }
                }
                //aidYear
                if (criteria.AidYear != null && ! string.IsNullOrEmpty(criteria.AidYear.Id))
                {
                    var aidYear = (aidYearsEntities == null || !aidYearsEntities.Any())? null : aidYearsEntities.FirstOrDefault(y => y.Guid.Equals(criteria.AidYear.Id, StringComparison.OrdinalIgnoreCase));
                    if(aidYear == null)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
                    }
                    aidYearId = aidYear.Code;                    
                }
                //awardFund
                if(criteria.AwardFund != null && ! string.IsNullOrEmpty(criteria.AwardFund.Id))
                {
                    var awardFund = (finAidFunds == null || !finAidFunds.Any()) ? null : finAidFunds.FirstOrDefault(f => f.Guid.Equals(criteria.AwardFund.Id, StringComparison.OrdinalIgnoreCase));
                    if(awardFund == null)
                    {
                        return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
                    }
                    awardFundId = awardFund.Code;
                }
                var ids = string.Concat(studentId, aidYearId, awardFundId);
                if(!string.IsNullOrEmpty(ids))
                {
                    criteriaEntity = new Domain.Student.Entities.StudentFinancialAidAward(studentId, awardFundId, aidYearId);
                }
            }

            //personFilter
            string[] personFilterIds = new List<string>().ToArray();
            if (!string.IsNullOrEmpty(personFilter))
            {
                try
                {
                    var personFilterKeys = (await _referenceDataRepository.GetPersonIdsByPersonFilterGuidAsync(personFilter));
                    if (personFilterKeys != null)
                    {
                        personFilterIds = personFilterKeys;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
                }
            }

            var studentFinancialAidAwardDtos = new List<Dtos.StudentFinancialAidAward2>();
            List<string> unrestrictedFunds = new List<string>();

            // Find a list of all non-securred financial aid funds
            var unrestrictedFundCategories = (await GetFinancialAidFundCategoriesAsync(bypassCache)).Where(fc => fc.restrictedFlag == false).Select(fc => fc.Code).ToList();
            unrestrictedFunds = finAidFunds.Where(fa => (unrestrictedFundCategories.Contains(fa.CategoryCode)) || (fa.CategoryCode == string.Empty) || (fa.CategoryCode == null)).Select(fa => fa.Code).ToList();

            IEnumerable<Domain.Student.Entities.StudentFinancialAidAward> studentFinancialAidAwardDomainEntities = null;
            int totalRecords = 0;
            try
            {
                // Get the student financial aid awards domain entity from the repository
                var studentFinancialAidAwardDomainTuple = await _studentFinancialAidAwardRepository.Get2Async(offset, limit, bypassCache, restricted, unrestrictedFunds, aidYears, criteriaEntity, personFilterIds, personFilter);
                studentFinancialAidAwardDomainEntities = studentFinancialAidAwardDomainTuple.Item1;
                totalRecords = studentFinancialAidAwardDomainTuple.Item2;
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (studentFinancialAidAwardDomainEntities == null || !studentFinancialAidAwardDomainEntities.Any())
            {
                return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(new List<Dtos.StudentFinancialAidAward2>(), 0);
            }

            //Collect Student Ids
            var studentIds = studentFinancialAidAwardDomainEntities.Where(st => !string.IsNullOrEmpty(st.StudentId)).Select(id => id.StudentId).Distinct();
            studentIdDict = await _personRepository.GetPersonGuidsCollectionAsync(studentIds);

            // Convert the student financial aid awards and all its child objects into DTOs.
            foreach (var entity in studentFinancialAidAwardDomainEntities)
            {
                if (entity != null)
                {
                    var FinancialAidAwardDto = await BuildStudentFinancialAidAward2DtoAsync(entity, bypassCache);
                    if (FinancialAidAwardDto != null)
                    {
                        studentFinancialAidAwardDtos.Add(FinancialAidAwardDto);
                    }
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>(studentFinancialAidAwardDtos, totalRecords);
        }

       
        private async Task<Dtos.StudentFinancialAidAward> BuildStudentFinancialAidAwardDtoAsync(Domain.Student.Entities.StudentFinancialAidAward studentFinancialAidAwardEntity, bool bypassCache = true)
        {
            var studentFinancialAidAwardDto = new Dtos.StudentFinancialAidAward();

            studentFinancialAidAwardDto.Student = new GuidObject2((!string.IsNullOrEmpty(studentFinancialAidAwardEntity.StudentId)) ?
                await _personRepository.GetPersonGuidFromIdAsync(studentFinancialAidAwardEntity.StudentId) :
                string.Empty);
            studentFinancialAidAwardDto.Id = studentFinancialAidAwardEntity.Guid;

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AwardFundId))
            {
                var awardFundEntity = (await GetFinancialAidFundsAsync(bypassCache)).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AwardFundId);
                if (awardFundEntity != null && !string.IsNullOrEmpty(awardFundEntity.Guid))
                {
                    studentFinancialAidAwardDto.AwardFund = new GuidObject2(awardFundEntity.Guid);
                }
            }

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AidYearId))
            {
                var aidYearEntity = (await GetFinancialAidYearsAsync(bypassCache)).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AidYearId);
                if (aidYearEntity != null && !string.IsNullOrEmpty(aidYearEntity.Guid))
                {
                    studentFinancialAidAwardDto.AidYear = new GuidObject2(aidYearEntity.Guid);
                }
            }

            var originalOffer = await ConvertOriginallyOfferedDate(studentFinancialAidAwardEntity.AwardHistory);
            if (originalOffer != null && originalOffer.HasValue)
            {
                studentFinancialAidAwardDto.OriginallyOfferedOn = originalOffer;
            }
            if (studentFinancialAidAwardEntity.AwardHistory != null && studentFinancialAidAwardEntity.AwardHistory.Any())
            {
                var awardsByPeriod = new List<Dtos.DtoProperties.StudentAwardsByPeriodDtoProperty>();
                foreach (var awardHistoryEntity in studentFinancialAidAwardEntity.AwardHistory)
                {
                    var awardPeriodEntity = (await GetFinancialAidAwardPeriodsAsync(bypassCache)).FirstOrDefault(t => t.Code == awardHistoryEntity.AwardPeriod);
                    if (awardPeriodEntity != null && !string.IsNullOrEmpty(awardPeriodEntity.Guid))
                    {
                        var studentAwardsByPeriod = new Dtos.DtoProperties.StudentAwardsByPeriodDtoProperty()
                        {
                            AwardPeriod = new GuidObject2(awardPeriodEntity.Guid),
                        };
                        if (awardPeriodEntity.AwardTerms != null && awardPeriodEntity.AwardTerms.Any())
                        {
                            var termGuids = new List<GuidObject2>();
                            foreach (var term in awardPeriodEntity.AwardTerms)
                            {
                                var termsEntity = (await GetTermsAsync()).FirstOrDefault(t => t.Code == term);
                                if (termsEntity != null && !string.IsNullOrEmpty(termsEntity.RecordGuid))
                                {
                                    termGuids.Add(new GuidObject2(termsEntity.RecordGuid));
                                }
                            }
                            studentAwardsByPeriod.AcademicPeriods = termGuids;
                        }
                        var amounts = await ConvertAwardPeriodAmounts(studentFinancialAidAwardEntity.AwardHistory, awardHistoryEntity.AwardPeriod);
                        var status = await ConvertStatusAsync(awardHistoryEntity.Status, awardHistoryEntity.StatusDate, bypassCache);

                        if (amounts != null && (amounts.Accepted != null ||
                            amounts.Declined != null ||
                            amounts.Offered != null ||
                            amounts.OriginalOffered != null ||
                            amounts.Rejected != null)) studentAwardsByPeriod.Amounts = amounts;
                        if (status != null) studentAwardsByPeriod.Status = status;

                        awardsByPeriod.Add(studentAwardsByPeriod);
                    }
                }
                if (awardsByPeriod != null && awardsByPeriod.Any())
                {
                    studentFinancialAidAwardDto.AwardsByPeriod = awardsByPeriod;
                }
            }
            return studentFinancialAidAwardDto;
        }

        private async Task<Dtos.StudentFinancialAidAward2> BuildStudentFinancialAidAward2DtoAsync(Domain.Student.Entities.StudentFinancialAidAward studentFinancialAidAwardEntity, bool bypassCache = true)
        {
            var studentFinancialAidAwardDto = new Dtos.StudentFinancialAidAward2();

            studentFinancialAidAwardDto.Id = studentFinancialAidAwardEntity.Guid;

            var studentGuid = string.Empty;
            if(studentIdDict != null && studentIdDict.TryGetValue(studentFinancialAidAwardEntity.StudentId, out studentGuid))
            {
                studentFinancialAidAwardDto.Student = new GuidObject2(studentGuid);
            }

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AwardFundId))
            {
                try
                {
                    var awardFundEntity = (await GetFinancialAidFundsAsync(bypassCache)).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AwardFundId);
                    if (awardFundEntity != null && !string.IsNullOrEmpty(awardFundEntity.Guid))
                    {
                        studentFinancialAidAwardDto.AwardFund = new GuidObject2(awardFundEntity.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("The financial-aid-funds guid was not found for award fund '{0}'.", studentFinancialAidAwardEntity.AwardFundId),
                            "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("The financial-aid-funds guid was not found for award fund '{0}'.", studentFinancialAidAwardEntity.AwardFundId),
                        "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                }
            }

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AidYearId))
            {
                try
                {
                    var aidYearEntity = (await GetFinancialAidYearsAsync(bypassCache)).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AidYearId);
                    if (aidYearEntity != null && !string.IsNullOrEmpty(aidYearEntity.Guid))
                    {
                        studentFinancialAidAwardDto.AidYear = new GuidObject2(aidYearEntity.Guid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("The financial-aid-years guid was not found for aid year '{0}'.", studentFinancialAidAwardEntity.AidYearId),
                        "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                    }
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("The financial-aid-years guid was not found for aid year '{0}'.", studentFinancialAidAwardEntity.AidYearId),
                        "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                }
            }

            try
            {
                var originalOffer = await ConvertOriginallyOfferedDate(studentFinancialAidAwardEntity.AwardHistory);
                if (originalOffer != null && originalOffer.HasValue)
                {
                    studentFinancialAidAwardDto.OriginallyOfferedOn = originalOffer;
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
            }

            if (studentFinancialAidAwardEntity.AwardHistory != null && studentFinancialAidAwardEntity.AwardHistory.Any())
            {
                var awardsByPeriod = new List<Dtos.DtoProperties.StudentAwardsByPeriod2DtoProperty>();
                foreach (var awardHistoryEntity in studentFinancialAidAwardEntity.AwardHistory)
                {
                    Domain.Student.Entities.FinancialAidAwardPeriod awardPeriodEntity = null;
                    try
                    {
                        awardPeriodEntity = (await GetFinancialAidAwardPeriodsAsync(bypassCache)).FirstOrDefault(t => t.Code == awardHistoryEntity.AwardPeriod);
                        if (awardPeriodEntity == null || string.IsNullOrEmpty(awardPeriodEntity.Guid))
                        {
                            IntegrationApiExceptionAddError(string.Format("The financial-aid-award-periods guid was not found for award period '{0}'.", awardHistoryEntity.AwardPeriod),
                            "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Format("The financial-aid-award-periods guid was not found for award period '{0}'.", awardHistoryEntity.AwardPeriod),
                            "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                    }

                    if (awardPeriodEntity != null && !string.IsNullOrEmpty(awardPeriodEntity.Guid))
                    {
                        var studentAwardsByPeriod = new Dtos.DtoProperties.StudentAwardsByPeriod2DtoProperty()
                        {
                            AwardPeriod = new GuidObject2(awardPeriodEntity.Guid),
                        };
                        if (awardPeriodEntity.AwardTerms != null && awardPeriodEntity.AwardTerms.Any())
                        {
                            var termGuids = new List<GuidObject2>();
                            foreach (var term in awardPeriodEntity.AwardTerms)
                            {
                                Domain.Student.Entities.Term termsEntity = null;
                                try
                                {
                                    termsEntity = (await GetTermsAsync(bypassCache)).FirstOrDefault(t => t.Code == term);
                                    if (termsEntity == null || string.IsNullOrEmpty(termsEntity.RecordGuid))
                                    {
                                        IntegrationApiExceptionAddError(string.Format("The academic-periods guid was not found for award period '{0}', academic period '{1}'.", awardHistoryEntity.AwardPeriod, term),
                                            "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                                    }
                                }
                                catch (Exception)
                                {
                                    IntegrationApiExceptionAddError(string.Format("The academic-periods guid was not found for award period '{0}', academic period '{1}'.", awardHistoryEntity.AwardPeriod, term),
                                        "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                                }
                                if (termsEntity != null && !string.IsNullOrEmpty(termsEntity.RecordGuid))
                                {
                                    termGuids.Add(new GuidObject2(termsEntity.RecordGuid));
                                }
                            }
                            studentAwardsByPeriod.AcademicPeriods = termGuids;
                        }

                        Dtos.DtoProperties.StudentAwardAmount2DtoProperty amounts = null;
                        try
                        {
                            amounts = await ConvertAwardPeriodAmounts2(studentFinancialAidAwardEntity.AwardHistory, awardHistoryEntity.AwardPeriod, bypassCache);
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot Convert award history amounts for award period '{0}'.", awardHistoryEntity.AwardPeriod),
                                "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                            IntegrationApiExceptionAddError(ex.Message,
                                "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                        }

                        Dtos.DtoProperties.StudentAwardStatusDtoProperty status = null;
                        try
                        {
                            status = await ConvertStatusAsync(awardHistoryEntity.Status, awardHistoryEntity.StatusDate, bypassCache);
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("Cannot Convert award history status code '{0}' and date '{1}'", awardHistoryEntity.Status, awardHistoryEntity.StatusDate),
                                "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                            IntegrationApiExceptionAddError(ex.Message,
                                "Bad.Data", studentFinancialAidAwardEntity.Guid, studentFinancialAidAwardEntity.StudentId);
                        }

                        if (amounts != null && (amounts.Accepted != null ||
                            amounts.Declined != null ||
                            amounts.Offered != null ||
                            amounts.OriginalOffered != null ||
                            amounts.Rejected != null)) studentAwardsByPeriod.Amounts = amounts;
                        if (status != null) studentAwardsByPeriod.Status = status;

                        awardsByPeriod.Add(studentAwardsByPeriod);
                    }
                }
                if (awardsByPeriod != null && awardsByPeriod.Any())
                {
                    studentFinancialAidAwardDto.AwardsByPeriod = awardsByPeriod;
                }
            }
            return studentFinancialAidAwardDto;
        }

        private async Task<Dtos.DtoProperties.StudentAwardAmountDtoProperty> ConvertAwardPeriodAmounts(IEnumerable<Domain.Student.Entities.StudentAwardHistoryByPeriod> awardHistories, string awardPeriod, bool bypassCache = false)
        {
            string hostCountry = await _studentReferenceDataRepository.GetHostCountryAsync();
            var studentAwardAmounts = new Dtos.DtoProperties.StudentAwardAmountDtoProperty();
            foreach (var awardHistory in awardHistories)
            {
                var awardStatus = (await GetAwardStatusesAsync(bypassCache)).FirstOrDefault(a => a.Code == awardHistory.Status);
                if (awardStatus != null && awardHistory.AwardPeriod == awardPeriod)
                    switch (awardStatus.Category)
                    {
                        case Domain.Student.Entities.AwardStatusCategory.Accepted:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var acceptedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Accepted = acceptedAmount;

                                var offeredAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Offered = offeredAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Denied:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var declinedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Declined = declinedAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Estimated:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var offeredAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Offered = offeredAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Pending:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var offeredAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Offered = offeredAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Rejected:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var rejectedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Rejected = rejectedAmount;
                            }
                            break;
                    }
            }
            // Get Originally Offered Amount
            var offerAmount = await ConvertOriginallyOfferedAmount(awardHistories, awardPeriod);
            if (offerAmount > 0)
            {
                var originalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Value = offerAmount,
                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                };
                studentAwardAmounts.OriginalOffered = originalAmount;
            }

            return studentAwardAmounts;
        }

        private async Task<Dtos.DtoProperties.StudentAwardAmount2DtoProperty> ConvertAwardPeriodAmounts2(IEnumerable<Domain.Student.Entities.StudentAwardHistoryByPeriod> awardHistories, string awardPeriod, bool bypassCache = false)
        {
            string hostCountry = await _studentReferenceDataRepository.GetHostCountryAsync();
            var studentAwardAmounts = new Dtos.DtoProperties.StudentAwardAmount2DtoProperty();
            foreach (var awardHistory in awardHistories)
            {
                var awardStatus = (await GetAwardStatusesAsync(bypassCache)).FirstOrDefault(a => a.Code == awardHistory.Status);
                if (awardStatus != null && awardHistory.AwardPeriod == awardPeriod)
                {
                    switch (awardStatus.Category)
                    {
                        case Domain.Student.Entities.AwardStatusCategory.Accepted:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var acceptedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Accepted = acceptedAmount;

                                var offeredAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Offered = offeredAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Denied:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var declinedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Declined = declinedAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Estimated:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var offeredAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Offered = offeredAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Pending:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var offeredAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Offered = offeredAmount;
                            }
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Rejected:
                            if (awardHistory.Amount != null && awardHistory.Amount > 0)
                            {
                                var rejectedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                                {
                                    Value = awardHistory.Amount,
                                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                                };
                                studentAwardAmounts.Rejected = rejectedAmount;
                            }
                            break;
                    }
                    if (awardHistory.XmitAmount >= 0)
                    {
                        var disbursedAmount = new Dtos.DtoProperties.AmountDtoProperty()
                        {
                            Value = awardHistory.XmitAmount,
                            Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                        };
                        studentAwardAmounts.Disbursed = disbursedAmount;
                    }
                }
            }
            // Get Originally Offered Amount
            var offerAmount = await ConvertOriginallyOfferedAmount(awardHistories, awardPeriod);
            if (offerAmount > 0)
            {
                var originalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Value = offerAmount,
                    Currency = (hostCountry == "USA" ? Dtos.EnumProperties.CurrencyCodes.USD : Dtos.EnumProperties.CurrencyCodes.CAD)
                };
                studentAwardAmounts.OriginalOffered = originalAmount;
            }

            return studentAwardAmounts;
        }

        private async Task<Dtos.DtoProperties.StudentAwardStatusDtoProperty> ConvertStatusAsync(string statusCode, DateTime? statusDate, bool bypassCache = false)
        {
            var state = Dtos.EnumProperties.StudentAwardStatus.NotSet;
            if (!string.IsNullOrEmpty(statusCode))
            {
                var awardAction = (await GetAwardStatusesAsync(bypassCache)).FirstOrDefault((a => a.Code == statusCode));

                if (awardAction != null)
                {
                    switch (awardAction.Category)
                    {
                        case Domain.Student.Entities.AwardStatusCategory.Estimated:
                            state = Dtos.EnumProperties.StudentAwardStatus.Offered;
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Pending:
                            state = Dtos.EnumProperties.StudentAwardStatus.Offered;
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Accepted:
                            state = Dtos.EnumProperties.StudentAwardStatus.Accepted;
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Denied:
                            state = Dtos.EnumProperties.StudentAwardStatus.Declined;
                            break;
                        case Domain.Student.Entities.AwardStatusCategory.Rejected:
                            state = Dtos.EnumProperties.StudentAwardStatus.Cancelled;
                            break;
                        default:
                            state = Dtos.EnumProperties.StudentAwardStatus.NotSet;
                            break;
                    }
                }
            }

            var status = new Dtos.DtoProperties.StudentAwardStatusDtoProperty();
            status.State = state;
            status.StateOn = statusDate;

            return status;
        }

        private async Task<DateTime?> ConvertOriginallyOfferedDate(IEnumerable<Domain.Student.Entities.StudentAwardHistoryByPeriod> awardHistories)
        {
            // Find the most current status record for this award
            DateTime? statusDate = null;
            if (awardHistories != null && awardHistories.Any())
            {
                var awardHistory = awardHistories.FirstOrDefault();
                if (awardHistory != null && !string.IsNullOrEmpty(awardHistory.Status))
                {
                    var statusCode = awardHistory.Status;
                    if (await ValidateAwardAction(statusCode))
                    {
                        statusDate = awardHistory.StatusDate;
                    }
                    foreach (var history in awardHistories)
                    {
                        // Exclude some award action categories
                        if ((history.StatusDate <= statusDate && (await ValidateAwardAction(history.Status))) || !(await ValidateAwardAction(statusCode)))
                        {
                            statusCode = history.Status;
                            statusDate = history.StatusDate;
                        }
                        // Now look through all status change history records.
                        if (history.StatusChanges != null && history.StatusChanges.Any())
                        {
                            foreach (var changes in history.StatusChanges)
                            {
                                // Exclude some award action categories
                                if ((history.StatusDate <= statusDate && (await ValidateAwardAction(changes.Status))) || !(await ValidateAwardAction(statusCode)))
                                {
                                    statusCode = changes.Status;
                                    statusDate = changes.StatusDate;
                                }
                            }
                        }
                    }
                }
            }
            return statusDate;
        }

        private async Task<Decimal?> ConvertOriginallyOfferedAmount(IEnumerable<Domain.Student.Entities.StudentAwardHistoryByPeriod> awardHistories, string awardPeriod)
        {
            // Find the first/oldest status change record for this award and period.
            Decimal? amount = 0;
            var statusCode = "";
            DateTime? statusDate = null;
            if (awardHistories != null && awardHistories.Any())
            {
                foreach (var history in awardHistories)
                {
                    if (history.AwardPeriod == awardPeriod)
                    {
                        // Now look through all status change history records.
                        if (history.StatusChanges != null && history.StatusChanges.Any())
                        {
                            foreach (var changes in history.StatusChanges)
                            {
                                // Exclude some award action categories
                                if (((history.StatusDate <= statusDate || statusDate == null) && (await ValidateAwardAction(changes.Status))) || !(await ValidateAwardAction(statusCode)))
                                {
                                    if (amount == 0)
                                    {
                                        statusCode = changes.Status;
                                        statusDate = changes.StatusDate;
                                        amount = changes.Amount;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return amount;
        }

        /// <summary>
        /// Exclude Award Status Categories that are in the FSP.NOT.AWARDED.CAT entry
        /// of the FA parameters form in Colleague.
        /// </summary>
        /// <param name="awardAction"></param>
        /// <returns>Boolean set to true if the Award Status is valid.</returns>
        private async Task<bool> ValidateAwardAction(string statusCode, bool bypassCache = false)
        {
            bool validAction = true;
            if (!string.IsNullOrEmpty(statusCode))
            {
                var awardAction = (await GetAwardStatusesAsync(bypassCache)).FirstOrDefault(a => a.Code == statusCode);
                var notAwardedCategories = await GetNotAwardedCategoriesAsync();
                if (awardAction != null && notAwardedCategories != null)
                {
                    foreach (var category in notAwardedCategories)
                    {
                        if (awardAction != null)
                        {
                            switch (awardAction.Category)
                            {
                                case Domain.Student.Entities.AwardStatusCategory.Estimated:
                                    if (category == "E") validAction = false;
                                    break;
                                case Domain.Student.Entities.AwardStatusCategory.Pending:
                                    if (category == "P") validAction = false;
                                    break;
                                case Domain.Student.Entities.AwardStatusCategory.Accepted:
                                    if (category == "A") validAction = false;
                                    break;
                                case Domain.Student.Entities.AwardStatusCategory.Denied:
                                    if (category == "D") validAction = false;
                                    break;
                                case Domain.Student.Entities.AwardStatusCategory.Rejected:
                                    if (category == "R") validAction = false;
                                    break;
                                default:
                                    validAction = true;
                                    break;
                            }
                        }
                    }
                }
            }

            return validAction;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student FinancialAidAwards.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentFinancialAidAwardsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentFinancialAidAwards);

            // User is not allowed to create or update Student FinancialAidAwards without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view Student FinancialAidAwards.", CurrentUser.UserId));
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Restricted Student FinancialAidAwards.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewRestrictedStudentFinancialAidAwardsPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards);

            // User is not allowed to create or update restricted Student FinancialAidAwards without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view Restricted Student FinancialAidAwards.", CurrentUser.UserId));
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student FinancialAidAwards.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentFinancialAidAwardsPermission2()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentFinancialAidAwards);

            // User is not allowed to create or update Student FinancialAidAwards without the appropriate permissions
            if (!hasPermission)
            {
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view student-financial-aid-awards.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Restricted Student FinancialAidAwards.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewRestrictedStudentFinancialAidAwardsPermission2()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards);

            // User is not allowed to create or update restricted Student FinancialAidAwards without the appropriate permissions
            if (!hasPermission)
            {
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view restricted-student-financial-aid-awards.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }
    }
}