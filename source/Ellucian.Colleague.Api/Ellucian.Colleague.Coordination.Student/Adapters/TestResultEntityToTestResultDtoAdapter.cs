// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping from the TestResult Domain Entity to the TestResult DTO 
    /// </summary>
    public class TestResultEntityToTestResultDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult>
    {
        public TestResultEntityToTestResultDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Map a TestResult Entity to a TestResult DTO
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Dtos.Student.TestResult MapToType(Domain.Student.Entities.TestResult source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Test Result source cannot be null.");
            }
            var componentTestAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest>();
            var subTestResultAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult>();

            Dtos.Student.TestResult testResultDto = new Dtos.Student.TestResult();
            switch (source.Category)
            {
                case Ellucian.Colleague.Domain.Student.Entities.TestType.Admissions:
                    testResultDto.Category = Dtos.Student.TestType.Admissions;
                    break;
                case Ellucian.Colleague.Domain.Student.Entities.TestType.Placement:
                    testResultDto.Category = Dtos.Student.TestType.Placement;
                    break;
                case Ellucian.Colleague.Domain.Student.Entities.TestType.Other:
                    testResultDto.Category = Dtos.Student.TestType.Other;
                    break;
                default:
                    break;
            }
            testResultDto.Code = source.Code;
            testResultDto.DateTaken = source.DateTaken;
            testResultDto.Description = source.Description;
            testResultDto.Percentile = source.Percentile;
            testResultDto.Score = source.Score.HasValue ? (int)Math.Round(source.Score.Value, 0, MidpointRounding.AwayFromZero) : default(int?);
            testResultDto.StatusCode = source.StatusCode;
            testResultDto.StatusDate = source.StatusDate;
            testResultDto.StudentId = source.StudentId;
            testResultDto.SubTests = new List<Ellucian.Colleague.Dtos.Student.SubTestResult>();
            foreach (var subtest in source.SubTests)
            {
                testResultDto.SubTests.Add(subTestResultAdapter.MapToType(subtest));
            }
            testResultDto.ComponentTests = new List<Ellucian.Colleague.Dtos.Student.ComponentTest>();
            foreach (var componentTest in source.ComponentTests)
            {
                testResultDto.ComponentTests.Add(componentTestAdapter.MapToType(componentTest));
            }

            return testResultDto;
            
        }
    }
}
