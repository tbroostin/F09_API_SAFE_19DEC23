// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class StudentWaiverDtoToEntityAdapterTests
    {
        StudentWaiver waiverDto;
        StudentWaiverDtoToEntityAdapter waiverAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            waiverAdapter = new StudentWaiverDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            DateTimeOffset dateTimeChanged = new DateTimeOffset(2014, 2, 11, 3, 10, 15, new TimeSpan(-3, 0, 0));
            waiverDto = new StudentWaiver()
            {
                Id = "2",
                StudentId = "0000002",
                SectionId = "SEC1",
                ReasonCode = "OTHER",
                Comment = "Multiline\nComment\r\nBy User",
                AuthorizedBy = "0000003",
                ChangedBy = "0000004",
                DateTimeChanged = dateTimeChanged,
                RequisiteWaivers = new List<RequisiteWaiver>() 
                {
                    new RequisiteWaiver() {RequisiteId = "R1", Status = WaiverStatus.Waived},
                    new RequisiteWaiver() {RequisiteId = "R2", Status = WaiverStatus.Denied}
                }
            };
        }

        [TestMethod]
        public void StudentWaiver_Id()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.Id, waiverEntity.Id);
        }

        [TestMethod]
        public void StudentWaiver_StudentId()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.StudentId, waiverEntity.StudentId);            
        }

        [TestMethod]
        public void StudentWaiver_SectionId()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.SectionId, waiverEntity.SectionId);
        }

        [TestMethod]
        public void StudentWaiver_ReasonCode()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.ReasonCode, waiverEntity.ReasonCode);
        }

        [TestMethod]
        public void StudentWaiver_AuthorizedBy()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.AuthorizedBy, waiverEntity.AuthorizedBy);
        }

        [TestMethod]
        public void StudentWaiver_ChangedBy()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.ChangedBy, waiverEntity.ChangedBy);
        }

        [TestMethod]
        public void StudentWaiver_DateTimeChanged()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            Assert.AreEqual(waiverDto.DateTimeChanged, waiverEntity.DateTimeChanged);
        }

        [TestMethod]
        public void StudentWaiver_Requisites()
        {
            var waiverEntity = waiverAdapter.MapToType(waiverDto);
            for (int i = 0; i < waiverDto.RequisiteWaivers.Count(); i++)
            {
                Assert.AreEqual(waiverDto.RequisiteWaivers.ElementAt(i).RequisiteId, waiverEntity.RequisiteWaivers.ElementAt(i).RequisiteId);
                switch (waiverDto.RequisiteWaivers.ElementAt(i).Status)
                {
                    case WaiverStatus.Denied:
                        Assert.AreEqual(Domain.Student.Entities.WaiverStatus.Denied, waiverEntity.RequisiteWaivers.ElementAt(i).Status);
                        break;
                    case WaiverStatus.NotSelected:
                        Assert.AreEqual(Domain.Student.Entities.WaiverStatus.NotSelected, waiverEntity.RequisiteWaivers.ElementAt(i).Status);
                        break;
                    case WaiverStatus.Waived:
                        Assert.AreEqual(Domain.Student.Entities.WaiverStatus.Waived, waiverEntity.RequisiteWaivers.ElementAt(i).Status);
                        break;
                }
            }
        }
    }
}
