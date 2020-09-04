/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Dto = Ellucian.Colleague.Dtos.HumanResources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestBenefitsEnrollmentRepository : IBenefitsEnrollmentRepository
    {
        #region Update Benefits Enrollment

        public Dto.EmployeeBenefitsEnrollmentPoolItem EmployeeBenefitsEnrollmentPoolItemDto = new Dto.EmployeeBenefitsEnrollmentPoolItem()
        {
            Id = "262",
            PersonId = "0018073",
            IsTrust = false,
            Prefix = "Miss",
            FirstName = "Neha",
            MiddleName = "K",
            LastName = "Prasad",
            Suffix = "Ph.D",
            AddressLine1 = "1982 Walnut Street",
            AddressLine2 = "Palm Meadows",
            City = "Beaufort",
            State = "WI",
            PostalCode = "53226",
            Country = "US",
            Relationship = "C",
            BirthDate = null,
            GovernmentId = "984561076",
            Gender = "F",
            MaritalStatus = "M",
            IsFullTimeStudent = true,
            OrganizationId = null,
            OrganizationName = null
        };

        public EmployeeBenefitsEnrollmentPoolItem EmployeeBenefitsEnrollmentPoolItemEntity = new EmployeeBenefitsEnrollmentPoolItem()
        {
            Id = "262",
            PersonId = "0018073",
            IsTrust = false,
            Prefix = "Miss",
            FirstName = "Neha",
            MiddleName = "K",
            LastName = "Prasad",
            Suffix = "Ph.D",
            AddressLine1 = "1982 Walnut Street",
            AddressLine2 = "Palm Meadows",
            City = "Beaufort",
            State = "WI",
            PostalCode = "53226",
            Country = "US",
            Relationship = "C",
            BirthDate = null,
            GovernmentId = "984561076",
            Gender = "F",
            MaritalStatus = "M",
            IsFullTimeStudent = true,
            OrganizationId = null,
            OrganizationName = null
        };
        #endregion
        public class EnrollmentPackageRecord
        {
            public string employeeId { get; set; }
            public string packageId { get; set; }
            public string packageDescription { get; set; }
            public string enrollmentPeriodId { get; set; }
            public string benefitsPageCustomText { get; set; }
            public string benefitsEnrollmentPageCustomText { get; set; }
            public string manageDepBenPageCustomText { get; set; }

            public List<BenefitTypeRecord> benefitTypes { get; set; }
        }
        public class BenefitTypeRecord
        {
            public string benefitType { get; set; }
            public string benefitTypeDescription { get; set; }
            public string benefitTypeIcon { get; set; }
            public string EpbCalculationMethod { get; set; }
            public string benefitsSelectionPageCustomText { get; set; }
        }

        public class BenefitEnrollmentCompletionInfoRecord
        {
            public string EmployeeId { get; set; }

            public string EnrollmentPeriodId { get; set; }

            public DateTime? EnrollmentConfirmationDate { get; set; }

            public IEnumerable<string> ErrorMessages { get; set; }
        }

        public BenefitEnrollmentCompletionInfoRecord benefitEnrollmentCompletionInfoRecord = new BenefitEnrollmentCompletionInfoRecord()
        {
            EmployeeId = "0014697",
            EnrollmentPeriodId = "19FALL",
            EnrollmentConfirmationDate = DateTime.Now.Date,
            ErrorMessages = new List<string>()
        };

        public List<EnrollmentPackageRecord> employeeEnrollmentPackageRecords = new List<EnrollmentPackageRecord>()
        {
            new EnrollmentPackageRecord() {
                employeeId = "0014697",
                packageId = "FALL19FT",
                packageDescription = "Fall 2019 Enrollment Package",
                enrollmentPeriodId = "FALL19",
                benefitTypes = new List<BenefitTypeRecord>()
                {
                    new BenefitTypeRecord()
                    {
                        benefitType = "MED",
                        benefitTypeDescription = "Medical plans",
                        benefitTypeIcon = "M",
                        benefitsSelectionPageCustomText = "custom text for Medical Plans"
                    },
                    new BenefitTypeRecord()
                    {
                        benefitType = "DEN",
                        benefitTypeDescription = "Dental plans",
                        benefitTypeIcon = "D",
                        benefitsSelectionPageCustomText = "custom text for Dental Plans"
                    },
                    new BenefitTypeRecord()
                    {
                        benefitType = "VIS",
                        benefitTypeDescription = "Vision plans",
                        benefitTypeIcon = "V",
                        benefitsSelectionPageCustomText = "custom text for Vision Plans"
                    }
                }
            },
            new EnrollmentPackageRecord() {
                employeeId = "0014698",
                packageId = "FALL19PT",
                packageDescription = "Fall 2019 Enrollment Package PT",
                enrollmentPeriodId = "FALL19",
                benefitTypes = new List<BenefitTypeRecord>()
                {
                    new BenefitTypeRecord()
                    {
                        benefitType = "MED1",
                        benefitTypeDescription = "Medical1 plans",
                        benefitTypeIcon = "M1",
                        benefitsSelectionPageCustomText = "custom text for Medical1 Plans"
                    },
                    new BenefitTypeRecord()
                    {
                        benefitType = "DEN1",
                        benefitTypeDescription = "Dental1 plans",
                        benefitTypeIcon = "D1",
                        benefitsSelectionPageCustomText = "custom text for Dental1 Plans"
                    },
                    new BenefitTypeRecord()
                    {
                        benefitType = "VIS1",
                        benefitTypeDescription = "Vision1 plans",
                        benefitTypeIcon = "V1",
                        benefitsSelectionPageCustomText = "custom text for Vision1 Plans"
                    }
                }
            }

        };

        public GetBenefitEnrollmentPackageResponse enrollmentPackageResponse = new GetBenefitEnrollmentPackageResponse()
        {
            PkgId = "FALL19FT",
            EnrollmentPeriodId = "FALL19",
            PkgDescripion = "Fall 2019 Enrollment Package",
            BenefitTypesGroup = new List<BenefitTypesGroup>()
            {
                new BenefitTypesGroup()
                {
                    BenefitTypes = "MED",
                    BenefitTypesDescriptions = "Medical plans",
                    BenefitTypesSplProcCode = "M",
                    BendedIds = "MEDICAL",
                    BenefitTypesBenIds = "MEDE"
                },
                new BenefitTypesGroup()
                {
                    BenefitTypes = "DEN",
                    BenefitTypesDescriptions = "Dental plans",
                    BenefitTypesSplProcCode = "D",
                    BendedIds = "DENTAL",
                    BenefitTypesBenIds = "DENE"
                },
                new BenefitTypesGroup()
                {
                    BenefitTypes = "VIS",
                    BenefitTypesDescriptions = "Vision plans",
                    BenefitTypesSplProcCode = "V",
                    BendedIds = "VISION",
                    BenefitTypesBenIds = "VISE"
                }
            }
        };

        public GetBenefitEnrollmentPackageResponse enrollmentPackageResponseWithError = new GetBenefitEnrollmentPackageResponse()
        {
            ErrorMessage = "NO_PACKAGES"
        };

        public List<GetBenefitTypeBenefitsResponse> enrollmentPeriodBenefitsResponses = new List<GetBenefitTypeBenefitsResponse>()
        {
            new GetBenefitTypeBenefitsResponse() {
                PkgId = "2020BENPKG",
                EnrPeriodBenTypeId = "MED",
                EnrPeriodId = "2020PER",
                Benefits = new List<Benefits>()
                {
                    new Benefits()
                    {
                        BendedDescs = "Medical plan family",
                        BendedIds = "MEDF",
                        BendedSelfSvsDescs = "Medical coverage - family",
                        EnrPeriodBenIds = "MEDF2020"
                    },
                    new Benefits()
                    {
                        BendedDescs = "Medical plan single",
                        BendedIds = "MEDS",
                        BendedSelfSvsDescs = "Medical coverage - single",
                        EnrPeriodBenIds = "MEDS2020"
                    },
                    new Benefits()
                    {
                        BendedDescs = "Medical plan couple",
                        BendedIds = "MEDC",
                        BendedSelfSvsDescs = "Medical coverage - couple",
                        EnrPeriodBenIds = "MEDC2020"
                    }
                }
            },
            new GetBenefitTypeBenefitsResponse() {
                PkgId = "2020BENPKG",
                EnrPeriodBenTypeId = "DEN",
                EnrPeriodId = "2020PER",
                Benefits = new List<Benefits>()
                {
                    new Benefits()
                    {
                        BendedDescs = "Dental plan family",
                        BendedIds = "DENF",
                        BendedSelfSvsDescs = "Dental coverage - family",
                        EnrPeriodBenIds = "DENF2020"
                    },
                    new Benefits()
                    {
                        BendedDescs = "Dental plan single",
                        BendedIds = "DENS",
                        BendedSelfSvsDescs = "Dental coverage - single",
                        EnrPeriodBenIds = "DENS2020"
                    },
                    new Benefits()
                    {
                        BendedDescs = "Dental plan couple",
                        BendedIds = "DENC",
                        BendedSelfSvsDescs = "Dental coverage - couple",
                        EnrPeriodBenIds = "DENC2020"
                    }
                }
            }
        };

        public class BeneficiaryCategoryInfoRecord
        {
            public BeneficiaryCategoryInfoRecord()
            {
            }

            public BeneficiaryCategoryInfoRecord(string code, string description, string processingCode)
            {
                this.code = code;
                this.description = description;
                this.processingCode = processingCode;
            }

            public string code { get; set; }
            public string description { get; set; }
            public string processingCode { get; set; }
        }

        public List<BeneficiaryCategoryInfoRecord> beneficiaryResponse = new List<BeneficiaryCategoryInfoRecord>()
         {
            new BeneficiaryCategoryInfoRecord()
              {
                code = "CO",
                description = "Co-Owner",
                processingCode = ""
               },

            new BeneficiaryCategoryInfoRecord()
             {
                code = "ALT",
                description = "Alternate Owner",
                processingCode = ""

             },

            new BeneficiaryCategoryInfoRecord()
             {
                code = "BENEF",
                description = "Beneficiary",
                processingCode = "P"
             }
         };

        public GetBenefitTypeBenefitsResponse enrollmentPeriodBenefitsResponseWithError = new GetBenefitTypeBenefitsResponse()
        {
            ErrorMessage = "NO_PACKAGE"
        };

        public List<GetEmployeeBenefitsEnrollmentInfoResponse> enrollmentBenefitsInfoResponses = new List<GetEmployeeBenefitsEnrollmentInfoResponse>()
        {
           new GetEmployeeBenefitsEnrollmentInfoResponse()
           {
                BenefitsPackageId = "19FALLFT",
                OptOutBenefitTypes = { "19FLCHARIT", "19FLOTHER" },
                OptOutBenefitTypesDesc = { "2020PER","hj" },
                ConfirmationDate = DateTime.Today,
                BenefitsEnrollmentInfoDetails = new List<BenefitsEnrollmentInfoDetails>()
                   {
                      new BenefitsEnrollmentInfoDetails()
                      {
                        WorkDetailIds = "925",
                        BenefitTypeIds= "19FLDENT",
                        EnrollmentPeriodBenefitIds = "",
                        BendedIds = "DEFA",
                        BendedDescriptions = "",
                        CoverageLevels = "",
                        ElectionActions = "",
                        ElectionActionDescs = "",
                        FlexBenefitIsRequired = "",
                        DependentNames = "",
                        DependentIds = "",
                        DependentPoolIds = "",
                        DependentProviderIds = "54321537",
                        DependentProviderNames = "Mary",
                        BeneficiaryNames = "",
                        BeneficiaryIds = "",
                        BeneficiaryPoolIds = "",
                        BeneficiaryTypes = "",
                        ProviderInformation = "",
                        BenefitAmount = null,
                        BenefitPercent = null,
                        BenefitFlexAnnualAmt = null,
                        BenefitInsureAmount = null,
                        EmployeeProviderId = "Brian",
                        EmployeeProviderName = "1234567"
                      },
                     new BenefitsEnrollmentInfoDetails()
                      {
                        WorkDetailIds = "946",
                        BenefitTypeIds= "19FLMED",
                        EnrollmentPeriodBenefitIds = "19FLMEDE",
                        BendedIds = "MEDE",
                        BendedDescriptions = "Medical Plan For Employee",
                        CoverageLevels = "Employee Only - Single",
                        ElectionActions = "CANCEL",
                        ElectionActionDescs = "Cancel",
                        FlexBenefitIsRequired = "",
                        DependentNames = "",
                        DependentIds = "",
                        DependentPoolIds = "",
                        BeneficiaryTypes = "",
                        DependentProviderIds = "23543736",
                        DependentProviderNames = "Lisa",
                        BeneficiaryNames = "",
                        BeneficiaryIds = "",
                        BeneficiaryPoolIds = "",
                        ProviderInformation = "",
                        BenefitAmount = null,
                        BenefitPercent = null,
                        BenefitFlexAnnualAmt = null,
                        BenefitInsureAmount = null,
                        EmployeeProviderId = "Harry",
                        EmployeeProviderName = "9845610763"
                      }
                   }
           }
        };

        public TxSubmitBenefitElectionResponse TxSubmitBenefitElectionResponseWithValidData = new TxSubmitBenefitElectionResponse()
        {
            EnrollmentConfirmationDate = DateTime.Now.Date,
            ErrorMessages = new List<string>()
        };

        public ReopenBenefitSelectionResponse ReopenCTXResponseWithValidData = new ReopenBenefitSelectionResponse()
        {
            EnrollmentConfirmationDate = null
        };

        public TxSubmitBenefitElectionResponse TxSubmitBenefitElectionResponseWithErrorMessage = new TxSubmitBenefitElectionResponse()
        {
            EnrollmentConfirmationDate = null,
            ErrorMessages = new List<string>() { "Ambiguity and Duplicate check failed" }
        };

        public List<EmployeeBenefitsEnrollmentPoolItem> expectedPoolItems = new List<EmployeeBenefitsEnrollmentPoolItem>()
        {
            new EmployeeBenefitsEnrollmentPoolItem("1", "Organization Name", ""),
            new EmployeeBenefitsEnrollmentPoolItem("2", "", "Last Name"),
            new EmployeeBenefitsEnrollmentPoolItem("3", "ABC Organization Name", ""),
            new EmployeeBenefitsEnrollmentPoolItem("4", "", "Last Name 1"),

        };

        public GetBenefitEnrollmentEligibilityResponse benefitEnrollmentEligibilityResponse = new GetBenefitEnrollmentEligibilityResponse()
        {
            EnrollmentConfirmationText = new List<string>() { "confirmation text" },
            EligiblePeriod = "2020FALL",
            EligiblePackage = "2020FALL_PKG",
            EnrollmentPeriodDesc = "2020 FALL",
            EnrollmentPeriodStartDate = new DateTime(2020, 01, 01),
            EnrollmentPeriodEndDate = new DateTime(2020, 08, 01),
            IsEnrollmentInitiated = true,
            IsPackageSubmitted = false
        };

        public GetBenefitEnrollmentEligibilityResponse benefitEnrollmentEligibilityResponseWithIneligibilityReason = new GetBenefitEnrollmentEligibilityResponse()
        {
            IneligibleReason = "NO BENEFITS"
        };

        public GetBenefitEnrollmentEligibilityResponse benefitEnrollmentEligibilityResponseWithError = new GetBenefitEnrollmentEligibilityResponse()
        {
            ErrorMessage = "Something went wrong"
        };

        public Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId)
        {
            var response = benefitEnrollmentEligibilityResponse;
            var EmployeeBenefitsEnrollmentEligibilityEntity = new EmployeeBenefitsEnrollmentEligibility(employeeId, response.EligiblePeriod, response.IneligibleReason)
            {
                Description = response.EnrollmentPeriodDesc,
                StartDate = response.EnrollmentPeriodStartDate,
                EndDate = response.EnrollmentPeriodEndDate,
                EligibilityPackage = response.EligiblePackage,
                EnrollmentConfirmationText = response.EnrollmentConfirmationText,
                ConfirmationCompleteText = response.EnrollmentConfirmCompleteText,
                IsEnrollmentInitiated = response.IsEnrollmentInitiated,
                IsPackageSubmitted = response.IsPackageSubmitted,
                BenefitsPageCustomText = response.BenefitsText,
                BenefitsEnrollmentPageCustomText = response.BenefitsEnrollmentText,
                ManageDepBenPageCustomText = response.ManageDepBenText
            };

            return Task.FromResult(EmployeeBenefitsEnrollmentEligibilityEntity);
        }

        public Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(string employeeId, string enrollmentPeriodId = null, string benefitTypeId = null)
        {
            EmployeeBenefitsEnrollmentInfo enrollmentBenefitsInfo = null;
            var response = enrollmentBenefitsInfoResponses.FirstOrDefault();
            if (response != null)
            {
                enrollmentBenefitsInfo = new EmployeeBenefitsEnrollmentInfo()
                {
                    EmployeeId = employeeId,
                    EnrollmentPeriodId = enrollmentPeriodId,
                    ConfirmationDate = response.ConfirmationDate,
                    BenefitPackageId = response.BenefitsPackageId,
                    OptOutBenefitTypes = response.OptOutBenefitTypes
                };
            }
            return Task.FromResult(enrollmentBenefitsInfo);
        }

        public Task<EmployeeBenefitsEnrollmentPackage> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null)
        {
            EmployeeBenefitsEnrollmentPackage enrollmentPackage = null;
            var enrollmentPackageRecord = employeeEnrollmentPackageRecords.FirstOrDefault(ep => ep.employeeId == employeeId);
            if (enrollmentPackageRecord != null)
            {
                enrollmentPackage = new EmployeeBenefitsEnrollmentPackage(enrollmentPackageRecord.employeeId,
                    enrollmentPackageRecord.packageId)
                {
                    BenefitsEnrollmentPeriodId = enrollmentPackageRecord.enrollmentPeriodId,
                    PackageDescription = enrollmentPackageRecord.packageDescription,
                    EmployeeEligibleBenefitTypes = BuildEmployeeBenefitTypes(enrollmentPackageRecord)
                };
            }
            return Task.FromResult(enrollmentPackage);
        }

        public Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfo employeeBenefitEnrollmentInfoDto)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<EmployeeBenefitType> BuildEmployeeBenefitTypes(EnrollmentPackageRecord enrollmentPackageRecord)
        {
            var types = new List<EmployeeBenefitType>();
            foreach (var benefitType in enrollmentPackageRecord.benefitTypes)
            {
                types.Add(new EmployeeBenefitType(benefitType.benefitType, benefitType.benefitTypeDescription)
                {
                    BenefitTypeSpecialProcessingCode = benefitType.benefitTypeIcon,
                    BenefitsSelectionPageCustomText = benefitType.benefitsSelectionPageCustomText
                });
            }
            return types;
        }

        public Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId)
        {
            throw new NotImplementedException();
        }

        public Task<EmployeeBenefitsEnrollmentPoolItem> AddEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            throw new NotImplementedException();
        }

        public Task<EmployeeBenefitsEnrollmentPoolItem> UpdateEmployeeBenefitsEnrollmentPoolAsync(string employeeId, EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(string benefitTypeId, string enrollmentPeriodId = null, string packageId = null, List<string> enrollmentPeriodBenefitIds = null)
        {
            var benefits = new List<EnrollmentPeriodBenefit>();
            var response = enrollmentPeriodBenefitsResponses
                .FirstOrDefault(r => r.EnrPeriodBenTypeId == benefitTypeId);
            if (response != null)
            {
                foreach (var benefit in response.Benefits)
                {
                    if (benefit != null)
                    {
                        benefits.Add(new EnrollmentPeriodBenefit(benefit.BendedIds, benefit.EnrPeriodBenIds, benefitTypeId)
                        {
                            BenefitDescription = !string.IsNullOrEmpty(benefit.BendedSelfSvsDescs) ?
                                    benefit.BendedSelfSvsDescs : benefit.BendedDescs
                        });
                    }

                }
            }
            return Task.FromResult(benefits.AsEnumerable());
        }

        public Task<bool> CheckDependentExistsAsync(string benefitsEnrollmentPoolId)
        {
            throw new NotImplementedException();
        }


        public Task<BenefitEnrollmentCompletionInfo> SubmitBenefitElectionAsync(string employeeId, string enrollmentPeriodId, string benefitPackageId)
        {
            BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoDTO = new BenefitEnrollmentCompletionInfo(
                benefitEnrollmentCompletionInfoRecord.EmployeeId,
                benefitEnrollmentCompletionInfoRecord.EnrollmentPeriodId,
                benefitEnrollmentCompletionInfoRecord.EnrollmentConfirmationDate,
                benefitEnrollmentCompletionInfoRecord.ErrorMessages.ToList());

            return Task.FromResult(benefitEnrollmentCompletionInfoDTO);

        }

        public Task<BenefitEnrollmentCompletionInfo> ReOpenBenefitElectionsAsync(string employeeId, string enrollmentPeriodId)
        {
            BenefitEnrollmentCompletionInfo benefitEnrollmentCompletionInfoDTO = new BenefitEnrollmentCompletionInfo(
                benefitEnrollmentCompletionInfoRecord.EmployeeId,
                benefitEnrollmentCompletionInfoRecord.EnrollmentPeriodId,
                null,
                null);

            return Task.FromResult(benefitEnrollmentCompletionInfoDTO);
        }

        public Task<IEnumerable<BeneficiaryCategory>> GetBeneficiaryCategoriesAsync()
        {
            var beneficiary = new List<BeneficiaryCategory>();
            var response = beneficiaryResponse.FirstOrDefault(r => r.code != null);
            foreach (var record in beneficiaryResponse)
            {
                beneficiary.Add(new BeneficiaryCategory(record.code, record.description, record.processingCode));
            }
            return Task.FromResult(beneficiary.AsEnumerable());
        }
    }
}
