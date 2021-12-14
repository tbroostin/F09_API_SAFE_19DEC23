// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using System.Linq;
using slf4net;
using System;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class PhotoService : BaseCoordinationService, IPhotoService
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IProxyRepository _proxyRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="photoRepository">The photo repository.</param>
        /// <param name="currentUserFactory">The current user factory</param>
        /// <param name="logger">The logger</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="proxyRepository">Proxy repository</param>
        public PhotoService(IPhotoRepository photoRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IProxyRepository proxyRepository,
            IAdapterRegistry adapterRegistry,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this._photoRepository = photoRepository;
            this._proxyRepository = proxyRepository;
        }

        public async Task<Photograph> GetPersonPhotoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.Error("Invalid id parameter: null or empty.");
                throw new ArgumentNullException("id", "No person ID provided.");
            }
            await VerifyUserCanViewPhotosAsync(id);

            // Following is still synchonous.
            return _photoRepository.GetPersonPhoto(id);
        }

        private async Task VerifyUserCanViewPhotosAsync(string id)
        {
            // A person is allowed to see their own photo. They are allowed to see another's photo if they are a proxy for that user or have the correct permission

            if (CurrentUser.IsPerson(id) || HasPermission(BasePermissionCodes.CanViewPersonPhotos) || HasProxyAccessForPerson(id))
            {
                return;
            }
            // If the person is not currently acting as this proxy, but this person is one of the user's possible proxy subjects
            // allow them to still see the proxy's photo. Need to get all subjects.
            var hasThisProxySubject = await PersonIsProxySubjectOfUserAsync(id);
            if (hasThisProxySubject)
            {
                return;
            }
            // If not one of the above conditions is true, the user does not have permissions to access this photo and we throw this exception
            throw new PermissionsException("You do not have permission to view photos for others.");
        }

        private async Task<bool> PersonIsProxySubjectOfUserAsync(string personId)
        {
            var proxySubjects = await _proxyRepository.GetUserProxySubjectsAsync(CurrentUser.PersonId);

            if (proxySubjects != null && proxySubjects.Any())
            {
                foreach (var subject in proxySubjects)
                {
                    if (subject.Id == personId)
                    {
                        return true;
                    }
                }
            }
            return false;  
        }

    }
}
