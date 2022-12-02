// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

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
    /// Mapper Class for Data Model Person Intgeration Social Media information
    /// </summary>
    public class PersonIntgSocialMediaEntityToDtoAdapter : AutoMapperAdapter<Tuple<IEnumerable<Domain.Base.Entities.SocialMedia>, IEnumerable<Domain.Base.Entities.SocialMediaType>>, IEnumerable<Dtos.DtoProperties.PersonSocialMediaDtoProperty>>
    {
        
         /// <summary>
        /// Initializes a new instance of the PersonIntgSocialMediaEntityToDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonIntgSocialMediaEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }


        /// <summary>
        /// Override MapToType for custom mapping
        /// </summary>
        /// <param name="source">Tuple that has the social media entities and the social media type reference data list</param>
        /// <returns></returns>
        public override IEnumerable<Dtos.DtoProperties.PersonSocialMediaDtoProperty> MapToType(Tuple<IEnumerable<Domain.Base.Entities.SocialMedia>, IEnumerable<Domain.Base.Entities.SocialMediaType>> source)
        {
            List<Dtos.DtoProperties.PersonSocialMediaDtoProperty> socialMediaEntries = new List<Dtos.DtoProperties.PersonSocialMediaDtoProperty>();

            var mediaTypes = source.Item1;
            var socialMediaTypeList = source.Item2;

            foreach (var mediaType in mediaTypes)
            {
                try
                {
                    var socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty();
                    if (mediaType.TypeCode.ToLowerInvariant() == "website")
                    {
                        string guid = "";
                        var socialMediaEntity = socialMediaTypeList.FirstOrDefault(ic => ic.Type.ToString() == mediaType.TypeCode);
                        if (socialMediaEntity != null)
                        {
                            guid = socialMediaEntity.Guid;
                            socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                            {
                                Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                {
                                    Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), mediaType.TypeCode.ToString()),
                                    Detail = new Dtos.GuidObject2(guid)
                                },
                                Address = mediaType.Handle
                            };
                        }
                        else
                        {
                            socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                            {
                                Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                {
                                    Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), mediaType.TypeCode.ToString())
                                },
                                Address = mediaType.Handle
                            };
                        }
                    }
                    else
                    {
                        var socialMediaEntity = socialMediaTypeList.FirstOrDefault(ic => ic.Code == mediaType.TypeCode);
                        socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                        {
                            Type = new Dtos.DtoProperties.PersonSocialMediaType()
                            {
                                Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), socialMediaEntity.Type.ToString()),
                                Detail = new Dtos.GuidObject2(socialMediaEntity.Guid)
                            },
                            Address = mediaType.Handle
                        };
                    }
                    if (mediaType.IsPreferred) socialMedia.Preference = Dtos.EnumProperties.PersonPreference.Primary;
                    
                    socialMediaEntries.Add(socialMedia);
                }
                catch (Exception ex)
                {
                    // Do not include code since we couldn't find a category
                    logger.Error(ex.Message, "could not find SocialMediaTypeCategory");
                }
            }

            return socialMediaEntries;
        }
    }
}
