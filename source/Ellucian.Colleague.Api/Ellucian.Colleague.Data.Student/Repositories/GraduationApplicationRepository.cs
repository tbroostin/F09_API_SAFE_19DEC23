// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GraduationApplicationRepository : BaseColleagueRepository, IGraduationApplicationRepository
    {
        private readonly string _colleagueTimeZone;
        protected const int GraduationFeeCacheTimeout = 120;

        /// <summary>
        /// Constructor for Graduation Application Repository
        /// </summary>
        /// <param name="cacheProvider">Cache Provider</param>
        /// <param name="transactionFactory">Colleague TX Factory</param>
        /// <param name="logger">Logger</param>
        /// <param name="apiSettings">API settings</param>
        public GraduationApplicationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Creates a new Graduation Application for the student in a specific program asynchronously.
        /// </summary>
        /// <param name="graduationApplication">Graduation Application Entity</param>
        /// <returns>Graduation Application Entity added</returns>
        public async Task<GraduationApplication> CreateGraduationApplicationAsync(GraduationApplication graduationApplication)
        {
            AddGraduationApplicationRequest graduationApplicationRequest = new AddGraduationApplicationRequest();
            if (graduationApplication == null)
            {
                logger.Info("You must provide the graduation application entity to add");
                throw new ArgumentNullException("graduationApplication", "You must provide the graduation application entity to add");
            }
            try
            {
                graduationApplicationRequest.AttendCommencement = graduationApplication.AttendingCommencement.HasValue ? ((graduationApplication.AttendingCommencement.Value) ? "Y" : "N") : string.Empty;
                graduationApplicationRequest.CapSize = graduationApplication.CapSize;
                graduationApplicationRequest.CommencementDate = graduationApplication.CommencementDate.ToLocalDateTime(_colleagueTimeZone);
                graduationApplicationRequest.CommencementSite = graduationApplication.CommencementLocation;
                graduationApplicationRequest.DiplomaName = graduationApplication.DiplomaName;
                graduationApplicationRequest.GownSize = graduationApplication.GownSize;
                graduationApplicationRequest.GraduationTerm = graduationApplication.GraduationTerm;
                graduationApplicationRequest.Hometown = graduationApplication.Hometown;
                graduationApplicationRequest.IncludeNameInProgram = graduationApplication.IncludeNameInProgram.HasValue ? ((graduationApplication.IncludeNameInProgram.Value) ? "Y" : "N") : string.Empty;
                graduationApplicationRequest.MailDiplomaAddressLines = graduationApplication.MailDiplomaToAddressLines;
                graduationApplicationRequest.MailDiplomaCity = graduationApplication.MailDiplomaToCity;
                graduationApplicationRequest.MailDiplomaCountryCode = graduationApplication.MailDiplomaToCountry;
                graduationApplicationRequest.MailDiplomaPostalCode = graduationApplication.MailDiplomaToPostalCode;
                graduationApplicationRequest.MailDiplomaState = graduationApplication.MailDiplomaToState;
                graduationApplicationRequest.NumberOfGuests = graduationApplication.NumberOfGuests;
                graduationApplicationRequest.PhoneticSpelling = graduationApplication.PhoneticSpellingOfName;
                graduationApplicationRequest.ProgramCode = graduationApplication.ProgramCode;
                graduationApplicationRequest.StudentId = graduationApplication.StudentId;
                graduationApplicationRequest.PrimaryLocation = graduationApplication.PrimaryLocation;
                if (graduationApplication.MilitaryStatus.HasValue)
                {
                    switch (graduationApplication.MilitaryStatus.Value)
                    {
                        case GraduateMilitaryStatus.ActiveMilitary:
                            graduationApplicationRequest.MilitaryStatus = "A";
                            break;
                        case GraduateMilitaryStatus.Veteran:
                            graduationApplicationRequest.MilitaryStatus = "V";
                            break;
                        case GraduateMilitaryStatus.NotApplicable:
                            graduationApplicationRequest.MilitaryStatus = "N";
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(graduationApplication.SpecialAccommodations))
                {
                    // We may have line break characters in the data. Split them out and add each line separately
                    // to preserve any line-to-line formatting the user entered. Note that these characters could be
                    // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
                    // any of the new line or carriage returns to the same thing, and then split the string on that.
                    string newLineCharacter = "\n";
                    string alternateNewLineCharacter = "\r\n";
                    string carriageReturnCharacter = "\r";
                    string temporaryText1 = graduationApplication.SpecialAccommodations.Replace(alternateNewLineCharacter, newLineCharacter);
                    string temporaryText2 = temporaryText1.Replace(carriageReturnCharacter, newLineCharacter);
                    var accommodationLines = temporaryText2.Split('\n');
                    foreach (var line in accommodationLines)
                    {
                        graduationApplicationRequest.SpecialAccommodations.Add(line);
                    }
                }
                graduationApplicationRequest.WillPickupDiploma = graduationApplication.WillPickupDiploma.HasValue ? ((graduationApplication.WillPickupDiploma.Value) ? "Y" : "N") : string.Empty;
                AddGraduationApplicationResponse graduationApplicationResponse = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.Student.Transactions.AddGraduationApplicationRequest, Ellucian.Colleague.Data.Student.Transactions.AddGraduationApplicationResponse>(graduationApplicationRequest);
                if (graduationApplicationResponse.AlreadyExists)
                {
                    logger.Error(string.Format("Graduation Application already exists for student Id {0} in program Code {1}", graduationApplication.StudentId, graduationApplication.ProgramCode));
                    throw new ExistingResourceException(string.Format("Graduation Application already exists for student Id {0} and program Code {1}", graduationApplication.StudentId, graduationApplication.ProgramCode), string.Concat(graduationApplication.StudentId.ToUpper(), "*", graduationApplication.ProgramCode.ToUpper()));
                }
                if (!string.IsNullOrEmpty(graduationApplicationResponse.ErrorMessage))
                {
                    logger.Error("GraduationApplicationRepository Error: " + graduationApplicationResponse.ErrorMessage);
                    throw new ArgumentException(graduationApplicationResponse.ErrorMessage);
                }
                var outputGraduationApplication = await GetGraduationApplicationAsync(graduationApplication.StudentId, graduationApplication.ProgramCode);
                return outputGraduationApplication;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to access Graduates record with studentId {0} and program Code {1}", graduationApplication.StudentId, graduationApplication.ProgramCode);
                logger.Info(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Returns the requested graduation application for given student Id and program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="programCode">Program Code</param>
        /// <returns> <see cref="GraduationApplication"/>The requested graduation application</returns>
        public async Task<GraduationApplication> GetGraduationApplicationAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                logger.Info("You must provide the student Id and program Code");
                throw new ArgumentNullException("studentId-programCode", "You must provide the student Id and program Code");
            }
            var graduateApplicationId = string.Concat(studentId.ToUpper(), "*", programCode.ToUpper());
            Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate;
            try
            {
                graduate = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(graduateApplicationId, false);
                if (graduate == null)
                {
                    var errorMessage = string.Format("Unable to access Graduates record with student Id {0} and program Code {1} ", studentId, programCode);
                    logger.Info(errorMessage);
                    throw new KeyNotFoundException(errorMessage);
                }
                var graduationApplication = BuildGraduationApplication(graduate);
                return graduationApplication;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to access Graduates record with student Id {0} and program Code {1} ", studentId, programCode);
                logger.Info(ex, errorMessage);
                throw;
            }

        }

        /// <summary>
        /// Returns the list of  graduation applications for given student Id asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>list of <see cref="GraduationApplication"/>graduation applications</returns>
        public async Task<List<GraduationApplication>> GetGraduationApplicationsAsync(string studentId)
        {
            List<GraduationApplication> gradutionApplicationsEntityLst = new List<GraduationApplication>();
            if (string.IsNullOrEmpty(studentId))
            {
                logger.Info("You must provide the student Id");
                throw new ArgumentNullException("studentId", "You must provide the student Id");
            }
            try
            {
                Collection<Graduates> graduates = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Graduates>(string.Format("GRAD.STUDENT.IDX = '{0}'", studentId), false);
                if (graduates == null)
                {
                    var errorMessage = string.Format("Unable to access Graduates records for student Id {0}", studentId);
                    logger.Info(errorMessage);
                    throw new KeyNotFoundException(errorMessage);
                }
                //map contract to entities
                foreach (var graduate in graduates)
                {
                    GraduationApplication graduationApplicationEntity = null;
                    try
                    {
                        graduationApplicationEntity = BuildGraduationApplication(graduate);
                    }
                    catch
                    {

                    }
                    if (graduationApplicationEntity != null)
                    {
                        gradutionApplicationsEntityLst.Add(graduationApplicationEntity);
                    }
                }
                return gradutionApplicationsEntityLst;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to access Graduates records for student Id {0}", studentId);
                logger.Info(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Updates a graduation Application for a specific student and program asynchronously.
        /// </summary>
        /// <param name="graduationApplication">The graduation application object to update</param>
        /// <returns><see cref="GraduationApplication"/>The graduation application retrieved after it was updated</returns>
        public async Task<GraduationApplication> UpdateGraduationApplicationAsync(GraduationApplication graduationApplication)
        {

            UpdateGraduationApplicationRequest graduationApplicationRequest = new UpdateGraduationApplicationRequest();
            if (graduationApplication == null)
            {
                logger.Info("You must provide the graduation application entity to update");
                throw new ArgumentNullException("graduationApplication", "You must provide the graduation application entity to update");
            }

            if (string.IsNullOrEmpty(graduationApplication.StudentId) || string.IsNullOrEmpty(graduationApplication.ProgramCode))
            {
                logger.Info("You must provide the student Id and program Code to update");
                throw new ArgumentNullException("studentId-programCode", "You must provide the student Id and program Code");
            }
            try
            {
                //no term and commencement date udated
                graduationApplicationRequest.AttendCommencement = graduationApplication.AttendingCommencement.HasValue ? ((graduationApplication.AttendingCommencement.Value) ? "Y" : "N") : string.Empty;
                graduationApplicationRequest.CapSize = graduationApplication.CapSize;
                graduationApplicationRequest.CommencementSite = graduationApplication.CommencementLocation;
                graduationApplicationRequest.DiplomaName = graduationApplication.DiplomaName;
                graduationApplicationRequest.GownSize = graduationApplication.GownSize;
                graduationApplicationRequest.Hometown = graduationApplication.Hometown;
                graduationApplicationRequest.IncludeNameInProgram = graduationApplication.IncludeNameInProgram.HasValue ? ((graduationApplication.IncludeNameInProgram.Value) ? "Y" : "N") : string.Empty;
                graduationApplicationRequest.MailDiplomaAddressLines = graduationApplication.MailDiplomaToAddressLines;
                graduationApplicationRequest.MailDiplomaCity = graduationApplication.MailDiplomaToCity;
                graduationApplicationRequest.MailDiplomaCountryCode = graduationApplication.MailDiplomaToCountry;
                graduationApplicationRequest.MailDiplomaPostalCode = graduationApplication.MailDiplomaToPostalCode;
                graduationApplicationRequest.MailDiplomaState = graduationApplication.MailDiplomaToState;
                graduationApplicationRequest.NumberOfGuests = graduationApplication.NumberOfGuests;
                graduationApplicationRequest.PhoneticSpelling = graduationApplication.PhoneticSpellingOfName;
                graduationApplicationRequest.ProgramCode = graduationApplication.ProgramCode;
                graduationApplicationRequest.StudentId = graduationApplication.StudentId;
                graduationApplicationRequest.PrimaryLocation = graduationApplication.PrimaryLocation;
                if (graduationApplication.MilitaryStatus.HasValue)
                {
                    switch (graduationApplication.MilitaryStatus.Value)
                    {
                        case GraduateMilitaryStatus.ActiveMilitary:
                            graduationApplicationRequest.MilitaryStatus = "A";
                            break;
                        case GraduateMilitaryStatus.Veteran:
                            graduationApplicationRequest.MilitaryStatus = "V";
                            break;
                        case GraduateMilitaryStatus.NotApplicable:
                            graduationApplicationRequest.MilitaryStatus = "N";
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(graduationApplication.SpecialAccommodations))
                {
                    graduationApplicationRequest.SpecialAccommodations = SeparateOnLineBreaks(graduationApplication.SpecialAccommodations);
                }
                graduationApplicationRequest.WillPickupDiploma = graduationApplication.WillPickupDiploma.HasValue ? ((graduationApplication.WillPickupDiploma.Value) ? "Y" : "N") : string.Empty;
                UpdateGraduationApplicationResponse graduationApplicationResponse = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.Student.Transactions.UpdateGraduationApplicationRequest, Ellucian.Colleague.Data.Student.Transactions.UpdateGraduationApplicationResponse>(graduationApplicationRequest);
                //check if throws key not found error
                if (!string.IsNullOrEmpty(graduationApplicationResponse.ErrorMessage))
                {
                    logger.Error("GraduationApplicationRepository Error: " + graduationApplicationResponse.ErrorMessage);
                    throw new ArgumentException(graduationApplicationResponse.ErrorMessage);
                }
                var outputGraduationApplication = await GetGraduationApplicationAsync(graduationApplication.StudentId, graduationApplication.ProgramCode);
                return outputGraduationApplication;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to update Graduates record with studentId {0} and program Code {1}", graduationApplication.StudentId, graduationApplication.ProgramCode);
                logger.Info(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Returns the graduation application fee for given student Id and program Code asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="programCode">Program Code</param>
        /// <returns> <see cref="GraduationApplicationFee"/>The requested graduation application fee object</returns>
        public async Task<GraduationApplicationFee> GetGraduationApplicationFeeAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                logger.Error("You must provide the student Id and program Code");
                throw new ArgumentNullException("studentId", "You must provide the student Id");
            }
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                logger.Error("You must provide the program Code");
                throw new ArgumentNullException("programCode", "You must provide the program Code");
            }

            GetGraduationApplicationFeeRequest getApplicationFeeRequest = new GetGraduationApplicationFeeRequest();
            getApplicationFeeRequest.StudentId = studentId;
            getApplicationFeeRequest.ProgramCode = programCode;
            try
            {
                // Call the Colleague Transaction used to calculate the fee and the distribution
                GetGraduationApplicationFeeResponse getApplicationFeeResponse = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.Student.Transactions.GetGraduationApplicationFeeRequest, Ellucian.Colleague.Data.Student.Transactions.GetGraduationApplicationFeeResponse>(getApplicationFeeRequest);
                // Take results and create the graduation application fee entity.
                GraduationApplicationFee graduationApplicationFee = new GraduationApplicationFee(studentId, programCode, getApplicationFeeResponse.ApplicationFee, getApplicationFeeResponse.DistributionCode);
                return graduationApplicationFee;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to determine graduation application fee for student Id {0} and program Code {1} ", studentId, programCode);
                logger.Error(ex, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Determines a student's eligibility to apply for graduation in the requested programs
        /// </summary>
        /// <param name="studentId">Id of student to determine eligibility</param>
        /// <param name="programCodes">Programs for which the eligibility is requested</param>
        /// <returns>List of Graduation Application Program Eligibility entities.</returns>
        public async Task<IEnumerable<GraduationApplicationProgramEligibility>> GetGraduationApplicationEligibilityAsync(string studentId, IEnumerable<string> programCodes)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Invalid Student Id");
            }
            if (programCodes == null || !programCodes.Any())
            {
                throw new ArgumentNullException("programCodes", "Must provide at least one program code to evaluate eligibility.");
            }

            GetGradApplEligibilityRequest graduationApplicationEligibilityRequest = new GetGradApplEligibilityRequest();
            graduationApplicationEligibilityRequest.StudentId = studentId;
            graduationApplicationEligibilityRequest.ProgramCodes = programCodes.ToList();
            GetGradApplEligibilityResponse graduationApplicationEligibilityResponse = null;
            try
            {
                graduationApplicationEligibilityResponse = await transactionInvoker.ExecuteAsync<GetGradApplEligibilityRequest, GetGradApplEligibilityResponse>(graduationApplicationEligibilityRequest);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to determine graduation application eligibility for student Id {0} and programs {1} ", studentId, string.Join(", ", programCodes));
                logger.Error(ex, errorMessage);
                throw;
            }

            // Build the Graduation Application Program Eligibility entities to return
            List<GraduationApplicationProgramEligibility> gradAppEligibilityEntities = new List<GraduationApplicationProgramEligibility>();
            if (graduationApplicationEligibilityResponse == null)
            {
                var message = string.Format("CTX returned null graduation application eligibility response for student Id " + studentId);
                logger.Info(message);
            }
            else
            {
                if (graduationApplicationEligibilityResponse.EligibilityResults != null && graduationApplicationEligibilityResponse.EligibilityResults.Any())
                {
                    foreach (var programResult in graduationApplicationEligibilityResponse.EligibilityResults)
                    {
                        if (programResult != null && !string.IsNullOrEmpty(programResult.ProgramCode))
                        {
                            var gradAppProgramEligibility = new GraduationApplicationProgramEligibility(studentId, programResult.ProgramCode, programResult.IsEligible);

                            if (!string.IsNullOrEmpty(programResult.FailureReasons))
                            {
                                // Reasons could be returned subvalued because more than 1 rule may have failed for a program so change those to commas
                                // Using Regex because a subvalue is a string not a char.
                                string[] failureReasons = Regex.Split(programResult.FailureReasons, DmiString.sSM);
                                foreach (var reason in failureReasons)
                                {
                                    if (!string.IsNullOrEmpty(reason))
                                    {
                                        gradAppProgramEligibility.AddIneligibleMessage(reason);
                                    }

                                }
                            }

                            gradAppEligibilityEntities.Add(gradAppProgramEligibility);
                        }

                    }
                }
            }
            return gradAppEligibilityEntities;
        }

        #region private methods
        private GraduationApplication BuildGraduationApplication(Ellucian.Colleague.Data.Student.DataContracts.Graduates graduate)
        {
            if (graduate == null)
            {
                logger.Info("You must provide the graduation application data contract.");
                throw new ArgumentNullException("graduationApplicationId", "You must provide the graduation application Id");
            }
            try
            {
                string[] ids = graduate.Recordkey.Split(new Char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                var studentId = ids[0];
                var programCode = ids[1];
                GraduationApplication graduationApplication = new GraduationApplication(graduate.Recordkey, studentId, programCode);
                /// FILL OUT REST OF THE ENTITY
                graduationApplication.AttendingCommencement = !string.IsNullOrEmpty(graduate.GradAttendCommencement) ? ((graduate.GradAttendCommencement.Equals("Y", StringComparison.OrdinalIgnoreCase)) ? true : false) : default(bool?);
                graduationApplication.CapSize = graduate.GradCapSize;
                graduationApplication.GraduationTerm = graduate.GradTerm;
                graduationApplication.CommencementDate = graduate.GradCommencementDate;
                graduationApplication.SubmittedDate = graduate.GraduatesAdddate;
                graduationApplication.CommencementLocation = graduate.GradCommencementSite;
                graduationApplication.DiplomaName = graduate.GradDiplomaName;
                graduationApplication.GownSize = graduate.GradGownSize;
                graduationApplication.Hometown = graduate.GradHometown;
                graduationApplication.IncludeNameInProgram = !string.IsNullOrEmpty(graduate.GradIncludeName) ? ((graduate.GradIncludeName.Equals("Y", StringComparison.OrdinalIgnoreCase)) ? true : false) : default(bool?);
                if (graduate.GradTranscriptAddress != null)
                {
                    graduationApplication.MailDiplomaToAddressLines = new List<string>();
                    graduationApplication.MailDiplomaToAddressLines = graduate.GradTranscriptAddress;
                }
                graduationApplication.MailDiplomaToCity = graduate.GradTranscriptCity;
                graduationApplication.MailDiplomaToCountry = graduate.GradTranscriptCountry;
                graduationApplication.MailDiplomaToPostalCode = graduate.GradTranscriptZip;
                graduationApplication.MailDiplomaToState = graduate.GradTranscriptState;
                graduationApplication.NumberOfGuests = !string.IsNullOrEmpty(graduate.GradNumberOfGuests) ? Convert.ToInt32(graduate.GradNumberOfGuests) : 0;
                graduationApplication.PhoneticSpellingOfName = graduate.GradPhoneticSpelling;
                graduationApplication.WillPickupDiploma = !string.IsNullOrEmpty(graduate.GradWillPickupDiploma) ? ((graduate.GradWillPickupDiploma.Equals("Y", StringComparison.OrdinalIgnoreCase)) ? true : false) : default(bool?);
                if (!string.IsNullOrEmpty(graduate.GradSpecialAccommodations))
                {
                    graduationApplication.SpecialAccommodations = graduate.GradSpecialAccommodations.Replace(DmiString._VM, '\n');
                }
                if (!string.IsNullOrEmpty(graduate.GradMilitaryStatus))
                {
                    switch (graduate.GradMilitaryStatus.ToUpper())
                    {
                        case "A":
                            graduationApplication.MilitaryStatus = GraduateMilitaryStatus.ActiveMilitary;
                            break;
                        case "V":
                            graduationApplication.MilitaryStatus = GraduateMilitaryStatus.Veteran;
                            break;
                        case "N":
                            graduationApplication.MilitaryStatus = GraduateMilitaryStatus.NotApplicable;
                            break;
                        default:
                            break;

                    }
                }
                graduationApplication.InvoiceNumber = graduate.GradInvoice;
                graduationApplication.PrimaryLocation = graduate.GradPrimaryLocation;
                graduationApplication.AcadCredentialsUpdated = !string.IsNullOrEmpty(graduate.GradAcadCredentialsUpdted) ? (graduate.GradAcadCredentialsUpdted.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false) : false;
                return graduationApplication;

            }
            catch (Exception ex)
            {
                LogDataError("graduation application", graduate.Recordkey, graduate, ex);
                throw;
            }
        }

        private List<string> SeparateOnLineBreaks(string contents)
        {
            List<string> lines = new List<string>();
            if (contents == null)
            {
                return null;
            }
            Regex rx = new Regex(@"(?<captureLine>[^\n|\r|\n\r]*)[\n|\r|\n\r]?", RegexOptions.Singleline);
            MatchCollection matches = rx.Matches(contents);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string lineContents = match.Groups["captureLine"].Value;
                    lines.Add(lineContents);
                }
            }
            return lines;
        }
        #endregion
    }
}
