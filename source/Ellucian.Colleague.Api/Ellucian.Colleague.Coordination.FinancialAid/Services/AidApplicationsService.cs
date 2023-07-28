/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// AidApplicationsService class coordinates domain entities to interact with Financial Aid Applications. 
    /// </summary>
    [RegisterType]
    public class AidApplicationsService : BaseCoordinationService, IAidApplicationsService 
    {
        private readonly IAidApplicationsRepository _aidApplicationsRepository;
        private readonly IFinancialAidReferenceDataRepository _financialAidReferenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAidApplicationDemographicsRepository _aidApplicationsDemoRepository;
       
        /// <summary>
        /// Constructor for AidApplicationsService
        /// </summary>
        /// <param name="aidApplicationsRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="logger"></param>
        public AidApplicationsService(
            IAidApplicationsRepository aidApplicationsRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAidApplicationDemographicsRepository aidApplicationsDemoRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _aidApplicationsRepository = aidApplicationsRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            _aidApplicationsDemoRepository = aidApplicationsDemoRepository;
        }

        #region GetAidApplicationsAsync
        /// <summary>
        /// Gets all aid applications matching the criteria
        /// </summary>
        /// <returns>Collection of AidApplications DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.FinancialAid.AidApplications>, int>> GetAidApplicationsAsync(int offset, int limit, Dtos.FinancialAid.AidApplications criteriaFilter)
            {
                  var aidApplicationsDtos = new List<Dtos.FinancialAid.AidApplications>();

                  if (criteriaFilter == null)
                  {
                        criteriaFilter = new Dtos.FinancialAid.AidApplications();
                  }
                  string appDemoId = criteriaFilter.AppDemoID;
                  string personIdCriteria = criteriaFilter.PersonId;
                  string aidApplicationType = criteriaFilter.ApplicationType;
                  string aidYear = criteriaFilter.AidYear;
                  string assignedID = criteriaFilter.ApplicantAssignedId;
                  var aidApplicantionsEntities = await _aidApplicationsRepository.GetAidApplicationsAsync(offset, limit, appDemoId, personIdCriteria, aidApplicationType, aidYear, assignedID);
                  var totalRecords = aidApplicantionsEntities.Item2;
                  if (aidApplicantionsEntities != null && aidApplicantionsEntities.Item1.Any())
                  {
                        foreach (var applications in aidApplicantionsEntities.Item1)
                        {
                              try
                              {
                                    aidApplicationsDtos.Add(ConvertAidApplicationsToDto(applications, false));
                              }
                              catch (Exception ex)
                              {
                                    IntegrationApiExceptionAddError("Error extracting request." + ex.Message, "Global.Internal.Error", applications.Id);
                              }
                              if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
                              {
                                    throw IntegrationApiException;
                              }

                        }
                  }
                  return new Tuple<IEnumerable<Dtos.FinancialAid.AidApplications>, int>(aidApplicationsDtos, totalRecords);
            }
            #endregion


        #region GetAidApplicationsByIdAsync
            /// <summary>
            /// Get a AidApplications from its Id
            /// </summary>
            /// <returns>AidApplications DTO object</returns>
            public async Task<Dtos.FinancialAid.AidApplications> GetAidApplicationsByIdAsync(string id)
            {
                  if (string.IsNullOrEmpty(id))
                  {
                        throw new ArgumentNullException("id required to get an aid applications");
                  }

                  Domain.FinancialAid.Entities.AidApplications applicationsEntity;
                  try
                  {
                        applicationsEntity = await _aidApplicationsRepository.GetAidApplicationsByIdAsync(id);
                  }
                  catch (RepositoryException ex)
                  {
                        throw ex;
                  }

                  if (applicationsEntity == null)
                  {
                        throw new KeyNotFoundException("Aid applications was not found for id " + id);
                  }

                  Dtos.FinancialAid.AidApplications getResponseAidApplicationsDto = null;
                  try
                  {
                        getResponseAidApplicationsDto = ConvertAidApplicationsToDto(applicationsEntity, true);
                  }
                  catch (Exception ex)
                  {
                        IntegrationApiExceptionAddError("Error extracting request." + ex.Message, "Global.Internal.Error", id);
                  }
                  if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
                  {
                        throw IntegrationApiException;
                  }

                  return getResponseAidApplicationsDto;
            }
            #endregion


        #region ConvertAidApplications Entity To Dto
            /// <summary>
            /// Converts a AidApplications domain entity to its corresponding AidApplications DTO
            /// </summary>
            /// <param name="applicationsEntity">AidApplications domain entity</param>
            /// <param name="isGetByID">is the request Get by ID? pass true if it's Get by ID or pass false if it's Get all</param>
            /// <returns>AidApplications DTO</returns>
            private Dtos.FinancialAid.AidApplications ConvertAidApplicationsToDto(Domain.FinancialAid.Entities.AidApplications aidApplicationsEntity, bool isGetByID)
        {

            var aidApplicationsDTO = new Dtos.FinancialAid.AidApplications();

            // assigning the entity to it's DTO property
            aidApplicationsDTO.Id = aidApplicationsEntity.Id;
            aidApplicationsDTO.AppDemoID = aidApplicationsEntity.AppDemoId;
            aidApplicationsDTO.PersonId = aidApplicationsEntity.PersonId;
            aidApplicationsDTO.ApplicationType = aidApplicationsEntity.AidApplicationType;
            aidApplicationsDTO.AidYear = aidApplicationsEntity.AidYear;
            aidApplicationsDTO.ApplicantAssignedId = string.IsNullOrEmpty(aidApplicationsEntity.AssignedID) ? null : aidApplicationsEntity.AssignedID;

            // Student marital status 
            if (!string.IsNullOrEmpty(aidApplicationsEntity.StudentMaritalStatus) || !string.IsNullOrEmpty(aidApplicationsEntity.StudentMaritalDate) )
            {
                aidApplicationsDTO.StudentMarital = new StudentMaritalInfo();
                aidApplicationsDTO.StudentMarital.Date = !string.IsNullOrEmpty(aidApplicationsEntity.StudentMaritalDate) ? aidApplicationsEntity.StudentMaritalDate : null;
                if (!string.IsNullOrEmpty(aidApplicationsEntity.StudentMaritalStatus))
                {
                    aidApplicationsDTO.StudentMarital.Status = string.IsNullOrEmpty(aidApplicationsEntity.StudentMaritalStatus) ? null : ConvertStudentMaritalToDto(aidApplicationsEntity.StudentMaritalStatus, isGetByID);

                }

            }

            #region Student Legal residence Dto
            if (!string.IsNullOrEmpty(aidApplicationsEntity.StudentLegalResSt) || aidApplicationsEntity.StudentLegalResB4 == true || !string.IsNullOrEmpty(aidApplicationsEntity.StudentLegalResDate))
            {
                aidApplicationsDTO.StudentLegalResidence = new LegalResidence()
                {
                    State = string.IsNullOrEmpty(aidApplicationsEntity.StudentLegalResSt) ? null : aidApplicationsEntity.StudentLegalResSt,
                    ResidentBefore = aidApplicationsEntity.StudentLegalResB4,
                    Date = !string.IsNullOrEmpty(aidApplicationsEntity.StudentLegalResDate) ? aidApplicationsEntity.StudentLegalResDate : null 
                };
            }
            #endregion

            #region Parents Dto
            aidApplicationsDTO.Parents = new ParentsInfo();
            // first parent's DTO
            if (!string.IsNullOrEmpty(aidApplicationsEntity.P1GradeLvl) || aidApplicationsEntity.P1Ssn != null || !string.IsNullOrEmpty(aidApplicationsEntity.P1LastName) || !string.IsNullOrEmpty(aidApplicationsEntity.P1FirstInit) || aidApplicationsEntity.P1Dob != null)
            {
                aidApplicationsDTO.Parents.FirstParent = new ParentDetails();
                aidApplicationsDTO.Parents.FirstParent.EducationalLevel = string.IsNullOrEmpty(aidApplicationsEntity.P1GradeLvl) ? null : ConvertParentEdLevelEntitytoDto(aidApplicationsEntity.P1GradeLvl, isGetByID);
                aidApplicationsDTO.Parents.FirstParent.SsnOrItin = aidApplicationsEntity.P1Ssn;
                aidApplicationsDTO.Parents.FirstParent.LastName = string.IsNullOrEmpty(aidApplicationsEntity.P1LastName) ? null : aidApplicationsEntity.P1LastName;
                aidApplicationsDTO.Parents.FirstParent.FirstInitial = string.IsNullOrEmpty(aidApplicationsEntity.P1FirstInit) ? null : aidApplicationsEntity.P1FirstInit;
                aidApplicationsDTO.Parents.FirstParent.BirthDate = aidApplicationsEntity.P1Dob;

            }
            // second parent's DTO
            if (!string.IsNullOrEmpty(aidApplicationsEntity.P2GradeLvl) || aidApplicationsEntity.P2Ssn != null || !string.IsNullOrEmpty(aidApplicationsEntity.P2LastName) || !string.IsNullOrEmpty(aidApplicationsEntity.P2FirstInit) || aidApplicationsEntity.P2Dob != null)
            {

                aidApplicationsDTO.Parents.SecondParent = new ParentDetails()
                {
                    EducationalLevel = string.IsNullOrEmpty(aidApplicationsEntity.P2GradeLvl) ? null : ConvertParentEdLevelEntitytoDto(aidApplicationsEntity.P2GradeLvl, isGetByID),
                    SsnOrItin = aidApplicationsEntity.P2Ssn,
                    LastName = string.IsNullOrEmpty(aidApplicationsEntity.P2LastName) ? null : aidApplicationsEntity.P2LastName,
                    FirstInitial = string.IsNullOrEmpty(aidApplicationsEntity.P2FirstInit) ? null : aidApplicationsEntity.P2FirstInit,
                    BirthDate = aidApplicationsEntity.P2Dob
                };
            }
            // parents' marital status DTO

            if (!string.IsNullOrEmpty(aidApplicationsEntity.PMaritalStatus) || !string.IsNullOrEmpty(aidApplicationsEntity.PMaritalDate))
            {
                aidApplicationsDTO.Parents.ParentMarital = new ParentMaritalInfo()
                {
                    Status = string.IsNullOrEmpty(aidApplicationsEntity.PMaritalStatus) ? null : ConvertParentMaritalToDto(aidApplicationsEntity.PMaritalStatus, isGetByID),
                    Date = !string.IsNullOrEmpty(aidApplicationsEntity.PMaritalDate) ? aidApplicationsEntity.PMaritalDate : null
                };
            }
            // Parent's legal residence DTO
            if (!string.IsNullOrEmpty(aidApplicationsEntity.PLegalResSt) || aidApplicationsEntity.PLegalResB4 == true || !string.IsNullOrEmpty(aidApplicationsEntity.PLegalResDate))
            {
                aidApplicationsDTO.Parents.ParentLegalResidence = new LegalResidence()
                {
                    State = string.IsNullOrEmpty(aidApplicationsEntity.PLegalResSt) ? null : aidApplicationsEntity.PLegalResSt,
                    ResidentBefore = aidApplicationsEntity.PLegalResB4,
                    Date = !string.IsNullOrEmpty(aidApplicationsEntity.PLegalResDate) ? aidApplicationsEntity.PLegalResDate : null
                };
            }

            // Parent's income DTO
            aidApplicationsDTO.Parents.Income = new ParentsIncome()
            {
                SsiBenefits = aidApplicationsEntity.PSsiBen,
                FoodStamps = aidApplicationsEntity.PFoodStamps,
                LunchBenefits = aidApplicationsEntity.PLunchBen,
                TanfBenefits = aidApplicationsEntity.PTanf,
                WicBenefits = aidApplicationsEntity.PWic,
                TaxReturnFiled = string.IsNullOrEmpty(aidApplicationsEntity.PTaxReturnFiled) ? null : ConvertTaxReturnFiledEntityToDto(aidApplicationsEntity.PTaxReturnFiled, isGetByID),
                TaxFormType = string.IsNullOrEmpty(aidApplicationsEntity.PTaxFormType) ? null : ConvertTaxFormEntityToDto(aidApplicationsEntity.PTaxFormType, isGetByID),
                TaxFilingStatus = string.IsNullOrEmpty(aidApplicationsEntity.PTaxFilingStatus) ? null : ConvertTaxFilingStatusEntityToDto(aidApplicationsEntity.PTaxFilingStatus, isGetByID),
                Schedule1Filed = string.IsNullOrEmpty(aidApplicationsEntity.PSched1) ? null : ConverYesOrNoToDto(aidApplicationsEntity.PSched1, isGetByID, "Parent Schedule1 Filed"),
                DislocatedWorker = string.IsNullOrEmpty(aidApplicationsEntity.PDisWorker) ? null : ConverYesOrNoToDto(aidApplicationsEntity.PDisWorker, isGetByID, "Parent Dislocated Worker"),
                AdjustedGrossIncome = aidApplicationsEntity.PAgi,
                UsTaxPaid = aidApplicationsEntity.PUsTaxPaid,
                FirstParentWorkEarnings = aidApplicationsEntity.P1Income,
                SecondParentworkEarnings = aidApplicationsEntity.P2Income,
                CashSavingsChecking = aidApplicationsEntity.PCash,
                InvestmentNetWorth = aidApplicationsEntity.PInvNetWorth,
                BusinessOrFarmNetWorth = aidApplicationsEntity.PBusNetWorth,
                EducationalCredits = aidApplicationsEntity.PEduCredit,
                ChildSupportPaid = aidApplicationsEntity.PChildSupportPd,
                NeedBasedEmployment = aidApplicationsEntity.PNeedBasedEmp,
                GrantOrScholarshipAid = aidApplicationsEntity.PGrantScholAid,
                CombatPay = aidApplicationsEntity.PCombatPay,
                CoopEarnings = aidApplicationsEntity.PCoOpEarnings,
                PensionPayments = aidApplicationsEntity.PPensionPymts,
                IraPayments = aidApplicationsEntity.PIraPymts,
                ChildSupportReceived = aidApplicationsEntity.PChildSupRcvd,
                TaxExemptInterstIncome = aidApplicationsEntity.PUntxIntInc,
                UntaxedIraAndPensions = aidApplicationsEntity.PUntxIraPen,
                MilitaryOrClergyAllowances = aidApplicationsEntity.PMilClerAllow,
                VeteranNonEdBenefits = aidApplicationsEntity.PVetNonEdBen,
                OtherUntaxedIncome = aidApplicationsEntity.POtherUntxInc
            };

            // check if the parent's income has any property
            var isObjectNotEmpty = aidApplicationsDTO.Parents.Income.GetType()
                                                            .GetProperties()
                                                            .Select(p => p.GetValue(aidApplicationsDTO.Parents.Income))
                                                            .Where(x => x != null || (x is string && (string)x != null))
                                                            .ToList();

            // if parent's income is an empty object, then make it null 
            if (isObjectNotEmpty.Count == 0 && (aidApplicationsDTO.Parents.Income.SsiBenefits != true && aidApplicationsDTO.Parents.Income.FoodStamps != true && aidApplicationsDTO.Parents.Income.LunchBenefits != true && aidApplicationsDTO.Parents.Income.TanfBenefits != true
                && aidApplicationsDTO.Parents.Income.WicBenefits != true))
            {
                aidApplicationsDTO.Parents.Income = null;
            }

            // other parent DTO field
            if (!string.IsNullOrEmpty(aidApplicationsEntity.ParentEmail))
            {
                aidApplicationsDTO.Parents.EmailAddress = string.IsNullOrEmpty(aidApplicationsEntity.ParentEmail) ? null : aidApplicationsEntity.ParentEmail;
            }
            if (aidApplicationsEntity.PNbrFamily != null || aidApplicationsEntity.PNbrFamily != 0)
            {
                aidApplicationsDTO.Parents.NumberInFamily = aidApplicationsEntity.PNbrFamily;
            }
            if (aidApplicationsEntity.PNbrCollege != 0 || aidApplicationsEntity.PNbrCollege != null)
            {
                aidApplicationsDTO.Parents.NumberInCollege = aidApplicationsEntity.PNbrCollege;
            }

            // check if the whole of parent object is empty, if empty then make it null so that it will not be displayed in response
            var isParentNotEmpty = aidApplicationsDTO.Parents.GetType()
                                                            .GetProperties()
                                                            .Select(p => p.GetValue(aidApplicationsDTO.Parents))
                                                            .Where(x => x != null || (x is string && (string)x != ""))
                                                            .ToList();
            if (!isParentNotEmpty.Any())
            {
                aidApplicationsDTO.Parents = null;
            }
            #endregion

            // highschool Dto
            aidApplicationsDTO.HighSchool = new HighSchoolDetails()
            {
                GradType = string.IsNullOrEmpty(aidApplicationsEntity.HsGradType) ? null : ConvertHSGradTypeToDto(aidApplicationsEntity.HsGradType, isGetByID),
                Name = string.IsNullOrEmpty(aidApplicationsEntity.HsName) ? null : aidApplicationsEntity.HsName,
                City = string.IsNullOrEmpty(aidApplicationsEntity.HsCity) ? null : aidApplicationsEntity.HsCity,
                State = string.IsNullOrEmpty(aidApplicationsEntity.HsState) ? null : aidApplicationsEntity.HsState,
                Code = !string.IsNullOrEmpty(aidApplicationsEntity.HsCode) ? aidApplicationsEntity.HsCode : null

            };

            //check if the High School object is empty, if empty make it null
            var isHSNotEmpty = aidApplicationsDTO.HighSchool.GetType()
                                                            .GetProperties()
                                                            .Select(p => p.GetValue(aidApplicationsDTO.HighSchool))
                                                            .Where(x => x != null || (x is string && (string)x != ""))
                                                            .ToList();
            if (!isHSNotEmpty.Any())
            {
                aidApplicationsDTO.HighSchool = null;
            }

            // degree by
            aidApplicationsDTO.DegreeBy = aidApplicationsEntity.DegreeBy;
            // Grad level in college
            aidApplicationsDTO.GradeLevelInCollege = string.IsNullOrEmpty(aidApplicationsEntity.GradeLevelInCollege) ? null : ConvertGradLevelinCollege(aidApplicationsEntity.GradeLevelInCollege, isGetByID);
            // Degree or Certificate
            aidApplicationsDTO.DegreeOrCertificate = string.IsNullOrEmpty(aidApplicationsEntity.DegreeOrCertificate) ? null : ConvertDegreeOrCertToDto(aidApplicationsEntity.DegreeOrCertificate, isGetByID);

            #region student income
            // student income information
            aidApplicationsDTO.StudentsIncome = new StudentIncome()
            {
                TaxReturnFiled = string.IsNullOrEmpty(aidApplicationsEntity.StudentTaxReturnFiled) ? null : ConvertTaxReturnFiledEntityToDto(aidApplicationsEntity.StudentTaxReturnFiled, isGetByID),
                TaxFormType = string.IsNullOrEmpty(aidApplicationsEntity.StudentTaxFormType) ? null : ConvertTaxFormEntityToDto(aidApplicationsEntity.StudentTaxFormType, isGetByID),
                TaxFilingStatus = string.IsNullOrEmpty(aidApplicationsEntity.StudentTaxFilingStatus) ? null : ConvertTaxFilingStatusEntityToDto(aidApplicationsEntity.StudentTaxFilingStatus, isGetByID),
                Schedule1Filed = string.IsNullOrEmpty(aidApplicationsEntity.StudentSched1) ? null : ConverYesOrNoToDto(aidApplicationsEntity.StudentSched1, isGetByID, "Student Schedule1 Filed"),
                AdjustedGrossIncome = aidApplicationsEntity.StudentAgi,
                UsTaxPaid = aidApplicationsEntity.StudentUsTaxPd,
                WorkEarnings = aidApplicationsEntity.SStudentInc,
                SpouseWorkEarnings = aidApplicationsEntity.SpouseInc,
                CashSavingsChecking = aidApplicationsEntity.StudentCash,
                InvestmentNetWorth = aidApplicationsEntity.StudentInvNetWorth,
                BusinessNetWorth = aidApplicationsEntity.StudentBusNetWorth,
                EducationalCredit = aidApplicationsEntity.StudentEduCredit,
                ChildSupportPaid = aidApplicationsEntity.StudentChildSupPaid,
                NeedBasedEmployment = aidApplicationsEntity.StudentNeedBasedEmp,
                GrantAndScholarshipAid = aidApplicationsEntity.StudentGrantScholAid,
                CombatPay = aidApplicationsEntity.StudentCombatPay,
                CoopEarnings = aidApplicationsEntity.StudentCoOpEarnings,
                PensionPayments = aidApplicationsEntity.StudentPensionPayments,
                IraPayments = aidApplicationsEntity.StudentIraPayments,
                ChildSupportReceived = aidApplicationsEntity.StudentChildSupRecv,
                InterestIncome = aidApplicationsEntity.StudentInterestIncome,
                UntaxedIraPension = aidApplicationsEntity.StudentUntxIraPen,
                MilitaryClergyAllowance = aidApplicationsEntity.StudentMilitaryClergyAllow,
                VeteranNonEdBenefits = aidApplicationsEntity.StudentVetNonEdBen,
                OtherUntaxedIncome = aidApplicationsEntity.StudentOtherUntaxedInc,
                OtherNonReportedMoney = aidApplicationsEntity.StudentOtherNonRepMoney,
                MedicaidOrSSIBenefits = aidApplicationsEntity.SSsiBen,
                FoodStamps = aidApplicationsEntity.SFoodStamps,
                LunchBenefits = aidApplicationsEntity.SLunchBen,
                TanfBenefits = aidApplicationsEntity.STanf,
                WicBenefits = aidApplicationsEntity.SWic,
                DislocatedWorker = string.IsNullOrEmpty(aidApplicationsEntity.SDislWorker) ? null : ConverYesOrNoToDto(aidApplicationsEntity.SDislWorker, isGetByID, "Student Dislocated Worker")
            };
            var isSIncomeNotEmpty = aidApplicationsDTO.StudentsIncome.GetType()
                                                                    .GetProperties()
                                                                    .Select(p => p.GetValue(aidApplicationsDTO.StudentsIncome))
                                                                    .Where(x => x != null || (x is string && (string)x != ""))
                                                                    .ToList();
            if (isSIncomeNotEmpty.Count == 0 && aidApplicationsDTO.StudentsIncome.MedicaidOrSSIBenefits != true && aidApplicationsDTO.StudentsIncome.FoodStamps != true && aidApplicationsDTO.StudentsIncome.LunchBenefits != true
                && aidApplicationsDTO.StudentsIncome.TanfBenefits != true && aidApplicationsDTO.StudentsIncome.WicBenefits != true)
            {
                aidApplicationsDTO.StudentsIncome = null;
            }
            #endregion
            // other applicant details
            aidApplicationsDTO.BornBefore = aidApplicationsEntity.BornBefore;
            aidApplicationsDTO.Married = aidApplicationsEntity.Married;
            aidApplicationsDTO.GradOrProfProgram = aidApplicationsEntity.GradOrProfProgram;
            aidApplicationsDTO.ActiveDuty = aidApplicationsEntity.ActiveDuty;
            aidApplicationsDTO.USVeteran = aidApplicationsEntity.UsVeteran;
            aidApplicationsDTO.DependentChildren = aidApplicationsEntity.DependentChildren;
            aidApplicationsDTO.OtherDependents = aidApplicationsEntity.OtherDependents;
            aidApplicationsDTO.OrphanWardFoster = aidApplicationsEntity.OrphanWardFoster;
            aidApplicationsDTO.EmancipatedMinor = aidApplicationsEntity.EmancipatedMinor;
            aidApplicationsDTO.LegalGuardianship = aidApplicationsEntity.LegalGuardianship;
            aidApplicationsDTO.HomelessBySchool = aidApplicationsEntity.HomelessBySchool;
            aidApplicationsDTO.HomelessByHud = aidApplicationsEntity.HomelessByHud;
            aidApplicationsDTO.HomelessAtRisk = aidApplicationsEntity.HomelessAtRisk;
            aidApplicationsDTO.StudentNumberInCollege = aidApplicationsEntity.StudentNumberInCollege;
            aidApplicationsDTO.StudentNumberInFamily = aidApplicationsEntity.StudentNumberInFamily;

            // school codes and housing plan Dto (1 - 10)
            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode1) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan1))
            {
                aidApplicationsDTO.SchoolCode1 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode1) ? null : aidApplicationsEntity.SchoolCode1,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan1) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan1, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode2) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan2))
            {
                aidApplicationsDTO.SchoolCode2 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode2) ? null : aidApplicationsEntity.SchoolCode2,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan2) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan2, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode3) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan3))
            {
                aidApplicationsDTO.SchoolCode3 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode3) ? null : aidApplicationsEntity.SchoolCode3,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan3) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan3, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode4) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan4))
            {
                aidApplicationsDTO.SchoolCode4 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode4) ? null : aidApplicationsEntity.SchoolCode4,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan4) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan4, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode5) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan5))
            {
                aidApplicationsDTO.SchoolCode5 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode5) ? null : aidApplicationsEntity.SchoolCode5,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan5) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan5, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode6) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan6))
            {
                aidApplicationsDTO.SchoolCode6 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode6) ? null : aidApplicationsEntity.SchoolCode6,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan6) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan6, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode7) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan7))
            {
                aidApplicationsDTO.SchoolCode7 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode7) ? null : aidApplicationsEntity.SchoolCode7,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan7) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan7, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode8) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan8))
            {
                aidApplicationsDTO.SchoolCode8 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode8) ? null : aidApplicationsEntity.SchoolCode8,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan8) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan8, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode9) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan9))
            {
                aidApplicationsDTO.SchoolCode9 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode9) ? null : aidApplicationsEntity.SchoolCode9,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan9) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan9, isGetByID)
                };
            }

            if (!string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode10) || !string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan10))
            {
                aidApplicationsDTO.SchoolCode10 = new SchoolCode
                {
                    Code = string.IsNullOrEmpty(aidApplicationsEntity.SchoolCode10) ? null : aidApplicationsEntity.SchoolCode10,
                    HousingPlan = string.IsNullOrEmpty(aidApplicationsEntity.HousingPlan10) ? null : ConvertHousingPlanEntityToDto(aidApplicationsEntity.HousingPlan10, isGetByID)
                };
            }

                  // other application details
                  aidApplicationsDTO.ApplicationCompleteDate = aidApplicationsEntity.ApplicationCompleteDate;
                  aidApplicationsDTO.SignedFlag = string.IsNullOrEmpty(aidApplicationsEntity.SignedFlag) ? null : ConvertSignedFlagtoDto(aidApplicationsEntity.SignedFlag, isGetByID);
                  aidApplicationsDTO.PreparerSsn = aidApplicationsEntity.PreparerSsn;
                  aidApplicationsDTO.PreparerEin = aidApplicationsEntity.PreparerEin;
                  aidApplicationsDTO.PreparerSigned = string.IsNullOrEmpty(aidApplicationsEntity.PreparerSigned) ? null : ConvertPreparerSigned(aidApplicationsEntity.PreparerSigned, isGetByID);

                  return aidApplicationsDTO;

        }
            #endregion


        #region Enum conversion entity to DTO

            private string ConvertPreparerSigned(string preparerSigned, bool getById)
                  {
                  string applicantPreparerSigned = null;
                  switch (preparerSigned)
                        {
                        case "1":
                              applicantPreparerSigned = "Yes";
                              break;
                        }
                  if (getById && applicantPreparerSigned == null && !string.IsNullOrEmpty(preparerSigned))
                        {
                        IntegrationApiExceptionAddError(string.Format("Value " + preparerSigned + " in the database is not a valid data for Applicant Preparer Signed."));
                        }
                  return applicantPreparerSigned;
                  }
            private string ConvertSignedFlagtoDto(string signedFlag, bool getById)
            {
                  string applicantSignedFlag = null;
                  switch (signedFlag)
                  {
                        case "A":
                              applicantSignedFlag = "ApplicantOnly";
                              break;
                        case "B":
                              applicantSignedFlag = "ApplicantAndParent";
                              break;
                        case "P":
                              applicantSignedFlag = "ParentOnly";
                              break;
                  }
                  if (getById && applicantSignedFlag == null && !string.IsNullOrEmpty(signedFlag))
                  {
                        IntegrationApiExceptionAddError(string.Format("Value " + signedFlag + " in the database does not match the values listed in the Applicant signed by type"));
                  }
                  return applicantSignedFlag;
            }

            private AidApplicationsParentEdLevel? ConvertParentEdLevelEntitytoDto(string parentGradLevelEntity, bool getById)
        {
            AidApplicationsParentEdLevel? parentGradLevelDto = null;
            switch (parentGradLevelEntity)
            {
                case "1":
                    parentGradLevelDto = AidApplicationsParentEdLevel.MiddleSchoolOrJrHigh;
                    break;
                case "2":
                    parentGradLevelDto = AidApplicationsParentEdLevel.HighSchool;
                    break;
                case "3":
                    parentGradLevelDto = AidApplicationsParentEdLevel.CollegeOrBeyond;
                    break;
                case "4":
                    parentGradLevelDto = AidApplicationsParentEdLevel.OtherOrUnknown;
                    break;
            }
            if (getById && parentGradLevelDto == null && !string.IsNullOrEmpty(parentGradLevelEntity))
                 IntegrationApiExceptionAddError(string.Format("Value " + parentGradLevelEntity + " in the database does not match the values listed in the Parent's educational level"));

            return parentGradLevelDto;
        }

        private  AidApplicationHousingPlanDto? ConvertHousingPlanEntityToDto(string housingPlan, bool getById)
        {
            AidApplicationHousingPlanDto? housingPlanCode = null;
            switch (housingPlan)
            {
                case "1":
                    housingPlanCode = AidApplicationHousingPlanDto.OnCampus;
                    break;
                case "2":
                    housingPlanCode = AidApplicationHousingPlanDto.WithParent;
                    break;
                case "3":
                    housingPlanCode = AidApplicationHousingPlanDto.OffCampus;
                    break;
            }
            if (getById && housingPlanCode == null && !string.IsNullOrEmpty(housingPlan))
            { IntegrationApiExceptionAddError(string.Format("Value " + housingPlan + " in the database does not match the values listed in the housing plans")); }

            return housingPlanCode;
        }

        private AidApplicationsTaxReturnFiledDto? ConvertTaxReturnFiledEntityToDto(string taxReturnFile, bool getById)
        {
            AidApplicationsTaxReturnFiledDto? taxreturnfilingstatus = null;
            switch (taxReturnFile)
            {
                case "1":
                    taxreturnfilingstatus = AidApplicationsTaxReturnFiledDto.AlreadyCompleted;
                    break;
                case "2":
                    taxreturnfilingstatus = AidApplicationsTaxReturnFiledDto.WillFile;
                    break;
                case "3":
                    taxreturnfilingstatus = AidApplicationsTaxReturnFiledDto.WillNotFile;
                    break;
            }
            if (getById && taxreturnfilingstatus == null && !string.IsNullOrEmpty(taxReturnFile))
            { IntegrationApiExceptionAddError(string.Format("Value " + taxReturnFile + " in the database does not match the values listed in the tax return filed status")); }
            return taxreturnfilingstatus;
        }

        private AidApplicationsTaxFormTypeDto? ConvertTaxFormEntityToDto(string formType, bool getById)
        {
            AidApplicationsTaxFormTypeDto? taxformtype = null;
            switch (formType)
            {
                case "1":
                    taxformtype = AidApplicationsTaxFormTypeDto.IRS1040;
                    break;
                case "3":
                    taxformtype = AidApplicationsTaxFormTypeDto.ForeignTaxReturn;
                    break;
                case "4":
                    taxformtype = AidApplicationsTaxFormTypeDto.ATaxReturnFromPuertoRicoOrAUsaTerritoryOrFreelyAssociatedState;
                    break;
            }
            if (getById && taxformtype == null && !string.IsNullOrEmpty(formType))
            { IntegrationApiExceptionAddError(string.Format("Value " + formType + " in the database does not match the values listed in the tax form types")); }

            return taxformtype;
        }

        private AidApplicationsTaxFilingStatusDto? ConvertTaxFilingStatusEntityToDto(string taxfilingstatus, bool getById)
        {
            AidApplicationsTaxFilingStatusDto? taxFileStatus = null;
            switch (taxfilingstatus)
            {
                case "1":
                    taxFileStatus = AidApplicationsTaxFilingStatusDto.Single;
                    break;
                case "2":
                    taxFileStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledJointReturn;
                    break;
                case "3":
                    taxFileStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledSeparateReturn;
                    break;
                case "4":
                    taxFileStatus = AidApplicationsTaxFilingStatusDto.HeadOfHousehold;
                    break;
                case "5":
                    taxFileStatus = AidApplicationsTaxFilingStatusDto.QualifyingWidowOrWidower;
                    break;
                case "6":
                    taxFileStatus = AidApplicationsTaxFilingStatusDto.DoNotKnow;
                    break;
            }
            if (getById && taxFileStatus == null && !string.IsNullOrEmpty(taxfilingstatus))
            { IntegrationApiExceptionAddError(string.Format("Value " + taxfilingstatus + " in the database does not match the values listed in the tax filing status")); }
            return taxFileStatus;
        }

        private AidApplicationsStudentMarital? ConvertStudentMaritalToDto(string studentMarital, bool getById)
        {
            AidApplicationsStudentMarital? studentmaritalstatus = null;
            switch (studentMarital)
            {
                case "1":
                    studentmaritalstatus = AidApplicationsStudentMarital.Single;
                    break;
                case "2":
                    studentmaritalstatus = AidApplicationsStudentMarital.MarriedOrRemarried;
                    break;
                case "3":
                    studentmaritalstatus = AidApplicationsStudentMarital.Separated;
                    break;
                case "4":
                    studentmaritalstatus = AidApplicationsStudentMarital.DivorcedOrWidowed;
                    break;
            }
            if (getById && studentmaritalstatus == null && !string.IsNullOrEmpty(studentMarital))
            {
                IntegrationApiExceptionAddError(string.Format("Value " + studentMarital + " in the database does not match the values listed in the Student marital statuses"));
            }
            return studentmaritalstatus;
        }


        private AidApplicationsParentMarital? ConvertParentMaritalToDto(string parentMarital, bool getById)
        {
            AidApplicationsParentMarital? parentmaritalstatus = null;
            switch (parentMarital)
            {
                case "1":
                    parentmaritalstatus = AidApplicationsParentMarital.MarriedOrRemarried;
                    break;
                case "2":
                    parentmaritalstatus = AidApplicationsParentMarital.NeverMarried;
                    break;
                case "3":
                    parentmaritalstatus = AidApplicationsParentMarital.DivorcedOrSeparated;
                    break;
                case "4":
                    parentmaritalstatus = AidApplicationsParentMarital.Widowed;
                    break;
                case "5":
                    parentmaritalstatus = AidApplicationsParentMarital.UnmarriedAndBothParentsLivingTogether;
                    break;
            }
            if (getById && parentmaritalstatus == null && !string.IsNullOrEmpty(parentMarital))
            {
                IntegrationApiExceptionAddError(string.Format("Value " + parentMarital + " in the database does not match the values listed in the Parent marital statuses"));
            }
            return parentmaritalstatus;

        }

        private AidApplicationsDegreeOrCert? ConvertDegreeOrCertToDto(string degreeOrCert, bool getById)
        {
            AidApplicationsDegreeOrCert? degorCert = null;
            switch (degreeOrCert)
            {
                case "1":
                    degorCert = AidApplicationsDegreeOrCert.FirstBachelorsDegree;
                    break;
                case "2":
                    degorCert = AidApplicationsDegreeOrCert.SecondBachelorsDegree;
                    break;
                case "3":
                    degorCert = AidApplicationsDegreeOrCert.AssociateDegreeOccupationalOrTechnicalProgram;
                    break;
                case "4":
                    degorCert = AidApplicationsDegreeOrCert.AssociateDegreeGeneralEducationOrTransferProgram;
                    break;
                case "5":
                    degorCert = AidApplicationsDegreeOrCert.CertificateOrDiplomaForCompletingAnOccupationalOrTechnicalOrEducationalProgramOfLessThanTwoYears;
                    break;
                case "6":
                    degorCert = AidApplicationsDegreeOrCert.CertificateOrDiplomaForCompletingAnOccupationalOrTechnicalOrEducationalProgramOfAtLeastTwoYears;
                    break;
                case "7":
                    degorCert = AidApplicationsDegreeOrCert.TeachingCredentialProgram;
                    break;
                case "8":
                    degorCert = AidApplicationsDegreeOrCert.GraduateOrProfessionalDegree;
                    break;
                case "9":
                    degorCert = AidApplicationsDegreeOrCert.OtherOrUndecided;
                    break;
            }
            if (getById && degorCert == null && !string.IsNullOrEmpty(degreeOrCert))
            { IntegrationApiExceptionAddError(string.Format("Value " + degreeOrCert + " in the database does not match the values listed in the Degree or Certificate")); }

            return degorCert;
        }

        private AidApplicationsHSGradtype? ConvertHSGradTypeToDto(string hSGradType, bool getById)
        {
            AidApplicationsHSGradtype? highSchoolGradType = null;
            switch (hSGradType)
            {
                case "1":
                    highSchoolGradType = AidApplicationsHSGradtype.HighSchoolDiploma;
                    break;
                case "2":
                    highSchoolGradType = AidApplicationsHSGradtype.GEDOrStateEquivalentTest;
                    break;
                case "3":
                    highSchoolGradType = AidApplicationsHSGradtype.HomeSchooled;
                    break;
                case "4":
                    highSchoolGradType = AidApplicationsHSGradtype.Others;
                    break;
            }
            if (getById && highSchoolGradType == null && !string.IsNullOrEmpty(hSGradType))
            { IntegrationApiExceptionAddError(string.Format("Value " + hSGradType + " in the database does not match the values listed in the High School grad type")); }

            return highSchoolGradType;
        }

        private AidApplicationsGradLvlInCollege? ConvertGradLevelinCollege(string gradLevelInCollege, bool getById)
        {
            AidApplicationsGradLvlInCollege? GradLvlInCollege = null;
            switch (gradLevelInCollege)
            {
                case "0":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.FirstYearNeverAttendedCollege;
                    break;
                case "1":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.FirstYearAttendedCollegeBefore;
                    break;
                case "2":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.SecondYearOrSophomore;
                    break;
                case "3":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.ThirdYearOrJunior;
                    break;
                case "4":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.FourthYearOrSenior;
                    break;
                case "5":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.FifthYearOrOtherUndergraduate;
                    break;
                case "6":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.FirstYearGraduateOrProfessional;
                    break;
                case "7":
                    GradLvlInCollege = AidApplicationsGradLvlInCollege.ContinuingGraduateOrProfesional;
                    break;
            }

            if (getById && GradLvlInCollege == null && !string.IsNullOrEmpty(gradLevelInCollege))
            { IntegrationApiExceptionAddError(string.Format("Value " + gradLevelInCollege + " in the database does not match the values listed in the Grade level in college")); }

            return GradLvlInCollege;
        }

        private AidApplicationsYesOrNoDto? ConverYesOrNoToDto(string inputValue, bool getByID, string inputField)
        {
            AidApplicationsYesOrNoDto? yesOrNoOrDontKnow = null;
            switch (inputValue)
            {
                case "1":
                    yesOrNoOrDontKnow = AidApplicationsYesOrNoDto.Yes;
                    break;
                case "2":
                    yesOrNoOrDontKnow = AidApplicationsYesOrNoDto.No;
                    break;
                case "3":
                    yesOrNoOrDontKnow = AidApplicationsYesOrNoDto.DonotKnow;
                    break;
            }
            if (getByID && yesOrNoOrDontKnow == null && !string.IsNullOrEmpty(inputValue))
            { IntegrationApiExceptionAddError(string.Format("Value " + inputValue + " in the database does not match the values listed in the " + inputField)); }

            return yesOrNoOrDontKnow;
        }

        #endregion


        #region Post AidApplicationsAsync
        /// <summary>
        /// Create a new AidApplications record
        /// </summary>
        /// <param name="aidApplicationsDto">AidApplications DTO</param>
        /// <returns>AidApplications domain entity</returns>
        public async Task<Dtos.FinancialAid.AidApplications> PostAidApplicationsAsync(Dtos.FinancialAid.AidApplications aidApplicationsDto)
        {
            _aidApplicationsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            Domain.FinancialAid.Entities.AidApplications createdAidApplications = null;
            Domain.FinancialAid.Entities.AidApplications aidApplications = null;
            Dtos.FinancialAid.AidApplications aidApplicationsResponse = null;
            try
            {
                ValidateAidApplications(aidApplicationsDto);
                aidApplicationsDto.Id = "New";
                aidApplications = await ConvertAidApplicationsDtoToEntity(aidApplicationsDto);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplications != null && !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            try
            {
                // create a AidApplications entity in the database
                createdAidApplications = await _aidApplicationsRepository.CreateAidApplicationsAsync(aidApplications);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplications != null && !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null); 
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }


            if (createdAidApplications != null)
            {
                // the newly created AidApplications Dto
                try
                {
                    aidApplicationsResponse = ConvertAidApplicationsToDto(createdAidApplications, true);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("Record created. Error building response. " + ex.Message, "Global.Internal.Error",
                        !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);
                }
                if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }
            }
            else
            {
                IntegrationApiExceptionAddError("Possible error creating record.  No additional details available. Please check to see if record was created.", "Global.Internal.Error",
                    aidApplications != null && !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);

                throw IntegrationApiException;
            }

            return aidApplicationsResponse;
        }
        #endregion


        #region Put AidApplicationsAsync
        /// <summary>
        /// Update an existing AidApplications record
        /// </summary>
        /// <param name="aidApplicationsDto">AidApplications DTO</param>
        /// <returns>AidApplications domain entity</returns>
        public async Task<Dtos.FinancialAid.AidApplications> PutAidApplicationsAsync(string id, Dtos.FinancialAid.AidApplications aidApplicationsDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("id required to update an aid applications");
            }

            _aidApplicationsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            Domain.FinancialAid.Entities.AidApplications aidApplications = null;
            try
            {
                ValidateAidApplications(aidApplicationsDto, false);
                aidApplications = await ConvertAidApplicationsDtoToEntity(aidApplicationsDto);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not updated. Error extracting request. " + ex.Message, "Global.Internal.Error",
                   aidApplications != null && !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            Domain.FinancialAid.Entities.AidApplications createdAidApplications = null;
            try
            {
                // create a AidApplications entity in the database
                createdAidApplications = await _aidApplicationsRepository.UpdateAidApplicationsAsync(aidApplications);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                    aidApplications != null && !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);
                
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            Dtos.FinancialAid.AidApplications aidApplicationsUpdateResponse = null; 


            if (createdAidApplications != null)
            {
                // the newly created AidApplications Dto
                try
                {
                    aidApplicationsUpdateResponse = ConvertAidApplicationsToDto(createdAidApplications, true);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("Record created. Error building response. " + ex.Message, "Global.Internal.Error",
                        !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);
                }
                if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }
            }
            else
            {
                IntegrationApiExceptionAddError("Possible error creating record.  No additional details available.  Please check to see if record was created.", "Global.Internal.Error",
                    aidApplications != null && !string.IsNullOrEmpty(aidApplications.Id) ? aidApplications.Id : null);

                throw IntegrationApiException;
            }

            // return the newly created AidApplications Dto
            return aidApplicationsUpdateResponse;

        }

        #endregion


        #region ConvertAidApplicationsDtoToEntity (POST and PUT)
        private async Task<Domain.FinancialAid.Entities.AidApplications> ConvertAidApplicationsDtoToEntity(Dtos.FinancialAid.AidApplications aidApplicationsDto)
        {
            Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographics;
           
            aidApplicationDemographics = await _aidApplicationsDemoRepository.GetAidApplicationDemographicsByIdAsync(aidApplicationsDto.AppDemoID);

            var finAidYears = await _financialAidReferenceDataRepository.GetFinancialAidYearsAsync(false);
            var aidApplicationTypes = await _studentReferenceDataRepository.GetAidApplicationTypesAsync();

            // validations
            if (string.IsNullOrEmpty(aidApplicationsDto.Id) && aidApplicationsDto.Id != aidApplicationsDto.AppDemoID)
            {
                IntegrationApiExceptionAddError(string.Format("Value of id and appDemoId does not match"));
                return null;
            }

            if (aidApplicationsDto.AidYear != null && aidApplicationsDto.AidYear != aidApplicationDemographics.AidYear)
            {
                IntegrationApiExceptionAddError(string.Format("The aidYear associated with appDemoId does not match the input aidYear"));
            }
            if (aidApplicationsDto.PersonId != null && aidApplicationsDto.PersonId != aidApplicationDemographics.PersonId)
            {
                IntegrationApiExceptionAddError(string.Format("The personId associated with appDemoId does not match the input personId"));
            }
            if (aidApplicationsDto.ApplicationType != null && aidApplicationsDto.ApplicationType != aidApplicationDemographics.ApplicationType)
            {
                IntegrationApiExceptionAddError(string.Format("The applicationType associated with appDemoId does not match the input aidApplicationType"));
            }

            aidApplicationsDto.AidYear = aidApplicationDemographics.AidYear;
            aidApplicationsDto.PersonId = aidApplicationDemographics.PersonId;
            aidApplicationsDto.ApplicationType = aidApplicationDemographics.ApplicationType;

            if (finAidYears == null || !finAidYears.Any(x => x.Code.ToLower() == aidApplicationsDto.AidYear))
            {
                IntegrationApiExceptionAddError(string.Format("aid year {0} not found in FA.SUITES.YEAR", aidApplicationsDto.AidYear));
            }
            if (aidApplicationTypes == null || !aidApplicationTypes.Any(x => x.Code.ToLower() == aidApplicationsDto.ApplicationType.ToLower()))
            {
                IntegrationApiExceptionAddError(string.Format("Application type {0} is not found in ST.VALCODES - FA.APPLN.TYPES", aidApplicationsDto.ApplicationType));
            }

            // create entity from DTO
            Domain.FinancialAid.Entities.AidApplications aidApplicationsEntity = new Domain.FinancialAid.Entities.AidApplications(aidApplicationsDto.Id, aidApplicationsDto.AppDemoID);
            aidApplicationsEntity.AidYear = aidApplicationsDto.AidYear;
            aidApplicationsEntity.AidApplicationType = aidApplicationsDto.ApplicationType;
            aidApplicationsEntity.PersonId = aidApplicationsDto.PersonId;
            aidApplicationsEntity.AssignedID = aidApplicationsDto.ApplicantAssignedId;
            if (aidApplicationsDto.StudentMarital != null)
            {
                aidApplicationsEntity.StudentMaritalStatus = aidApplicationsDto.StudentMarital.Status != null ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsStudentMarital), aidApplicationsDto.StudentMarital.Status, "StudentMaritalStatus") : null;
                aidApplicationsEntity.StudentMaritalDate = aidApplicationsDto.StudentMarital.Date;
            }
            if(aidApplicationsDto.StudentLegalResidence != null)
            {
                aidApplicationsEntity.StudentLegalResSt = aidApplicationsDto.StudentLegalResidence.State;
                aidApplicationsEntity.StudentLegalResB4 = aidApplicationsDto.StudentLegalResidence.ResidentBefore;
                aidApplicationsEntity.StudentLegalResDate = aidApplicationsDto.StudentLegalResidence.Date;
            }
            if (aidApplicationsDto.Parents != null)
            {
                if(aidApplicationsDto.Parents.FirstParent != null)
                {
                    aidApplicationsEntity.P1FirstInit = aidApplicationsDto.Parents.FirstParent.FirstInitial;
                    aidApplicationsEntity.P1GradeLvl = aidApplicationsDto.Parents.FirstParent.EducationalLevel != null ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsParentEdLevel), aidApplicationsDto.Parents.FirstParent.EducationalLevel, "Parent1 Education Level") : null;
                    aidApplicationsEntity.P1Dob = aidApplicationsDto.Parents.FirstParent.BirthDate;
                    aidApplicationsEntity.P1Ssn = aidApplicationsDto.Parents.FirstParent.SsnOrItin;
                    aidApplicationsEntity.P1LastName = aidApplicationsDto.Parents.FirstParent.LastName;
                }
                
                if(aidApplicationsDto.Parents.SecondParent != null)
                {
                    aidApplicationsEntity.P2FirstInit = aidApplicationsDto.Parents.SecondParent.FirstInitial;
                    aidApplicationsEntity.P2GradeLvl = aidApplicationsDto.Parents.SecondParent.EducationalLevel != null ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsParentEdLevel), aidApplicationsDto.Parents.SecondParent.EducationalLevel, "Parent2 Education Level") : null;
                    aidApplicationsEntity.P2Dob = aidApplicationsDto.Parents.SecondParent.BirthDate;
                    aidApplicationsEntity.P2Ssn = aidApplicationsDto.Parents.SecondParent.SsnOrItin;
                    aidApplicationsEntity.P2LastName = aidApplicationsDto.Parents.SecondParent.LastName;
                }
                
                if(aidApplicationsDto.Parents.ParentMarital != null)
                {
                    aidApplicationsEntity.PMaritalStatus = aidApplicationsDto.Parents.ParentMarital.Status != null ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsParentMarital), aidApplicationsDto.Parents.ParentMarital.Status, "Parent Marital Status") : null;
                    aidApplicationsEntity.PMaritalDate = aidApplicationsDto.Parents.ParentMarital.Date;

                }

                aidApplicationsEntity.ParentEmail = aidApplicationsDto.Parents.EmailAddress;

                if(aidApplicationsDto.Parents.ParentLegalResidence != null)
                {
                    aidApplicationsEntity.PLegalResB4 = aidApplicationsDto.Parents.ParentLegalResidence.ResidentBefore;
                    aidApplicationsEntity.PLegalResSt = aidApplicationsDto.Parents.ParentLegalResidence.State;
                    aidApplicationsEntity.PLegalResDate = aidApplicationsDto.Parents.ParentLegalResidence.Date;
                }
                aidApplicationsEntity.PNbrCollege = aidApplicationsDto.Parents.NumberInCollege;
                aidApplicationsEntity.PNbrFamily = aidApplicationsDto.Parents.NumberInFamily;
                if (aidApplicationsDto.Parents.Income != null)
                {
                    aidApplicationsEntity.PSsiBen = aidApplicationsDto.Parents.Income.SsiBenefits;
                    aidApplicationsEntity.PFoodStamps = aidApplicationsDto.Parents.Income.FoodStamps;
                    aidApplicationsEntity.PLunchBen = aidApplicationsDto.Parents.Income.LunchBenefits;
                    aidApplicationsEntity.PTanf = aidApplicationsDto.Parents.Income.TanfBenefits;
                    aidApplicationsEntity.PWic = aidApplicationsDto.Parents.Income.WicBenefits;
                    aidApplicationsEntity.PTaxFilingStatus = aidApplicationsDto.Parents.Income.TaxFilingStatus.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsTaxFilingStatusDto), aidApplicationsDto.Parents.Income.TaxFilingStatus, "Parent's tax filing status") : null;
                    aidApplicationsEntity.PTaxFormType = aidApplicationsDto.Parents.Income.TaxFormType.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsTaxFormTypeDto), aidApplicationsDto.Parents.Income.TaxFormType, "Parent's Tax form type") : null;
                    aidApplicationsEntity.PTaxReturnFiled = aidApplicationsDto.Parents.Income.TaxReturnFiled.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsTaxReturnFiledDto), aidApplicationsDto.Parents.Income.TaxReturnFiled, "Parent's Tax return filed") : null;
                    aidApplicationsEntity.PSched1 = aidApplicationsDto.Parents.Income.Schedule1Filed.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsYesOrNoDto), aidApplicationsDto.Parents.Income.Schedule1Filed, "Parent Schedule 1 filed") : null;
                    aidApplicationsEntity.PDisWorker = aidApplicationsDto.Parents.Income.DislocatedWorker.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsYesOrNoDto), aidApplicationsDto.Parents.Income.DislocatedWorker, "Parent Dislocated worker") : null;
                    aidApplicationsEntity.PAgi = aidApplicationsDto.Parents.Income.AdjustedGrossIncome;
                    aidApplicationsEntity.PUsTaxPaid = aidApplicationsDto.Parents.Income.UsTaxPaid;
                    aidApplicationsEntity.P1Income = aidApplicationsDto.Parents.Income.FirstParentWorkEarnings;
                    aidApplicationsEntity.P2Income = aidApplicationsDto.Parents.Income.SecondParentworkEarnings;
                    aidApplicationsEntity.PCash = aidApplicationsDto.Parents.Income.CashSavingsChecking;
                    aidApplicationsEntity.PInvNetWorth = aidApplicationsDto.Parents.Income.InvestmentNetWorth;
                    aidApplicationsEntity.PBusNetWorth = aidApplicationsDto.Parents.Income.BusinessOrFarmNetWorth;
                    aidApplicationsEntity.PEduCredit = aidApplicationsDto.Parents.Income.EducationalCredits;
                    aidApplicationsEntity.PChildSupportPd = aidApplicationsDto.Parents.Income.ChildSupportPaid;
                    aidApplicationsEntity.PNeedBasedEmp = aidApplicationsDto.Parents.Income.NeedBasedEmployment;
                    aidApplicationsEntity.PGrantScholAid = aidApplicationsDto.Parents.Income.GrantOrScholarshipAid;
                    aidApplicationsEntity.PCombatPay = aidApplicationsDto.Parents.Income.CombatPay;
                    aidApplicationsEntity.PCoOpEarnings = aidApplicationsDto.Parents.Income.CoopEarnings;
                    aidApplicationsEntity.PPensionPymts = aidApplicationsDto.Parents.Income.PensionPayments;
                    aidApplicationsEntity.PIraPymts = aidApplicationsDto.Parents.Income.IraPayments;
                    aidApplicationsEntity.PChildSupRcvd = aidApplicationsDto.Parents.Income.ChildSupportReceived;
                    aidApplicationsEntity.PUntxIntInc = aidApplicationsDto.Parents.Income.TaxExemptInterstIncome;
                    aidApplicationsEntity.PUntxIraPen = aidApplicationsDto.Parents.Income.UntaxedIraAndPensions;
                    aidApplicationsEntity.PMilClerAllow = aidApplicationsDto.Parents.Income.MilitaryOrClergyAllowances;
                    aidApplicationsEntity.PVetNonEdBen = aidApplicationsDto.Parents.Income.VeteranNonEdBenefits;
                    aidApplicationsEntity.POtherUntxInc = aidApplicationsDto.Parents.Income.OtherUntaxedIncome;
                }
            }
            if(aidApplicationsDto.HighSchool != null)
            {
                aidApplicationsEntity.HsGradType = aidApplicationsDto.HighSchool.GradType.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsHSGradtype), aidApplicationsDto.HighSchool.GradType, "Highschool Grad type") : null;
                aidApplicationsEntity.HsName = aidApplicationsDto.HighSchool.Name;
                aidApplicationsEntity.HsCity = aidApplicationsDto.HighSchool.City;
                aidApplicationsEntity.HsState = aidApplicationsDto.HighSchool.State;
                aidApplicationsEntity.HsCode = aidApplicationsDto.HighSchool.Code;
            }
            aidApplicationsEntity.DegreeBy = aidApplicationsDto.DegreeBy;
            aidApplicationsEntity.GradeLevelInCollege = aidApplicationsDto.GradeLevelInCollege != null ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsGradLvlInCollege), aidApplicationsDto.GradeLevelInCollege, "Grade level in college") : null;
            aidApplicationsEntity.DegreeOrCertificate = aidApplicationsDto.DegreeOrCertificate != null ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsDegreeOrCert), aidApplicationsDto.DegreeOrCertificate, "Degree or Certificate") : null;
            if(aidApplicationsDto.StudentsIncome != null)
            {
                aidApplicationsEntity.StudentTaxReturnFiled = aidApplicationsDto.StudentsIncome.TaxReturnFiled.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsTaxReturnFiledDto), aidApplicationsDto.StudentsIncome.TaxReturnFiled, "student tax return filed") : null;
                aidApplicationsEntity.StudentTaxFormType = aidApplicationsDto.StudentsIncome.TaxFormType.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsTaxFormTypeDto), aidApplicationsDto.StudentsIncome.TaxFormType, "Student tax form type") : null;
                aidApplicationsEntity.StudentTaxFilingStatus = aidApplicationsDto.StudentsIncome.TaxFilingStatus.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsTaxFilingStatusDto), aidApplicationsDto.StudentsIncome.TaxFilingStatus, "Student tax filing status") : null;
                aidApplicationsEntity.StudentSched1 = aidApplicationsDto.StudentsIncome.Schedule1Filed.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsYesOrNoDto), aidApplicationsDto.StudentsIncome.Schedule1Filed, "Student Schedule 1 filed") : null;
                aidApplicationsEntity.StudentAgi = aidApplicationsDto.StudentsIncome.AdjustedGrossIncome;
                aidApplicationsEntity.StudentUsTaxPd = aidApplicationsDto.StudentsIncome.UsTaxPaid;
                aidApplicationsEntity.SStudentInc = aidApplicationsDto.StudentsIncome.WorkEarnings;
                aidApplicationsEntity.SpouseInc = aidApplicationsDto.StudentsIncome.SpouseWorkEarnings;
                aidApplicationsEntity.StudentCash = aidApplicationsDto.StudentsIncome.CashSavingsChecking;
                aidApplicationsEntity.StudentInvNetWorth = aidApplicationsDto.StudentsIncome.InvestmentNetWorth;
                aidApplicationsEntity.StudentBusNetWorth = aidApplicationsDto.StudentsIncome.BusinessNetWorth;
                aidApplicationsEntity.StudentEduCredit = aidApplicationsDto.StudentsIncome.EducationalCredit;
                aidApplicationsEntity.StudentChildSupPaid = aidApplicationsDto.StudentsIncome.ChildSupportPaid;
                aidApplicationsEntity.StudentNeedBasedEmp = aidApplicationsDto.StudentsIncome.NeedBasedEmployment;
                aidApplicationsEntity.StudentGrantScholAid = aidApplicationsDto.StudentsIncome.GrantAndScholarshipAid;
                aidApplicationsEntity.StudentCombatPay = aidApplicationsDto.StudentsIncome.CombatPay;
                aidApplicationsEntity.StudentCoOpEarnings = aidApplicationsDto.StudentsIncome.CoopEarnings;
                aidApplicationsEntity.StudentPensionPayments = aidApplicationsDto.StudentsIncome.PensionPayments;
                aidApplicationsEntity.StudentIraPayments = aidApplicationsDto.StudentsIncome.IraPayments;
                aidApplicationsEntity.StudentInterestIncome = aidApplicationsDto.StudentsIncome.InterestIncome;
                aidApplicationsEntity.StudentChildSupRecv = aidApplicationsDto.StudentsIncome.ChildSupportReceived;
                aidApplicationsEntity.StudentOtherUntaxedInc = aidApplicationsDto.StudentsIncome.OtherUntaxedIncome;
                aidApplicationsEntity.StudentUntxIraPen = aidApplicationsDto.StudentsIncome.UntaxedIraPension;
                aidApplicationsEntity.StudentMilitaryClergyAllow = aidApplicationsDto.StudentsIncome.MilitaryClergyAllowance;
                aidApplicationsEntity.StudentVetNonEdBen = aidApplicationsDto.StudentsIncome.VeteranNonEdBenefits;
                aidApplicationsEntity.StudentOtherNonRepMoney = aidApplicationsDto.StudentsIncome.OtherNonReportedMoney;
                aidApplicationsEntity.SSsiBen = aidApplicationsDto.StudentsIncome.MedicaidOrSSIBenefits;
                aidApplicationsEntity.SFoodStamps = aidApplicationsDto.StudentsIncome.FoodStamps;
                aidApplicationsEntity.SLunchBen = aidApplicationsDto.StudentsIncome.LunchBenefits;
                aidApplicationsEntity.STanf = aidApplicationsDto.StudentsIncome.TanfBenefits;
                aidApplicationsEntity.SWic = aidApplicationsDto.StudentsIncome.WicBenefits;
                aidApplicationsEntity.SDislWorker = aidApplicationsDto.StudentsIncome.DislocatedWorker.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationsYesOrNoDto), aidApplicationsDto.StudentsIncome.DislocatedWorker, "Student dislocated worker") : null;
            }
            aidApplicationsEntity.BornBefore = aidApplicationsDto.BornBefore;
            aidApplicationsEntity.Married = aidApplicationsDto.Married;
            aidApplicationsEntity.GradOrProfProgram = aidApplicationsDto.GradOrProfProgram;
            aidApplicationsEntity.ActiveDuty = aidApplicationsDto.ActiveDuty;
            aidApplicationsEntity.UsVeteran = aidApplicationsDto.USVeteran;
            aidApplicationsEntity.DependentChildren = aidApplicationsDto.DependentChildren;
            aidApplicationsEntity.OtherDependents = aidApplicationsDto.OtherDependents;
            aidApplicationsEntity.OrphanWardFoster = aidApplicationsDto.OrphanWardFoster;
            aidApplicationsEntity.EmancipatedMinor = aidApplicationsDto.EmancipatedMinor;
            aidApplicationsEntity.LegalGuardianship = aidApplicationsDto.LegalGuardianship;
            aidApplicationsEntity.HomelessBySchool = aidApplicationsDto.HomelessBySchool;
            aidApplicationsEntity.HomelessByHud = aidApplicationsDto.HomelessByHud;
            aidApplicationsEntity.HomelessAtRisk = aidApplicationsDto.HomelessAtRisk;
            aidApplicationsEntity.StudentNumberInCollege = aidApplicationsDto.StudentNumberInCollege;
            aidApplicationsEntity.StudentNumberInFamily = aidApplicationsDto.StudentNumberInFamily;
            
            if(aidApplicationsDto.SchoolCode1 != null)
            {
                aidApplicationsEntity.SchoolCode1 = aidApplicationsDto.SchoolCode1.Code;
                aidApplicationsEntity.HousingPlan1 = aidApplicationsDto.SchoolCode1.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode1.HousingPlan, "Housing Plan 1") : null;
            }
            if(aidApplicationsDto.SchoolCode2 != null)
            {
                aidApplicationsEntity.SchoolCode2 = aidApplicationsDto.SchoolCode2.Code;
                aidApplicationsEntity.HousingPlan2 = aidApplicationsDto.SchoolCode2.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode2.HousingPlan, "Housing Plan 2") : null;

            }

            if (aidApplicationsDto.SchoolCode3 != null)
            {
                aidApplicationsEntity.SchoolCode3 = aidApplicationsDto.SchoolCode3.Code;
                aidApplicationsEntity.HousingPlan3 = aidApplicationsDto.SchoolCode3.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode3.HousingPlan, "Housing Plan 3") : null;
            }

            if (aidApplicationsDto.SchoolCode4 != null)
            {
                aidApplicationsEntity.SchoolCode4 = aidApplicationsDto.SchoolCode4.Code;
                aidApplicationsEntity.HousingPlan4 = aidApplicationsDto.SchoolCode4.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode4.HousingPlan, "Housing Plan 4") : null;
            }

            if (aidApplicationsDto.SchoolCode5 != null)
            {
                aidApplicationsEntity.SchoolCode5 = aidApplicationsDto.SchoolCode5.Code;
                aidApplicationsEntity.HousingPlan5 = aidApplicationsDto.SchoolCode5.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode5.HousingPlan, "Housing Plan 5") : null;
            }

            if (aidApplicationsDto.SchoolCode6 != null)
            {
                aidApplicationsEntity.SchoolCode6 = aidApplicationsDto.SchoolCode6.Code;
                aidApplicationsEntity.HousingPlan6 = aidApplicationsDto.SchoolCode6.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode6.HousingPlan, "Housing Plan 6") : null;

            }

            if (aidApplicationsDto.SchoolCode7 != null)
            {
                aidApplicationsEntity.SchoolCode7 = aidApplicationsDto.SchoolCode7.Code;
                aidApplicationsEntity.HousingPlan7 = aidApplicationsDto.SchoolCode7.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode7.HousingPlan, "Housing Plan 7") : null;
            }

            if (aidApplicationsDto.SchoolCode8 != null)
            {
                aidApplicationsEntity.SchoolCode8 = aidApplicationsDto.SchoolCode8.Code;
                aidApplicationsEntity.HousingPlan8 = aidApplicationsDto.SchoolCode8.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode8.HousingPlan, "Housing Plan 8") : null;

            }

            if (aidApplicationsDto.SchoolCode9 != null)
            {
                aidApplicationsEntity.SchoolCode9 = aidApplicationsDto.SchoolCode9.Code;
                aidApplicationsEntity.HousingPlan9 = aidApplicationsDto.SchoolCode9.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode9.HousingPlan, "Housing Plan 9") : null;
            }
            if (aidApplicationsDto.SchoolCode10 != null)
            {
                aidApplicationsEntity.SchoolCode10 = aidApplicationsDto.SchoolCode10.Code;
                aidApplicationsEntity.HousingPlan10 = aidApplicationsDto.SchoolCode10.HousingPlan.HasValue ? ValidateEnumAndConvertToEntity(typeof(AidApplicationHousingPlanDto), aidApplicationsDto.SchoolCode10.HousingPlan, "Housing Plan 10") : null;
            }

            aidApplicationsEntity.ApplicationCompleteDate = aidApplicationsDto.ApplicationCompleteDate;
            aidApplicationsEntity.SignedFlag = ConvertSignedFlagDtoToEntity(aidApplicationsDto.SignedFlag);
            aidApplicationsEntity.PreparerSsn = aidApplicationsDto.PreparerSsn;
            aidApplicationsEntity.PreparerEin = aidApplicationsDto.PreparerEin;
            if(string.Equals(aidApplicationsDto.PreparerSigned, "1") || string.IsNullOrEmpty(aidApplicationsDto.PreparerSigned) || string.Equals(aidApplicationsDto.PreparerSigned, "Yes"))
            {
               aidApplicationsEntity.PreparerSigned = !string.IsNullOrEmpty(aidApplicationsDto.PreparerSigned) ? "1" : null;
            } else
                 IntegrationApiExceptionAddError(string.Format("Invalid Preparer Signed :" + aidApplicationsDto.PreparerSigned));

           return aidApplicationsEntity;

        }
        #endregion

    private string ConvertSignedFlagDtoToEntity(string input)
        {
            //AidApplicationsSignedFlag
            string signedFlag = null;

            if (!string.IsNullOrEmpty(input))
            {
                if (input == "ApplicantOnly" || input == "A")
                {
                    signedFlag = "A";
                    return signedFlag;
                }
                if (input == "ApplicantAndParent" || input == "B")
                {
                    signedFlag = "B";
                    return signedFlag;
                }
                if (input == "ParentOnly" || input == "P")
                {
                    signedFlag = "P";
                    return signedFlag;
                }
                if (string.IsNullOrEmpty(signedFlag))
                {
                    IntegrationApiExceptionAddError(string.Format("Invalid Signed Flag :" + input));
                }
            }
            return signedFlag;
        }
    private string ValidateEnumAndConvertToEntity(Type enumType, object value, string fieldName)
        {
            string output = null;
            if (value != null)
            {
                if (Enum.IsDefined(enumType, value))
                {
                    output = value.GetHashCode().ToString();
                }
                else
                    IntegrationApiExceptionAddError(string.Format("Invalid " + fieldName + " : " + value.ToString()));

            }
            return output;
        }
        
    // validate the aidapplications DTO
    private void ValidateAidApplications(Dtos.FinancialAid.AidApplications aidApplicationsDto, bool createRecord = true)
        {
            string createUpdateText = createRecord ? "creating record" : "updating record";

            if (aidApplicationsDto == null)
                IntegrationApiExceptionAddError(string.Format("Must provide a aidApplications for {0}", createUpdateText));

            // aid applications Id must be provided to update 
            if (!createRecord && string.IsNullOrEmpty(aidApplicationsDto.Id))
                IntegrationApiExceptionAddError(string.Format("Must provide a id for aidApplications for {0}", createUpdateText));
            
            // aid app demo ID is required field
            if (string.IsNullOrEmpty(aidApplicationsDto.AppDemoID))
                IntegrationApiExceptionAddError(string.Format("Must provide an appDemoID for aidApplications for {0}", createUpdateText));

            if (IntegrationApiException != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
        }
    
    }
}