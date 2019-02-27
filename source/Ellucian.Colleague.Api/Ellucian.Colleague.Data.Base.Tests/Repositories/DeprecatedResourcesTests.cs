// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class DeprecatedResourcesTests
    {
        public string name;
        public string xMediaType;
        public List<string> methods;
        public DateTime? deprecatedOn;
        public DateTime? sunsetOn;
        public string description;
        public DeprecatedResources deprecatedResources;

        [TestInitialize]
        public void Initialize()
        {
            name = "name1";
            xMediaType = "mediaType1";
            deprecatedOn = new DateTime(2018, 3, 1);
            sunsetOn = new DateTime(2018, 5, 1);
            description = "description1";
            methods = new List<string>() { "method1" };

            deprecatedResources = new DeprecatedResources()
            {
                Name = name,
                Representations = new List<Representation>()
                {
                    new Representation()
                    {
                        XMediaType = xMediaType,
                        DeprecationNotice = new DeprecationNotice()
                        {
                            DeprecatedOn = deprecatedOn,
                            Description = description,
                            SunsetOn = sunsetOn
                        },
                        Methods = methods
                    }
                }
            };
        }

        [TestMethod]
        public void DeprecatedResourcesTests_Name()
        {
            Assert.AreEqual(name, deprecatedResources.Name);
        }

        [TestMethod]
        public void DeprecatedResourcesTests_XMediaType()
        {
            Assert.AreEqual(xMediaType, deprecatedResources.Representations[0].XMediaType);
        }

        [TestMethod]
        public void DeprecatedResourcesTests_DeprecatedResources_Methods()
        {
            Assert.AreEqual(methods, deprecatedResources.Representations[0].Methods);
        }

        [TestMethod]
        public void DeprecatedResourcesTests_DeprecatedResources_DeprecatedOn()
        {
            Assert.AreEqual(deprecatedOn, deprecatedResources.Representations[0].DeprecationNotice.DeprecatedOn);
        }

        [TestMethod]
        public void DeprecatedResourcesTests_DeprecatedResources_SunsetOn()
        {
            Assert.AreEqual(sunsetOn, deprecatedResources.Representations[0].DeprecationNotice.SunsetOn);
        }

        [TestMethod]
        public void DeprecatedResourcesTests_DeprecatedResources_Description()
        {
            Assert.AreEqual(description, deprecatedResources.Representations[0].DeprecationNotice.Description);
        }
    }
}
