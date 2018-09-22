// Copyright 2017-2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
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
        private readonly IFinancialAidFundRepository _financialAidFundRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly ITermRepository _termRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IStudentFinancialAidOfficeRepository _studentFinancialAidOfficeRepository;


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
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync()
        {
            if (_financialAidAwardPeriods == null)
            {
                _financialAidAwardPeriods = await _studentReferenceDataRepository.GetFinancialAidAwardPeriodsAsync(false);
            }
            return _financialAidAwardPeriods;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidFund> _financialAidFunds = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidFund>> GetFinancialAidFundsAsync()
        {
            if (_financialAidFunds == null)
            {
                _financialAidFunds = await _financialAidFundRepository.GetFinancialAidFundsAsync(false);
            }
            return _financialAidFunds;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidYear> _financialAidYears = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidYear>> GetFinancialAidYearsAsync()
        {
            if (_financialAidYears == null)
            {
                _financialAidYears = await _studentReferenceDataRepository.GetFinancialAidYearsAsync(false);
            }
            return _financialAidYears;
        }

        private IEnumerable<Domain.Student.Entities.FinancialAidFundCategory> _financialAidFundCategories = null;
        private async Task<IEnumerable<Domain.Student.Entities.FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync()
        {
            if (_financialAidFundCategories == null)
            {
                _financialAidFundCategories = await _studentReferenceDataRepository.GetFinancialAidFundCategoriesAsync(false);
            }
            return _financialAidFundCategories;
        }

        private IEnumerable<Domain.Student.Entities.Term> _terms = null;
        private async Task<IEnumerable<Domain.Student.Entities.Term>> GetTermsAsync()
        {
            if (_terms == null)
            {
                _terms = await _termRepository.GetAsync();
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
            CheckViewStudentFinancialAidAwardsPermission();
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
                // Running for retricted only so error if its unrestricted.
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
        public async Task<Dtos.StudentFinancialAidAward2> GetById2Async(string id, bool restricted)
        {
            if (restricted == false)
            {
                CheckViewStudentFinancialAidAwardsPermission();
            }
            if (restricted == true)
            {
                CheckViewRestrictedStudentFinancialAidAwardsPermission();
            }

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
                // Running for retricted only so error if its unrestricted.
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
            return await BuildStudentFinancialAidAward2DtoAsync(studentFinancialAidAwardDomainEntity);
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
            CheckViewStudentFinancialAidAwardsPermission();

            var studentFinancialAidAwardDtos = new List<Dtos.StudentFinancialAidAward>();
            var aidYearsEntities = await GetFinancialAidYearsAsync();
            var aidYears = aidYearsEntities.Where(ay => ay.status != "D").Select(ay => ay.Code).Distinct().ToList();
            List<string> unrestrictedFunds = new List<string>();

            // Find a list of all non-securred financial aid funds
            var unrestrictedFundCategories = (await GetFinancialAidFundCategoriesAsync()).Where(fc => fc.restrictedFlag == false).Select(fc => fc.Code).ToList();
            unrestrictedFunds = (await GetFinancialAidFundsAsync()).Where(fa => (unrestrictedFundCategories.Contains(fa.CategoryCode)) || (fa.CategoryCode == string.Empty) || (fa.CategoryCode == null)).Select(fa => fa.Code).ToList();

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
        public async Task<Tuple<IEnumerable<Dtos.StudentFinancialAidAward2>, int>> Get2Async(int offset, int limit, bool bypassCache, bool restricted)
        {
            if (restricted == false)
            {
                CheckViewStudentFinancialAidAwardsPermission();
            }
            if (restricted == true)
            {
                CheckViewRestrictedStudentFinancialAidAwardsPermission();
            }

            var studentFinancialAidAwardDtos = new List<Dtos.StudentFinancialAidAward2>();
            var aidYearsEntities = await GetFinancialAidYearsAsync();
            var aidYears = aidYearsEntities.Where(ay => ay.status != "D").Select(ay => ay.Code).Distinct().ToList();
            List<string> unrestrictedFunds = new List<string>();

            // Find a list of all non-securred financial aid funds
            var unrestrictedFundCategories = (await GetFinancialAidFundCategoriesAsync()).Where(fc => fc.restrictedFlag == false).Select(fc => fc.Code).ToList();
            unrestrictedFunds = (await GetFinancialAidFundsAsync()).Where(fa => (unrestrictedFundCategories.Contains(fa.CategoryCode)) || (fa.CategoryCode == string.Empty) || (fa.CategoryCode == null)).Select(fa => fa.Code).ToList();

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
                    var FinancialAidAwardDto = await BuildStudentFinancialAidAward2DtoAsync(entity, bypassCache);
                    studentFinancialAidAwardDtos.Add(FinancialAidAwardDto);
                }
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
                var awardFundEntity = (await GetFinancialAidFundsAsync()).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AwardFundId);
                if (awardFundEntity != null && !string.IsNullOrEmpty(awardFundEntity.Guid))
                {
                    studentFinancialAidAwardDto.AwardFund = new GuidObject2(awardFundEntity.Guid);
                }
            }

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AidYearId))
            {
                var aidYearEntity = (await GetFinancialAidYearsAsync()).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AidYearId);
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
                    var awardPeriodEntity = (await GetFinancialAidAwardPeriodsAsync()).FirstOrDefault(t => t.Code == awardHistoryEntity.AwardPeriod);
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

            studentFinancialAidAwardDto.Student = new GuidObject2((!string.IsNullOrEmpty(studentFinancialAidAwardEntity.StudentId)) ?
                await _personRepository.GetPersonGuidFromIdAsync(studentFinancialAidAwardEntity.StudentId) :
                string.Empty);
            studentFinancialAidAwardDto.Id = studentFinancialAidAwardEntity.Guid;

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AwardFundId))
            {
                var awardFundEntity = (await GetFinancialAidFundsAsync()).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AwardFundId);
                if (awardFundEntity != null && !string.IsNullOrEmpty(awardFundEntity.Guid))
                {
                    studentFinancialAidAwardDto.AwardFund = new GuidObject2(awardFundEntity.Guid);
                }
            }

            if (!string.IsNullOrEmpty(studentFinancialAidAwardEntity.AidYearId))
            {
                var aidYearEntity = (await GetFinancialAidYearsAsync()).FirstOrDefault(t => t.Code == studentFinancialAidAwardEntity.AidYearId);
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
                var awardsByPeriod = new List<Dtos.DtoProperties.StudentAwardsByPeriod2DtoProperty>();
                foreach (var awardHistoryEntity in studentFinancialAidAwardEntity.AwardHistory)
                {
                    var awardPeriodEntity = (await GetFinancialAidAwardPeriodsAsync()).FirstOrDefault(t => t.Code == awardHistoryEntity.AwardPeriod);
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
                                var termsEntity = (await GetTermsAsync()).FirstOrDefault(t => t.Code == term);
                                if (termsEntity != null && !string.IsNullOrEmpty(termsEntity.RecordGuid))
                                {
                                    termGuids.Add(new GuidObject2(termsEntity.RecordGuid));
                                }
                            }
                            studentAwardsByPeriod.AcademicPeriods = termGuids;
                        }
                        var amounts = await ConvertAwardPeriodAmounts2(studentFinancialAidAwardEntity.AwardHistory, awardHistoryEntity.AwardPeriod, bypassCache);

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
    }
}