// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
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
    
    [RegisterType]
    public class StudentRecordsReleaseService :  StudentCoordinationService,IStudentRecordsReleaseService
    {
        private readonly IStudentRecordsReleaseRepository _studentRecordsReleaseRepository;
        private IPersonBaseRepository _personBaseRepository;
        

        public StudentRecordsReleaseService(IStudentRecordsReleaseRepository studentRecordsReleaseRepository, IStudentRepository studentRepository, IPersonBaseRepository personBaseRepository,
           IConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory,
           IRoleRepository roleRepository, ILogger logger)
           : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _studentRecordsReleaseRepository = studentRecordsReleaseRepository;
            _personBaseRepository = personBaseRepository;
            
        }

        /// <summary>
        /// Get the student records release information for this student Id.
        /// </summary>
        /// <param name="studentId">Id of the student for which to retrieve tudent records release information</param>
        /// <returns>A collection of<see cref="Dtos.Student.StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see> dto objects</returns>
        public async Task<IEnumerable<StudentRecordsReleaseInfo>> GetStudentRecordsReleaseInformationAsync(string studentId)
        {
            // Throw argument null exception if student Id not provided
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            await AuthorizeUser(studentId);
            List<StudentRecordsReleaseInfo> studentsRecordsReleaseInfoDto = new List<StudentRecordsReleaseInfo>();
            try
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo> studentsRecordsReleaseEntity = await _studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoAsync(studentId);
                if (studentsRecordsReleaseEntity != null && studentsRecordsReleaseEntity.Any())
                {
                    var studentsRecordsReleaseDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>();
                    studentsRecordsReleaseInfoDto.AddRange(studentsRecordsReleaseEntity.Select(s => studentsRecordsReleaseDtoAdapter.MapToType(s)));
                }
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = string.Format("Colleague session expired while retrieving student records release information for student {0}", studentId);
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to retrieve student records release information from repository using student id " + studentId + "Exception message: " + ex.Message;
                logger.Error(message);
                throw;
            }
            return studentsRecordsReleaseInfoDto;
        }
        /// <summary>
        /// Creates a new Student Records Release Information
        /// </summary>
        /// <param name="studentRecordsReleaseInfo"></param>
        /// <returns>A new Student Records Release Information that got created</returns>
        public async Task<StudentRecordsReleaseInfo> AddStudentRecordsReleaseInfoAsync(StudentRecordsReleaseInfo studentRecordsReleaseInfo)
        {
            if (studentRecordsReleaseInfo == null)
            {
                throw new ArgumentNullException("studentRecordsReleaseInfo", "student Records Release info must be provided to create a new record.");
            }

            await AuthorizeUser(studentRecordsReleaseInfo.StudentId);

            var studentRecordsReleaseEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentRecordsReleaseInfo, Domain.Student.Entities.StudentRecordsReleaseInfo>();
            var addstudentRecordsReleaseEntity = studentRecordsReleaseEntityAdapter.MapToType(studentRecordsReleaseInfo);

            try
            {
                var newAddStudentRecordsRelease = await _studentRecordsReleaseRepository.AddStudentRecordsReleaseInfoAsync(addstudentRecordsReleaseEntity);
                var addstudentRecordsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseInfo, Dtos.Student.StudentRecordsReleaseInfo>();
                Dtos.Student.StudentRecordsReleaseInfo studentRecordsReleaseInfoDto = addstudentRecordsDtoAdapter.MapToType(newAddStudentRecordsRelease);
                return studentRecordsReleaseInfoDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for newly created student records release information";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while adding student records release information";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to create student records release information ";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Delete a student records release information for a student
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="studentReleaseId">Student Release Id</param>
        /// <returns>StudentRecordsReleaseInfo object</returns>
        public async Task<StudentRecordsReleaseInfo> DeleteStudentRecordsReleaseInfoAsync(string studentId, string studentReleaseId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Must provide the student id to delete student records release info.");
            }
            if (string.IsNullOrEmpty(studentReleaseId))
            {
                throw new ArgumentNullException("studentReleaseId", "Must provide the student release id to delete student records release info.");
            }

            await AuthorizeUser(studentId);
            await CheckIfReleaseRecordIdIsValidForStudent(studentId, studentReleaseId);

            try
            {
                var studentRecordsRelease = await _studentRecordsReleaseRepository.DeleteStudentRecordsReleaseInfoAsync(studentReleaseId);
                var studentRecordsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseInfo, Dtos.Student.StudentRecordsReleaseInfo>();
                Dtos.Student.StudentRecordsReleaseInfo studentRecordsReleaseInfoDto = studentRecordsDtoAdapter.MapToType(studentRecordsRelease);
                return studentRecordsReleaseInfoDto;                
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found while deleting student records release information";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while deleting student records release information";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to delete student records release information ";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Get the student records release deny access information for this student Id.
        /// </summary>
        /// <param name="studentId">Id of the student for which to retrieve student records release deny access information</param>
        /// <returns>The<see cref="Dtos.Student.StudentRecordsReleaseDenyAccess">StudentRecordsReleaseDenyAccess</see></returns>
        public async Task<StudentRecordsReleaseDenyAccess> GetStudentRecordsReleaseDenyAccessAsync(string studentId)
        {
            // Throw argument null exception if student Id not provided
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            await AuthorizeUser(studentId);

            try
            {
                StudentRecordsReleaseDenyAccess studentsRecordsReleasedenyAccessDto = new StudentRecordsReleaseDenyAccess();
                var studentsRecordsReleaseDenyAccessEntity = await _studentRecordsReleaseRepository.GetStudentRecordsReleaseDenyAccessAsync(studentId);

                // Get the right adapter for the type mapping
                var studentsRecordsReleaseDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseDenyAccess, StudentRecordsReleaseDenyAccess>();
                studentsRecordsReleasedenyAccessDto = studentsRecordsReleaseDtoAdapter.MapToType(studentsRecordsReleaseDenyAccessEntity);
                return studentsRecordsReleasedenyAccessDto;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = string.Format("Colleague session expired while retrieving student records release deny access information for student {0}", studentId);
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to retrieve student records release deny access information from repository using student id " + studentId + "Exception message: " + ex.Message;
                logger.Error(message);
                throw;
            }

        }
        /// <summary>
        /// Deny access to student records release information 
        /// </summary>
        /// <param name="studentRecordsRelDenyAccess">The student records release deny access information for denying access to student records release</param>
        /// <returns>A collection of updated<see cref="Dtos.Student.StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see> dto objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentRecordsReleaseInfo>> DenyStudentRecordsReleaseAccessAsync(Dtos.Student.DenyStudentRecordsReleaseAccessInformation studentRecordsRelDenyAccess)
        {
            if (studentRecordsRelDenyAccess == null)
            {
                throw new ArgumentNullException("studentRecordsRelDenyAccess", "student records release deny access item must be provided to deny access.");
            }

            await AuthorizeUser(studentRecordsRelDenyAccess.StudentId);

            var studentRecordsRelDenyAccessEntityAdapter = _adapterRegistry.GetAdapter<DenyStudentRecordsReleaseAccessInformation, Ellucian.Colleague.Domain.Student.Entities.DenyStudentRecordsReleaseAccessInformation>();
            var studentRecordsRelDenyAccessEntity = studentRecordsRelDenyAccessEntityAdapter.MapToType(studentRecordsRelDenyAccess);
        
            List<StudentRecordsReleaseInfo> updatedStudentRecordsReleaseInfoDto = new List<StudentRecordsReleaseInfo>();
            try
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentRecordsReleaseInfo> updatedStudentRecordsReleaseInfoEntity = await _studentRecordsReleaseRepository.DenyStudentRecordsReleaseAccessAsync(studentRecordsRelDenyAccessEntity);
                if (updatedStudentRecordsReleaseInfoEntity != null && updatedStudentRecordsReleaseInfoEntity.Any())
                {
                    var updatedStudentRecordsReleaseInfoDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseInfo, Ellucian.Colleague.Dtos.Student.StudentRecordsReleaseInfo>();
                    updatedStudentRecordsReleaseInfoDto.AddRange(updatedStudentRecordsReleaseInfoEntity.Select(s => updatedStudentRecordsReleaseInfoDtoAdapter.MapToType(s)));
                }
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for denying access to student records release information";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while denyig access to student records release information";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to deny access to student records release information";
                logger.Error(ex, message);
                throw;
            }
            return updatedStudentRecordsReleaseInfoDto;
        }

        public async Task<bool> AuthorizeUser(string personId)
        {
            string message = string.Empty;
            if (UserIsSelf(personId))
            {               
                // check if user is a student           
                if (await _personBaseRepository.IsStudentAsync(personId))
                  return true; 
                else
                {
                    message = "User is not a student.";
                    logger.Info(message);
                    throw new PermissionsException(message);
                }
            }
            message = "User can only get or add Student Records Release information to self.";
            logger.Info(message);
            throw new PermissionsException(message);
        }

        public async Task<bool> CheckIfReleaseRecordIdIsValidForStudent(string studentId,string releaseRecordId)
        {
            try
            {
               var studentRecordsReleaseInfo = await _studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoByIdAsync(releaseRecordId);
               if(studentRecordsReleaseInfo.StudentId == studentId)
               {
                    return true;
               }               
                string message = "User can only update Student Records Release information of self.";
                logger.Info(message);
                throw new PermissionsException(message);
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for checking student records release information";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while checking student records release information";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while checking student records release information ";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Updates the existing student records release information
        /// </summary>
        /// <param name="studentRecordsReleaseInfo">The StudentRecordsReleaseInfo to Update</param>
        /// <returns>True if the operation is successful</returns>
        public async Task<StudentRecordsReleaseInfo> UpdateStudentRecordsReleaseInfoAsync(StudentRecordsReleaseInfo studentRecordsReleaseInfo)
        {
            if (studentRecordsReleaseInfo == null)
            {
                throw new ArgumentNullException("studentRecordsReleaseInfo", "student Records Release info must be provided to update record.");
            }
            if (string.IsNullOrEmpty(studentRecordsReleaseInfo.StudentId))
            {
                throw new ArgumentNullException("studentId", "Must provide the student id to update student records release info.");
            }
            if (string.IsNullOrEmpty(studentRecordsReleaseInfo.Id))
            {               
                throw new ArgumentNullException("studentReleaseId", "Must provide the student release id to update student records release info.");
            }
            if (studentRecordsReleaseInfo.AccessAreas == null || !studentRecordsReleaseInfo.AccessAreas.Any() || studentRecordsReleaseInfo.AccessAreas.Any(a => a == null))
            {
                throw new ArgumentNullException("Must provide the Access area codes to update student Records Release information.");
            }

            await AuthorizeUser(studentRecordsReleaseInfo.StudentId);
            await CheckIfReleaseRecordIdIsValidForStudent(studentRecordsReleaseInfo.StudentId, studentRecordsReleaseInfo.Id);

            var studentRecordsReleaseEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentRecordsReleaseInfo, Domain.Student.Entities.StudentRecordsReleaseInfo>();
            var updatestudentRecordsReleaseEntity = studentRecordsReleaseEntityAdapter.MapToType(studentRecordsReleaseInfo);

            try
            {
                var updateStudentRecordsRelease = await _studentRecordsReleaseRepository.UpdateStudentRecordsReleaseInfoAsync(updatestudentRecordsReleaseEntity);
                var updateStudentRecordsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseInfo, Dtos.Student.StudentRecordsReleaseInfo>();
                Dtos.Student.StudentRecordsReleaseInfo studentRecordsReleaseInfoDto = updateStudentRecordsDtoAdapter.MapToType(updateStudentRecordsRelease);
                return studentRecordsReleaseInfoDto;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "Record not found for updating student records release information";
                logger.Error(kex, message);
                throw;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session expired while updating student records release information";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to update student records release information ";
                logger.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Gets the student records release information based on the Id
        /// </summary>
        /// <param name="studentRecordsReleaseId"></param>
        /// <returns>StudentRecordsReleaseInfo object for the requested Id</returns>
        public async Task<Dtos.Student.StudentRecordsReleaseInfo> GetStudentRecordsReleaseInfoByIdAsync(string studentRecordsReleaseId)
        {
            var studentRecordsRelease = await _studentRecordsReleaseRepository.GetStudentRecordsReleaseInfoByIdAsync(studentRecordsReleaseId);
            var studentRecordsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentRecordsReleaseInfo, Dtos.Student.StudentRecordsReleaseInfo>();
            Dtos.Student.StudentRecordsReleaseInfo studentRecordsReleaseInfoDto = studentRecordsDtoAdapter.MapToType(studentRecordsRelease);
            return studentRecordsReleaseInfoDto;
        }
    }
}
