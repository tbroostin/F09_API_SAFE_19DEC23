// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentConfigurationService : IStudentConfigurationService
    {
        private readonly IStudentConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        public StudentConfigurationService(IStudentConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ILogger logger)
        {
            _configurationRepository = configurationRepository;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Get Student Profile configurations
        /// </summary>
        /// <returns>StudentProfileConfiguration object</returns>
        public async Task<Dtos.Student.StudentProfileConfiguration> GetStudentProfileConfigurationAsync()
        {
            Dtos.Student.StudentProfileConfiguration studenProfileConfigurationDto = new Dtos.Student.StudentProfileConfiguration();
            Ellucian.Colleague.Domain.Student.Entities.StudentProfileConfiguration configuration = await _configurationRepository.GetStudentProfileConfigurationAsync();
            var configurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentProfileConfiguration, Dtos.Student.StudentProfileConfiguration>();
            studenProfileConfigurationDto = configurationDtoAdapter.MapToType(configuration);
            return studenProfileConfigurationDto;
        }
    }
}
