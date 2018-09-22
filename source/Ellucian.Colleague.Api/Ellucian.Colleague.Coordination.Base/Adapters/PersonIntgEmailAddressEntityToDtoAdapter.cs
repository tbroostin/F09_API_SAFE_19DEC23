// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Mapper Class for Data Model Person Intgeration Email Address information
    /// </summary>
    public class PersonIntgEmailAddressEntityToDtoAdapter
    {
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;

        public PersonIntgEmailAddressEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Map Domain emails to person intg email dto type
        /// </summary>
        /// <param name="sourceEmailAddresses">IEnumerable of Domain email address to map</param>
        /// <param name="emailTypes">Email Types list</param>
        /// <returns>IEnumerable of PersonEmailDtoProperty</returns>
        public IEnumerable<Dtos.DtoProperties.PersonEmailDtoProperty> MapToType(IEnumerable<Domain.Base.Entities.EmailAddress> sourceEmailAddresses, IEnumerable<Domain.Base.Entities.EmailType> emailTypes)
        {
            var emailAddressDtos = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
            if (sourceEmailAddresses != null && sourceEmailAddresses.Any())
            {
                foreach (var emailAddressEntity in sourceEmailAddresses)
                {
                    try
                    {
                        var codeItem = emailTypes.FirstOrDefault(pt => pt.Code == emailAddressEntity.TypeCode);
                        if (codeItem != null && !string.IsNullOrEmpty(codeItem.Guid) && codeItem.EmailTypeCategory != null)
                        {
                            var addressDto = new Dtos.DtoProperties.PersonEmailDtoProperty()
                            {
                                Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty()
                                {
                                    EmailType = (Dtos.EmailTypeList)Enum.Parse(typeof (Dtos.EmailTypeList), codeItem.EmailTypeCategory.ToString()),
                                    Detail = new Dtos.GuidObject2(codeItem.Guid)
                                },
                                Address = emailAddressEntity.Value
                            };

                            if (emailAddressEntity.IsPreferred)
                            {
                                addressDto.Preference = Dtos.EnumProperties.PersonEmailPreference.Primary;
                            }

                            emailAddressDtos.Add(addressDto);
                        }
                        else
                        {
                            _logger.Error(string.Concat("Could not convert entity to dto for email address : ", emailAddressEntity.Value));      
                        }
                    }
                    catch (Exception ex)
                    {
                        //Don't rethrow, log issue. If there isn't an email address, cant log anything.
                        if (!string.IsNullOrEmpty(emailAddressEntity.Value))
                        {
                            _logger.Error(ex, string.Concat("Could not convert entity to dto for email address : ", emailAddressEntity.Value));
                        }
                    }
                }
            }
            return emailAddressDtos;
        }
    }
}
