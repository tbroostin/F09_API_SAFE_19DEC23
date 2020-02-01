// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordinates information to enable retrieval and update of Waivers
    /// </summary>
    [RegisterType]
    public class GraduationApplicationService : StudentCoordinationService, IGraduationApplicationService
    {
        private readonly IGraduationApplicationRepository graduationApplicationRepository;
        private readonly ITermRepository termRepository;
        private readonly IProgramRepository programRepository;
        private readonly IStudentRepository studentRepository;
        private IStudentConfigurationRepository studentConfigurationRepository;
        private  IAddressRepository addressRepository;


        /// <summary>
        /// Initialize the service for accessing graduation application functions
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="graduationApplicationRepository">Graduation Application repository</param>
        /// <param name="termRepository">Term repository</param>
        /// <param name="currentUserFactory">Current User Factory</param>
        /// <param name="roleRepository">Role Repository</param>
        /// <param name="logger">error logging</param>
        public GraduationApplicationService(IAdapterRegistry adapterRegistry, IGraduationApplicationRepository graduationApplicationRepository, ITermRepository termRepository, IProgramRepository programRepository, IStudentRepository studentRepository, IStudentConfigurationRepository studentConfigurationRepository,
            IAddressRepository addressRepository,  ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository, IStaffRepository staffRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository, staffRepository)
        {
            this.graduationApplicationRepository = graduationApplicationRepository;
            this.termRepository = termRepository;
            this.programRepository = programRepository;
            this.studentRepository = studentRepository;
            this.studentConfigurationRepository = studentConfigurationRepository;
            this.addressRepository = addressRepository;
        }

        /// <summary>
        /// Retrieves a graduation application for the given student Id and program code asynchronously.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">program code that student belongs to</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object that was created</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.GraduationApplication> GetGraduationApplicationAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                var message = "Student Id and program Code must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            // Make sure the person requesting the application is the student.
            if (CurrentUser.PersonId != studentId)
            {
                var message = "Current user is not the student of requested graduation application and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            Domain.Student.Entities.GraduationApplication graduationApplicationEntity = null;
            try
            {
                graduationApplicationEntity = await graduationApplicationRepository.GetGraduationApplicationAsync(studentId, programCode);


            }
            catch (KeyNotFoundException)
            {
                logger.Error(string.Format("Graduation Application not found in repository for given student Id {0} and program Id {1}", studentId, programCode));
                throw;
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to read graduation application from repository using  student Id {0} and program Id {1}  Exception message: ", studentId, programCode, ex.Message);
                logger.Error(ex, message);
                throw;
            }
            // Make sure the person requesting the application is the student.
            if (graduationApplicationEntity.StudentId != studentId || graduationApplicationEntity.ProgramCode != programCode)
            {
                var message = "Current user is not the student of requested graduation application and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            try
            {
                var applicationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.GraduationApplication, Dtos.Student.GraduationApplication>();
                var graduationDto = applicationDtoAdapter.MapToType(graduationApplicationEntity);

                //check for diploma and preferred addreses are same
                IEnumerable<Address> addresses = addressRepository.GetPersonAddresses(studentId);
                var preferredAddress = addresses != null ? addresses.ToList().Where(a => a.IsPreferredAddress).FirstOrDefault() : null;
                if ((preferredAddress != null && graduationApplicationEntity.MailDiplomaToAddressLines != null && graduationApplicationEntity.MailDiplomaToAddressLines.Count > 0))
                {
                    string studentPreferredAddress = ConvertToAddressLabel(preferredAddress.AddressLines, preferredAddress.City, preferredAddress.State, preferredAddress.PostalCode, preferredAddress.CountryCode);
                    string studentGraduationAddress = ConvertToAddressLabel(graduationApplicationEntity.MailDiplomaToAddressLines, graduationApplicationEntity.MailDiplomaToCity, graduationApplicationEntity.MailDiplomaToState, graduationApplicationEntity.MailDiplomaToPostalCode, graduationApplicationEntity.MailDiplomaToCountry);
                    if (studentPreferredAddress.Equals(studentGraduationAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        graduationDto.IsDiplomaAddressSameAsPreferred = true;
                    }
                    else
                    {
                        graduationDto.IsDiplomaAddressSameAsPreferred = false;
                    }

                }
                return graduationDto;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting graduation application Entity to Dto: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validates a new graduation application and creates a new application in the database, returning the created application asynchronously.
        /// </summary>
        /// <param name="graduateApplication">New graduate application object to add</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object that was created</returns>
        public async Task<Dtos.Student.GraduationApplication> CreateGraduationApplicationAsync(Dtos.Student.GraduationApplication graduationApplicationDto)
        {
            // Throw exception if incoming graduation application is null
            if (graduationApplicationDto == null)
            {
                var message = "Graduation Application object must be provided.";
                logger.Error(message);
                throw new ArgumentNullException("graduationApplication", message);
            }
            // Throw Exception if the incoming dto is missing any required paramters.
            if (string.IsNullOrEmpty(graduationApplicationDto.StudentId) || string.IsNullOrEmpty(graduationApplicationDto.ProgramCode))
            {
                var message = "Graduation Application is missing a required property.";
                logger.Error(message);
                throw new ArgumentException("graduationApplication", message);
            }
            // Throw exception if application is being submitted by someone other than the student.
            if (CurrentUser.PersonId != graduationApplicationDto.StudentId)
            {
                var message = "User does not have permissions to create a Graduation Application for this student.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            // VALIDATE THE ENTITY
            if (!string.IsNullOrWhiteSpace(graduationApplicationDto.GraduationTerm))
            {
                var term = await termRepository.GetAsync(graduationApplicationDto.GraduationTerm);
                if (term == null || (term != null && term.Code != graduationApplicationDto.GraduationTerm))
                {
                    var message = string.Format("Provided Term {0} does not exists in repository", graduationApplicationDto.GraduationTerm);
                    logger.Error(message);
                    throw new ArgumentException("gradualtionApplication", message);
                }
            }
            var program = await programRepository.GetAsync(graduationApplicationDto.ProgramCode);
            if (program == null || (program != null && program.Code != graduationApplicationDto.ProgramCode))
            {
                var message = string.Format("Provided Program Code {0} does not exists in repository", graduationApplicationDto.ProgramCode);
                logger.Error(message);
                throw new ArgumentException("gradualtionApplication", message);
            }
            var student = await studentRepository.GetAsync(graduationApplicationDto.StudentId);
            if (student == null || (student != null && student.Id != graduationApplicationDto.StudentId))
            {
                var message = string.Format("Provided Student Id {0} does not exists in repository", graduationApplicationDto.StudentId);
                logger.Error(message);
                throw new ArgumentException("gradualtionApplication", message);
            }
            if (!student.ProgramIds.Contains(graduationApplicationDto.ProgramCode))
            {
                var message = string.Format("Student Id {0} does not have active Program Code {1}", graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode);
                logger.Error(message);
                throw new ArgumentException("gradualtionApplication", message);
            }
            Domain.Student.Entities.GraduationApplication graduationApplicationToAddEntity = null;
            try
            {
                //if there is no diploma address provided, save preferred address in graduation file
                //this is done by updating diploma mail address with preferred address
                if (graduationApplicationDto.MailDiplomaToAddressLines == null || graduationApplicationDto.MailDiplomaToAddressLines.Count == 0)

                {
                    UpdateGraduationDiplomaMailAddress(graduationApplicationDto);
                }
                var graduationApplicationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.GraduationApplication, Domain.Student.Entities.GraduationApplication>();
                graduationApplicationToAddEntity = graduationApplicationDtoToEntityAdapter.MapToType(graduationApplicationDto);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting incoming graduation application Dto to Entity: " + ex.Message);
                throw;
            }
            try
            {
                var graduationApplicationAddedEntity = await graduationApplicationRepository.CreateGraduationApplicationAsync(graduationApplicationToAddEntity);
                var applicationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.GraduationApplication, Dtos.Student.GraduationApplication>();
                return applicationDtoAdapter.MapToType(graduationApplicationAddedEntity);
            }
            catch (ExistingResourceException egx)
            {
                logger.Error(string.Format("Graduation Application already exists for student Id {0} in program Code {1}", graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode)); logger.Info(egx.ToString());
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Retrieve list of all  the graduation applications for the given student Id asynchronously.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">List of Graduation Application</see></returns>
        public async Task<PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>> GetGraduationApplicationsAsync(string studentId)
        {
            var hasPrivacyRestriction = false;
            Dtos.Student.GraduationApplication graduationDto = null;
            List<Dtos.Student.GraduationApplication> graduationApplicationsDtoLst = new List<Dtos.Student.GraduationApplication>();
            List<Domain.Student.Entities.GraduationApplication> GraduationApplicationsEntityLst = null;
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            if (CurrentUser.PersonId != studentId && !(await UserIsAdvisorAsync(studentId)))
            {
                var message = "Current user is not the student of requested graduation applications or current user is advisor but doesn't have appropriate permissions and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            try
            {
                if (!UserIsSelf(studentId))
                {
                    //read student record for privacy code
                    var studentEntity = await studentRepository.GetAsync(studentId);
                    if (studentEntity == null)
                    {
                        hasPrivacyRestriction = true;
                    }
                    else
                    {
                        //if appropriate permissions exists then check if student have a privacy code and logged-in user have a staff record with same privacy code.
                        hasPrivacyRestriction = string.IsNullOrEmpty(studentEntity.PrivacyStatusCode) ? false : !HasPrivacyCodeAccess(studentEntity.PrivacyStatusCode);
                    }
                }

                GraduationApplicationsEntityLst = await graduationApplicationRepository.GetGraduationApplicationsAsync(studentId);
                if (GraduationApplicationsEntityLst == null)
                {
                    var errorMessage = string.Format("Unable to access Graduates record with student Id {0} ", studentId);
                    throw new KeyNotFoundException(errorMessage);
                }
            }
            catch (KeyNotFoundException)
            {
                logger.Error(string.Format("Graduation Applications not found in repository for given student Id {0}", studentId));
                throw;
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to retrieve graduation applications from repository for  student Id {0} - Exception message: ", studentId, ex.Message);
                logger.Error(ex, message);
                throw;
            }
            try
            {
                var applicationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.GraduationApplication, Dtos.Student.GraduationApplication>();
                foreach (var graduationApplicationEntity in GraduationApplicationsEntityLst)
                {

                    if (hasPrivacyRestriction)
                    {
                        graduationDto = new Dtos.Student.GraduationApplication() { Id = graduationApplicationEntity.Id, StudentId = studentId };
                    }
                    else
                    {
                        graduationDto = applicationDtoAdapter.MapToType(graduationApplicationEntity);
                    }
                    graduationApplicationsDtoLst.Add(graduationDto);
                }
                return new PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>(graduationApplicationsDtoLst, hasPrivacyRestriction);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting incoming graduation application Entity to Dto: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validates an existing graduation application and updates it in the database, returning the updated application asynchronously.
        /// </summary>
        /// <param name="graduateApplication">Updated graduate application object</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object that was updated</returns>
        public async Task<Dtos.Student.GraduationApplication> UpdateGraduationApplicationAsync(Dtos.Student.GraduationApplication graduationApplicationDto)
        {
            try
            {
                // Throw exception if incoming graduation application is null
                if (graduationApplicationDto == null)
                {
                    var message = "Graduation Application object must be provided to update.";
                    logger.Error(message);
                    throw new ArgumentNullException("graduationApplication", message);
                }
                // Throw Exception if the incoming dto is missing any required paramters.
                if (string.IsNullOrWhiteSpace(graduationApplicationDto.StudentId) || string.IsNullOrWhiteSpace(graduationApplicationDto.ProgramCode))
                {
                    var message = "Graduation Application is missing a required property to update.";
                    logger.Error(message);
                    throw new ArgumentException("graduationApplication", message);
                }
                // Throw exception if application is being submitted by someone other than the student.
                if (CurrentUser.PersonId != graduationApplicationDto.StudentId)
                {
                    var message = "User does not have permissions to update a Graduation Application for this student.";
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
                // VALIDATE THE ENTITY
                var program = await programRepository.GetAsync(graduationApplicationDto.ProgramCode);
                if (program == null || (program != null && program.Code != graduationApplicationDto.ProgramCode))
                {
                    var message = string.Format("Provided Program Code {0} does not exists in repository", graduationApplicationDto.ProgramCode);
                    logger.Error(message);
                    throw new ArgumentException("gradualtionApplication", message);
                }
                var student = await studentRepository.GetAsync(graduationApplicationDto.StudentId);
                if (student == null || (student != null && student.Id != graduationApplicationDto.StudentId))
                {
                    var message = string.Format("Provided Student Id {0} does not exists in repository", graduationApplicationDto.StudentId);
                    logger.Error(message);
                    throw new ArgumentException("gradualtionApplication", message);
                }
                if (!student.ProgramIds.Contains(graduationApplicationDto.ProgramCode))
                {
                    var message = string.Format("Student Id {0} does not have active Program Code {1}", graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode);
                    logger.Error(message);
                    throw new ArgumentException("gradualtionApplication", message);
                }
                GraduationApplication application = await graduationApplicationRepository.GetGraduationApplicationAsync(graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode);
                if (application == null)
                {
                    var errorMessage = string.Format("Unable to retrieve graduate record for student Id {0} and program Code {1} ", graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode);
                    logger.Error(errorMessage);
                    throw new KeyNotFoundException(errorMessage);
                }
                Domain.Student.Entities.GraduationConfiguration studentConfiguration = await studentConfigurationRepository.GetGraduationConfigurationAsync();
                if (studentConfiguration.GraduationTerms == null || (studentConfiguration.GraduationTerms != null && !studentConfiguration.GraduationTerms.Contains(application.GraduationTerm)))
                {
                    var errorMessage = string.Format("Unable to update Graduates record with student Id {0} and program Code {1}, Term {2} is closed", graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode, application.GraduationTerm);
                    logger.Error(errorMessage);
                    throw new Exception(errorMessage);
                }
                //if there are no diploma address provided, save preferred address in graduation file
                //this is done by updating diploma mail address with preferred address
                if (graduationApplicationDto.MailDiplomaToAddressLines == null || graduationApplicationDto.MailDiplomaToAddressLines.Count == 0)
                {
                    UpdateGraduationDiplomaMailAddress(graduationApplicationDto);

                }

                Domain.Student.Entities.GraduationApplication graduationApplicationToUpdateEntity = null;
                try
                {
                    var graduationApplicationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.GraduationApplication, Domain.Student.Entities.GraduationApplication>();
                    graduationApplicationToUpdateEntity = graduationApplicationDtoToEntityAdapter.MapToType(graduationApplicationDto);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error converting incoming graduation application Dto to Entity: " + ex.Message);
                    throw;
                }

                var graduationApplicationUpdatedEntity = await graduationApplicationRepository.UpdateGraduationApplicationAsync(graduationApplicationToUpdateEntity);
                var applicationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.GraduationApplication, Dtos.Student.GraduationApplication>();
                return applicationDtoAdapter.MapToType(graduationApplicationUpdatedEntity);
            }
            catch (KeyNotFoundException)
            {
                logger.Error(string.Format("Graduation Application not found in repository for given student Id {0} and program Id {1}", graduationApplicationDto.StudentId, graduationApplicationDto.ProgramCode));
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.ToString());
                throw;
            }
        }



        /// <summary>
        /// Retrieves graduation application fee information for the given student Id and program code asynchronously. 
        /// </summary>
        /// <remarks>Users may only request graduation application fee information for themselves.</remarks>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">program code that student for which student is applying for graduation</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplicationFee">Graduation Application</see> object that was created</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.GraduationApplicationFee> GetGraduationApplicationFeeAsync(string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(programCode))
            {
                var message = "Student Id and program Code must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            if (!CurrentUser.IsPerson(studentId))
            {
                var message = string.Format("Authenticated user {0} cannot request graduation application fee information for student {1}; users may only request their own graduation application fee information.", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            Domain.Student.Entities.GraduationApplicationFee graduationApplicationFeeEntity = null;
            try
            {
                graduationApplicationFeeEntity = await graduationApplicationRepository.GetGraduationApplicationFeeAsync(studentId, programCode);
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to get graduation application fee from repository using  student Id {0} and program Id {1}  Exception message: ", studentId, programCode, ex.Message);
                logger.Error(ex, message);
                throw;
            }
            try
            {
                var applicationFeeDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.GraduationApplicationFee, Dtos.Student.GraduationApplicationFee>();
                var graduationFeeDto = applicationFeeDtoAdapter.MapToType(graduationApplicationFeeEntity);
                return graduationFeeDto;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting graduation application fee Entity to Dto: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Determines a student's eligibility to apply for graduation in the requested programs
        /// </summary>
        /// <param name="studentId">Id of student to determine eligibility</param>
        /// <param name="programCodes">Programs for which the eligibility is requested</param>
        /// <returns>List of Graduation Application Program Eligibility DTOs.</returns>
        public async Task<IEnumerable<Dtos.Student.GraduationApplicationProgramEligibility>> GetGraduationApplicationEligibilityAsync(string studentId, IEnumerable<string> programCodes)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Invalid Student Id");
            }
            if (programCodes == null || !programCodes.Any())
            {
                throw new ArgumentNullException("programCodes", "Must provide at least one program code to evaluate eligibility.");
            }

            // Make sure user has access to this student--If not, method throws permission exception
            await CheckStudentAdvisorUserAccessAsync(studentId);

            List<Dtos.Student.GraduationApplicationProgramEligibility> graduationApplicationProgramDtos = new List<Dtos.Student.GraduationApplicationProgramEligibility>();
            try
            {
                IEnumerable<GraduationApplicationProgramEligibility> graduationApplicationPrograms = await graduationApplicationRepository.GetGraduationApplicationEligibilityAsync(studentId, programCodes);

                var gradAppProgramEligibilityDtoAdapter = _adapterRegistry.GetAdapter<GraduationApplicationProgramEligibility, Dtos.Student.GraduationApplicationProgramEligibility>();
                foreach (var gradAppProgramEligibility in graduationApplicationPrograms)
                {
                    Dtos.Student.GraduationApplicationProgramEligibility gradAppProgramEligibilityDto = gradAppProgramEligibilityDtoAdapter.MapToType(gradAppProgramEligibility);
                    graduationApplicationProgramDtos.Add(gradAppProgramEligibilityDto);
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to determine graduation application eligibility using  student Id {0} and program Ids {1}  Exception message: ", studentId, string.Join(",", programCodes), ex.Message);
                logger.Info(ex, message);
                throw;
            }

            return graduationApplicationProgramDtos;
        }

        /// <summary>
        /// convert address properties to string
        /// </summary>
        /// <param name="addressLines"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalCode"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        private string ConvertToAddressLabel(List<string> addressLines, string city, string state, string postalCode, string country)
        {
            StringBuilder addressLabel = new StringBuilder();
            if (addressLines != null && addressLines.Count > 0)
            {
                addressLines.ForEach(item => addressLabel.Append(item));
            }
            if (!string.IsNullOrEmpty(city))
            {
                addressLabel.Append(city);
            }
            if (!string.IsNullOrEmpty(state))
            {
                addressLabel.Append(state);
            }
            if (!string.IsNullOrEmpty(postalCode))
            {
                addressLabel.Append(postalCode);
            }
            if (!string.IsNullOrEmpty(country))
            {
                addressLabel.Append(country);
            }
            return addressLabel.ToString();
        }

        /// <summary>
        /// this is to update diploma address with preferred address
        /// </summary>
        /// <param name="graduationApplicationDto"></param>
        private void UpdateGraduationDiplomaMailAddress(Dtos.Student.GraduationApplication graduationApplicationDto)
        {
            var addresses = addressRepository.GetPersonAddresses(graduationApplicationDto.StudentId);
            var preferredAddress = addresses != null ? addresses.ToList().Where(a => a.IsPreferredAddress).FirstOrDefault() : null;
            if (preferredAddress != null)
            {
                graduationApplicationDto.MailDiplomaToAddressLines = preferredAddress.AddressLines;
                graduationApplicationDto.MailDiplomaToCity = preferredAddress.City;
                graduationApplicationDto.MailDiplomaToCountry = preferredAddress.CountryCode;
                graduationApplicationDto.MailDiplomaToPostalCode = preferredAddress.PostalCode;
                graduationApplicationDto.MailDiplomaToState = preferredAddress.State;
            }
        }
    }
}
