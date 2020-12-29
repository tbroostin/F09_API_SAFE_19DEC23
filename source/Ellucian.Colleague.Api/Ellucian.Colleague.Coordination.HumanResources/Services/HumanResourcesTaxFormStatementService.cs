﻿// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Service for tax form statements
    /// </summary>
    [RegisterType]
    public class HumanResourcesTaxFormStatementService : BaseCoordinationService, IHumanResourcesTaxFormStatementService
    {
        private IHumanResourcesTaxFormStatementRepository taxFormStatementRepository;
        private IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor for a tax form statement service
        /// </summary>
        /// <param name="taxFormStatementRepository"Tax Form Statement Repository</param>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="currentUserFactory">Current user factory</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="logger">Logger</param>
        public HumanResourcesTaxFormStatementService(IHumanResourcesTaxFormStatementRepository taxFormStatementRepository, IConfigurationRepository configurationService, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.taxFormStatementRepository = taxFormStatementRepository;
            this.configurationRepository = configurationService;
        }

        /// <summary>
        /// Retrieve a set of tax form statements DTOs for the specified person and type of tax form.
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="taxForm">The type of tax form: 1099-MISC, W-2, etc</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<Dtos.Base.TaxFormStatement3>> Get2Async(string personId, string taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            switch (taxForm)
            {
                case TaxFormTypes.FormW2:
                    if (!(HasPermission(HumanResourcesPermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)) && !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeW2))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;
                case TaxFormTypes.Form1095C:
                    if (!(HasPermission(HumanResourcesPermissionCodes.View1095C) && CurrentUser.IsPerson(personId)) && !HasPermission(HumanResourcesPermissionCodes.ViewEmployee1095C))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case TaxFormTypes.FormT4:
                    if (!(HasPermission(HumanResourcesPermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)) && !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeT4))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("taxForm", "taxForm '" + taxForm.ToString() + "' is not a valid HR taxForm.");
            }

            // Get tax form statement entities
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

            // Get tax form availability
            var taxFormConfiguration = await configurationRepository.GetTaxFormAvailabilityConfiguration2Async(taxForm);

            if (taxFormEntities == null)
                throw new ApplicationException("taxFormEntities cannot be null.");

            if (taxFormConfiguration == null)
                throw new ApplicationException("taxFormConfiguration cannot be null.");

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TaxFormStatement3, Dtos.Base.TaxFormStatement3>();

            var statementDtos = new List<Dtos.Base.TaxFormStatement3>();

            foreach (var entity in taxFormEntities)
            {
                if (entity != null)
                {
                    // Convert the domain entity into a DTO
                    var statementDto = adapter.MapToType(entity);

                    switch (taxForm)
                    {
                        case TaxFormTypes.FormW2:
                            // Get the availability record for the W-2 for the statement tax year
                            TaxFormAvailability w2Availability = null;

                            if (taxFormConfiguration.Availabilities != null)
                            {
                                foreach (var avail in taxFormConfiguration.Availabilities)
                                {
                                    if (avail.TaxYear == entity.TaxYear)
                                    {
                                        w2Availability = avail;
                                    }
                                }
                            }

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (w2Availability == null || !w2Availability.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }

                            // If the W-2 is a correction and not a W-2c, remove the PDF record ID.
                            if (statementDto.Notation == Dtos.Base.TaxFormNotations2.Correction && statementDto.TaxForm != TaxFormTypes.FormW2C)
                                statementDto.PdfRecordId = string.Empty;
                            break;

                        case TaxFormTypes.Form1095C:
                            // Get the availability record for the 1095-C for the statement tax year
                            TaxFormAvailability availability1095C = null;
                            if (taxFormConfiguration.Availabilities != null)
                            {
                                foreach (var avail in taxFormConfiguration.Availabilities)
                                {
                                    if (avail.TaxYear == entity.TaxYear)
                                    {
                                        availability1095C = avail;
                                    }
                                }
                            }

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (availability1095C == null || !availability1095C.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }
                            break;

                        case TaxFormTypes.FormT4:
                            // Get the availability record for the T4 for the statement tax year
                            TaxFormAvailability t4Availability = null;

                            if (taxFormConfiguration.Availabilities != null)
                            {
                                foreach (var avail in taxFormConfiguration.Availabilities)
                                {
                                    if (avail.TaxYear == entity.TaxYear)
                                    {
                                        t4Availability = avail;
                                    }
                                }
                            }

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (t4Availability == null || !t4Availability.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }

                            // If the T4 is a correction, remove the PDF record ID.
                            if (statementDto.Notation == Dtos.Base.TaxFormNotations2.Correction)
                                statementDto.PdfRecordId = string.Empty;
                            break;
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
                case Dtos.Base.TaxForms.FormW2:
                    taxFormDomainId = TaxForms.FormW2;
                    if (!(HasPermission(HumanResourcesPermissionCodes.ViewW2) && CurrentUser.IsPerson(personId)) && !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeW2))
                    {
                        throw new PermissionsException("Insufficient access to W2 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.Form1095C:
                    taxFormDomainId = TaxForms.Form1095C;
                    if (!(HasPermission(HumanResourcesPermissionCodes.View1095C) && CurrentUser.IsPerson(personId)) && !HasPermission(HumanResourcesPermissionCodes.ViewEmployee1095C))
                    {
                        throw new PermissionsException("Insufficient access to 1095C data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT4:
                    taxFormDomainId = TaxForms.FormT4;
                    if (!(HasPermission(HumanResourcesPermissionCodes.ViewT4) && CurrentUser.IsPerson(personId)) && !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeT4))
                    {
                        throw new PermissionsException("Insufficient access to T4 data.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("taxForm", "taxForm '" + taxForm.ToString() + "' is not a valid HR taxForm.");
            }

            // Get tax form statement entities
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

            // Get tax form availability
            var taxFormConfiguration = await configurationRepository.GetTaxFormAvailabilityConfigurationAsync(taxFormDomainId);

            if (taxFormEntities == null)
                throw new ApplicationException("taxFormEntities cannot be null.");

            if (taxFormConfiguration == null)
                throw new ApplicationException("taxFormConfiguration cannot be null.");

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>();

            var statementDtos = new List<Dtos.Base.TaxFormStatement2>();
            foreach (var entity in taxFormEntities)
            {
                if (entity != null)
                {
                    // Convert the domain entity into a DTO
                    var statementDto = adapter.MapToType(entity);

                    switch (taxFormDomainId)
                    {
                        case TaxForms.FormW2:
                            // Get the availability record for the W-2 for the statement tax year
                            TaxFormAvailability w2Availability = null;

                            if (taxFormConfiguration.Availabilities != null)
                            {
                                foreach (var avail in taxFormConfiguration.Availabilities)
                                {
                                    if (avail.TaxYear == entity.TaxYear)
                                    {
                                        w2Availability = avail;
                                    }
                                }
                            }

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (w2Availability == null || !w2Availability.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }

                            // If the W-2 is a correction and not a W-2c, remove the PDF record ID.
                            if (statementDto.Notation == Dtos.Base.TaxFormNotations2.Correction && statementDto.TaxForm != Dtos.Base.TaxForms.FormW2C)
                                statementDto.PdfRecordId = string.Empty;
                            break;

                        case TaxForms.Form1095C:
                            // Get the availability record for the 1095-C for the statement tax year
                            TaxFormAvailability availability1095C = null;
                            if (taxFormConfiguration.Availabilities != null)
                            {
                                foreach (var avail in taxFormConfiguration.Availabilities)
                                {
                                    if (avail.TaxYear == entity.TaxYear)
                                    {
                                        availability1095C = avail;
                                    }
                                }
                            }

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (availability1095C == null || !availability1095C.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }
                            break;

                        case TaxForms.FormT4:
                            // Get the availability record for the T4 for the statement tax year
                            TaxFormAvailability t4Availability = null;

                            if (taxFormConfiguration.Availabilities != null)
                            {
                                foreach (var avail in taxFormConfiguration.Availabilities)
                                {
                                    if (avail.TaxYear == entity.TaxYear)
                                    {
                                        t4Availability = avail;
                                    }
                                }
                            }

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (t4Availability == null || !t4Availability.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }

                            // If the T4 is a correction, remove the PDF record ID.
                            if (statementDto.Notation == Dtos.Base.TaxFormNotations2.Correction)
                                statementDto.PdfRecordId = string.Empty;
                            break;
                    }

                    statementDtos.Add(statementDto);
                }
            }

            return statementDtos;
        }

        #endregion
    }
}
