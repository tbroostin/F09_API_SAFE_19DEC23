// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
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
        /// Get a set of TaxFormConsent2 DTOs for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Set of TaxFormConsent2 DTOs.</returns>
        public async Task<IEnumerable<Dtos.Base.TaxFormConsent2>> Get2Async(string personId, string taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            switch (taxForm)
            {
                case TaxFormTypes.FormW2C:
                case TaxFormTypes.FormW2:
                    if (!(HasPermission(BasePermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployeeW2))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;
                case TaxFormTypes.Form1095C:
                    if (!(HasPermission(BasePermissionCodes.View1095C) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployee1095C))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case TaxFormTypes.Form1098:
                    if (!(HasPermission(BasePermissionCodes.View1098) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewStudent1098) && !HasProxyAccessForPerson(personId))
                    {
                        throw new PermissionsException("Insufficient access to 1098 data.");
                    }
                    break;
                case TaxFormTypes.FormT4:
                    if (!(HasPermission(BasePermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployeeT4))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                case TaxFormTypes.FormT4A:
                    if (!(HasPermission(BasePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewRecipientT4A))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case TaxFormTypes.FormT2202A:
                    if (!(HasPermission(BasePermissionCodes.ViewT2202A) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewStudentT2202A))
                    {
                        throw new PermissionsException("Insufficient access to T2202A data.");
                    }
                    break;
                case TaxFormTypes.Form1099MI:
                    if (!(HasPermission(BasePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099MI data.");
                    }
                    break;
                case TaxFormTypes.Form1099NEC:
                    if (!(HasPermission(BasePermissionCodes.View1099NEC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099NEC data.");
                    }
                    break;
                default:
                    throw new PermissionsException("Invalid taxform.");
            }

            var taxFormConsent2Dtos = new List<Dtos.Base.TaxFormConsent2>();
            var taxFormConsents = await this.taxFormConsentRepository.Get2Async(personId, taxForm);

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TaxFormConsent2, Dtos.Base.TaxFormConsent2>();
            foreach (var item in taxFormConsents)
            {
                var dto = adapter.MapToType(item);
                taxFormConsent2Dtos.Add(dto);
            }

            return taxFormConsent2Dtos;
        }

        /// <summary>
        /// Create a new TaxFormConsent2 record.
        /// </summary>
        /// <param name="newTaxFormConsent2Dto">TaxFormConsent2 DTO</param>
        /// <returns>TaxFormConsent2 DTO</returns>
        public async Task<Dtos.Base.TaxFormConsent2> Post2Async(Dtos.Base.TaxFormConsent2 newTaxFormConsent2Dto)
        {
            if (newTaxFormConsent2Dto == null)
                throw new ArgumentNullException("newTaxFormConsentDto", "newTaxFormConsentDto cannot be null.");

            string personId = newTaxFormConsent2Dto.PersonId;
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            string taxFormType = newTaxFormConsent2Dto.TaxForm;
            if (string.IsNullOrEmpty(taxFormType))
                throw new ArgumentNullException("taxFormType", "The tax form type must be specified.");

            switch (taxFormType)
            {
                case TaxFormTypes.FormW2:
                    if (!(HasPermission(BasePermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;
                case TaxFormTypes.Form1095C:
                    if (!(HasPermission(BasePermissionCodes.View1095C) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case TaxFormTypes.Form1098:
                    if (!(HasPermission(BasePermissionCodes.View1098) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1098 data.");
                    }
                    break;
                case TaxFormTypes.FormT4:
                    if (!(HasPermission(BasePermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                case TaxFormTypes.FormT4A:
                    if (!(HasPermission(BasePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case TaxFormTypes.FormT2202A:
                    if (!(HasPermission(BasePermissionCodes.ViewT2202A) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to T2202A data.");
                    }
                    break;
                case TaxFormTypes.Form1099MI:
                    if (!(HasPermission(BasePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-MISC data.");
                    }
                    break;
                case TaxFormTypes.Form1099NEC:
                    if (!(HasPermission(BasePermissionCodes.View1099NEC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-NEC data.");
                    }
                    break;
                default:
                    throw new PermissionsException("Invalid taxform.");
            }

            var adapter = _adapterRegistry.GetAdapter<Dtos.Base.TaxFormConsent2, Domain.Base.Entities.TaxFormConsent2>();
            var newTaxFormConsent2Entity = adapter.MapToType(newTaxFormConsent2Dto);

            var consentEntity = await taxFormConsentRepository.Post2Async(newTaxFormConsent2Entity);

            return newTaxFormConsent2Dto;
        }

        /// <summary>
        /// Determines if the current user can view, as an administrator,
        /// the tax forms for the recipient.
        /// </summary>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Task(boolean)</returns>
        public async Task<bool> CanViewTaxDataWithOrWithoutConsent2Async(string taxForm)
        {
            var canView = false;
            switch (taxForm)
            {
                case TaxFormTypes.FormW2:
                    canView = HasPermission(BasePermissionCodes.ViewEmployeeW2);
                    break;
                case TaxFormTypes.FormW2C:
                    canView = HasPermission(BasePermissionCodes.ViewEmployeeW2);
                    break;
                case TaxFormTypes.Form1095C:
                    canView = HasPermission(BasePermissionCodes.ViewEmployee1095C);
                    break;
                case TaxFormTypes.Form1098:
                    canView = HasPermission(BasePermissionCodes.ViewStudent1098);
                    break;
                case TaxFormTypes.FormT2202A:
                    canView = HasPermission(BasePermissionCodes.ViewStudentT2202A);
                    break;
                case TaxFormTypes.FormT4:
                    canView = HasPermission(BasePermissionCodes.ViewEmployeeT4);
                    break;
                case TaxFormTypes.FormT4A:
                    canView = HasPermission(BasePermissionCodes.ViewRecipientT4A);
                    break;
            }
            return await Task.FromResult(canView);
        }

        #region OBSOLETE METHODS

        /// <summary>
        /// Get a set of TaxFormConsent DTOs for the specified person.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Set of TaxFormConsent DTOs.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        public async Task<IEnumerable<Dtos.Base.TaxFormConsent>> GetAsync(string personId, Dtos.Base.TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "Person ID must be specified.");

            Domain.Base.Entities.TaxForms taxFormDomain = new Domain.Base.Entities.TaxForms();

            switch (taxForm)
            {
                case Dtos.Base.TaxForms.FormW2C:
                case Dtos.Base.TaxForms.FormW2:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormW2;
                    if (!(HasPermission(BasePermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployeeW2))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1095C:
                    taxFormDomain = Domain.Base.Entities.TaxForms.Form1095C;
                    if (!(HasPermission(BasePermissionCodes.View1095C) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployee1095C))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1098:
                    taxFormDomain = Domain.Base.Entities.TaxForms.Form1098;
                    if (!(HasPermission(BasePermissionCodes.View1098) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewStudent1098) && !HasProxyAccessForPerson(personId))
                    {
                        throw new PermissionsException("Insufficient access to 1098 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormT4;
                    if (!(HasPermission(BasePermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewEmployeeT4))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4A:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormT4A;
                    if (!(HasPermission(BasePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewRecipientT4A))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT2202A:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormT2202A;
                    if (!(HasPermission(BasePermissionCodes.ViewT2202A) && CurrentUser.IsPerson(personId)) && !HasPermission(BasePermissionCodes.ViewStudentT2202A))
                    {
                        throw new PermissionsException("Insufficient access to T2202A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1099MI:
                    taxFormDomain = Domain.Base.Entities.TaxForms.Form1099MI;
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
        /// Create a new TaxFormConsent record.
        /// </summary>
        /// <param name="newTaxFormConsentDto">TaxFormConsent DTO</param>
        /// <returns>TaxFormConsent DTO</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Post2Async instead.")]
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

        /// <summary>
        /// Determines if the current user can view, as an administrator,
        /// the tax forms for the recipient.
        /// </summary>
        /// <param name="taxForm"></param>
        /// <returns>Task(boolean)</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use CanViewTaxDataWithOrWithoutConsent2Async instead.")]
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

        #endregion

    }
}
