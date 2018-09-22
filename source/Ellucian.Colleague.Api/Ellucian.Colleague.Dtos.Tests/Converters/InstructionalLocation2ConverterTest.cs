// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Tests.Converters
{
    [TestClass]
    public class InstructionalLocation2ConverterTest
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        readonly string locationGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";

        [TestInitialize]
        public void SetupCustomConverter()
        {
            jsonSettings.Converters.Add(new InstructionalLocation2Converter());
            jsonSettings.TypeNameHandling = TypeNameHandling.All;
            jsonSettings.DefaultValueHandling = DefaultValueHandling.Include;
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
            jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
            jsonSettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            jsonSettings.CheckAdditionalContent = false;
        }

        [TestMethod]
        public void ReadJsonTest_InstructionalRoom2()
        {
            var instructionalLocation = new InstructionalRoom2() 
            { 
                LocationType = InstructionalLocationType.InstructionalRoom, 
                Room = new GuidObject2(locationGuid) 
            };

            string data = JsonConvert.SerializeObject(instructionalLocation, jsonSettings);

            var deserializeObject = JsonConvert.DeserializeObject<InstructionalLocation2>(data,  jsonSettings);
            Assert.AreEqual(instructionalLocation.LocationType, deserializeObject.LocationType);
            Assert.AreEqual(instructionalLocation.Room.Id, ((InstructionalRoom2)deserializeObject).Room.Id);
        }


        [TestMethod]
        public void ReadJsonTest_InstructionalSite2()
        {
            var instructionalLocation = new InstructionalSite2()
            {
                LocationType = InstructionalLocationType.InstructionalSite,
                Site = new GuidObject2(locationGuid)
            };

            string data = JsonConvert.SerializeObject(instructionalLocation, jsonSettings);

            var deserializeObject = JsonConvert.DeserializeObject<InstructionalLocation2>(data, jsonSettings);
            Assert.AreEqual(instructionalLocation.LocationType, deserializeObject.LocationType);
            Assert.AreEqual(instructionalLocation.Site.Id, ((InstructionalSite2)deserializeObject).Site.Id);
        }

        [TestMethod]
        public void ReadJsonTest_InstructionalOtherLocation2()
        {
            var instructionalLocation = new InstructionalOtherLocation2()
            {
                LocationType = InstructionalLocationType.InstructionalOtherLocation,
                Title = new GuidObject2(locationGuid)
            };

            string data = JsonConvert.SerializeObject(instructionalLocation, jsonSettings);

            var deserializeObject = JsonConvert.DeserializeObject<InstructionalLocation2>(data, jsonSettings);
            Assert.AreEqual(instructionalLocation.LocationType, deserializeObject.LocationType);
            Assert.AreEqual(instructionalLocation.Title.Id, ((InstructionalOtherLocation2)deserializeObject).Title.Id);
        }

        [TestMethod]
        public void ReadJsonTest_InstructionalOnline2()
        {
            var instructionalLocation = new InstructionalOnline2()
            {
                LocationType = InstructionalLocationType.InstructionalOnline,
                PhoneExtension = "123",
                PhoneNumber = "456-7899",
                WebAddress = "ellucian.com"
            };

            string data = JsonConvert.SerializeObject(instructionalLocation, jsonSettings);

            var deserializeObject = JsonConvert.DeserializeObject<InstructionalLocation2>(data, jsonSettings);
            Assert.AreEqual(instructionalLocation.LocationType, deserializeObject.LocationType);
            Assert.AreEqual(instructionalLocation.PhoneExtension, ((InstructionalOnline2)deserializeObject).PhoneExtension);
            Assert.AreEqual(instructionalLocation.PhoneNumber, ((InstructionalOnline2)deserializeObject).PhoneNumber);
            Assert.AreEqual(instructionalLocation.WebAddress, ((InstructionalOnline2)deserializeObject).WebAddress);
        }
    }
}