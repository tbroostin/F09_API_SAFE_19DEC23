// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Service for tax form statements
    /// </summary>
    [RegisterType]
    public class ColleagueFinanceTaxFormStatementService : BaseCoordinationService, IColleagueFinanceTaxFormStatementService
    {
        private IColleagueFinanceTaxFormStatementRepository taxFormStatementRepository;

        /// <summary>
        /// Constructor for a tax form statement service
        /// </summary>
        /// <param name="taxFormStatementRepository">Tax Form Statement Repository</param>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="currentUserFactory">Current user factory</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="logger">Logger</param>
        public ColleagueFinanceTaxFormStatementService(IColleagueFinanceTaxFormStatementRepository taxFormStatementRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.taxFormStatementRepository = taxFormStatementRepository;
        }

        /// <summary>
        /// Retrieve a set of tax form statements DTOs for the specified person and type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc.</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<Dtos.Base.TaxFormStatement3>> Get2Async(string personId, string taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            switch (taxForm)
            {
                case TaxFormTypes.FormT4A:
                    if (!(HasPermission(ColleagueFinancePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)) && !HasPermission(ColleagueFinancePermissionCodes.ViewRecipientT4A))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case TaxFormTypes.Form1099MI:
                    if (!(HasPermission(ColleagueFinancePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-MISC data.");
                    }
                    break;

                    if (!(HasPermission(ColleagueFinancePermissionCodes.View1099NEC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-NEC data.");
                    }
                    break;
                case TaxFormTypes.Form1099NEC:
                    if (!(HasPermission(ColleagueFinancePermissionCodes.View1099NEC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-NEC data.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("taxForm", "taxForm '" + taxForm.ToString() + "' is not a valid CF taxForm.");
            }

            // Get T4A/1099-MISC/1099-NEC tax form statement entities.
            var taxFormEntities = await taxFormStatementRepository.Get2Async(personId, taxForm);

            if (taxFormEntities == null)
                throw new ApplicationException("taxFormEntities cannot be null.");

            foreach (var taxFormEntity in taxFormEntities)
            {
                // Validate that the domain entity recipient ID is the same as the person ID requested.
                if (taxFormEntity.PersonId != personId)
                {
                    throw new PermissionsException("Insufficient access to tax form statements data.");
                }
            }

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TaxFormStatement3, Dtos.Base.TaxFormStatement3>();

            var statementDtos = new List<Dtos.Base.TaxFormStatement3>();
            foreach (var entity in taxFormEntities)
            {
                if (entity != null)
                {
                    // Convert the domain entity into a DTO.
                    var statementDto = adapter.MapToType(entity);

                    // If the statement is not available, remove the record ID.
                    if (statementDto.Notation == Dtos.Base.TaxFormNotations2.NotAvailable)
                    {
                        statementDto.PdfRecordId = string.Empty;
                    }
                    statementDtos.Add(statementDto);
                }
            }

            return statementDtos;
        }


        #region OBSOLETE METHODS

        /// <summary>
        /// Retrieve a set of tax form statements DTOs for the specified person and type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get2Async instead.")]
        public async Task<IEnumerable<Dtos.Base.TaxFormStatement2>> GetAsync(string personId, Dtos.Base.TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            // Convert the tax form dto into the domain tax form
            TaxForms taxFormDomainId = new TaxForms();
            switch (taxForm)
            {
                case Dtos.Base.TaxForms.FormT4A:
                    taxFormDomainId = TaxForms.FormT4A;
                    if (!(HasPermission(ColleagueFinancePermissionCodes.ViewT4A) && CurrentUser.IsPerson(personId)) && !HasPermission(ColleagueFinancePermissionCodes.ViewRecipientT4A))
                    {
                        throw new PermissionsException("Insufficient access to T4A data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1099MI:
                    taxFormDomainId = TaxForms.Form1099MI;
                    if (!(HasPermission(ColleagueFinancePermissionCodes.View1099MISC) && CurrentUser.IsPerson(personId)))
                    {
                        throw new PermissionsException("Insufficient access to 1099-MISC data.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("taxForm", "taxForm '" + taxForm.ToString() + "' is not a valid CF taxForm.");
            }

            // Get T4A/1099MI tax form statement entities
            var taxFormEntities = await taxFormStatementRepository.GetAsync(personId, taxFormDomainId);

            if (taxFormEntities == null)
                throw new ApplicationException("taxFormEntities cannot be null.");

            foreach (var taxFormEntity in taxFormEntities)
            {
                // Validate that the domain entity recipient ID is the same as the person ID requested.
                if (taxFormEntity.PersonId != personId)
                {
                    throw new PermissionsException("Insufficient access to tax form statements data.");
                }
            }

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>();

            var statementDtos = new List<Dtos.Base.TaxFormStatement2>();
            foreach (var entity in taxFormEntities)
            {
                if (entity != null)
                {
                    // Convert the domain entity into a DTO
                    var statementDto = adapter.MapToType(entity);
                    // If the statement is not available, remove the record ID.
                    if (statementDto.Notation == Dtos.Base.TaxFormNotations2.NotAvailable)
                    {
                        statementDto.PdfRecordId = string.Empty;
                    }
                    statementDtos.Add(statementDto);
                }
            }

            return statementDtos;
        }

        #endregion
    }
}
