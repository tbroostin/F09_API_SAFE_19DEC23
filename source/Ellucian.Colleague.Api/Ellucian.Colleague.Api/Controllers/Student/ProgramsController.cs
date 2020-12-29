﻿// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.Requirements;
using System.Linq;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Filters;
using System.Threading.Tasks;
using Ellucian.Web.Utility;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Academic Program data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ProgramsController : BaseCompressedApiController
    {
        private readonly IProgramRepository _ProgramRepository;
        private readonly IProgramRequirementsRepository _ProgramRequirementsRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the ProgramsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="programRepository">Repository of type <see cref="IProgramRepository">IProgramRepository</see></param>
        /// <param name="programRequirementsRepository">Repository of type <see cref="IProgramRequirementsRepository">IProgramRequirementsRepository</see></param>
        public ProgramsController(IAdapterRegistry adapterRegistry, IProgramRepository programRepository, IProgramRequirementsRepository programRequirementsRepository)
        {
            _adapterRegistry = adapterRegistry;
            _ProgramRepository = programRepository;
            _ProgramRequirementsRepository = programRequirementsRepository;
        }

        /// <summary>
        /// Retrieves all Programs
        /// </summary>
        /// <returns>All <see cref="Program">Programs</see></returns>
        public async Task<IEnumerable<Program>> GetAsync()
        {
            var ProgramCollection = await _ProgramRepository.GetAsync();

            // Get the right adapter for the type mapping
            var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>();

            // Map the program entity to the program DTO
            var programDtoCollection = new List<Program>();
            foreach (var program in ProgramCollection)
            {
                programDtoCollection.Add(programDtoAdapter.MapToType(program));
            }
            return programDtoCollection;
        }

        /// <summary>
        /// Retrieves all active Programs.
        /// </summary>
        /// <returns>All active <see cref="Program">Programs</see></returns>
        [Obsolete("Obsolete as of API version 1.2, use version 2 of this API")]
        public async Task<IEnumerable<Program>> GetActiveProgramsAsync()
        {
            var ProgramCollection = (await _ProgramRepository.GetAsync()).Where(p => p.IsActive == true);

            // Get the right adapter for the type mapping
            var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>();

            // Map the program entity to the program DTO
            var programDtoCollection = new List<Program>();
            foreach (var program in ProgramCollection)
            {
                programDtoCollection.Add(programDtoAdapter.MapToType(program));
            }
            return programDtoCollection;
        }

        /// <summary>
        /// Retrieves all active Programs.
        /// </summary>
        /// <returns>All active <see cref="Program">Programs</see></returns>
        public async Task<IEnumerable<Program>> GetActivePrograms2Async(bool IncludeEndedPrograms = true)
        {
           
            var ProgramCollection = (IncludeEndedPrograms)?
                                    (await _ProgramRepository.GetAsync()).Where(p => p != null && p.IsActive == true &&
                                    p.IsSelectable == true)
                                    .ToList():
                                    (await _ProgramRepository.GetAsync()).Where(p => p != null && p.IsActive == true && 
                                    p.IsSelectable == true && (p.ProgramEndDate == null || 
                                    (p.ProgramEndDate.HasValue && p.ProgramEndDate >= DateTime.Now)))
                                    .ToList();          
            
            // Get the right adapter for the type mapping
            var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>();

            // Map the program entity to the program DTO
            var programDtoCollection = new List<Program>();
            foreach (var program in ProgramCollection)
            {
                programDtoCollection.Add(programDtoAdapter.MapToType(program));
            }
            return programDtoCollection;
        }

        /// <summary>
        /// Retrieves a single Program by ID.
        /// </summary>
        /// <param name="id">Id of program to retrieve</param>
        /// <returns>The requested <see cref="Program">Program</see></returns>
        public async Task<Program> GetAsync(string id)
        {
            var program = await _ProgramRepository.GetAsync(id);

            // Get the right adapter for the type mapping
            var programDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>();

            // Map the program entity to the program DTO
            var programDto = programDtoAdapter.MapToType(program);

            return programDto;
        }     

        /// <summary>
        /// Retrieves program requirements.
        /// </summary>
        /// <param name="id">Id of the program</param>
        /// <param name="catalog">Catalog code</param>
        /// <returns>The <see cref="ProgramRequirements">Program Requirements</see> for the program catalog combination.</returns>
        [ParameterSubstitutionFilter]
        public async Task<ProgramRequirements> GetRequirementsAsync(string id, string catalog)
        {
            var pr = await _ProgramRequirementsRepository.GetAsync(id, catalog);

            // Get the right adapter for the type mapping
            var programRequirementsDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.ProgramRequirements, ProgramRequirements>();

            // Map the program requirements entity to the program requirements DTO
            var programRequirementsDto = programRequirementsDtoAdapter.MapToType(pr);

            return programRequirementsDto;
        }
    }
}
