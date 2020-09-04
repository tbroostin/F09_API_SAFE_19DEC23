//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
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
    [RegisterType]
    public class StudentFinancialAidNeedSummaryService : BaseCoordinationService, IStudentFinancialAidNeedSummaryService
    {

        private readonly IStudentFinancialAidNeedSummaryRepository _studentFinancialAidNeedSummaryRespository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public StudentFinancialAidNeedSummaryService(

            IStudentFinancialAidNeedSummaryRepository StudentFinancialAidNeedSummaryRespository,
            IPersonRepository personRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,

            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            _studentFinancialAidNeedSummaryRespository = StudentFinancialAidNeedSummaryRespository;
            _personRepository = personRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Gets all financial-aid-applications
        /// </summary>
        /// <returns>Collection of FinancialAidApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>, int>> GetAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewStudentFinancialAidNeedSummariesPermission();

            // Get all financial aid years
            var aidYearEntity = (await _studentReferenceDataRepository.GetFinancialAidYearsAsync(bypassCache));
            if (aidYearEntity == null)
            {
                IntegrationApiExceptionAddError("Unable to retrieve any financial aid years.", "Bad.Data");
                throw IntegrationApiException;
            }

            List<string> faSuiteYears = aidYearEntity.Select(k => k.Code).ToList();

            var studentFinancialAidNeedSummaryDtos = new List<Dtos.StudentFinancialAidNeedSummary>();

            Tuple<IEnumerable<StudentNeedSummary>, int> studentNeedSummaryDomainTuple = null;
            try
            {
               studentNeedSummaryDomainTuple = await _studentFinancialAidNeedSummaryRespository.GetAsync(offset, limit, bypassCache, faSuiteYears);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            var studentFinancialAidNeedSummaryDomainEntities = studentNeedSummaryDomainTuple.Item1;
            var totalRecords = studentNeedSummaryDomainTuple.Item2;
            if (studentFinancialAidNeedSummaryDomainEntities == null || totalRecords == 0)
            {
                return new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, 0);
            }
            

            var personIds = studentFinancialAidNeedSummaryDomainEntities
                    .Where(x => (!string.IsNullOrEmpty(x.StudentId)))
                    .Select(x => x.StudentId).Distinct().ToList();
            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);
           

            var isirIdsFederal = studentFinancialAidNeedSummaryDomainEntities
                    .Where(x => (!string.IsNullOrEmpty(x.CsFederalIsirId)))
                    .Select(x => x.CsFederalIsirId).Distinct().ToList();

            var isirIdsInst = studentFinancialAidNeedSummaryDomainEntities
                    .Where(x => (!string.IsNullOrEmpty(x.CsInstitutionalIsirId)))
                    .Select(x => x.CsInstitutionalIsirId).Distinct().ToList();

            var isirIds = isirIdsFederal.Union(isirIdsInst);


            var csFederalIsirCollection = await _studentFinancialAidNeedSummaryRespository.GetIsirCalcResultsGuidsCollectionAsync(isirIds);


            // Convert the student financial aid need sumary and all its child objects into DTOs.
            foreach (var entity in studentFinancialAidNeedSummaryDomainEntities)
            {
                if (entity != null)
                {
                    var studentFinancialAidNeedSummaryDto = await BuildStudentFinancialAidNeedSummaryDtoAsync(entity, personGuidCollection, csFederalIsirCollection, bypassCache);
                    studentFinancialAidNeedSummaryDtos.Add(studentFinancialAidNeedSummaryDto);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int>(studentFinancialAidNeedSummaryDtos, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 9</remarks>
        /// <summary>
        /// Get a StudentFinancialAidNeedSummaries from its GUID
        /// </summary>
        /// <returns>FinancialAidApplications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary> GetByIdAsync(string id)
        {

            CheckViewStudentFinancialAidNeedSummariesPermission();

            StudentNeedSummary studentNeedSummaryDomainEntity = null;

            try
            {
                //// Get the student financial aid awards domain entity from the repository
                studentNeedSummaryDomainEntity = await _studentFinancialAidNeedSummaryRespository.GetByIdAsync(id);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: id);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }


            var personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(new List<string> { studentNeedSummaryDomainEntity.StudentId });

            var csFederalIsirCollection = await _studentFinancialAidNeedSummaryRespository.GetIsirCalcResultsGuidsCollectionAsync(
                    new List<string> { studentNeedSummaryDomainEntity.CsFederalIsirId, studentNeedSummaryDomainEntity.CsInstitutionalIsirId }
                    );

            //// Convert the financial aid application object into DTO.
            var retVal = await BuildStudentFinancialAidNeedSummaryDtoAsync(studentNeedSummaryDomainEntity, personGuidCollection, csFederalIsirCollection);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return retVal;
        }

        private async Task<Dtos.StudentFinancialAidNeedSummary> BuildStudentFinancialAidNeedSummaryDtoAsync(StudentNeedSummary studentNeedSummaryEntity,
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> isirCollection, bool bypassCache = true)
        {
            var studentFinancialAidNeedSummaryDto = new Dtos.StudentFinancialAidNeedSummary();
            var needsByMethodology = new List<Dtos.DtoProperties.StudentFinancialAidNeedsByMethodologyDtoProperty>();


            try
            {
                studentFinancialAidNeedSummaryDto.Id = studentNeedSummaryEntity.Guid;

                #region applicant

                if (string.IsNullOrEmpty(studentNeedSummaryEntity.StudentId))
                {
                    IntegrationApiExceptionAddError("Applicant is required.", "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);

                }
                else
                {
                    if (personGuidCollection == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate GUID for applicant ID (PERSON): {0}.", studentNeedSummaryEntity.StudentId), "Bad.Data", studentNeedSummaryEntity.Guid,
                            studentNeedSummaryEntity.StudentId);

                    }
                    else
                    {
                        var personGuid = string.Empty;
                        personGuidCollection.TryGetValue(studentNeedSummaryEntity.StudentId, out personGuid);
                        if (string.IsNullOrEmpty(personGuid))
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for applicant ID (PERSON): {0}.", studentNeedSummaryEntity.StudentId), "Bad.Data", studentNeedSummaryEntity.Guid, 
                                studentNeedSummaryEntity.StudentId);
                        }
                        else
                        {
                            studentFinancialAidNeedSummaryDto.Applicant = new GuidObject2(personGuid);
                        }
                    }
                }

                #endregion

                #region aidYear

                if (string.IsNullOrEmpty(studentNeedSummaryEntity.AwardYear))
                     IntegrationApiExceptionAddError("AidYear is required.", "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
                else
                {
                    try
                    {
                        var awardYearGuid = await _studentReferenceDataRepository.GetFinancialAidYearsGuidAsync(studentNeedSummaryEntity.AwardYear);
                        if (!string.IsNullOrEmpty(awardYearGuid))
                        {
                            studentFinancialAidNeedSummaryDto.AidYear = new GuidObject2(awardYearGuid);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
                    }
                }

                #endregion

                #region needsByMethodology

                if (studentNeedSummaryEntity.FederalNeedAmount.HasValue)
                {

                    var federalNeed = new StudentFinancialAidNeedsByMethodologyDtoProperty();
                    federalNeed.Methodology = Dtos.EnumProperties.StudentFinancialAidNeedMethodology.Federal;
                   
                    if (string.IsNullOrEmpty(studentNeedSummaryEntity.CsFederalIsirId))
                    {
                        IntegrationApiExceptionAddError("CsFederalIsirId is required.", "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);

                    }
                    else
                    {
                        if (isirCollection == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for CsFederalIsirId ID : {0}.", studentNeedSummaryEntity.CsFederalIsirId), "Bad.Data",
                                studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
                        }
                        else
                        {
                            var outcomeId = string.Empty;
                            isirCollection.TryGetValue(studentNeedSummaryEntity.CsFederalIsirId, out outcomeId);
                            if (string.IsNullOrEmpty(outcomeId))
                            {
                                var message = "Institutional need in CS.INST.NEED exists without valid CS.FED.ISIR.ID pointer to ISIR.CALC.RESULTS for ID " + studentNeedSummaryEntity.Guid;

                                IntegrationApiExceptionAddError(message, "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);

                                logger.Error(message);
                            }
                            else
                            {
                                federalNeed.ApplicationOutcome = new GuidObject2(outcomeId);
                                federalNeed.BudgetDuration = studentNeedSummaryEntity.BudgetDuration;

                                var totalCostOfAttendance = new Dtos.DtoProperties.AmountDtoProperty();
                                if (studentNeedSummaryEntity.FederalTotalExpenses.HasValue)
                                {
                                    if (studentNeedSummaryEntity.FederalTotalExpenses >= 0)
                                    {
                                        totalCostOfAttendance.Value = studentNeedSummaryEntity.FederalTotalExpenses;
                                        totalCostOfAttendance.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                        federalNeed.TotalCostOfAttendance = totalCostOfAttendance;
                                    }
                                }

                                var familyContribution = new Dtos.DtoProperties.AmountDtoProperty();
                                if (studentNeedSummaryEntity.FederalFamilyContribution.HasValue)
                                {
                                    if (studentNeedSummaryEntity.FederalFamilyContribution >= 0)
                                    {
                                        familyContribution.Value = studentNeedSummaryEntity.FederalFamilyContribution;
                                        familyContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                        federalNeed.ExpectedFamilyContribution = familyContribution;
                                    }
                                }

                                var grossNeed = new Dtos.DtoProperties.AmountDtoProperty();
                                grossNeed.Value = studentNeedSummaryEntity.FederalNeedAmount;
                                grossNeed.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                federalNeed.GrossNeed = grossNeed;

                                if (studentNeedSummaryEntity.HasAward == true)
                                {
                                    var totalNeedReduction = new Dtos.DtoProperties.AmountDtoProperty();
                                    if (studentNeedSummaryEntity.FederalTotalNeedReduction.HasValue)
                                    {
                                        if (studentNeedSummaryEntity.FederalTotalNeedReduction >= 0)
                                        {
                                            totalNeedReduction.Value = studentNeedSummaryEntity.FederalTotalNeedReduction;
                                            totalNeedReduction.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                            federalNeed.TotalNeedReduction = totalNeedReduction;
                                        }
                                    }
                                }

                                needsByMethodology.Add(federalNeed);
                            }

                        }
                    }
                }

                #endregion

                #region  NeedByMethodology for institutional

                if (studentNeedSummaryEntity.InstitutionalNeedAmount.HasValue)
                {
                    var institutionalNeed = new StudentFinancialAidNeedsByMethodologyDtoProperty();
                    institutionalNeed.Methodology = Dtos.EnumProperties.StudentFinancialAidNeedMethodology.Institutional;
                     
                    if (string.IsNullOrEmpty(studentNeedSummaryEntity.CsInstitutionalIsirId))
                    {
                        IntegrationApiExceptionAddError("CsInstitutionalIsirId is required.", "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
                    }
                    else
                    {
                        if (isirCollection == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to locate guid for CsInstitutionalIsirId ID : {0}.", studentNeedSummaryEntity.CsInstitutionalIsirId), 
                                "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
                        }
                        else
                        {
                            var outcomeId = string.Empty;
                            isirCollection.TryGetValue(studentNeedSummaryEntity.CsInstitutionalIsirId, out outcomeId);
                            if (string.IsNullOrEmpty(outcomeId))
                            {

                                var message = "Federal need in CS.NEED exists without valid CS.INST.ISIR.ID pointer to ISIR.CALC.RESULTS for ID " + studentNeedSummaryEntity.Guid;

                                IntegrationApiExceptionAddError(message, "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);

                                logger.Error(message);
                            }
                            else
                            {

                                institutionalNeed.ApplicationOutcome = new GuidObject2(outcomeId);
                                institutionalNeed.BudgetDuration = studentNeedSummaryEntity.BudgetDuration;

                                var totalCostOfAttendance = new Dtos.DtoProperties.AmountDtoProperty();
                                if (studentNeedSummaryEntity.InstitutionalTotalExpenses.HasValue)
                                {
                                    if (studentNeedSummaryEntity.InstitutionalTotalExpenses >= 0)
                                    {
                                        totalCostOfAttendance.Value = studentNeedSummaryEntity.InstitutionalTotalExpenses;
                                        totalCostOfAttendance.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                        institutionalNeed.TotalCostOfAttendance = totalCostOfAttendance;
                                    }
                                }

                                var familyContribution = new Dtos.DtoProperties.AmountDtoProperty();
                                if (studentNeedSummaryEntity.InstitutionalFamilyContribution.HasValue)
                                {
                                    if (studentNeedSummaryEntity.InstitutionalFamilyContribution >= 0)
                                    {
                                        familyContribution.Value = studentNeedSummaryEntity.InstitutionalFamilyContribution;
                                        familyContribution.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                        institutionalNeed.ExpectedFamilyContribution = familyContribution;
                                    }
                                }

                                var grossNeed = new Dtos.DtoProperties.AmountDtoProperty();
                                grossNeed.Value = studentNeedSummaryEntity.InstitutionalNeedAmount;
                                grossNeed.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                institutionalNeed.GrossNeed = grossNeed;

                                if (studentNeedSummaryEntity.HasAward == true)
                                {
                                    var totalNeedReduction = new Dtos.DtoProperties.AmountDtoProperty();
                                    if (studentNeedSummaryEntity.InstitutionalTotalNeedReduction.HasValue)
                                    {
                                        if (studentNeedSummaryEntity.InstitutionalTotalNeedReduction >= 0)
                                        {
                                            totalNeedReduction.Value = studentNeedSummaryEntity.InstitutionalTotalNeedReduction;
                                            totalNeedReduction.Currency = Dtos.EnumProperties.CurrencyCodes.USD;
                                            institutionalNeed.TotalNeedReduction = totalNeedReduction;
                                        }
                                    }
                                }

                                needsByMethodology.Add(institutionalNeed);
                            }
                        }
                    }
                }

                #endregion

                if (needsByMethodology.Any())
                {
                    studentFinancialAidNeedSummaryDto.NeedsByMethodology = needsByMethodology;
                }
                else
                {
                    var message = "Unable to build needsByMethodology for ID " + studentNeedSummaryEntity.Guid;
                    logger.Error(message);
                    IntegrationApiExceptionAddError(message, "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
            }
            catch
            {
                var message = "Unable to process Student Financial Aid Need Summary for ID " + studentNeedSummaryEntity.Guid + ".  Check API error log.";
                IntegrationApiExceptionAddError(message, "Bad.Data", studentNeedSummaryEntity.Guid, studentNeedSummaryEntity.StudentId);
            }

            return studentFinancialAidNeedSummaryDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Student StudentFinancialAidNeedSummaries.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewStudentFinancialAidNeedSummariesPermission()
        {
            bool hasPermission = HasPermission(StudentPermissionCodes.ViewStudentFinancialAidNeedSummaries);

            // User is not allowed to read StudentFinancialAidNeedSummaries without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view student-financial-aid-need-summaries.", CurrentUser.UserId));
            }
        }
    }
}