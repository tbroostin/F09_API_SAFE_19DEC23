// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class DeprecatedResourcesRepository : IDeprecatedResourcesRepository
    {
        private readonly string FileName;

        public DeprecatedResourcesRepository()
        {
            FileName = HostingEnvironment.MapPath("~/App_Data/deprecatedResources.json");
        }

        public List<DeprecatedResources> Get()
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    return null;
                }
                // Read from json file
                using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                {
                    List<DeprecatedResources> deprecatedResources = Parse(fs);
                    return deprecatedResources;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<DeprecatedResources> Parse(Stream fs)
        {
            JsonSerializer serializer = new JsonSerializer();

            try
            {
                // Avoid overhead of having the entire JSON string in memory
                using (var sr = new StreamReader(fs))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return (List<DeprecatedResources>)serializer.Deserialize(jsonTextReader, typeof(List<DeprecatedResources>));
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
