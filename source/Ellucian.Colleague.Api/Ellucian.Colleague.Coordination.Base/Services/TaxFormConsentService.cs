// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Service for tax form consents.
    /// </summary>
    [RegisterType]
    public class TaxFormConsentService : BaseCoordinationService, ITaxFormConsentService
    {
        private ITaxFormConsentRepository taxFormConsentRepository;

        /// <summary>
        /// Constructor TaxFormConsentService
        /// </summary>
        /// <param name="taxFormConsentRepository">TaxFormConsentRepository</param>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public TaxFormConsentService(ITaxFormConsentRepository taxFormConsentRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.taxFormConsentRepository = taxFormConsentRepository;
        }

        /// <summary>
        /// Get a set of TaxFormConsent DTOs for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of TaxFormConsent DTOs.</returns>
        public async Task<IEnumerable<Dtos.Base.TaxFormConsent>> GetAsync(string personId, Dtos.Base.TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            TaxForms taxFormDomain = new TaxForms();

            switch (taxForm)
            {
                case Dtos.Base.TaxForms.FormW2C:
                case Dtos.Base.TaxForms.FormW2:
                    taxFormDomain = TaxForms.FormW2;
                    if (!(HasPermission(BasePermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployeeW2))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;       
                case Dtos.Base.TaxForms.Form1095C:
                    taxFormDomain = TaxForms.Form1095C;
                    if (!(HasPermission(BasePermissionCodes.View1095C) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployee1095C))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1098:
                    taxFormDomain = TaxForms.Form1098;
                    if (!(HasPermission(BasePermissionCodes.View1098) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewStudent1098) && !HasProxyAccessForPerson(personId))
                    {
                        throw new PermissionsException("Insufficient access to 1098 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4:
                    taxFormDomain = TaxForms.FormT4;
                    if (!(HasPermission(BasePermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployeeT4))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4A:
                    taxFormDomain = TaxForms.FormT4A;
                    if (!(HasPermission(BasePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewRecipientT4A))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT2202A:
                    taxFormDomain = TaxForms.FormT2202A;
                    if (!(HasPermission(BasePermissionCodes.ViewT2202A) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewStudentT2202A))
                    {
                        throw new PermissionsException("Insufficient access to T2202A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1099MI:
                    taxFormDomain = TaxForms.Form1099MI;
                    if (!(HasPermission(BasePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099MI data.");
                    }
                    break;
            }

            var taxFormConsents = await this.taxFormConsentRepository.GetAsync(personId, taxFormDomain);

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TaxFormConsent, Dtos.Base.TaxFormConsent>();
            var taxFormConsentsDtos = new List<Dtos.Base.TaxFormConsent>();
            foreach (var item in taxFormConsents)
            {
                var dto = adapter.MapToType(item);
                taxFormConsentsDtos.Add(dto);
            }

            return taxFormConsentsDtos;
        }

        /// <summary>
        /// Determines if the current user can view employee / student tax forms as an administrator
        /// </summary>
        /// <param name="taxForm"></param>
        /// <returns>Task(boolean)</returns>
        public async Task<bool> CanViewTaxDataWithOrWithoutConsent(Dtos.Base.TaxForms taxForm)
        {
            var canView = false;
            switch (taxForm)
            {
                case Dtos.Base.TaxForms.FormW2:
                    canView = HasPermission(BasePermissionCodes.ViewEmployeeW2);
                    break;
                case Dtos.Base.TaxForms.FormW2C:
                    canView = HasPermission(BasePermissionCodes.ViewEmployeeW2);
                    break;
                case Dtos.Base.TaxForms.Form1095C:
                    canView = HasPermission(BasePermissionCodes.ViewEmployee1095C);
                    break;
                case Dtos.Base.TaxForms.Form1098:
                    canView = HasPermission(BasePermissionCodes.ViewStudent1098);
                    break;
                case Dtos.Base.TaxForms.FormT2202A:
                    canView = HasPermission(BasePermissionCodes.ViewStudentT2202A);
                    break;
                case Dtos.Base.TaxForms.FormT4:
                    canView = HasPermission(BasePermissionCodes.ViewEmployeeT4);
                    break;
                case Dtos.Base.TaxForms.FormT4A:
                    canView = HasPermission(BasePermissionCodes.ViewRecipientT4A);
                    break;
            }
            return await Task.FromResult(canView);
        }

        /// <summary>
        /// Create a new TaxFormConsent record.
        /// </summary>
        /// <param name="newTaxFormConsentDto">TaxFormConsent DTO</param>
        /// <returns>TaxFormConsent DTO</returns>
        public async Task<Dtos.Base.TaxFormConsent> PostAsync(Dtos.Base.TaxFormConsent newTaxFormConsentDto)
        {
            if (newTaxFormConsentDto == null)
                throw new ArgumentNullException("newTaxFormConsentDto", "newTaxFormConsentDto cannot be null.");

            string personId = newTaxFormConsentDto.PersonId;
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            switch (newTaxFormConsentDto.TaxForm)
            {
                case Dtos.Base.TaxForms.FormW2:
                    if (!(HasPermission(BasePermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1095C:
                    if (!(HasPermission(BasePermissionCodes.View1095C) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1098:
                    if (!(HasPermission(BasePermissionCodes.View1098) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1098 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4:
                    if (!(HasPermission(BasePermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4A:
                    if (!(HasPermission(BasePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT2202A:
                    if (!(HasPermission(BasePermissionCodes.ViewT2202A) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to T2202A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1099MI:
                    if (!(HasPermission(BasePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-MISC data.");
                    }
                    break;
                default:
                    throw new PermissionsException("Invalid taxform.");                    
            }

            var adapter = _adapterRegistry.GetAdapter<Dtos.Base.TaxFormConsent, Domain.Base.Entities.TaxFormConsent>();
            var newTaxFormConsentEntity = adapter.MapToType(newTaxFormConsentDto);

            var consentEntity = await taxFormConsentRepository.PostAsync(newTaxFormConsentEntity);

            return newTaxFormConsentDto;
        }
    }
}
