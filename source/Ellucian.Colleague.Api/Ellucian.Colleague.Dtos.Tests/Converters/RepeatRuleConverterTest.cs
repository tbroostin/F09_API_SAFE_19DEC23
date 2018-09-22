// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Collections.Generic;


namespace Ellucian.Colleague.Dtos.Tests.Converters
{
    [TestClass]
    public class RepeatRuleConverterTest
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        [TestInitialize]
        public void SetupCustomConverter()
        {         
            jsonSettings.Converters.Add(new RepeatRuleConverter());
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
        public void ReadJsonTest_RepeatRuleDaily_Repetitions()
        {
            var repeatRuleDaily = new RepeatRuleDaily()
                    { 
                    Interval = 1, 
                    Type = FrequencyType2.Daily, 
                    Ends = new RepeatRuleEnds(){ Repetitions = 14} 
                    };
           
           string data = JsonConvert.SerializeObject(repeatRuleDaily, jsonSettings);

            var deserializeObject = JsonConvert.DeserializeObject<IRepeatRule>(data,  jsonSettings);
            Assert.AreEqual(repeatRuleDaily.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleDaily.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleDaily.Type, deserializeObject.Type);


            data = "{\"type\":\"daily\",\"interval\":1,\"ends\":{\"repetitions\":14}}";

            deserializeObject = JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleDaily.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleDaily.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleDaily.Type, deserializeObject.Type);
        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleDaily_Date()
        {
           var repeatRuleDaily = new RepeatRuleDaily()
                    { 
                    Interval = 1, 
                    Type = FrequencyType2.Daily
                    ,   Ends = new RepeatRuleEnds(){ Date = new DateTime(2016,05,12) } 
                    };
           
          var data = "{\"type\":\"daily\",\"interval\":1,\"ends\":{\"date\":\"2016-05-12\"}}";
            
           var deserializeObject = JsonConvert.DeserializeObject<IRepeatRule>(data,  jsonSettings);
            Assert.AreEqual(repeatRuleDaily.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleDaily.Ends.Date, deserializeObject.Ends.Date);
            Assert.AreEqual(repeatRuleDaily.Type, deserializeObject.Type);
        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleWeekly_Repetitions()
        {
            var repeatRuleWeekly = new RepeatRuleWeekly()
            {
                Interval = 1,
                Type = FrequencyType2.Weekly,
                DayOfWeek = new List<HedmDayOfWeek?>(){ HedmDayOfWeek.Monday, HedmDayOfWeek.Tuesday},
                Ends = new RepeatRuleEnds() { Repetitions = 14 }
            };

            string data = JsonConvert.SerializeObject(repeatRuleWeekly, jsonSettings);

            var deserializeObject = (RepeatRuleWeekly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleWeekly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleWeekly.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[0], deserializeObject.DayOfWeek[0]);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[1], deserializeObject.DayOfWeek[1]);
            Assert.AreEqual(repeatRuleWeekly.Type, deserializeObject.Type);

            data = "{\"type\":\"weekly\",\"interval\":1,\"ends\":{\"repetitions\":14}, \"daysOfWeek\":[\"Monday\", \"Tuesday\"]}";

            deserializeObject = (RepeatRuleWeekly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleWeekly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleWeekly.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[0], deserializeObject.DayOfWeek[0]);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[1], deserializeObject.DayOfWeek[1]);
            Assert.AreEqual(repeatRuleWeekly.Type, deserializeObject.Type);

        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleWeekly_Date()
        {
            var repeatRuleWeekly = new RepeatRuleWeekly()
            {
                Interval = 1,
                Type = FrequencyType2.Weekly,
                DayOfWeek = new List<HedmDayOfWeek?>(){ HedmDayOfWeek.Monday, HedmDayOfWeek.Tuesday},
                Ends = new RepeatRuleEnds() { Date = new DateTime(2016, 05, 12) } 
            };

            string data = JsonConvert.SerializeObject(repeatRuleWeekly, jsonSettings);

            var deserializeObject = (RepeatRuleWeekly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleWeekly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[0], deserializeObject.DayOfWeek[0]);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[1], deserializeObject.DayOfWeek[1]);
            Assert.AreEqual(repeatRuleWeekly.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleWeekly.Type, deserializeObject.Type);

            data = "{\"type\":\"weekly\",\"interval\":1,\"ends\":{\"date\":\"2016-05-12\"}, \"daysOfWeek\":[\"Monday\", \"Tuesday\"]}";

            deserializeObject = (RepeatRuleWeekly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleWeekly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleWeekly.Ends.Date, deserializeObject.Ends.Date);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[0], deserializeObject.DayOfWeek[0]);
            Assert.AreEqual(repeatRuleWeekly.DayOfWeek[1], deserializeObject.DayOfWeek[1]);
            Assert.AreEqual(repeatRuleWeekly.Type, deserializeObject.Type);

        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleYearly_Repetitions()
        {
            var repeatRuleYearly = new RepeatRuleYearly()
            {
                Interval = 2,
                Type = FrequencyType2.Yearly,
                Ends = new RepeatRuleEnds() { Repetitions = 14 }
            };

            string data = JsonConvert.SerializeObject(repeatRuleYearly, jsonSettings);

            var deserializeObject = (RepeatRuleYearly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleYearly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleYearly.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleYearly.Type, deserializeObject.Type);

            data = "{\"type\":\"yearly\",\"interval\":2,\"ends\":{\"repetitions\":14}}";

            deserializeObject = (RepeatRuleYearly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleYearly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleYearly.Ends.Repetitions, deserializeObject.Ends.Repetitions);
            Assert.AreEqual(repeatRuleYearly.Type, deserializeObject.Type);

        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleYearly_Date()
        {
            var repeatRuleYearly = new RepeatRuleYearly()
            {
                Interval = 2,
                Type = FrequencyType2.Yearly,
                Ends = new RepeatRuleEnds() { Date = new DateTime(2016, 05, 12) } 
            };

            string data = JsonConvert.SerializeObject(repeatRuleYearly, jsonSettings);

            var deserializeObject = (RepeatRuleYearly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleYearly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleYearly.Ends.Date, deserializeObject.Ends.Date);
            Assert.AreEqual(repeatRuleYearly.Type, deserializeObject.Type);

            data = "{\"type\":\"yearly\",\"interval\":2,\"ends\":{\"date\":\"2016-05-12\"}}";

            deserializeObject = (RepeatRuleYearly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleYearly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleYearly.Ends.Date, deserializeObject.Ends.Date);
            Assert.AreEqual(repeatRuleYearly.Type, deserializeObject.Type);

        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleMonthly_RepeatRuleRepeatBy_DayOfMonth()
        {
            var repeatRuleMonthly = new RepeatRuleMonthly()
            {
                Interval = 2,
                Type = FrequencyType2.Monthly,
                RepeatBy = new RepeatRuleRepeatBy() { DayOfMonth = 15 }
            };

            string data = JsonConvert.SerializeObject(repeatRuleMonthly, jsonSettings);

            var deserializeObject = (RepeatRuleMonthly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleMonthly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfMonth, deserializeObject.RepeatBy.DayOfMonth);
            Assert.AreEqual(repeatRuleMonthly.Type, deserializeObject.Type);

            data = "{\"type\":\"monthly\",\"interval\":2,\"repeatBy\":{\"dayOfMonth\":15}}";

            deserializeObject = (RepeatRuleMonthly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleMonthly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfMonth, deserializeObject.RepeatBy.DayOfMonth);
            Assert.AreEqual(repeatRuleMonthly.Type, deserializeObject.Type);
        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleMonthly_RepeatRuleRepeatBy_DayOfWeek_Positive()
        {
            var repeatRuleMonthly = new RepeatRuleMonthly()
            {
                Interval = 2,
                Type = FrequencyType2.Monthly,
                RepeatBy = new RepeatRuleRepeatBy() { DayOfWeek = new RepeatRuleDayOfWeek() { Day  = HedmDayOfWeek.Monday, Occurrence = 2 } } 
            };

            string data = JsonConvert.SerializeObject(repeatRuleMonthly, jsonSettings);

            var deserializeObject = (RepeatRuleMonthly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleMonthly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Day, deserializeObject.RepeatBy.DayOfWeek.Day);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence, deserializeObject.RepeatBy.DayOfWeek.Occurrence);
            Assert.AreEqual(repeatRuleMonthly.Type, deserializeObject.Type);

            data = "{\"type\":\"monthly\",\"interval\":2,\"repeatBy\":{\"dayOfWeek\":{\"day\":\"Monday\",\"occurrence\":2 }}}";

            deserializeObject = (RepeatRuleMonthly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleMonthly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Day, deserializeObject.RepeatBy.DayOfWeek.Day);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence, deserializeObject.RepeatBy.DayOfWeek.Occurrence);
            Assert.AreEqual(repeatRuleMonthly.Type, deserializeObject.Type);
        }

        [TestMethod]
        public void ReadJsonTest_RepeatRuleMonthly_RepeatRuleRepeatBy_DayOfWeek_Negative()
        {
            var repeatRuleMonthly = new RepeatRuleMonthly()
            {
                Interval = 2,
                Type = FrequencyType2.Monthly,
                RepeatBy = new RepeatRuleRepeatBy() { DayOfWeek = new RepeatRuleDayOfWeek() { Day = HedmDayOfWeek.Monday, Occurrence = -2 } }
            };

            string data = JsonConvert.SerializeObject(repeatRuleMonthly, jsonSettings);

            var deserializeObject = (RepeatRuleMonthly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleMonthly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Day, deserializeObject.RepeatBy.DayOfWeek.Day);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence, deserializeObject.RepeatBy.DayOfWeek.Occurrence);
            Assert.AreEqual(repeatRuleMonthly.Type, deserializeObject.Type);

            data = "{\"type\":\"monthly\",\"interval\":2,\"repeatBy\":{\"dayOfWeek\":{\"day\":\"Monday\",\"occurrence\":-2 }}}";

            deserializeObject = (RepeatRuleMonthly)JsonConvert.DeserializeObject<IRepeatRule>(data, jsonSettings);
            Assert.AreEqual(repeatRuleMonthly.Interval, deserializeObject.Interval);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Day, deserializeObject.RepeatBy.DayOfWeek.Day);
            Assert.AreEqual(repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence, deserializeObject.RepeatBy.DayOfWeek.Occurrence);
            Assert.AreEqual(repeatRuleMonthly.Type, deserializeObject.Type);
        }
    }
}