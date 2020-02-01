// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Service for tax form statements
    /// </summary>
    [RegisterType]
    public class StudentTaxFormStatementService : BaseCoordinationService, IStudentTaxFormStatementService
    {
        private IStudentTaxFormStatementRepository taxFormStatementRepository;
        private IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor for a tax form statement service
        /// </summary>
        /// <param name="taxFormStatementRepository"Tax Form Statement Repository</param>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="currentUserFactory">Current user factory</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="logger">Logger</param>
        public StudentTaxFormStatementService(IStudentTaxFormStatementRepository taxFormStatementRepository, IConfigurationRepository configurationService, IAdapterRegistry adapterRegistry,
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
        /// <param name="taxForm">Type of tax form</param>
        /// <returns>Set of tax form statements</returns>
        public async Task<IEnumerable<Dtos.Base.TaxFormStatement2>> GetAsync(string personId, Dtos.Base.TaxForms taxForm)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is a required field.");

            // Convert the tax form dto into the domain tax form
            TaxForms taxFormDomainId = new TaxForms();
            switch (taxForm)
            {
                case Dtos.Base.TaxForms.Form1098:
                    taxFormDomainId = TaxForms.Form1098;
                    if (!(HasPermission(StudentPermissionCodes.View1098) && CurrentUser.IsPerson(personId)) && !HasPermission(StudentPermissionCodes.ViewStudent1098) && !HasProxyAccessForPerson(personId))
                    {
                        throw new PermissionsException("Insufficient access to 1098 data.");
                    }
                    break;
                case Dtos.Base.TaxForms.FormT2202A:
                    taxFormDomainId = TaxForms.FormT2202A;
                    if (!(HasPermission(StudentPermissionCodes.ViewT2202A) && CurrentUser.IsPerson(personId)) && !HasPermission(StudentPermissionCodes.ViewStudentT2202A))
                    {
                        throw new PermissionsException("Insufficient access to T2202A data.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("taxForm", "taxForm '" + taxForm.ToString() + "' is not a valid ST taxForm.");
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

            TaxFormConfiguration taxFormConfiguration = null;
            TaxFormConfiguration taxFormConfigurationFor1098T = null;
            TaxFormConfiguration taxFormConfigurationFor1098E = null;
            // Get tax form availability
            if (taxFormDomainId == TaxForms.Form1098)
            {
                // An institution may only support one of the forms so there should only be an exception if both 1098T and 1098E configurations are missing.
                taxFormConfigurationFor1098T = await configurationRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);
                taxFormConfigurationFor1098E = await configurationRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098E);
                if (taxFormConfigurationFor1098E == null && taxFormConfigurationFor1098T == null)
                {
                    throw new ApplicationException("taxFormConfigurationFor1098E and taxFormConfigurationFor1098T cannot both be null.");
                }
            }
            else
            {
                taxFormConfiguration = await configurationRepository.GetTaxFormAvailabilityConfigurationAsync(taxFormDomainId);
                if (taxFormConfiguration == null)
                    throw new ApplicationException("taxFormConfiguration cannot be null.");
            }

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
                        case TaxForms.Form1098:
                            if (entity.TaxForm == TaxForms.Form1098T)
                            {
                                // Get the availability record for the 1098T for the statement tax year (if one exists)
                                TaxFormAvailability availability1098T = null;
                                if (taxFormConfigurationFor1098T != null && taxFormConfigurationFor1098T.Availabilities != null)
                                {
                                    availability1098T = taxFormConfigurationFor1098T.Availabilities.FirstOrDefault(x => x.TaxYear == entity.TaxYear);
                                }

                                // Mark the statement as Not Available and remove the PDF record ID,
                                // if the following conditions are true:
                                //   1) The availablility entry is not present
                                //   2) The available date is in the future
                                if (availability1098T == null || !availability1098T.Available)
                                {
                                    statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                    statementDto.PdfRecordId = string.Empty;
                                }
                            }
                            else
                            {
                                // Get the availability record for the 1098E for the statement tax year (if one exists)
                                TaxFormAvailability availability1098E = null;
                                if (taxFormConfigurationFor1098E != null && taxFormConfigurationFor1098E.Availabilities != null)
                                {
                                    availability1098E = taxFormConfigurationFor1098E.Availabilities.FirstOrDefault(x => x.TaxYear == entity.TaxYear);
                                }

                                // Mark the statement as Not Available and remove the PDF record ID,
                                // if the following conditions are true:
                                //   1) The availablility entry is not present
                                //   2) The available date is in the future
                                if (availability1098E == null || !availability1098E.Available)
                                {
                                    statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                    statementDto.PdfRecordId = string.Empty;
                                }
                            }
                            break;
                        case TaxForms.FormT2202A:
                            // Get the availability record for the T2202A for the statement tax year
                            TaxFormAvailability availabilityT2202a = taxFormConfiguration.Availabilities.FirstOrDefault(x => x.TaxYear == entity.TaxYear);

                            // Mark the statement as Not Available and remove the PDF record ID,
                            // if the following conditions are true:
                            //   1) The availablility entry is not present
                            //   2) The available date is in the future
                            if (availabilityT2202a == null || !availabilityT2202a.Available)
                            {
                                statementDto.Notation = Dtos.Base.TaxFormNotations2.NotAvailable;
                                statementDto.PdfRecordId = string.Empty;
                            }

                            // Remove the PdfRecordId if the statement is marked as cancelled.
                            if (statementDto.Notation == Dtos.Base.TaxFormNotations2.Cancelled)
                            {
                                statementDto.PdfRecordId = string.Empty;
                            }

                            break;
                    }

                    statementDtos.Add(statementDto);
                }
            }

            return statementDtos;
        }
    }
}
