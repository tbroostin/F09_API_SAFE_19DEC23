// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ScheduleRepeatTests
    {
        static string code;
        static string desc;
        static string interval;
        static FrequencyType? type;
        static ScheduleRepeat repeat;

        [TestInitialize]
        public void Initialize()
        {
            code = "ABCD";
            desc = "1234";
            interval = "10";
            type = FrequencyType.Daily;
            repeat = new ScheduleRepeat(code, desc, interval, type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleRepeat_Constructor_NullCode()
        {
            repeat = new ScheduleRepeat(null, desc, interval, type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleRepeat_Constructor_EmptyCode()
        {
            repeat = new ScheduleRepeat(string.Empty, desc, interval, type);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_Code()
        {
            repeat = new ScheduleRepeat(code, desc, interval, type);
            Assert.AreEqual(code, repeat.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleRepeat_Constructor_NullDescription()
        {
            repeat = new ScheduleRepeat(code, null, interval, type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduleRepeat_Constructor_EmptyDescription()
        {
            repeat = new ScheduleRepeat(code, string.Empty, interval, type);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_Description()
        {
            repeat = new ScheduleRepeat(code, desc, interval, type);
            Assert.AreEqual(desc, repeat.Description);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_NullInterval()
        {
            repeat = new ScheduleRepeat(code, desc, null, type);
            Assert.AreEqual(FrequencyType.Daily, repeat.FrequencyType);
            Assert.AreEqual(null, repeat.Interval);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_EmptyInterval()
        {
            repeat = new ScheduleRepeat(code, desc, string.Empty, type);
            Assert.AreEqual(FrequencyType.Daily, repeat.FrequencyType);
            Assert.AreEqual(null, repeat.Interval);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_NullFrequencyType()
        {
            repeat = new ScheduleRepeat(code, desc, interval, null);
            Assert.AreEqual(FrequencyType.Daily, repeat.FrequencyType);
            Assert.AreEqual(null, repeat.Interval);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_CannotParseInterval()
        {
            repeat = new ScheduleRepeat(code, desc, "abcd", type);
            Assert.AreEqual(FrequencyType.Daily, repeat.FrequencyType);
            Assert.AreEqual(null, repeat.Interval);
        }

        [TestMethod]
        public void ScheduleRepeat_Constructor_ValidIntervalAndFrequencyType()
        {
            repeat = new ScheduleRepeat(code, desc, interval, type);
            Assert.AreEqual(type, repeat.FrequencyType);
            Assert.AreEqual(int.Parse(interval), repeat.Interval);
        }
    }
}
