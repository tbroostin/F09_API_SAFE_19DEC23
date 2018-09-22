// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class EmailTypeService : BaseCoordinationService, IEmailTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private const string _dataOrigin = "Colleague";

        public EmailTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IPersonRepository personRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all email types
        /// </summary>
        /// <returns>Collection of EmailType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmailType>> GetEmailTypesAsync(bool bypassCache = false)
        {
            var emailTypeCollection = new List<Ellucian.Colleague.Dtos.EmailType>();

            var emailEntities = await _referenceDataRepository.GetEmailTypesAsync(bypassCache);
            if (emailEntities != null && emailEntities.Count() > 0)
            {
                foreach (var email in emailEntities)
                {
                    emailTypeCollection.Add(ConvertEmailTypeEntityToEmailTypeDto(email));
                }
            }
            return emailTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a email type from its GUID
        /// </summary>
        /// <returns>EmailType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmailType> GetEmailTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmailTypeEntityToEmailTypeDto((await _referenceDataRepository.GetEmailTypesAsync(true)).Where(rt => rt.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Email Type not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Gets base email types
        /// </summary>
        /// <returns>Collection of EmailType Base DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.EmailType>> GetBaseEmailTypesAsync()
        {
            var emailTypeCollection = new List<Ellucian.Colleague.Dtos.Base.EmailType>();
            try
            {
                var emailTypeEntities = await _referenceDataRepository.GetEmailTypesAsync(false);
                if (emailTypeEntities != null && emailTypeEntities.Count() > 0)
                {
                    var emailTypeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailType, Colleague.Dtos.Base.EmailType>(_adapterRegistry, logger);
                    foreach (var emailType in emailTypeEntities)
                    {
                        emailTypeCollection.Add(emailTypeAdapter.MapToType(emailType));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error getting phone types " + ex.Message);
            }
            return emailTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a EmailType domain entity to its corresponding EmailType DTO
        /// </summary>
        /// <param name="source">Email domain entity</param>
        /// <returns>EmailType DTO</returns>
        private Dtos.EmailType ConvertEmailTypeEntityToEmailTypeDto(EmailType source)
        {
            var emailType = new Dtos.EmailType();
            emailType.Id = source.Guid;
            emailType.Code = source.Code;
            emailType.Title = source.Description;
            emailType.Description = null;
            emailType.EmailTypeList = new Dtos.EmailTypeList();
            emailType.EmailTypeList = ConvertEmailTypeDomainEnumToEmailTypeDtoEnum(source.EmailTypeCategory);

            return emailType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a EmailType domain enumeration value to its corresponding EmailType DTO enumeration value
        /// </summary>
        /// <param name="source">EmailTypeCategory domain enumeration value</param>
        /// <returns>EmailTypeList DTO enumeration value</returns>
        private Dtos.EmailTypeList ConvertEmailTypeDomainEnumToEmailTypeDtoEnum(EmailTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.EmailTypeCategory.Personal:
                    return Dtos.EmailTypeList.Personal;
                case Domain.Base.Entities.EmailTypeCategory.Business:
                    return Dtos.EmailTypeList.Business;
                case Domain.Base.Entities.EmailTypeCategory.School:
                    return Dtos.EmailTypeList.School;
                case Domain.Base.Entities.EmailTypeCategory.Parent:
                    return Dtos.EmailTypeList.Parent;
                case Domain.Base.Entities.EmailTypeCategory.Family:
                    return Dtos.EmailTypeList.Family;
                case Domain.Base.Entities.EmailTypeCategory.Sales:
                    return Dtos.EmailTypeList.Sales;
                case Domain.Base.Entities.EmailTypeCategory.Support:
                    return Dtos.EmailTypeList.Support;
                case Domain.Base.Entities.EmailTypeCategory.General:
                    return Dtos.EmailTypeList.General;
                case Domain.Base.Entities.EmailTypeCategory.Billing:
                    return Dtos.EmailTypeList.Billing;
                case Domain.Base.Entities.EmailTypeCategory.Legal:
                    return Dtos.EmailTypeList.Legal;
                case Domain.Base.Entities.EmailTypeCategory.HR:
                    return Dtos.EmailTypeList.HR;
                case Domain.Base.Entities.EmailTypeCategory.Media:
                    return Dtos.EmailTypeList.Media;
                case Domain.Base.Entities.EmailTypeCategory.MatchingGifts:
                    return Dtos.EmailTypeList.MatchingGifts;
                default:
                    return Dtos.EmailTypeList.Other;
            }
        }
    }
}
