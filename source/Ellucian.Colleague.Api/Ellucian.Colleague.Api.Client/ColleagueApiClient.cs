// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Client.Core;
using Ellucian.Colleague.Api.Client.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Utility;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client
{
    /// <summary>
    /// Provides a REST HTTP client for use with the Colleague Web API.
    /// </summary>
    public partial class ColleagueApiClient
    {
        private ColleagueServiceClient serviceClient;
        private Ellucian.Rest.Client.ServiceClientUser userContext;
        private ILogger logger;

        public const string ProductNameHeaderKey = "X-ProductName";
        public const string ProductVersionHeaderKey = "X-ProductVersion";
        public const string AcceptHeaderKey = "Accept";
        public const string CredentialsHeaderKey = "X-CustomCredentials";
        public const string StepUpAuthenticationHeaderKey = "X-Step-Up-Authentication";

        private static readonly string _academicCreditsPath = "academic-credits";
        private static readonly string _academicLevelsPath = "academic-levels";
        private static readonly string _academicHistoryPath = "academic-history";
        private static readonly string _academicHistoryLevelPath = "academic-history-levels";
        private static readonly string _academicProgramsPath = "academic-programs";
        private static readonly string _academicCatalogsPath = "academic-catalogs";
        private static readonly string _academicProgressAppealCodesPath = "academic-progress-appeal-codes";
        private static readonly string _academicProgressEvaluationsPath = "academic-progress-evaluations";
        private static readonly string _evaluationsPath = "evaluation";
        private static readonly string _academicProgressStatusesPath = "academic-progress-statuses";
        private static readonly string _academicStandingsPath = "academic-standings";
        private static readonly string _accountActivityPeriodsForStudentPath = "account-activity/admin";
        private static readonly string _accountActivityByTermForStudentPath = "account-activity/term/admin";
        private static readonly string _accountActivityByPeriodForStudentPath = "account-activity/period/admin";
        private static readonly string _addAuthorizationsPath = "add-authorizations";
        private static readonly string _addressPath = "addresses";
        private static readonly string _addressTypesPath = "address-types";
        private static readonly string _advisementsComplete = "completed-advisements";
        private static readonly string _affiliationsPath = "affiliations";
        private static readonly string _paymentsDueByPeriodForStudentPath = "account-due/period/admin";
        private static readonly string _getPaymentsDueByTermForStudentPath = "account-due/term/admin";
        private static readonly string _advisorsPath = "advisors";
        private static readonly string _admittedStatusesPath = "admitted-statuses";
        private static readonly string _agreementPeriodsPath = "agreement-periods";
        private static readonly string _applicantPath = "applicants";
        private static readonly string _applicationStatusesPath = "application-statuses";
        private static readonly string _applicationStatusCategoriesPath = "application-status-categories";
        private static readonly string _applicationInfluencesPath = "application-influences";
        private static readonly string _advisorTypesPath = "advisor-types";
        private static readonly string _attachmentsPath = "attachments";
        private static readonly string _attachmentsCollectionPath = "attachment-collections";
        private static readonly string _attachmentsCollectionEffectivePermissionsPath = "effective-permissions";
        private static readonly string _awardCategoriesPath = "award-categories";
        private static readonly string _awardPackageChangeRequestsPath = "award-package-change-requests";
        private static readonly string _awardPeriodsPath = "award-periods";
        private static readonly string _awardStatusesPath = "award-statuses";
        private static readonly string _awardTypesPath = "award-types";
        private static readonly string _awardsPath = "awards";
        private static readonly string _awardYearsPath = "award-years";
        private static readonly string _approvalsPath = "approvals";
        private static readonly string _averageAwardPackgePath = "average-award-packages";
        private static readonly string _authenticationSchemePath = "authentication-scheme";
        private static readonly string _awardLettersPath = "award-letters";
        private static readonly string _bankingInformationConfigurationPath = "banking-information-configuration";
        private static readonly string _banksPath = "banks";
        private static readonly string _bankAccountsPath = "bank-accounts";
        private static readonly string _awardLetterConfigurationsPath = "award-letter-configurations";
        private static readonly string _bookPath = "books";
        private static readonly string _bookOptionsPath = "book-options";
        private static readonly string _buildingsPath = "buildings";
        private static readonly string _campusCalendarsPath = "campus-calendars";
        private static readonly string _campusOrganization2Path = "campus-organization";
        private static readonly string _capSizesPath = "cap-sizes";
        private static readonly string _careerGoalsPath = "career-goals";
        private static readonly string _ccdsPath = "ccds";
        private static readonly string _citizenTypesPath = "citizen-types";
        private static readonly string _commencementSitesPath = "commencement-sites";
        private static readonly string _communicationCodesPath = "communication-codes";
        private static readonly string _compTimeAccrualPath = "comp-time-accrual";
        private static readonly string _configurationPath = "configuration";
        private static readonly string _contentKeysPath = "content-keys";
        private static readonly string _contractsPath = "contracts";
        private static readonly string _convenienceFeesPath = "convenience-fees";
        private static readonly string _countriesPath = "countries";
        private static readonly string _courseLevelsPath = "course-levels";
        private static readonly string _courseTypesPath = "course-types";
        private static readonly string _topicCodesPath = "topic-codes";
        private static readonly string _classLevelsPath = "class-levels";
        private static readonly string _correspondenceRequestsPath = "correspondence-requests";
        private static readonly string _costCentersPath = "cost-centers";
        private static readonly string _courseCatalogPath = "course-catalog";
        private static readonly string _coursesPath = "courses";
        private static readonly string _coursesSearchPath = "courses/search";
        private static readonly string _creditTypesPath = "credit-types";
        private static readonly string _degreesPath = "degrees";
        private static readonly string _degreePlansPath = "degree-plans";
        private static readonly string _degreePlanArchivesPath = "degree-plan-archives";
        private static readonly string _degreeTypesPath = "degree-types";
        private static readonly string _denominationsPath = "denominations";
        private static readonly string _departmentsPath = "departments";
        private static readonly string _distributionsPath = "payment/distributions";
        private static readonly string _depositsPath = "deposits";
        private static readonly string _directDepositsPath = "direct-deposits";
        private static readonly string _documentsPath = "documents";
        private static readonly string _earningsTypesPath = "earnings-types";
        private static readonly string _earningsTypeGroupsPath = "earnings-type-groups";
        private static readonly string _ecommercePath = "ecommerce";
        private static readonly string _educationHistoryPath = "education-history";
        private static readonly string _employeesPath = "employees";
        private static readonly string _employeeLeavePlansPath = "employee-leave-plans";
        private static readonly string _employeeCompensationPath = "employee-compensation";
        private static readonly string _employeeSummaryPath = "employee-summary";
        private static readonly string _employeeTimeSummaryPath = "employee-time-summary";
        private static readonly string _leaveConfigurationPath = "leave-plans-configuration";
        private static readonly string _employeeCurrentBenefitsPath = "employee-current-benefits";
        private static readonly string _emergencyInformationPath = "emergency-information";
        private static readonly string _disabilityTypesPath = "disability-types";
        private static readonly string _externalTranscriptStatusesPath = "external-transcript-statuses";
        private static readonly string _ethnicitiesPath = "ethnicities";
        private static readonly string _facultyPath = "faculty";
        private static readonly string _officeHoursPath = "office-hours";
        private static readonly string _facultyGradingPath = "faculty-grading";
        private static readonly string _facultyIdsPath = "query-faculty-ids";
        private static readonly string _studentProfilePath = "student-profile";
        private static readonly string _fafsaPath = "fafsa";
        private static readonly string _fafsasPath = "fafsas";
        private static readonly string _faLinkBookSpendingPath = "book-spending";
        private static readonly string _federalCourseClassificationsPath = "federal-course-classifications";
        private static readonly string _financialAidApplicationsPath = "financial-aid-applications";
        private static readonly string _financialAidBudgetComponentsPath = "financial-aid-budget-components";
        private static readonly string _financialAidChecklistItemsPath = "financial-aid-checklist-items";
        private static readonly string _financialAidChecklistPath = "financial-aid-checklists";
        private static readonly string _financialAidCounselorsPath = "financial-aid-counselors";
        private static readonly string _financialAidExplanationsPath = "financial-aid-explanations";
        private static readonly string _financialAidOfficesPath = "financial-aid-offices";
        private static readonly string _financialAidPersonsPath = "financial-aid-persons";
        private static readonly string _fixedAssetTransferFlagsPath = "fixed-asset-transfer-flags";
        private static readonly string _frequencyCodesPath = "frequency-codes";
        private static readonly string _genderIdentityTypesPath = "gender-identity-types";
        private static readonly string _gownSizesPath = "gown-sizes";
        private static readonly string _gradesPath = "grades";
        private static readonly string _gradeSchemesPath = "grade-schemes";
        private static readonly string _gradeSubschemesPath = "grade-subschemes";        
        private static readonly string _graduationApplicationPath = "graduation-application";
        private static readonly string _graduationApplicationsPath = "graduation-applications";
        private static readonly string _graduationApplicationFeesPath = "graduation-application-fees";
        private static readonly string _graduationApplicationEligibilityPath = "graduation-application-eligibility";
        private static readonly string _graduationConfigurationPath = "configuration/student-graduation";
        private static readonly string _holdRequestTypesPath = "hold-request-types";
        private static readonly string _humanResourceDemographicsPath = "human-resources";
        private static readonly string _iCalPath = "section-events-ical";
        private static readonly string _importantNumbersPath = "important-numbers";
        private static readonly string _institutionsPath = "institutions";
        private static readonly string _institutionTypesPath = "institution-types";
        private static readonly string _instructionalMethodsPath = "instructional-methods";
        private static readonly string _studentEnrollmentKeysPath = "invalid-student-enrollments";
        private static readonly string _ipedsInstitutionsPath = "ipeds-institutions";
        private static readonly string _localCourseClassificationsPath = "local-course-classifications";
        private static readonly string _interestsPath = "interests";
        private static readonly string _languagesPath = "languages";
        private static readonly string _linksPath = "financial-aid-links";
        private static readonly string _loadPeriodsPath = "load-periods";
        private static readonly string _loanLimitsPath = "loan-limits";
        private static readonly string _loanRequestsPath = "loan-requests";
        private static readonly string _loanSummaryPath = "loan-summary";
        private static readonly string _locationsPath = "locations";
        private static readonly string _majorsPath = "majors";
        private static readonly string _maritalStatusesPath = "marital-statuses";
        private static readonly string _minorsPath = "minors";
        private static readonly string _nonAcademicAttendancesPath = "nonacademic-attendances";
        private static readonly string _nonAcademicAttendanceEventTypesPath = "nonacademic-attendance-event-types";
        private static readonly string _nonAcademicAttendanceRequirementsPath = "nonacademic-attendance-requirements";
        private static readonly string _nonacademicEventsPath = "nonacademic-events";
        private static readonly string _officeCodesPath = "office-codes";
        private static readonly string _overtimePath = "overtime";
        private static readonly string _overtimeCompTimeThresholdAllocationpath = "overtime-comp-time-threshold-allocation";
        private static readonly string _overtimeCalculationDefinitionsPath = "overtime-calculation-definitions";
        private static readonly string _passwordResetTokenRequestPath = "password-reset-token-request";
        private static readonly string _confirmStudentPaymentPath = "payment/confirm";
        private static readonly string _electronicCheckPaymentPath = "payment/echeck";
        private static readonly string _electronicCheckPayerPath = "payment/echeck/payer";
        private static readonly string _processStudentPaymentPath = "payment/process";
        private static readonly string _cashReceiptPath = "payment/receipt";
        private static readonly string _payCyclesPath = "pay-cycles";
        private static readonly string _payableDepositsPath = "payable-deposits";
        private static readonly string _payableDepositDirectivesPath = "payable-deposit-directives";
        private static readonly string _payrollDepositDirectivesPath = "payroll-deposit-directives";
        private static readonly string _payStatementsPath = "pay-statements";
        private static readonly string _payStatementConfigurationPath = "pay-statement-configuration";
        private static readonly string _personalPronounTypesPath = "personal-pronoun-types";
        private static readonly string _personPhotoPath = "photos/people";
        private static readonly string _personAgreementsPath = "person-agreements";
        private static readonly string _personPositionsPath = "person-positions";
        private static readonly string _personPositionWagesPath = "person-position-wages";
        private static readonly string _personStipendPath = "person-stipend";
        private static readonly string _personEmploymentStatusesPath = "person-employment-statuses";
        private static readonly string _phoneNumberPath = "phone-numbers";
        private static readonly string _positionsPath = "positions";
        private static readonly string _prefixesPath = "prefixes";
        private static readonly string _profileApplicationsPath = "profile-applications";
        private static readonly string _programsPath = "programs";
        private static readonly string _prospectSourcesPath = "prospect-sources";
        private static readonly string _quickRegistrationSectionsPath = "quick-registration-sections";
        private static readonly string _racesPath = "races";
        private static readonly string _receivablesPath = "receivables";
        private static readonly string _receivableInvoicesPath = "receivable-invoices";
        private static readonly string _recoverUserIdPath = "recover-user-id";
        private static readonly string _recruiterApplicationStatusesPath = "recruiter-application-statuses";
        private static readonly string _recruiterApplicationsPath = "recruiter-applications";
        private static readonly string _recruiterCommunicationHistoryPath = "recruiter-communication-history";
        private static readonly string _recruiterCommunicationHistoryRequestPath = "recruiter-communication-history-request";
        private static readonly string _recruiterConnectionStatusPath = "recruiter-connection-status";
        private static readonly string _recruiterTestScoresPath = "recruiter-test-scores";
        private static readonly string _recruiterTranscriptCoursesPath = "recruiter-transcript-courses";
        private static readonly string _registrationPath = "registration";
        private static readonly string _relatedPersonsPath = "related-persons";
        private static readonly string _relationshipsPath = "relationships";
        private static readonly string _relationshipTypesPath = "relationship-types";
        private static readonly string _requirementsPath = "requirements";
        private static readonly string _resetPasswordPath = "reset-password";
        private static readonly string _restrictionTypesPath = "restriction-types";
        private static readonly string _restrictionConfigurationPath = "restriction";
        private static readonly string _rolesPath = "roles";
        private static readonly string _roomsPath = "rooms";
        private static readonly string _paymentControlsPath = "payment-controls";
        private static readonly string _paymentPlansPath = "payment-plans";
        private static readonly string _paymentPlansProposedPath = "payment-plans/proposed-plan";
        private static readonly string _petitionStatusesPath = "petition-statuses";
        private static readonly string _planningPath = "planning";
        private static readonly string _requiredDocument = "required-document";
        private static readonly string _schoolsPath = "schools";
        private static readonly string _sectionAttendancesPath = "section-attendances";
        private static readonly string _sectionsPath = "sections";
        private static readonly string _sectionMeetingInstancesPath = "section-meeting-instances";
        private static readonly string _sectionTextbooksPath = "section-textbooks";
        private static readonly string _sectionTransferStatusesPath = "section-transfer-statuses";
        private static readonly string _selfServicePath = "self-service";
        private static readonly string _selfServicePreferencesPath = "self-service-preferences";
        private static readonly string _sessionPath = "session";
        private static readonly string _sessionCyclesPath = "session-cycles";
        private static readonly string _shipToCodesPath = "ship-to-codes";
        private static readonly string _commodityCodesPath = "commodity-codes";
        private static readonly string _commodityUnitTypesPath = "commodity-unit-types";
        private static readonly string _specializationsPath = "specializations";
        private static readonly string _studentAwardYearsPath = "award-years";
        private static readonly string _studentDefaultAwardPeriodsPath = "default-award-periods";
        private static readonly string _staffPath = "staff";
        private static readonly string _accountHoldersPath = "account-holders";
        private static readonly string _statementsPath = "statement";
        private static readonly string _statesPath = "states";
        private static readonly string _studentAttendancesPath = "student-attendances";
        private static readonly string _studentSectionAttendancesPath = "student-section-attendances";
        private static readonly string _studentAwardSummaryPath = "student-award-summary";
        private static readonly string _studentAffiliationsPath = "student-affiliations";
        private static readonly string _studentAwardDisbursementsInfoPath = "disbursements";
        private static readonly string _studentLoadsPath = "student-loads";
        private static readonly string _studentsPath = "students";
        private static readonly string _studentEnrollmentRequests = "student-enrollment-requests";
        private static readonly string _studentIdsPath = "query-student-ids";
        private static readonly string _studentNsldsInformationPath = "nslds-information";
        private static readonly string _studentOutsideAwardsPath = "outside-awards";
        private static readonly string _studentPetitionsPath = "student-petitions";
        private static readonly string _studentPetitionReasonsPath = "student-petition-reasons";
        private static readonly string _studentProgramsPath = "student-programs";
        private static readonly string _studentRequestPath = "student-request";
        private static readonly string _studentRequestFeesPath = "student-request-fees";
        private static readonly string _studentRestrictionsPath = "student-restrictions";
        private static readonly string _studentShoppingSheetsPath = "shopping-sheets";
        private static readonly string _studentStandingsPath = "student-standings";
        private static readonly string _studentTermsPath = "student-terms";
        private static readonly string _studentTranscriptRequests = "student-transcript-requests";
        private static readonly string _studentTermsGpaPath = "student-terms-gpa";
        private static readonly string _studentTypesPath = "student-types";
        private static readonly string _studentWaiverReasonsPath = "student-waiver-reasons";
        private static readonly string _subjectsPath = "subjects";
        private static readonly string _suffixesPath = "suffixes";
        private static readonly string _termsPath = "terms";
        private static readonly string _noncourseStatusesPath = "noncourse-statuses";
        private static readonly string _testsPath = "tests";
        private static readonly string _testResultsPath = "test-results";
        private static readonly string _timecardsPath = "timecards";
        private static readonly string _timecardHistoriesPath = "timecard-histories";
        private static readonly string _timecardStatusesPath = "timecard-statuses";
        private static readonly string _timeEntryCommentsPath = "time-entry-comments";
        private static readonly string _timeHistoryCommentsPath = "time-history-comments";
        private static readonly string _timeManagementConfigurationPath = "time-management-configuration";
        private static readonly string _transcriptCategoriesPath = "transcript-categories";
        private static readonly string _transcriptGroupingsPath = "transcript-groupings";
        private static readonly string _usersPath = "users";
        private static readonly string _validateCompTime = "validate-comp-time";
        private static readonly string _versionPath = "version";
        private static readonly string _visaTypesPath = "visa-types";
        private static readonly string _qapiPath = "qapi";
        private static readonly string _projectsPath = "projects";
        private static readonly string _adviseesPath = "advisees";
        private static readonly string _projectTypesPath = "project-types";
        private static readonly string _projectItemCodesPath = "project-item-codes";
        private static readonly string _accountsPayableTaxCodesPath = "accounts-payable-taxes";
        private static readonly string _accountsPayableTypeCodesPath = "accounts-payable-types";
        private static readonly string _vouchersPath = "vouchers";
        private static readonly string _purchaseOrdersPath = "purchase-orders";
        private static readonly string _purchaseOrdersSummaryPath = "purchase-orders-summary";
        private static readonly string _blanketPurchaseOrdersPath = "blanket-purchase-orders";
        private static readonly string _requisitionsPath = "requisitions";
        private static readonly string _journalEntriesPath = "journal-entries";
        private static readonly string _recurringVouchersPath = "recurring-vouchers";
        private static readonly string _personsPath = "persons";
        private static readonly string _taxFormConsentsPath = "tax-form-consents";
        private static readonly string _taxFormsPath = "tax-forms";
        private static readonly string _taxFormStatementsPath = "tax-form-statements";
        private static readonly string _phoneTypesPath = "phone-types";
        private static readonly string _emailTypesPath = "email-types";
        private static readonly string _taxFormW2PdfPath = "formW2s";
        private static readonly string _taxFormW2cPdfPath = "formW2cs";
        private static readonly string _taxFormT4PdfPath = "formT4s";
        private static readonly string _taxFormT4aPdfPath = "formT4as";
        private static readonly string _taxForm1095cPdfPath = "form1095cs";
        private static readonly string _taxForm1098tPdfPath = "form1098ts";
        private static readonly string _taxFormT2202aPdfPath = "formT2202as";
        private static readonly string _proxySubjects = "proxy-subjects";
        private static readonly string _proxyCandidatesPath = "proxy-candidates";
        private static readonly string _proxyUsersPath = "proxy-users";
        private static readonly string _yearlyCyclesPath = "yearly-cycles";
        private static readonly string _fiscalYearsPath = "fiscal-years";
        private static readonly string _todaysFiscalYearPath = "fiscal-years/today";
        private static readonly string _generalLedgerActivityDetailsPath = "general-ledger-activity-details";
        private static readonly string _generalLedgerConfigurationPath = "configuration/general-ledger";
        private static readonly string _budgetAdjustmentConfigurationPath = "configuration/budget-adjustment-validation";
        private static readonly string _budgetAdjustmentEnabledPath = "configuration/budget-adjustment-enabled";
        private static readonly string _glAccountValidationPath = "general-ledger-account-validation";
        private static readonly string _budgetAdjustmentsPath = "budget-adjustments";
        private static readonly string _budgetAdjustmentsPendingApprovalDetailPath = "budget-adjustments-pending-approval-detail";
        private static readonly string _draftBudgetAdjustmentsPath = "draft-budget-adjustments";
        private static readonly string _budgetAdjustmentsSummaryPath = "budget-adjustments-summary";
        private static readonly string _nextApproversPath = "next-approvers";
        private static readonly string _budgetAdjustmentsPendingApprovalPath = "budget-adjustments-pending-approval-summary";
        private static readonly string _workTasksPath = "work-tasks";
        private static readonly string _messagePath = "message";
        private static readonly string _textDocumentsPath = "text-documents";
        private static readonly string _generalLedgerAccountsPath = "general-ledger-accounts";
        private static readonly string _generalLedgerObjectCodesPath = "general-ledger-object-codes";
        private static readonly string _organizationalRelationshipsPath = "organizational-relationships";
        private static readonly string _organizationalPersonPositionsPath = "organizational-person-positions";
        private static readonly string _organizationalPositionsPath = "organizational-positions";
        private static readonly string _organizationalPositionRelationshipsPath = "organizational-position-relationships";
        private static readonly string _attendanceCategoriesPath = "attendance-categories";
        private static readonly string _backupApiConfigurationPath = "backup-api-config";
        private static readonly string _restoreApiConfigurationPath = "restore-api-config";
        private static readonly string _dropReasonsPath = "drop-reasons";
        private static readonly string _taxForm1099MiPdfPath = "form1099Miscs";
        private static readonly string _taxFormCodesPath = "tax-form-codes";
        private static readonly string _financeQueryPath = "finance-query";
        private static readonly string _glFiscalYearConfigurationPath = "configuration/gl-fiscal-year-configuration";
        private static readonly string _budgetDevelopmentConfigurationPath = "configuration/budget-development";
        private static readonly string _budgetDevelopmentWorkingBudgetPath = "budget-development/working-budget";
        private static readonly string _waitlistInfoPath = "waitlist-info";
        private static readonly string _waitlistStatusesPath = "waitlist-statuses"; 
        private static readonly string _workingBudgetPath = "working-budget";
        private static readonly string _budgetOfficersPath = "budget-officers";
        private static readonly string _budgetReportingUnitsPath = "budget-reporting-units";
        private static readonly string _requisitionsSummaryPath = "requisitions-summary";        
        private static readonly string _cfWebConfigurationsPath = "cf-web-configurations";
        private static readonly string _vendorsPath = "vendors";
        private static readonly string _requisitionModifyPath = "requisitions-modify";
        private static readonly string _taxFormBoxCodesPath = "tax-form-boxcodes";

        private static readonly string _privacyStatusesPath = "privacy-statuses";
        private static readonly string _privacyMessagesPath = "privacy-messages";
        private static readonly string _privacyPath = "privacy";
        private static readonly string _mediaTypeHeaderVersion1 = "application/vnd.ellucian.v1+json";
        private static readonly string _mediaTypeHeaderVersion2 = "application/vnd.ellucian.v2+json";
        private static readonly string _mediaTypeHeaderVersion3 = "application/vnd.ellucian.v3+json";
        private static readonly string _mediaTypeHeaderVersion4 = "application/vnd.ellucian.v4+json";
        private static readonly string _mediaTypeHeaderVersion5 = "application/vnd.ellucian.v5+json";
        private static readonly string _mediaTypeHeaderVersion6 = "application/vnd.ellucian.v6+json";
        private static readonly string _mediaTypeHeaderPdfVerion1 = "application/vnd.ellucian.v1+pdf";
        private static readonly string _mediaTypeHeaderPlanningVersion1 = "application/vnd.ellucian-planning-student.v1+json";
        private static readonly string _mediaTypeHeaderPersonProfileVersion1 = "application/vnd.ellucian-person-profile.v1+json";
        private static readonly string _mediaTypeHeaderPersonProfileVersion2 = "application/vnd.ellucian-person-profile.v2+json";
        private static readonly string _mediaTypeHeaderProxyUserVersion1 = "application/vnd.ellucian-proxy-user.v1+json";
        private static readonly string _mediaTypeHeaderPersonNameSearchVersion1 = "application/vnd.ellucian-person-name-search.v1+json";
        private static readonly string _mediaTypeHeaderEmployeeNameSearchVersion1 = "application/vnd.ellucian-employee-name-search.v1+json";
        private static readonly string _hedtechIntegrationMediaTypeFormatVersion6 = "application/vnd.hedtech.integration.v6+json";
        private static readonly string _mediaTypeHeaderPilotVersion1 = "application/vnd.ellucian-pilot.v1+json";
        private static readonly string _mediaTypeHeaderInvoicePaymentVersion1 = "application/vnd.ellucian-invoice-payment.v1+json";
        private static readonly string _mediaTypeHeaderIlpVersion1 = "application/vnd.ellucian-ilp.v1+json";
        private static readonly string _mediaTypeHeaderHumanResourceDemographics = "application/vnd.ellucian-human-resource-demographics.v1+json";
        private static readonly string _mediaTypeHeaderHumanResourceDemographicsVersion2 = "application/vnd.ellucian-human-resource-demographics.v2+json";
        private static readonly string _mediaTypeStepUpAuthenticationVersion1 = "application/vnd.ellucian-step-up-authentication.v1+json";
        private static readonly string _mediaTypeHeaderStudentFinanceDisbursementsVersion1 = "application/vnd.ellucian-student-finance-disbursements.v1+json";
        private static readonly string _mediaTypeHeaderFALinkBookSpendingVersion1 = "application/vnd.ellucian-falink-book-spending.v1+json";
        private static readonly string _mediaTypeEllucianConfigurationVersion1 = "application/vnd.ellucian-configuration.v1+json";
        private static readonly string _mediaTypeEllucianInvalidKeysFormatVersion1 = "application/vnd.ellucian-with-invalid-keys.v1+json";
        private static readonly string _mediaTypeEllucianPersonSearchExactMatchFormat = "application/vnd.ellucian-person-search-exact-match.v1+json";


        /// <summary>
        /// Creates a new ColleagueApiClient.
        /// </summary>
        /// <param name="baseUrl">Colleague Web API base URL (ending in /ColleagueApi)</param>
        /// <param name="logger">logging instance</param>
        public ColleagueApiClient(string baseUrl, ILogger logger)
            : this(baseUrl, 2, logger)
        {
        }

        /// <summary>
        /// Creates a new ColleagueApiClient specifying the maximum number of connections 
        /// that can be made to the Colleague Web API.
        /// </summary>
        /// <param name="baseUrl">Colleague Web API base URL (ending in /ColleagueApi)</param>
        /// <param name="maxConnections">Maximum number of concurrent connections that can be made by this client.</param>
        /// <param name="logger">logging instance</param>
        public ColleagueApiClient(string baseUrl, int maxConnections, ILogger logger)
        {
            serviceClient = ColleagueServiceClientManager.Instance.GetColleagueServiceClient(baseUrl, maxConnections, logger);
            this.logger = logger;
        }

        /// <summary>
        /// Unit Testing Constructor
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="logger">logging mechanism</param>
        public ColleagueApiClient(HttpClient httpClient, ILogger logger)
        {
            serviceClient = new ColleagueServiceClient(httpClient, logger);
            this.logger = logger;
        }

        /// <summary>
        /// Gets or Sets the Colleague Web API credentials (Colleague Web API Token).
        /// </summary>
        public string Credentials
        {
            get
            {
                if (this.userContext != null)
                {
                    return this.userContext.CustomCredentials;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.userContext != null)
                {
                    this.userContext.CustomCredentials = value;
                }
                else
                {
                    this.userContext = new Ellucian.Rest.Client.ServiceClientUser() { CustomCredentials = value };
                }
            }
        }

        /// <summary>
        /// Gets or sets the duration before an API request times out.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The timeout specified is less than or equal to zero.</exception>
        public TimeSpan Timeout
        {
            get
            {
                return serviceClient.Timeout;
            }
            set
            {
                serviceClient.Timeout = value;
            }
        }

        /// <summary>
        /// Get the version of the API
        /// </summary>
        /// <returns>Returns the API Version</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public ApiVersion GetVersion()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_versionPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ApiVersion>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get ApiVersion");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ApiVersion");
                throw;
            }
        }

        /// <summary>
        /// Get the version of the API asynchronously
        /// </summary>
        /// <returns>Returns the API Version</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<ApiVersion> GetVersionAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_versionPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<ApiVersion>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get ApiVersion");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get ApiVersion");
                throw;
            }
        }

        /// <summary>
        /// POSTs a login request with the specified credentials. An exception is thrown if the login fails.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="productName">optional name of product using the client</param>
        /// <param name="productVersion">optional version of the product using the client</param>
        /// <returns>a JSON Web Token string</returns>
        public string Login(string userId, string password, string productName = null, string productVersion = null)
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                headers.Add(ProductNameHeaderKey, productName);
                headers.Add(ProductVersionHeaderKey, productVersion);
            }

            // do not log the request body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                var loginResult = ExecutePostRequestWithResponse<Credentials>(new Credentials() { UserId = userId, Password = password }, UrlUtility.CombineUrlPath(_sessionPath, "login"), headers: headers);
                if (loginResult.IsSuccessStatusCode)
                {
                    string token = loginResult.Content.ReadAsStringAsync().Result;
                    Credentials = token;
                    return token;
                }
            }
            catch (LoginException liex)
            {
                logger.Debug(liex.Message);
                throw;
            }
            catch (HttpRequestFailedException hrfe)
            {
                logger.Error(hrfe.Message);
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new PasswordExpiredException(hrfe.Message);
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// POSTs a login request with the specified credentials asynchronously. An exception is thrown if the login fails.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="productName">optional name of product using the client</param>
        /// <param name="productVersion">optional version of the product using the client</param>
        /// <returns>a JSON Web Token string</returns>
        [Obsolete("Obsolete as of API 1.12. Use Login2Async.")]
        public async Task<string> LoginAsync(string userId, string password, string productName = null, string productVersion = null)
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                headers.Add(ProductNameHeaderKey, productName);
                headers.Add(ProductVersionHeaderKey, productVersion);
            }

            // do not log the request body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                var loginResult = await ExecutePostRequestWithResponseAsync<Credentials>(new Credentials() { UserId = userId, Password = password }, UrlUtility.CombineUrlPath(_sessionPath, "login"), headers: headers);
                if (loginResult.IsSuccessStatusCode)
                {
                    string token = await loginResult.Content.ReadAsStringAsync();
                    Credentials = token;
                    return token;
                }
            }
            catch (LoginException liex)
            {
                logger.Debug(liex.Message);
                throw;
            }
            catch (HttpRequestFailedException hrfe)
            {
                logger.Error(hrfe.Message);
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new PasswordExpiredException(hrfe.Message);
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// POSTs a login request with the specified credentials asynchronously. An exception is thrown if the login fails.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="productName">optional name of product using the client</param>
        /// <param name="productVersion">optional version of the product using the client</param>
        /// <returns>a JSON Web Token string or one of the following exceptions:
        /// LoginException : invalid credentials specified; 
        /// PasswordExpiredException : user's password has expired and needs to be reset; 
        /// ListenerNotFoundException: listener is nonresponsive, system is unavailable
        /// </returns>
        public async Task<string> Login2Async(string userId, string password, string productName = null, string productVersion = null)
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);
            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                headers.Add(ProductNameHeaderKey, productName);
                headers.Add(ProductVersionHeaderKey, productVersion);
            }

            // do not log the request body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                var loginResult = await ExecutePostRequestWithResponseAsync<Credentials>(new Credentials() { UserId = userId, Password = password }, UrlUtility.CombineUrlPath(_sessionPath, "login"), headers: headers);
                if (loginResult.IsSuccessStatusCode)
                {
                    string token = await loginResult.Content.ReadAsStringAsync();
                    Credentials = token;
                    return token;
                }
            }
            catch (LoginException liex)
            {
                if (liex.Message.Contains("10014"))
                {
                    throw new LoginDisabledException(liex.Message);
                }
                else
                {
                    logger.Debug(liex.Message);
                    throw;
                }
            }
            catch (HttpRequestFailedException hrfe)
            {
                logger.Error(hrfe.Message);
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new PasswordExpiredException(hrfe.Message);
                }
                else
                {
                    throw;
                }
            }
            catch (ResourceNotFoundException rnfex)
            {
                logger.Error(rnfex.Message);
                throw new ListenerNotFoundException(rnfex.Message);
            }
            catch
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// POSTs a proxy login request with the proxy credentials. An exception is thrown if the login fails.
        /// </summary>
        /// <param name="proxyId">The proxy ID.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="productName">optional name of product using the client</param>
        /// <param name="productVersion">optional version of the product using the client</param>
        /// <returns>a JSON Web Token string</returns>
        public string ProxyLogin(string proxyId, string proxyPassword, string userId, string productName = null, string productVersion = null)
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                headers.Add(ProductNameHeaderKey, productName);
                headers.Add(ProductVersionHeaderKey, productVersion);

            }

            // do not log the request body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                var loginResult = ExecutePostRequestWithResponse<ProxyCredentials>(
                    new ProxyCredentials() { ProxyId = proxyId, ProxyPassword = proxyPassword, UserId = userId },
                    UrlUtility.CombineUrlPath(_sessionPath, "proxy-login"), headers: headers);
                if (loginResult.IsSuccessStatusCode)
                {
                    string token = loginResult.Content.ReadAsStringAsync().Result;
                    Credentials = token;
                    return token;
                }
            }
            catch (LoginException liex)
            {
                logger.Debug(liex.Message);
                throw;
            }
            catch (HttpRequestFailedException hrfe)
            {
                logger.Error(hrfe.Message);
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new PasswordExpiredException(hrfe.Message);
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// POSTs a proxy login request with the proxy credentials asynchronously. An exception is thrown if the login fails.
        /// </summary>
        /// <param name="proxyId">The proxy ID.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="productName">optional name of product using the client</param>
        /// <param name="productVersion">optional version of the product using the client</param>
        /// <returns>a JSON Web Token string</returns>
        [Obsolete("Obsolete as of API 1.12. Use ProxyLogin2Async.")]
        public async Task<string> ProxyLoginAsync(string proxyId, string proxyPassword, string userId, string productName = null, string productVersion = null)
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                headers.Add(ProductNameHeaderKey, productName);
                headers.Add(ProductVersionHeaderKey, productVersion);

            }

            // do not log the request body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                var loginResult = await ExecutePostRequestWithResponseAsync<ProxyCredentials>(
                    new ProxyCredentials() { ProxyId = proxyId, ProxyPassword = proxyPassword, UserId = userId },
                    UrlUtility.CombineUrlPath(_sessionPath, "proxy-login"), headers: headers);
                if (loginResult.IsSuccessStatusCode)
                {
                    string token = await loginResult.Content.ReadAsStringAsync();
                    Credentials = token;
                    return token;
                }
            }
            catch (LoginException liex)
            {
                logger.Debug(liex.Message);
                throw;
            }
            catch (HttpRequestFailedException hrfe)
            {
                logger.Error(hrfe.Message);
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new PasswordExpiredException(hrfe.Message);
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// POSTs a proxy login request with the proxy credentials asynchronously. An exception is thrown if the login fails.
        /// </summary>
        /// <param name="proxyId">The proxy ID.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="productName">optional name of product using the client</param>
        /// <param name="productVersion">optional version of the product using the client</param>
        /// <returns>a JSON Web Token string  or one of the following exceptions:
        /// LoginException : invalid credentials specified; 
        /// PasswordExpiredException : user's password has expired and needs to be reset; 
        /// ListenerNotFoundException: listener is nonresponsive, system is unavailable
        /// </returns>
        public async Task<string> ProxyLogin2Async(string proxyId, string proxyPassword, string userId, string productName = null, string productVersion = null)
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion2);

            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(productVersion))
            {
                headers.Add(ProductNameHeaderKey, productName);
                headers.Add(ProductVersionHeaderKey, productVersion);

            }

            // do not log the request body
            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                var loginResult = await ExecutePostRequestWithResponseAsync<ProxyCredentials>(
                    new ProxyCredentials() { ProxyId = proxyId, ProxyPassword = proxyPassword, UserId = userId },
                    UrlUtility.CombineUrlPath(_sessionPath, "proxy-login"), headers: headers);
                if (loginResult.IsSuccessStatusCode)
                {
                    string token = await loginResult.Content.ReadAsStringAsync();
                    Credentials = token;
                    return token;
                }
            }
            catch (LoginException liex)
            {
                logger.Debug(liex.Message);
                throw;
            }
            catch (HttpRequestFailedException hrfe)
            {
                logger.Error(hrfe.Message);
                if (hrfe.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new PasswordExpiredException(hrfe.Message);
                }
                else if (hrfe.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new ListenerNotFoundException(hrfe.Message);
                }
                else
                {
                    throw;
                }
            }
            catch (ResourceNotFoundException rnfex)
            {
                logger.Error(rnfex.Message);
                throw new ListenerNotFoundException(rnfex.Message);
            }
            catch
            {
                throw;
            }

            return null;
        }

        /// <summary>
        /// POSTs a logout request with the specified JSON Web Token. 
        /// </summary>
        /// <param name="token">JSON web token string</param>
        public void Logout(string token)
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                if (string.IsNullOrEmpty(Credentials) && !string.IsNullOrEmpty(token))
                {
                    Credentials = token;
                }
                ExecutePostRequestWithResponse<string>("", UrlUtility.CombineUrlPath(_sessionPath, "logout"), headers: headers);
            }
            catch
            {
                // Ignore
            }
            finally
            {
                Credentials = string.Empty;
            }
        }

        /// <summary>
        /// POSTs a logout request with the specified JSON Web Token asynchronously. 
        /// </summary>
        /// <param name="token">JSON web token string</param>
        public async Task LogoutAsync(string token)
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                if (string.IsNullOrEmpty(Credentials) && !string.IsNullOrEmpty(token))
                {
                    Credentials = token;
                }
                await ExecutePostRequestWithResponseAsync<string>("", UrlUtility.CombineUrlPath(_sessionPath, "logout"), headers: headers);
            }
            catch
            {
                // Ignore
            }
            finally
            {
                Credentials = string.Empty;
            }
        }

        /// <summary>
        /// Get a JSON web token back in exchange for an existing Colleague session.
        /// </summary>
        /// <param name="colleagueSecurityToken"></param>
        /// <param name="colleagueControlId"></param>
        /// <returns>Returns the token string</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public string GetToken(string colleagueSecurityToken, string colleagueControlId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sessionPath, "token");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var lcs = new LegacyColleagueSession() { SecurityToken = colleagueSecurityToken, ControlId = colleagueControlId };
                var response = ExecutePostRequestWithResponse<LegacyColleagueSession>(lcs, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get string");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get string");
                throw;
            }
        }

        /// <summary>
        /// Get a JSON web token back in exchange for an existing Colleague session asynchronously.
        /// </summary>
        /// <param name="colleagueSecurityToken"></param>
        /// <param name="colleagueControlId"></param>
        /// <returns>Returns the token string</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<string> GetTokenAsync(string colleagueSecurityToken, string colleagueControlId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_sessionPath, "token");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var lcs = new LegacyColleagueSession() { SecurityToken = colleagueSecurityToken, ControlId = colleagueControlId };
                var response = await ExecutePostRequestWithResponseAsync<LegacyColleagueSession>(lcs, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get string");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get string");
                throw;
            }
        }

        /// <summary>
        /// Changes the password of the specified user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        public void ChangePassword(string userId, string oldPassword, string newPassword)
        {
            var request = new ChangePassword();
            request.UserId = userId;
            request.OldPassword = oldPassword;
            request.NewPassword = newPassword;
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                HttpResponseMessage changePasswordResult = ExecutePostRequestWithResponse<ChangePassword>(request, UrlUtility.CombineUrlPath(_sessionPath, "change-password"), headers: headers);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Changes the password of the specified user asynchronously.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var request = new ChangePassword();
            request.UserId = userId;
            request.OldPassword = oldPassword;
            request.NewPassword = newPassword;
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            AddLoggingRestrictions(ref headers, LoggingRestrictions.DoNotLogRequestContent);

            try
            {
                HttpResponseMessage changePasswordResult = await ExecutePostRequestWithResponseAsync<ChangePassword>(request, UrlUtility.CombineUrlPath(_sessionPath, "change-password"), headers: headers);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get all roles.
        /// </summary>
        /// <returns>Returns all the roles</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<Role> GetRoles()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_rolesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Role>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Role>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Role>");
                throw;
            }
        }

        /// <summary>
        /// Get all roles asynchronously.
        /// </summary>
        /// <returns>Returns all the roles</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_rolesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<Role>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<Role>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<Role>");
                throw;
            }
        }

        /// <summary>
        /// Get users matching the specified partial login username text.
        /// </summary>
        /// <returns>Returns users</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public IEnumerable<User> GetUsers(string partialLogin)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = ExecuteGetRequestWithResponse(urlPath, "q=" + partialLogin, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<User>>(response.Content.ReadAsStringAsync().Result);
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<User>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<User>");
                throw;
            }
        }

        /// <summary>
        /// Get users matching the specified partial login username text asynchronously.
        /// </summary>
        /// <returns>Returns users</returns>
        /// <exception cref="ResourceNotFoundException">The requested resource cannot be found.</exception>
        public async Task<IEnumerable<User>> GetUsersAsync(string partialLogin)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, "q=" + partialLogin, headers: headers);
                var resource = JsonConvert.DeserializeObject<IEnumerable<User>>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (ResourceNotFoundException rnfe)
            {
                logger.Error(rnfe, "Unable to get IEnumerable<User>");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get IEnumerable<User>");
                throw;
            }
        }

        /// <summary>
        /// Post changes to a user's proxy permissions
        /// </summary>
        /// <param name="assignment">The proxy permissions being changed</param>
        /// <returns>A collection of proxy access permissions</returns>
        public async Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsAsync(ProxyPermissionAssignment assignment)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, assignment.ProxySubjectId, "proxy-permissions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync(assignment, urlPath, headers: headers);
                var permissions = JsonConvert.DeserializeObject<IEnumerable<ProxyAccessPermission>>(await response.Content.ReadAsStringAsync());
                return permissions;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to set proxy permissions");
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of proxy access permissions, by user, for the supplied person
        /// </summary>
        /// <param name="userId">The identifier of the entity of interest</param>
        /// <returns>A collection of proxy access permissions for the supplied person</returns>
        public async Task<IEnumerable<ProxyUser>> GetUserProxyPermissionsAsync(string userId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, userId, "proxy-permissions");
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var permissions = JsonConvert.DeserializeObject<IEnumerable<ProxyUser>>(await response.Content.ReadAsStringAsync());
                return permissions;
            }
            // Log any exception, then rethrow it and let calling code determine how to handle it.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve proxy permissions for user " + userId);
                throw;
            }
        }

        /// <summary>
        /// Gets the proxy access information granted by the specified proxy subject and return a new
        /// JSON Web Token that includes proxy subject's claims (roles + permissions). This will also
        /// update the Colleague web session token for the proxy user.
        /// </summary>
        /// <param name="proxySubject">The proxy subject. Only the ID is required. If this ID is empty, 
        /// then any previously assigned proxy subjects claims will be removed.</param>
        /// <returns>A new JSON Web Token that includes proxy subject's claims (roles + permissions)</returns>
        public async Task<string> PutSessionProxySubjectsAsync(ProxySubject proxySubject)
        {
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);

            try
            {
                var result = await ExecutePutRequestWithResponseAsync(
                    proxySubject,
                    UrlUtility.CombineUrlPath(_sessionPath, _proxySubjects), headers: headers);
                string token = await result.Content.ReadAsStringAsync();
                Credentials = token;
                return token;
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the proxy subjects associated with the specified proxy user.
        /// </summary>
        /// <param name="proxyPersonId">The proxy user's person ID.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ProxySubject>> GetUserProxySubjectsAsync(string proxyPersonId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, proxyPersonId, _proxySubjects);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var proxySubjects = JsonConvert.DeserializeObject<IEnumerable<ProxySubject>>(await response.Content.ReadAsStringAsync());
                return proxySubjects;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve proxy subjects for user " + proxyPersonId);
                throw;
            }
        }

        public async Task<RestrictionConfiguration> GetRestrictionConfigurationAsync()
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _restrictionConfigurationPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var rc = JsonConvert.DeserializeObject<RestrictionConfiguration>(await response.Content.ReadAsStringAsync());
                return rc;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve restriction configuration");
                throw;
            }
        }

        /// <summary>
        /// Creates a Proxy Candidate
        /// </summary>
        /// <param name="candidate">The <see cref="ProxyCandidate"/> to create</param>
        /// <returns>The created <see cref="ProxyCandidate"/></returns>
        public async Task<ProxyCandidate> PostProxyCandidateAsync(ProxyCandidate candidate)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, candidate.ProxySubject, _proxyCandidatesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<Dtos.Base.ProxyCandidate>(candidate, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Dtos.Base.ProxyCandidate>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to create a proxy candidate.");
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of proxy candidates that the proxy user has submitted for evaluation.
        /// </summary>
        /// <param name="grantorId">ID of the user granting access</param>
        /// <returns>A collection of proxy candidates</returns>
        public async Task<IEnumerable<ProxyCandidate>> GetUserProxyCandidatesAsync(string grantorId)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, grantorId, _proxyCandidatesPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var proxyCandidates = JsonConvert.DeserializeObject<IEnumerable<ProxyCandidate>>(await response.Content.ReadAsStringAsync());
                return proxyCandidates;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to retrieve proxy candidates for user " + grantorId);
                throw;
            }
        }

        /// <summary>
        /// Creates a Proxy User
        /// </summary>
        /// <param name="user">The <see cref="PersonProxyUser">proxy user</see> to create</param>
        /// <returns>The created <see cref="PersonProxyUser">proxy user</see></returns>
        public async Task<PersonProxyUser> PostProxyUserAsync(PersonProxyUser user)
        {
            try
            {
                string urlPath = UrlUtility.CombineUrlPath(_usersPath, _proxyUsersPath);
                var headers = new NameValueCollection();
                headers.Add(AcceptHeaderKey, _mediaTypeHeaderVersion1);
                var response = await ExecutePostRequestWithResponseAsync<Dtos.Base.PersonProxyUser>(user, urlPath, headers: headers);
                var resource = JsonConvert.DeserializeObject<Dtos.Base.PersonProxyUser>(await response.Content.ReadAsStringAsync());
                return resource;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to create a proxy user.");
                throw;
            }
        }

        /// <summary>
        /// Causes this API instance to perform a backup of its configurations
        /// </summary>
        /// <returns></returns>
        public async Task PostBackupApiConfigDataAsync()
        {
            string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _backupApiConfigurationPath);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeEllucianConfigurationVersion1);

            try
            {
                var result2 = await ExecutePostRequestWithResponseAsync<string>("", urlPath, headers: headers);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to backup API config.");
                throw;
            }
        }

        /// <summary>
        /// Causes this API instance to perform a restore of its configurations using the latest backup record
        /// </summary>
        /// <returns></returns>
        public async Task PostRestoreApiConfigDataAsync()
        {
            string urlPath = UrlUtility.CombineUrlPath(_configurationPath, _restoreApiConfigurationPath);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeEllucianConfigurationVersion1);

            try
            {
                await ExecutePostRequestWithResponseAsync<string>("", urlPath, headers: headers);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to restore API config.");
                throw;
            }
        }

        /// <summary>
        /// Stores backup data in Colleague DB
        /// </summary>
        /// <param name="backupConfiguration"></param>
        /// <returns></returns>
        public async Task<BackupConfiguration> PostBackupConfigDataAsync(BackupConfiguration backupConfiguration)
        {
            if (backupConfiguration == null)
            {
                throw new ArgumentNullException("backupConfiguration");
            }

            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeEllucianConfigurationVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<BackupConfiguration>(
                    backupConfiguration, _configurationPath, headers: headers);
                var backupData = JsonConvert.DeserializeObject<BackupConfiguration>(await response.Content.ReadAsStringAsync());
                return backupData;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to submit backup config data to Colleague.");
                throw;
            }
        }

        /// <summary>
        /// Retreives backup data from Colleague DB.
        /// </summary>
        /// <param name="id">ID of the config record to get.</param>
        /// <returns></returns>
        public async Task<BackupConfiguration> GetBackupConfigDataAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            string urlPath = UrlUtility.CombineUrlPath(_configurationPath, id);
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeEllucianConfigurationVersion1);

            try
            {
                var response = await ExecuteGetRequestWithResponseAsync(urlPath, headers: headers);
                var backupData = JsonConvert.DeserializeObject<BackupConfiguration>(await response.Content.ReadAsStringAsync());
                return backupData;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve backup config data from Colleague.");
                throw;
            }
        }

        /// <summary>
        /// Retreives backup data from Colleague DB.
        /// </summary>
        /// <param name="backupDataQuery">Object containing lookup criteria for the backup config record.
        /// Namespace is required. OnOrBeforeDateTimeUtc is optional - if not specified, the latest record will be returned.</param>
        /// <returns></returns>
        public async Task<IEnumerable<BackupConfiguration>> QueryBackupConfigDataByPostAsync(BackupConfigurationQueryCriteria backupDataQuery)
        {
            if (backupDataQuery == null)
            {
                throw new ArgumentNullException("backupDataQuery");
            }
            else
            {
                if ((backupDataQuery.ConfigurationIds == null || backupDataQuery.ConfigurationIds.Count() == 0)
                        && string.IsNullOrWhiteSpace(backupDataQuery.Namespace))
                {
                    throw new ArgumentException("ConfigurationIds and Namespace can't both be null.");
                }
            }
            var headers = new NameValueCollection();
            headers.Add(AcceptHeaderKey, _mediaTypeEllucianConfigurationVersion1);

            try
            {
                var response = await ExecutePostRequestWithResponseAsync<BackupConfigurationQueryCriteria>(
                    backupDataQuery, UrlUtility.CombineUrlPath(_qapiPath, _configurationPath), headers: headers);
                var backupData = JsonConvert.DeserializeObject<IEnumerable<BackupConfiguration>>(await response.Content.ReadAsStringAsync());
                return backupData;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to retrieve backup config data from Colleague.");
                throw;
            }
        }


        /*************************** PRIVATE METHODS ****************************/


        /// <summary>
        /// Executes an HTTP GET request and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private HttpResponseMessage ExecuteGetRequestWithResponse(string urlPath, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return serviceClient.ExecuteGetRequestWithResponse(urlPath, this.userContext, urlArguments, headers, useCache);
        }

        /// <summary>
        /// Executes an HTTP GET request asynchronously and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private Task<HttpResponseMessage> ExecuteGetRequestWithResponseAsync(string urlPath, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return serviceClient.ExecuteGetRequestWithResponseAsync(urlPath, this.userContext, urlArguments, headers, useCache);
        }

        /// <summary>
        /// Executes an HTTP POST request and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private HttpResponseMessage ExecutePostRequestWithResponse<T>(T objectToSend, string urlPath, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return serviceClient.ExecutePostRequestWithResponse<T>(objectToSend, urlPath, this.userContext, urlArguments, headers, useCache);
        }

        /// <summary>
        /// Executes an HTTP POST request asynchronously and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private Task<HttpResponseMessage> ExecutePostRequestWithResponseAsync<T>(T objectToSend, string urlPath, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return serviceClient.ExecutePostRequestWithResponseAsync<T>(objectToSend, urlPath, this.userContext, urlArguments, headers, useCache);
        }

        /// <summary>
        /// Executes an HTTP POST request asynchronously and returns the resulting response.
        /// </summary>
        /// <param name="httpContent">The http content to be sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <param name="useCache">Specifies whether or not this request can be serviced via an HTTP cache or must be retrieved from the fresh from the API</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private Task<HttpResponseMessage> ExecutePostHttpContentRequestWithResponseAsync(HttpContent httpContent, string urlPath, string urlArguments = null, NameValueCollection headers = null, bool useCache = true)
        {
            return serviceClient.ExecutePostHttpContentRequestWithResponseAsync(httpContent, urlPath, this.userContext, urlArguments, headers, useCache);
        }

        /// <summary>
        /// Executes an HTTP PUT request and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private HttpResponseMessage ExecutePutRequestWithResponse<T>(T objectToSend, string urlPath, string urlArguments = null, NameValueCollection headers = null)
        {
            return serviceClient.ExecutePutRequestWithResponse<T>(objectToSend, urlPath, this.userContext, urlArguments, headers);
        }

        /// <summary>
        /// Executes an HTTP PUT request asynchronously and returns the resulting response.
        /// </summary>
        /// <typeparam name="T">Object to be serialized and sent.</typeparam>
        /// <param name="objectToSend">Object to be serialized and sent.</param>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private Task<HttpResponseMessage> ExecutePutRequestWithResponseAsync<T>(T objectToSend, string urlPath, string urlArguments = null, NameValueCollection headers = null)
        {
            return serviceClient.ExecutePutRequestWithResponseAsync<T>(objectToSend, urlPath, this.userContext, urlArguments, headers);
        }

        /// <summary>
        /// Executes an HTTP DELETE request and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private HttpResponseMessage ExecuteDeleteRequestWithResponse(string urlPath, string urlArguments = null, NameValueCollection headers = null)
        {
            return serviceClient.ExecuteDeleteRequestWithResponse(urlPath, this.userContext, urlArguments, headers);
        }

        /// <summary>
        /// Executes an HTTP DELETE request asynchronously and returns the resulting response.
        /// </summary>
        /// <param name="urlPath">Relative request path.</param>
        /// <param name="urlArguments">Optional URL arguments to be added as query parameters.</param>
        /// <param name="headers">Optional HTTP request headers to add to the request.</param>
        /// <returns>The result as an <see cref="HttpResponseMessage"/></returns>
        private Task<HttpResponseMessage> ExecuteDeleteRequestWithResponseAsync(string urlPath, string urlArguments = null, NameValueCollection headers = null)
        {
            return serviceClient.ExecuteDeleteRequestWithResponseAsync(urlPath, this.userContext, urlArguments, headers);
        }

        /// <summary>
        /// Adds flags to the request indicating that parts of the request/response should not be logged during client debugging.
        /// You may apply more than one <see cref="LoggingRestrictions"/> by using bitwise or (|) operator.
        /// </summary>
        /// <param name="headers"><see cref="NameValueCollection"/> request headers as a reference</param>
        /// <param name="restrictions"><see cref="LoggingRestrictions"/>flags to apply.</param>
        private void AddLoggingRestrictions(ref NameValueCollection headers, LoggingRestrictions restrictions)
        {
            if (logger != null && logger.IsDebugEnabled && headers != null)
            {
                headers.Add(ColleagueServiceClient.LoggingRestrictionsHeaderKey, restrictions.ToString("X"));
            }
        }
    }
}