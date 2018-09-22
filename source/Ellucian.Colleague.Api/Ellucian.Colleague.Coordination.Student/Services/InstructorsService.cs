//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.DtoProperties;


namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class InstructorsService : StudentCoordinationService, IInstructorsService
    {

        private readonly IPersoRepository _instructorRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        public InstructorsService(

            IPersoRepository instructorRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _instructorRepository = instructorRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all instructors
        /// </summary>
        /// <returns>Collection of Instructors DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.Instructor>, int>> GetInstructorsAsync(int offset, int limit, string instructor, string primaryLocation, bool bypassCache = false)
        {
            try
            {
                CheckUserInstructorsViewPermissions();

                var instructorsCollection = new List<Ellucian.Colleague.Dtos.Instructor>();
                var newPrimaryLocation = "";
                try
                {
                    newPrimaryLocation = string.IsNullOrEmpty(primaryLocation) ? string.Empty : ConvertGuidToCode(await SitesAsync(), primaryLocation);

                }
                catch (ArgumentException ex)
                {
                    // no results
                    return new Tuple<IEnumerable<Dtos.Instructor>, int>(new List<Dtos.Instructor>(), 0);
                }
                Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor>, int> instructorsEntities = await _instructorRepository.GetInstructorsAsync(offset, limit, instructor, newPrimaryLocation, bypassCache);
                if (instructorsEntities != null && instructorsEntities.Item1.Any())
                {
                    _personIds = instructorsEntities.Item1.Select(i => i.RecordKey).ToList();
                    foreach (var instructors in instructorsEntities.Item1)
                    {
                        instructorsCollection.Add(await ConvertInstructorsEntityToDto(instructors));
                    }
                }

                return (instructorsCollection.Any()) ?
                    new Tuple<IEnumerable<Dtos.Instructor>, int>(instructorsCollection, instructorsEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.Instructor>, int>(instructorsCollection, 0);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }            
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all instructors
        /// </summary>
        /// <returns>Collection of Instructors DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.Instructor2>, int>> GetInstructors2Async(int offset, int limit, string instructor, string primaryLocation, bool bypassCache = false)
        {
            try
            {
                CheckUserInstructorsViewPermissions();

                var instructorsCollection = new List<Ellucian.Colleague.Dtos.Instructor2>();
                var newPrimaryLocation = "";
                try
                {
                    newPrimaryLocation = string.IsNullOrEmpty(primaryLocation) ? string.Empty : ConvertGuidToCode(await SitesAsync(), primaryLocation);
                    
                }
                catch(ArgumentException ex)
                {
                    // no results
                    return new Tuple<IEnumerable<Dtos.Instructor2>, int>(new List<Dtos.Instructor2>(), 0);
                }
                Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor>, int> instructorsEntities = await _instructorRepository.GetInstructorsAsync(offset, limit, instructor, newPrimaryLocation, bypassCache);
                if (instructorsEntities != null && instructorsEntities.Item1.Any())
                {
                    _personIds = instructorsEntities.Item1.Select(i => i.RecordKey).ToList();
                    foreach (var instructors in instructorsEntities.Item1)
                    {
                        instructorsCollection.Add(await ConvertInstructorsEntityToDto2(instructors));
                    }
                }

                return (instructorsCollection.Any()) ?
                    new Tuple<IEnumerable<Dtos.Instructor2>, int>(instructorsCollection, instructorsEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.Instructor2>, int>(instructorsCollection, 0);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a Instructors from its GUID
        /// </summary>
        /// <returns>Instructors DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Instructor> GetInstructorByGuidAsync(string guid)
        {
            try
            {
                CheckUserInstructorsViewPermissions();

                Ellucian.Colleague.Domain.Student.Entities.Instructor instructorEntity = await _instructorRepository.GetInstructorByIdAsync(guid);
                _personIds = new List<string>() { instructorEntity.RecordKey };

                return await ConvertInstructorsEntityToDto(instructorEntity);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a Instructors from its GUID
        /// </summary>
        /// <returns>Instructors2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Instructor2> GetInstructorByGuid2Async(string guid)
        {
            try
            {
                CheckUserInstructorsViewPermissions();

                Ellucian.Colleague.Domain.Student.Entities.Instructor instructorEntity = await _instructorRepository.GetInstructorByIdAsync(guid);
                _personIds = new List<string>() { instructorEntity.RecordKey };

                return await ConvertInstructorsEntityToDto2(instructorEntity);
            }
            catch (ArgumentNullException ex)
            {
                logger.Error(ex.Message);
                throw new ArgumentNullException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                logger.Error(ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex.Message);
                throw new PermissionsException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view applications.
        /// </summary>
        private void CheckUserInstructorsViewPermissions()
        {
            // access is ok if the current user has the view instructors permission
            if (!HasPermission(StudentPermissionCodes.ViewInstructors))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view instructors.");
                throw new PermissionsException("User is not authorized to view instructors.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Instructors domain entity to its corresponding Instructors DTO
        /// </summary>
        /// <param name="source">Instructors domain entity</param>
        /// <returns>Instructors DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Instructor> ConvertInstructorsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.Instructor source)
        {
            try
            {
                var instructor = new Ellucian.Colleague.Dtos.Instructor();

                instructor.Id = source.RecordGuid;
                instructor.IndividualInstructor = await ConvertEntityKeyToPersonGuidObjectAsync(source.RecordKey);
                instructor.InstitutionalUnits = await ConvertEntityToInstitutionslUnitsAsync(source.Departments);
                instructor.PrimaryLocation = await ConvertEntityToLocationGuidObjectDtoAsync(source.HomeLocation);
                instructor.Category = await ConvertEntityToCategoryGuidObject(source.SpecialStatus);
                instructor.StaffType = await ConvertEntityToInstructorContractGuidObject(source.ContractType);

                return instructor;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(string.Concat(ex.Message, "Instructor guid: ", source.RecordGuid), ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat(ex.Message, "Instructor guid: ", source.RecordGuid), ex);
            }
            catch (Exception ex)
            {
                var error = string.Concat(ex.Message, "Something unexpected happened for guid ", source.RecordGuid);
                throw new Exception(error, ex);
            } 
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Instructors domain entity to its corresponding Instructors DTO
        /// </summary>
        /// <param name="source">Instructors domain entity</param>
        /// <returns>Instructors2 DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Instructor2> ConvertInstructorsEntityToDto2(Ellucian.Colleague.Domain.Student.Entities.Instructor source)
        {
            try
            {
                var instructor = new Ellucian.Colleague.Dtos.Instructor2();

                instructor.Id = source.RecordGuid;
                instructor.IndividualInstructor = await ConvertEntityKeyToPersonGuidObjectAsync(source.RecordKey);
                instructor.InstitutionalUnits = await ConvertEntityToInstitutionslUnits2Async(source.Departments);
                instructor.PrimaryLocation = await ConvertEntityToLocationGuidObjectDtoAsync(source.HomeLocation);
                instructor.Category = await ConvertEntityToCategoryGuidObject(source.SpecialStatus);
                instructor.StaffType = await ConvertEntityToInstructorContractGuidObject(source.ContractType);
                if (!string.IsNullOrEmpty(source.TentureType))
                    {
                    var tenure = new InstructorsTenure();
                    tenure.TenureType = await ConvertEntityToInstructorTenureTypeGuidObject(source.TentureType);
                    tenure.TenureStatusStartOn = source.TenureTypeDate;
                    instructor.Tenure = tenure;
                }
                return instructor;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(string.Concat(ex.Message, "Instructor guid: ", source.RecordGuid), ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat(ex.Message, "Instructor guid: ", source.RecordGuid), ex);
            }
            catch (Exception ex)
            {
                var error = string.Concat(ex.Message, "Something unexpected happened for guid ", source.RecordGuid);
                throw new Exception(error, ex);
            }
        }

        /// <summary>
        /// Gets person guid object
        /// </summary>
        /// <param name="personRecordKey"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityKeyToPersonGuidObjectAsync(string personRecordKey)
        {
            if (string.IsNullOrEmpty(personRecordKey))
            {
                throw new ArgumentNullException("Person key is required. ");
            }

            var source = (await PersonGuidsAsync()).FirstOrDefault(i => i.Key.Equals(personRecordKey, StringComparison.OrdinalIgnoreCase));
            if (source.Equals(default(KeyValuePair<string, string>)))
            {
                var error = string.Format("Person not found for key {0}. ", personRecordKey);
                throw new KeyNotFoundException(error);
            }
            return string.IsNullOrEmpty(source.Value) ? null : new GuidObject2(source.Value);
        }

        /// <summary>
        /// Converts entity to InstructorsInstitutionalUnit
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.InstructorsInstitutionalUnit>> ConvertEntityToInstitutionslUnitsAsync(IEnumerable<FacultyDeptLoad> source)
        {
            if (source == null || (source != null && !source.Any()))
            {
                return null;
            }
            List<Dtos.DtoProperties.InstructorsInstitutionalUnit> instrUnits = new List<Dtos.DtoProperties.InstructorsInstitutionalUnit>();

            foreach (var sourceItem in source)
            {
                Dtos.DtoProperties.InstructorsInstitutionalUnit instrUnit = new Dtos.DtoProperties.InstructorsInstitutionalUnit()
                {
                    Percentage = sourceItem.DeptPcts.HasValue ? sourceItem.DeptPcts : null,
                    Department = await ConvertEntityToDepartmetAsync(sourceItem.FacultyDepartment)
                };
                instrUnits.Add(instrUnit);
            }

            return instrUnits;
        }


        /// <summary>
        /// Converts entity to InstructorsInstitutionalUnit
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.InstructorsInstitutionalUnit2>> ConvertEntityToInstitutionslUnits2Async(IEnumerable<FacultyDeptLoad> source)
        {
            if (source == null || (source != null && !source.Any()))
            {
                return null;
            }
            List<Dtos.DtoProperties.InstructorsInstitutionalUnit2> instrUnits = new List<Dtos.DtoProperties.InstructorsInstitutionalUnit2>();

            foreach (var sourceItem in source)
            {
                  Dtos.DtoProperties.InstructorsInstitutionalUnit2 instrUnit = new Dtos.DtoProperties.InstructorsInstitutionalUnit2()
                {
                    Percentage = sourceItem.DeptPcts.HasValue ? sourceItem.DeptPcts : null,
                    Department = await ConvertEntityToDepartmetAsync(sourceItem.FacultyDepartment),
                };
                instrUnits.Add(instrUnit);
            }
            //Set the department with highest percent as primary.If there is just one department, then that would be "primary".If there is more than one, whichever has the highest percentage would be primary.If both has the same percentage, pick the first one on the list as primary,
            if (instrUnits != null && instrUnits.Any())
            {
                instrUnits.OrderByDescending(i => i.Percentage).FirstOrDefault().AdministrativeUnit = InstructorsAdministrativeUnit.Primary;
            }
            return instrUnits;
        }

        /// <summary>
        /// Convert entity to guid object
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToDepartmetAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var source = (await DepartmentsAsync()).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                var error = string.Format("Department not found for code {0}. ", sourceCode);
                throw new KeyNotFoundException(error);
            }
            return string.IsNullOrEmpty(source.Guid) ? null : new GuidObject2(source.Guid);
        }  

        /// <summary>
        /// Convert entity to guid object.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToLocationGuidObjectDtoAsync(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var guid = string.Empty;
            if (sourceCode != null && sourceCode.Any())
            {
                var siteList = sourceCode.Distinct();
                var source = (await SitesAsync()).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    throw new KeyNotFoundException(string.Format("Site not found for code {0} ", sourceCode));
                }
                guid = source.Guid;
            }
            return string.IsNullOrEmpty(guid) ? null : new GuidObject2(guid);
        }

        /// <summary>
        /// Convert a GUID to a code in a code file
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="guid">GUID corresponding to a code</param>
        /// <returns>The code corresponding to the GUID</returns>
        protected string ConvertGuidToCode(IEnumerable<Domain.Entities.GuidCodeItem> codeList, string guid)
        {
            if (codeList == null || codeList.Count() == 0)
            {
                throw new ArgumentNullException("codeList");
            }
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var entity = codeList.FirstOrDefault(c => c.Guid == guid);
            if (entity != null)
                return entity.Code;
            else
                throw new ArgumentException("Invalid guid " + guid + " in the arguments.");
        }

        /// <summary>
        /// Convert entity to guid object.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToCategoryGuidObject(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var guid = string.Empty;
            if (sourceCode != null && sourceCode.Any())
            {
                var siteList = sourceCode.Distinct();
                var source = (await FacultySpecialStatuses()).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    throw new KeyNotFoundException(string.Format("Faculty special status not found for code {0}. ", sourceCode));
                }
                guid = source.Guid;
            }
            return string.IsNullOrEmpty(guid) ? null : new GuidObject2(guid);
        }

        /// <summary>
        /// Convert entity to guid object.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToInstructorContractGuidObject(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var guid = string.Empty;
            if (sourceCode != null && sourceCode.Any())
            {
                var siteList = sourceCode.Distinct();
                var source = (await FacultyContractTypes()).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    throw new KeyNotFoundException(string.Format("Faculty contract type not found for code {0}. ", sourceCode));
                }
                guid = source.Guid;
            }
            return string.IsNullOrEmpty(guid) ? null : new GuidObject2(guid);
        }

        /// <summary>
        /// Convert tenure type to guid object.
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToInstructorTenureTypeGuidObject(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return null;
            }

            var guid = string.Empty;
            if (sourceCode != null && sourceCode.Any())
            {
                var siteList = sourceCode.Distinct();
                var source = (await FacultyTenureTypes()).FirstOrDefault(i => i.Code.Equals(sourceCode, StringComparison.OrdinalIgnoreCase));
                if (source == null)
                {
                    throw new KeyNotFoundException(string.Format("Faculty tenure type not found for code {0}. ", sourceCode));
                }
                guid = source.Guid;
            }
            return string.IsNullOrEmpty(guid) ? null : new GuidObject2(guid);
        }

        /// <summary>
        /// Person ids, guid key value pairs
        /// </summary>
        private IDictionary<string, string> _personGuidsDict;
        private IEnumerable<string> _personIds;

        private async Task<IDictionary<string, string>> PersonGuidsAsync()
        {
            if (_personIds != null && _personIds.Any())
            {
                if (_personGuidsDict == null)
                {
                    IDictionary<string, string> dict = await _instructorRepository.GetPersonGuidsAsync(_personIds);
                    if (dict != null && dict.Any())
                    {
                        _personGuidsDict = new Dictionary<string, string>();
                        dict.ToList().ForEach(i =>
                        {
                            if (!_personGuidsDict.ContainsKey(i.Key))
                            {
                                _personGuidsDict.Add(i.Key, i.Value);
                            }
                        });
                    }
                }
            }
            return _personGuidsDict;
        }

        #region Reference Data
        /// <summary>
        /// Department: FAC.DEPTS => educational-institution-units
        /// </summary>
        private IEnumerable<Domain.Base.Entities.Department> _depts;
        private async Task<IEnumerable<Domain.Base.Entities.Department>> DepartmentsAsync()
        {
            if (_depts == null)
            {
                _depts = await _referenceDataRepository.GetDepartmentsAsync(false);
            }
            return _depts;
        }

        /// <summary>
        /// Sites
        /// </summary>
        private IEnumerable<Domain.Base.Entities.Location> _locations;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> SitesAsync()
        {
            if (_locations == null)
            {
                _locations = await _referenceDataRepository.GetLocationsAsync(false);
            }
            return _locations;
        }

        /// <summary>
        /// InstructorTypes
        /// </summary>
        private IEnumerable<Domain.Student.Entities.FacultyContractTypes> _facultyContractTypes;
        private async Task<IEnumerable<Domain.Student.Entities.FacultyContractTypes>> FacultyContractTypes()
        {
            if (_facultyContractTypes == null)
            {
                _facultyContractTypes = await _studentReferenceDataRepository.GetFacultyContractTypesAsync(false);
            }
            return _facultyContractTypes;
        }

        /// <summary>
        /// Instructor Tenure Types
        /// </summary>
        private IEnumerable<Domain.Student.Entities.TenureTypes> _facultyTenureTypes;
        private async Task<IEnumerable<Domain.Student.Entities.TenureTypes>> FacultyTenureTypes()
        {
            if (_facultyTenureTypes == null)
            {
                _facultyTenureTypes = await _instructorRepository.GetTenureTypesAsync(false);
            }
            return _facultyTenureTypes;
        }

        /// <summary>
        /// FacultySpecialStatuses
        /// </summary>
        private IEnumerable<Domain.Student.Entities.FacultySpecialStatuses> _facultySpecialStatuses;
        private async Task<IEnumerable<Domain.Student.Entities.FacultySpecialStatuses>> FacultySpecialStatuses()
        {
            if (_facultySpecialStatuses == null)
            {
                _facultySpecialStatuses = await _studentReferenceDataRepository.GetFacultySpecialStatusesAsync(false);
            }
            return _facultySpecialStatuses;
        }

        #endregion
    }
}