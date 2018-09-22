// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Ellucian.Colleague.Api.Converters;

namespace Ellucian.Colleague.Api.Tests.Converters
{
    [TestClass]
    public class ColleagueDateTimeConverterTest
    {
        // Picked a time zone that is not Eastern Time on purpose
        // so that the results cannot coincidentally be correct due to 
        // the fact that they're run on a computer in ET.
        string _fakeColleagueTimeZone = "Pacific Standard Time";

        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        [TestInitialize]
        public void SetupCustomConverter()
        {
            jsonSettings.DateParseHandling = DateParseHandling.None;
            jsonSettings.Converters.Add(new ColleagueDateTimeConverter(_fakeColleagueTimeZone));
        }

        [TestMethod]
        public void ReadJsonTest()
        {
            // Verify that json datetime string without offset will be appended the Colleague time zone offset when deserialized
            DateTimeOffset expectedWithDST = DateTimeOffset.Parse("2014-04-01T15:00:00-7:00"); // PDT: -7
            DateTimeOffset actualWithDST = JsonConvert.DeserializeObject<DateTimeOffset>("\"2014-04-01T15:00:00\"", jsonSettings);
            Assert.AreEqual(expectedWithDST, actualWithDST);

            DateTimeOffset expectedWithoutDST = DateTimeOffset.Parse("2014-02-01T15:00:00-8:00"); // PST: -8
            DateTimeOffset actualWithoutDST = JsonConvert.DeserializeObject<DateTimeOffset>("\"2014-02-01T15:00:00\"", jsonSettings);
            Assert.AreEqual(expectedWithoutDST, actualWithoutDST);

            // Verify that json datetime string with an offset will be unmodified after deserialization
            DateTimeOffset expectedUnmodified = DateTimeOffset.Parse("2014-04-01T15:00:00-1:00");
            DateTimeOffset actualUnmodified = JsonConvert.DeserializeObject<DateTimeOffset>("\"2014-04-01T15:00:00-1:00\"", jsonSettings);
            Assert.AreEqual(expectedUnmodified, actualUnmodified);

            expectedUnmodified = DateTimeOffset.Parse("2013-12-01T15:00:00+9:00");
            actualUnmodified = JsonConvert.DeserializeObject<DateTimeOffset>("\"2013-12-01T15:00:00+9:00\"", jsonSettings);
            Assert.AreEqual(expectedUnmodified, actualUnmodified);

        }

        [TestMethod]
        public void WriteJsonTest()
        {
            //Verify that a DateTimeObject object will be serialized to a UTC time string
            DateTimeOffset input = DateTimeOffset.Parse("2014-04-01T15:00:00-7:00"); 
            string expected = "\"2014-04-01T22:00:00Z\"";
            string actual = JsonConvert.SerializeObject(input, jsonSettings);
            Assert.AreEqual(expected, actual);

            DateTimeOffset input2 = DateTimeOffset.Parse("2014-04-01T15:00:00+4:00"); 
            string expected2 = "\"2014-04-01T11:00:00Z\"";
            string actual2 = JsonConvert.SerializeObject(input2, jsonSettings);
            Assert.AreEqual(expected2, actual2);

            // nullable case
            DateTimeOffset? input2b = input2;
            string actual2b = JsonConvert.SerializeObject(input2b, jsonSettings);
            Assert.AreEqual(expected2, actual2b);

            //Verify that a DateTime object will be serialized to the same time string (unaffected).
            DateTime input3 = DateTime.Parse("2014-04-01T15:00:00");
            string expected3 = "\"2014-04-01T15:00:00\"";
            string actual3 = JsonConvert.SerializeObject(input3, jsonSettings);
            Assert.AreEqual(expected3, actual3);

        }
    }
}
