// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;

namespace Ellucian.Colleague.Dtos.Tests.Converters
{
    [TestClass]
    public class GuidObject2FilterConverterTests
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        string guid1 = Guid.NewGuid().ToString();
        string guid2 = Guid.NewGuid().ToString();

        [TestInitialize]
        public void SetupCustomConverter()
        {

            jsonSettings.TypeNameHandling = TypeNameHandling.All;
            jsonSettings.DefaultValueHandling = DefaultValueHandling.Include;
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
            jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
            jsonSettings.CheckAdditionalContent = false;
        }

        [TestMethod]
        public void GuidObject2FilterConverter_GuidObject_Valid()
        {
            var criteria = string.Concat("{ \"id\":\"", guid1, "\" }");

            var filterConverter = new GuidObject2FilterConverter();

            var rawFilterData = JsonConvert.DeserializeObject<GuidObject2>(criteria,
                filterConverter);

            Assert.IsNotNull(rawFilterData);
            Assert.IsTrue(rawFilterData.GetType() == typeof(GuidObject2));
            Assert.IsTrue(!string.IsNullOrEmpty(rawFilterData.Id));
            Assert.AreEqual(guid1, rawFilterData.Id);
      
        }

        [TestMethod]
        public void GuidObject2FilterConverter_SingleValue_Valid()
        {
            var criteria = string.Concat("\"", guid1, "\"");

            var filterConverter = new GuidObject2FilterConverter();

            var rawFilterData = JsonConvert.DeserializeObject<GuidObject2>(criteria,
                filterConverter);

            Assert.IsNotNull(rawFilterData);
            Assert.IsTrue(rawFilterData.GetType() == typeof(GuidObject2));
            Assert.IsTrue(!string.IsNullOrEmpty(rawFilterData.Id));
            Assert.AreEqual(guid1, rawFilterData.Id);
           
        }
    }
}