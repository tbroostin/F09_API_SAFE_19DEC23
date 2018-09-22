// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class SectionGradeResponseAdapterTests
    {
        [TestMethod]
        public void SectionGradeResponseAdapter_MapToType()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();
            var sectionGradeResponseAdapter = new SectionGradeResponseAdapter(adapterRegistry, loggerMock.Object);
            
            var entity = new Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponse();
            entity.Status = "success";
            entity.StudentId = "123";
            entity.Errors = new List<Domain.Student.Entities.SectionGradeResponseError>();
            var error = new Ellucian.Colleague.Domain.Student.Entities.SectionGradeResponseError();
            error.Message = "message";
            error.Property = "property";
            entity.Errors.Add(error);

            var dto = sectionGradeResponseAdapter.MapToType(entity);

            Assert.AreEqual(entity.Status, dto.Status);
            Assert.AreEqual(entity.StudentId, dto.StudentId);
            Assert.AreEqual(entity.Errors.Count(), dto.Errors.Count());
            Assert.AreEqual(entity.Errors[0].Message, dto.Errors[0].Message);
            Assert.AreEqual(entity.Errors[0].Property, dto.Errors[0].Property);
        }
    }
}
