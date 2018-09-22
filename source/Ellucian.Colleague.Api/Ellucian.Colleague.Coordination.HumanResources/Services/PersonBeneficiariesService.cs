//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PersonBeneficiariesService : BaseCoordinationService, IPersonBeneficiariesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;
        private readonly IPersonBeneficiariesRepository _personBeneficiariesRepository;
        private readonly IPersonRepository _personsRepository;
        private readonly IPayrollDeductionArrangementRepository _payrollDeductionArrangementRepository;

        public PersonBeneficiariesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IPersonBeneficiariesRepository personBeneficiariesRepository,
            IPersonRepository personsRepository,
            IPayrollDeductionArrangementRepository payrollDeductionRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _personBeneficiariesRepository = personBeneficiariesRepository;
            _personsRepository = personsRepository;
            _payrollDeductionArrangementRepository = payrollDeductionRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all person-beneficiaries
        /// </summary>
        /// <returns>Collection of PersonBeneficiaries DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonBeneficiaries>, int>> GetPersonBeneficiariesAsync(int offset, int limit, bool bypassCache = false)
        {
            var personBeneficiariesCollection = new List<Ellucian.Colleague.Dtos.PersonBeneficiaries>();

            var pageOfItems = await _personBeneficiariesRepository.GetPersonBeneficiariesAsync(offset, limit, bypassCache);

            var personBeneficiariesEntities = pageOfItems.Item1;
            int totalRecords = pageOfItems.Item2;

            if (personBeneficiariesEntities != null && personBeneficiariesEntities.Any())
            {
                foreach (var personBeneficiaries in personBeneficiariesEntities)
                {
                    personBeneficiariesCollection.Add(await ConvertPersonBeneficiariesEntityToDto(personBeneficiaries, bypassCache));
                }
                return new Tuple<IEnumerable<Dtos.PersonBeneficiaries>, int>(personBeneficiariesCollection, totalRecords);
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.PersonBeneficiaries>, int>(new List<Dtos.PersonBeneficiaries>(), 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonBeneficiaries from its GUID
        /// </summary>
        /// <returns>PersonBeneficiaries DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonBeneficiaries> GetPersonBeneficiariesByGuidAsync(string guid)
        {
            try
            {
                return await ConvertPersonBeneficiariesEntityToDto((await _personBeneficiariesRepository.GetPersonBeneficiariesAsync(0, 0, true)).Item1.Where(r => r.Id == guid).First(), true);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("person-beneficiaries not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("person-beneficiaries not found for GUID " + guid, ex);
            }
        }

        //get beneficiary-preference-types
        private IEnumerable<BeneficiaryTypes> _beneficiaryTypes = null;
        private async Task<IEnumerable<BeneficiaryTypes>> GetBeneficiaryPreferenceTypesAsync(bool bypassCache)
        {
            if (_beneficiaryTypes == null)
            {
                _beneficiaryTypes = await _referenceDataRepository.GetBeneficiaryTypesAsync(bypassCache);
            }
            return _beneficiaryTypes;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Perben domain entity to its corresponding PersonBeneficiaries DTO
        /// </summary>
        /// <param name="source">Perben domain entity</param>
        /// <returns>PersonBeneficiaries DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PersonBeneficiaries> ConvertPersonBeneficiariesEntityToDto(PersonBeneficiary source, bool bypassCache)
        {
            var personBeneficiaries = new Ellucian.Colleague.Dtos.PersonBeneficiaries();

            personBeneficiaries.Id = source.Id;
            var deductionGuid = await _personBeneficiariesRepository.GetGuidFromIdAsync(source.DeductionArrangement, "PERBEN");
            personBeneficiaries.DeductionArrangement = new GuidObject2(deductionGuid);
            var typeEntities = await GetBeneficiaryPreferenceTypesAsync(bypassCache);

            if (!string.IsNullOrEmpty(source.PerbenBeneficiaryId))
            {
				if(source.Person)
                {
					var personGuid = await _personBeneficiariesRepository.GetGuidFromIdAsync(source.DeductionArrangement, "PERSON");
                    personBeneficiaries.Beneficiary = new Dtos.DtoProperties.PersonBeneficiariesBeneficiary()
                    {
                        Person = new GuidObject2(personGuid)
                    };
                }
				else if(source.Organization)
                {
                    var personGuid = await _personBeneficiariesRepository.GetGuidFromIdAsync(source.DeductionArrangement, "PERSON");
                    personBeneficiaries.Beneficiary = new Dtos.DtoProperties.PersonBeneficiariesBeneficiary()
                    {
                        Organization = new GuidObject2(personGuid)
                    };
                }
				else if (source.Institution)
                {
                    var personGuid = await _personBeneficiariesRepository.GetGuidFromIdAsync(source.DeductionArrangement, "PERSON");
                    personBeneficiaries.Beneficiary = new Dtos.DtoProperties.PersonBeneficiariesBeneficiary()
                    {
                        Institution = new GuidObject2(personGuid)
                    };
                }
                else
                {
                    throw new InvalidOperationException("A beneficiary is required for person-beneficiary.");
                }
                if (typeEntities.Any())
                {
                    var typeEntity = typeEntities.FirstOrDefault(ep => ep.Code == source.PerbenBeneficiaryType);
                    if (typeEntity != null)
                    {
                        personBeneficiaries.Preference = new GuidObject2(typeEntity.Guid);
                    }
                }
                if (source.PerbenBfcyDesgntnPct != null)
                {
                    personBeneficiaries.Designation = new Dtos.DtoProperties.PersonBeneficiariesDesignation()
                    {
                        Percentage = (decimal)source.PerbenBfcyDesgntnPct
                    };
                }
                personBeneficiaries.StartOn = source.PerbenBfcyStartDate;
                personBeneficiaries.EndOn = source.PerbenBfcyEndDate;
            }
            else if (!string.IsNullOrEmpty(source.PerbenOrgBeneficiary))
            {
                personBeneficiaries.Beneficiary = new Dtos.DtoProperties.PersonBeneficiariesBeneficiary()
                    {
                        Name = source.PerbenOrgBeneficiary
                    };

                if (typeEntities.Any())
                {
                    var typeEntity = typeEntities.FirstOrDefault(ep => ep.Code == source.PerbenOrgBfcyType);
                    if (typeEntity != null)
                    {
                        personBeneficiaries.Preference = new GuidObject2(typeEntity.Guid);
                    }
                }
                if (source.PerbenOrgBfcyDesgntnPct != null)
                {
                    personBeneficiaries.Designation = new Dtos.DtoProperties.PersonBeneficiariesDesignation()
                    {
                        Percentage = (decimal)source.PerbenOrgBfcyDesgntnPct
                    };
                }
                personBeneficiaries.StartOn = source.PerbenOrgStartDate;
                personBeneficiaries.EndOn = source.PerbenOrgEndDate;
            }

            return personBeneficiaries;
        }

    }

}