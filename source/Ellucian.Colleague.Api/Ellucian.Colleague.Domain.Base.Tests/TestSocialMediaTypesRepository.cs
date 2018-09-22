// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestSocialMediaTypesRepository
    {
        private string[,] socialMediaType = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "BL", "Blog", "blog"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "FB", "Facebook", "facebook"},
                                            {"b769e6a9-da86-47a9-ab21-b17198880439", "FQ", "Foursquare", "foursquare"}, 
                                            {"e297656e-8d50-4c63-a2dd-0fcfc46647c4", "HO", "Hangouts", "hangouts"}, 
                                            {"8d0e291e-7246-4067-aff1-47ff6adc0392", "IQ", "ICQ", "icq"}, 
                                            {"b91bbee8-88d1-4063-86e2-e7cb1865b45a", "IN", "Intagram", "instagram"}, 
                                            {"4eaca2e7-fb59-44b6-be64-ce9e2ad73e81", "JA", "Jabber", "jabber"}, 
                                            {"c76a6755-7594-4a24-a821-be2c8293ff78", "LI", "Linkedin", "linkedin"}, 
                                            {"95860685-7a99-476b-99f0-34066a5c20f6", "OT", "Other", "other"}, 
                                            {"119cdf92-18b4-44f0-9fcb-6b3dd9702f67", "PI", "Pinterest", "pinterest"}, 
                                            {"b772f098-77f3-48ef-b691-ea5b8aff5646", "QQ", "QQ", "qq"}, 
                                            {"e692812d-a23f-4601-a112-dc2d58389045", "SK", "Skype", "skype"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "TU", "Tumblr", "tumblr"}, 
                                            {"13660156-d481-4b3d-b617-92136979314c", "TW", "Twitter", "twitter"}, 
                                            {"bcea6b4e-01ff-4d52-b4d5-7f6a5aa10820", "WS", "Website", "website"}, 
                                            {"2198dcfa-cd4b-4df3-ab17-73b63ad595ee", "WL", "WindowsLive", "windowsLive"},
                                            {"c37a2fde-4bac-4c84-b530-6b6f7d1f490a", "YA", "Yahoo", "yahoo"}, 
                                            {"400dce82-2cdc-4990-a864-fc9943084d1a", "YT", "Youtube", "youtube"}

                                            };

        public IEnumerable<SocialMediaType> GetSocialMediaTypes()
        {
            var socialMediaTypeList = new List<SocialMediaType>();

            // There are 3 fields for each social media type in the array
            var items = socialMediaType.Length / 4;

            for (int x = 0; x < items; x++)
            {
                socialMediaTypeList.Add(
                    new SocialMediaType(
                        socialMediaType[x, 0], socialMediaType[x, 1], socialMediaType[x, 2], 
                        ConvertSocialMediaTypeCategoryCodeToSocialMediaTypeCategory(socialMediaType[x, 3]) 
                    ));
            }
            return socialMediaTypeList;
        }

        private SocialMediaTypeCategory ConvertSocialMediaTypeCategoryCodeToSocialMediaTypeCategory(string code)
        {
            if (string.IsNullOrEmpty(code))
                return SocialMediaTypeCategory.other;

            switch (code.ToLowerInvariant())
            {
                case "windowslive":
                    return SocialMediaTypeCategory.windowsLive;
                case "yahoo":
                    return SocialMediaTypeCategory.yahoo;
                case "skype":
                    return SocialMediaTypeCategory.skype;
                case "qq":
                    return SocialMediaTypeCategory.qq;
                case "hangouts":
                    return SocialMediaTypeCategory.hangouts;
                case "icq":
                    return SocialMediaTypeCategory.icq;
                case "jabber":
                    return SocialMediaTypeCategory.jabber;
                case "facebook":
                    return SocialMediaTypeCategory.facebook;
                case "twitter":
                    return SocialMediaTypeCategory.twitter;
                case "instagram":
                    return SocialMediaTypeCategory.instagram;
                case "tumblr":
                    return SocialMediaTypeCategory.tumblr;
                case "pinterest":
                    return SocialMediaTypeCategory.pinterest;
                case "linkedin":
                    return SocialMediaTypeCategory.linkedin;
                case "foursquare":
                    return SocialMediaTypeCategory.foursquare;
                case "youtube":
                    return SocialMediaTypeCategory.youtube;
                case "blog":
                    return SocialMediaTypeCategory.blog;
                case "website":
                    return SocialMediaTypeCategory.website;
                default:
                    return SocialMediaTypeCategory.other;
            }
        }
    }
}