// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class MiscellaneousTextService : BaseCoordinationService, IMiscellaneousTextService
    {
        private readonly IMiscellaneousTextRepository _miscellaneousTextRepository;
        private readonly ILogger repoLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacilitiesService"/> class.
        /// </summary>
        /// <param name="miscellaneousTextRepository">The configuration repository.</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public MiscellaneousTextService(IMiscellaneousTextRepository miscellaneousTextRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _miscellaneousTextRepository = miscellaneousTextRepository;
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.repoLogger = logger;
        }

        /// <summary>
        /// Calls the configuration repository and returns a list of Miscellaneous Text DTOs
        /// </summary>
        /// <returns><see cref="IEnumerable<MiscellaneousText>">IEnumerable of Miscellaneous Text</see></returns>
        public async Task<IEnumerable<Dtos.Base.MiscellaneousText>> GetAllMiscellaneousTextAsync()
        {
            var miscellaneousTextEntities = await _miscellaneousTextRepository.GetAllMiscellaneousTextAsync();
            List<Dtos.Base.MiscellaneousText> dtoCollection = new List<Dtos.Base.MiscellaneousText>();
            foreach (var entity in miscellaneousTextEntities)
            {
                dtoCollection.Add(new Dtos.Base.MiscellaneousText() { Id = entity.Id, Message = entity.Message });
            }
            return dtoCollection;
        }
    }
}
