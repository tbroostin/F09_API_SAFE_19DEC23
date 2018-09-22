//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
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
    [RegisterType]
    public class StudentFinancialAidAcademicProgressStatusesService : BaseCoordinationService, IStudentFinancialAidAcademicProgressStatusesService
    {

        private readonly IStudentFinancialAidAcademicProgressStatusesRepository _studentFinancialAidAcademicProgressStatusesRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termsRepository;


        public StudentFinancialAidAcademicProgressStatusesService(
            IStudentFinancialAidAcademicProgressStatusesRepository studentFinancialAidAcademicProgressStatusesRepository,
            IStudentReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository,
            ITermRepository termsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentFinancialAidAcademicProgressStatusesRepository = studentFinancialAidAcademicProgressStatusesRepository;
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _termsRepository = termsRepository;
        }

        /// <summary>
        /// Gets all student-financial-aid-academic-progress-statuses
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>, int>> GetStudentFinancialAidAcademicProgressStatusesAsync
            (int offset, int limit, Dtos.StudentFinancialAidAcademicProgressStatuses criteria = null, bool bypassCache = false)
        {
            if (!await CheckViewStudentFinancialAidAcadProgressStatusesPermission())
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view student-financial-aid-academic-progress-statuses.");
                throw new PermissionsException("User is not authorized to view student-financial-aid-academic-progress-statuses.");
            }

            string personId = string.Empty, statusId = string.Empty, typeId = string.Empty;
            if (criteria != null)
            {
                if (criteria.Person != null && !string.IsNullOrEmpty(criteria.Person.Id))
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(criteria.Person.Id);
                    if(string.IsNullOrEmpty(personId))
                    {
                        return new Tuple<IEnumerable<StudentFinancialAidAcademicProgressStatuses>, int>(new List<Dtos.StudentFinancialAidAcademicProgressStatuses>(), 0);
                    }
                }

                if (criteria.Status != null && !string.IsNullOrEmpty(criteria.Status.Id))
                {
                    statusId = ConvertGuidToCode(await SapStatusesAsync(bypassCache), criteria.Status.Id);
                    if(string.IsNullOrEmpty(statusId))
                    {
                        return new Tuple<IEnumerable<StudentFinancialAidAcademicProgressStatuses>, int>(new List<Dtos.StudentFinancialAidAcademicProgressStatuses>(), 0);
                    }
                }

                if (criteria.ProgressType != null && !string.IsNullOrEmpty(criteria.ProgressType.Id))
                {
                    typeId = ConvertGuidToCode(await SapTypesAsync(bypassCache), criteria.ProgressType.Id);
                    if (string.IsNullOrEmpty(typeId))
                    {
                        return new Tuple<IEnumerable<StudentFinancialAidAcademicProgressStatuses>, int>(new List<Dtos.StudentFinancialAidAcademicProgressStatuses>(), 0);
                    }
                }
            }
            var dtos = new List<Dtos.StudentFinancialAidAcademicProgressStatuses>();
            var totalCount = 0;

            var entities = await _studentFinancialAidAcademicProgressStatusesRepository.GetSapResultsAsync(offset, limit, personId, statusId, typeId, bypassCache);

            if (entities != null && entities.Item1 != null && entities.Item1.Any())
            {
                totalCount = entities.Item2;
                //Get all person ids
                var ids = entities.Item1.Where(p => !string.IsNullOrEmpty(p.PersonId)).Select(i => i.PersonId);
                Dictionary<string, string> _people = await GetPeopleAsync(ids);

                foreach (var studentFinancialAidAcademicProgressStatuses in entities.Item1)
                {
                    dtos.Add(await ConvertStudentFinancialAidAcademicProgressStatusesEntityToDto(studentFinancialAidAcademicProgressStatuses, _people, bypassCache));
                }
            }
            return dtos != null && dtos.Any()? 
                new Tuple<IEnumerable<StudentFinancialAidAcademicProgressStatuses>, int>(dtos, totalCount) :
                new Tuple<IEnumerable<StudentFinancialAidAcademicProgressStatuses>, int>(new List<Dtos.StudentFinancialAidAcademicProgressStatuses>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a StudentFinancialAidAcademicProgressStatuses from its GUID
        /// </summary>
        /// <returns>StudentFinancialAidAcademicProgressStatuses DTO object</returns>
        public async Task<Dtos.StudentFinancialAidAcademicProgressStatuses> GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                if (!await CheckViewStudentFinancialAidAcadProgressStatusesPermission())
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view student-financial-aid-academic-progress-statuses.");
                    throw new PermissionsException("User is not authorized to view student-financial-aid-academic-progress-statuses.");
                }

                var entity = await _studentFinancialAidAcademicProgressStatusesRepository.GetSapResultByGuidAsync(guid);
                if (entity == null)
                {
                    return new Dtos.StudentFinancialAidAcademicProgressStatuses();
                }

                Dictionary<string, string> _people = !string.IsNullOrEmpty(entity.PersonId)? await GetPeopleAsync(new List<string>() { entity.PersonId}): null;

                return await ConvertStudentFinancialAidAcademicProgressStatusesEntityToDto(entity, _people, bypassCache);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-financial-aid-academic-progress-status not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Sapresults domain entity to its corresponding StudentFinancialAidAcademicProgressStatuses DTO
        /// </summary>
        /// <param name="source">Sapresults domain entity</param>
        /// <returns>StudentFinancialAidAcademicProgressStatuses DTO</returns>
        private async Task<Dtos.StudentFinancialAidAcademicProgressStatuses> ConvertStudentFinancialAidAcademicProgressStatusesEntityToDto(SapResult source, Dictionary<string, string> personIds, bool bypassCache = false)
        {
            var dto = new Ellucian.Colleague.Dtos.StudentFinancialAidAcademicProgressStatuses();

            dto.Id = source.RecordGuid;
            dto.AssignedOn = source.OvrResultsAddDate.HasValue ? source.OvrResultsAddDate : default(DateTime?);

            /*
                The effective date will be either the specific evaluation period end date or it will be the term end date for the evaluation term specified.
                If SAPR.EVAL.PD.END.TERM is not null,  retrieve TERM.END.DATE from TERMS file
                else
                If SAPR.EVAL.PD.END.DATE is not null, use this date.   else
                if SAPR.CALC.THRU.TERM is not null then retrieve TERM.END.DATE from TERMS file
                else
                if SAPR.CALC.THRU.DATE is not null then use this date. 
                Once a date has been determined, there is no need to continue checking this logic. 
            */
            if (!string.IsNullOrEmpty(source.SaprEvalPdEndTerm))
            {
                var term = (await TermsAsync(bypassCache)).FirstOrDefault(t => t.Code.Equals(source.SaprEvalPdEndTerm, StringComparison.OrdinalIgnoreCase));
                if (term == null)
                {
                    throw new KeyNotFoundException(string.Format("Term not found for id: {0}", source.SaprEvalPdEndTerm));
                }
                dto.EffectiveOn = term.EndDate;
            }
            else if (source.SaprEvalPdEndDate.HasValue)
            {
                dto.EffectiveOn = source.SaprEvalPdEndDate.Value;
            }
            else if (!string.IsNullOrEmpty(source.SaprCalcThruTerm))
            {
                var term = (await TermsAsync(bypassCache)).FirstOrDefault(t => t.Code.Equals(source.SaprCalcThruTerm, StringComparison.OrdinalIgnoreCase));
                if (term == null)
                {
                    throw new KeyNotFoundException(string.Format("Term not found for id: {0}", source.SaprCalcThruTerm));
                }
                dto.EffectiveOn = term.EndDate;
            }
            else
            {
                dto.EffectiveOn = source.SaprCalcThruDate.HasValue ? source.SaprCalcThruDate.Value : default(DateTime?);
            }

            if (!string.IsNullOrEmpty(source.PersonId) && personIds != null && personIds.Any())
            {
                var guid = string.Empty;
                var personFound = personIds.TryGetValue(source.PersonId, out guid);
                if (!personFound)
                {
                    throw new KeyNotFoundException(string.Format("Person guid not found for id {0}.", guid));
                }
                dto.Person = new GuidObject2(guid);
            }

            if (!string.IsNullOrEmpty(source.SapTypeId))
            {
                var progType = (await SapTypesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.SapTypeId, StringComparison.OrdinalIgnoreCase));
                if (progType == null)
                {
                    throw new KeyNotFoundException(string.Format("Type not found for id {0}.", source.SapTypeId));
                }
                dto.ProgressType = new GuidObject2(progType.Guid);

            }
            if (!string.IsNullOrEmpty(source.SapStatus))
            {
                var status = (await SapStatusesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.SapStatus, StringComparison.OrdinalIgnoreCase));
                if (status == null)
                {
                    throw new KeyNotFoundException(string.Format("Status not found for id {0}.", source.SapStatus));
                }
                dto.Status = new GuidObject2(status.Guid);
            }
            return dto;
        }

        /// <summary>
        /// Sap Statuses
        /// </summary>
        private IEnumerable<Domain.Student.Entities.SapStatuses> _sapStatuses = null;
        private async Task<IEnumerable<Domain.Student.Entities.SapStatuses>> SapStatusesAsync(bool bypassCache)
        {
            if(_sapStatuses == null)
            {
                _sapStatuses = await _referenceDataRepository.GetSapStatusesAsync(string.Empty, bypassCache);
            }
            return _sapStatuses;
        }

        /// <summary>
        /// Sap Types
        /// </summary>
        private IEnumerable<SapType> _sapTypes = null;
        private async Task<IEnumerable<SapType>> SapTypesAsync(bool bypassCache)
        {
            if (_sapTypes == null)
            {
                _sapTypes = await _referenceDataRepository.GetSapTypesAsync(bypassCache);
            }
            return _sapTypes;
        }

        /// <summary>
        /// Gets terms.
        /// </summary>
        private IEnumerable<Term> _terms = null;
        private async Task<IEnumerable<Term>> TermsAsync(bool bypassCache)
        {
            if(_terms == null)
            {
                _terms = await _termsRepository.GetAsync(bypassCache);
            }
            return _terms;
        }

        /// <summary>
        /// Get peoples guids dictionary.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetPeopleAsync(IEnumerable<string> ids)
        {
            if(ids != null && ids.Any())
            {
                return await _personRepository.GetPersonGuidsCollectionAsync(ids);
            }
            return null;
        }

        /// <summary>
        /// Permissions code that allows an external system to perform the READ operation.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task<bool> CheckViewStudentFinancialAidAcadProgressStatusesPermission()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(StudentPermissionCodes.ViewStudentFinancialAidAcadProgress))
            {
                return true;
            }
            return false;
        }
    }
}