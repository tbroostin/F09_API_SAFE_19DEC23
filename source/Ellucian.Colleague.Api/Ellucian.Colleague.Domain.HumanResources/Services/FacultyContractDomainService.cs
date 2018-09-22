using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Services
{
    /// <summary>
    /// Creates the Faculty Contracts
    /// </summary>
    [RegisterType]
    public class FacultyContractDomainService : IFacultyContractDomainService
    {
        private IFacultyContractRepository _facultyContractRepository;
        private ICampusOrganizationRepository _campusOrganizationRepository;
        private ISectionRepository _sectionRepository;
        private IPositionRepository _positionRepository;
        private IStudentReferenceDataRepository _studentReferenceDataRepository;
        private IHumanResourcesReferenceDataRepository _humanResourcesDataReferencesRepository;
        private ILogger _logger;
        public FacultyContractDomainService(IFacultyContractRepository facultyContractRepository, ICampusOrganizationRepository campusOrganizationRepository,
            ISectionRepository sectionRepository, IPositionRepository positionRepository, IStudentReferenceDataRepository studentReferenceDataRepository, IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository, ILogger logger)
        {
            _logger = logger;
            _facultyContractRepository = facultyContractRepository;
            _campusOrganizationRepository = campusOrganizationRepository;
            _sectionRepository = sectionRepository;
            _positionRepository = positionRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _humanResourcesDataReferencesRepository = humanResourcesReferenceDataRepository;
        }
        public async Task<IEnumerable<FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("ids", "ids cannot be null or empty");
            }

            var facultyContractEntities = await _facultyContractRepository.GetFacultyContractsByFacultyIdAsync(facultyId);
            var positions = await _positionRepository.GetPositionsAsync();
            var campusOrganizationEntities = await _campusOrganizationRepository.GetCampusOrganizationsAsync(false);
            var instructionalMethodsEntities = await _studentReferenceDataRepository.GetInstructionalMethodsAsync();

            var assignments = facultyContractEntities.SelectMany(contract => contract.FacultyContractPositions).SelectMany(position => position.FacultyContractAssignments);
            if (!assignments.Equals(null) && assignments.Any())
            {
                var userHrpId = new List<string>();
                userHrpId.Add(assignments.First().HrpId);

                // These entities contain intended load, start date, end date, and role for members and advisors
                var memAssignmentInfoEntities = await _campusOrganizationRepository.GetCampusOrgMembersAsync(userHrpId);
                var advAssignmentInfoEntities = await _campusOrganizationRepository.GetCampusOrgAdvisorsAsync(userHrpId);

                // Then grab the roles from all the entities and find their descriptions
                // These entities contain the role description
                var orgRoleDescriptionEntities = await _studentReferenceDataRepository.CampusOrgRolesAsync();

                // Retrieve the course sec faculty tuple
                var facultyCountTuple = await _sectionRepository.GetSectionFacultyAsync(0, 0, "", userHrpId.First(), new List<string>());

                // Pull the courseSecFaculty entities out of the tuple
                var courseSecFacultyEntities = facultyCountTuple.Item1;

                // Filter through and get the Section Ids 
                var courseSectionIds = courseSecFacultyEntities.Select(entity => entity.SectionId);

                // Grab the entities for matching Ids from course section
                var courseSecEntities = await _sectionRepository.GetNonCachedSectionsAsync(courseSectionIds);

                // Get the AssignmentContractTypes to modify the contract types to full descriptions
                var assignmentContractTypesEntities = await _humanResourcesDataReferencesRepository.GetAssignmentContractTypesAsync(false);

                UpdateAssignmentDetails(facultyContractEntities, campusOrganizationEntities, memAssignmentInfoEntities, advAssignmentInfoEntities,
                    orgRoleDescriptionEntities, courseSecFacultyEntities, courseSecEntities, instructionalMethodsEntities);
                UpdatePositionDetails(facultyContractEntities, positions);
                UpdateFacultyContractTypes(facultyContractEntities, assignmentContractTypesEntities);
            }
            return facultyContractEntities;

        }

        private void UpdateFacultyContractTypes(IEnumerable<FacultyContract> facultyContracts, IEnumerable<AsgmtContractTypes> assignmentContractTypes)
        {
            var facContracts = facultyContracts.Select(contract => contract);
            foreach (var contract in facContracts)
            {
                var matchingAssignmentContractType = assignmentContractTypes.Where(type => type.Code.Equals(contract.ContractType)).FirstOrDefault();
                if (matchingAssignmentContractType != null)
                {
                    contract.ContractType = matchingAssignmentContractType.Description;
                }
            }
        }

        private void UpdatePositionDetails(IEnumerable<Domain.HumanResources.Entities.FacultyContract> facContracts, IEnumerable<Domain.HumanResources.Entities.Position> positions)
        {
            var facultyContractPositions = facContracts.SelectMany(contract => contract.FacultyContractPositions).ToList();
            var positionIds = facultyContractPositions.Select(position => position.PositionId);
            var positionData = positions.Where(position => positionIds.Contains(position.Id));
            foreach (var position in facultyContractPositions)
            {
                var matchingPosition = positionData.FirstOrDefault(pos => pos.Id.Equals(position.PositionId));
                if (matchingPosition == null)
                {
                    position.Title = string.Empty;
                }
                else
                {
                    position.Title = matchingPosition.Title;
                }
            }
        }

        private void UpdateAssignmentDetails(
            IEnumerable<Domain.HumanResources.Entities.FacultyContract> facContracts, IEnumerable<Domain.Student.Entities.CampusOrganization> campusOrgs,
            IEnumerable<Domain.Student.Entities.CampusOrgMemberRole> memEntities, IEnumerable<Domain.Student.Entities.CampusOrgAdvisorRole> advEntities,
            IEnumerable<Domain.Student.Entities.CampusOrgRole> orgRoles, IEnumerable<Domain.Student.Entities.SectionFaculty> courseSecFac, IEnumerable<Domain.Student.Entities.Section> facSections,
            IEnumerable<Domain.Student.Entities.InstructionalMethod> instructionalMethods)
        {
            var assignments = facContracts.SelectMany(contract => contract.FacultyContractPositions).SelectMany(position => position.FacultyContractAssignments);
            foreach (var assignment in assignments)
            {
                if (assignment.AssignmentType == Domain.HumanResources.Entities.FacultyContractAssignmentType.CampusOrganizationAdvisor
                    || assignment.AssignmentType == Domain.HumanResources.Entities.FacultyContractAssignmentType.CampusOrganizationMember)
                {
                    var assignmentCampusOrgCode = assignment.AssignmentId.Split('*').FirstOrDefault();
                    if (!string.IsNullOrEmpty(assignmentCampusOrgCode))
                    {
                        var matchingOrg = campusOrgs.Where(org => assignmentCampusOrgCode.Equals(org.Code)).FirstOrDefault();
                        if (matchingOrg != null)
                        {
                            if (!string.IsNullOrEmpty(matchingOrg.Description))
                            {
                                if (assignment.AssignmentType == Domain.HumanResources.Entities.FacultyContractAssignmentType.CampusOrganizationAdvisor)
                                {
                                    var matchingAdvisor = advEntities.Where(adv => assignment.Id.Equals(adv.Assignment)).FirstOrDefault();
                                    if (matchingAdvisor == null)
                                    {
                                        assignment.AssignmentDescription = matchingOrg.Description;
                                        _logger.Info("Assignment " + assignment.Id + " has no matching advisor resulting in no data for StartDate, EndDate, Role, and IntendedLoad");
                                    }
                                    else
                                    {
                                        var matchingRoleEntity = orgRoles.Where(role => matchingAdvisor.Role.Equals(role.Code)).FirstOrDefault();
                                        if (matchingRoleEntity != null)
                                        {
                                            assignment.Role = matchingRoleEntity.Description;
                                        }
                                        assignment.AssignmentDescription = matchingOrg.Description;
                                        assignment.StartDate = matchingAdvisor.StartDate;
                                        assignment.EndDate = matchingAdvisor.EndDate;
                                        assignment.IntendedLoad = matchingAdvisor.IntendedLoad;
                                    }
                                }
                                else
                                {
                                    // If the FacultyContractAssignmentType is CampusOrganizationMember
                                    var matchingMember = memEntities.Where(mem => assignment.Id.Equals(mem.Assignment)).FirstOrDefault();
                                    if (matchingMember == null)
                                    {
                                        assignment.AssignmentDescription = matchingOrg.Description;
                                        _logger.Info("Assignment " + assignment.Id + " has no matching member resulting in no data for StartDate, EndDate, Role, and IntendedLoad");
                                    }
                                    else
                                    {
                                        var matchingRoleEntity = orgRoles.Where(role => matchingMember.Role.Equals(role.Code)).FirstOrDefault();
                                        if (matchingRoleEntity != null)
                                        {
                                            assignment.Role = matchingRoleEntity.Description;
                                        }
                                        assignment.AssignmentDescription = matchingOrg.Description;
                                        assignment.StartDate = matchingMember.StartDate;
                                        assignment.EndDate = matchingMember.EndDate;
                                        assignment.IntendedLoad = matchingMember.IntendedLoad;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Error("Assignment " + assignment.Id + " has no matching organization");
                        }
                    }
                    else
                    {
                        _logger.Error("Assignment " + assignment.Id + " has a null campus org code(assignmentId)");
                    }
                }
                else
                {
                    // If the FacultyContractAssignmentType is CourseSectionFaculty
                    var assignmentCourseSecFacultyId = assignment.AssignmentId;
                    if (!string.IsNullOrEmpty(assignmentCourseSecFacultyId))
                    {
                        var matchingCourseSecFacEntity = courseSecFac.FirstOrDefault(entity => entity.Id.Equals(assignmentCourseSecFacultyId));
                        if (matchingCourseSecFacEntity != null)
                        {
                            assignment.StartDate = matchingCourseSecFacEntity.StartDate;
                            assignment.EndDate = matchingCourseSecFacEntity.EndDate;
                            assignment.IntendedLoad = matchingCourseSecFacEntity.LoadFactor;
                            var matchingFacSectionEntity = facSections.FirstOrDefault(entity => entity.Id.Equals(matchingCourseSecFacEntity.SectionId));
                            if (matchingFacSectionEntity != null)
                            {
                                assignment.AssignmentDescription = matchingFacSectionEntity.Name + ' ' + matchingFacSectionEntity.Title;
                                var matchingInstructionalEntity = instructionalMethods.FirstOrDefault(entity => entity.Code.Equals(matchingCourseSecFacEntity.InstructionalMethodCode));
                                if (matchingInstructionalEntity != null)
                                {
                                    assignment.Role = matchingInstructionalEntity.Description;
                                }
                            }
                        }
                        else
                        {
                            _logger.Error("Assignment " + assignment.Id + " has no matching course sec faculty entity");
                        }
                    }
                    else
                    {
                        _logger.Error("Assignment " + assignment.Id + " has a null or empty coursSecFacultyId");
                    }
                }
            }
        }
    }
}
