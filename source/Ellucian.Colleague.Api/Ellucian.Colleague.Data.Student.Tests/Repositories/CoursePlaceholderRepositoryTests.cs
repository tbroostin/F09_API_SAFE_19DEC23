// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class CoursePlaceholderRepositoryTests : BaseRepositorySetup
    {
        private CoursePlaceholderRepository repository;

        private List<CoursePlaceholders> coursePlaceholderData;

        [TestInitialize]
        public void CoursePlaceholderRepository_Initialize()
        {
            MockInitialize();

            repository = new CoursePlaceholderRepository(cacheProvider, transFactory, logger, apiSettings);

            coursePlaceholderData = new List<CoursePlaceholders>()
            {
                new CoursePlaceholders() { Recordkey = "1", CphCredits = "3 to 5 credits", CphDescription = "Placeholder 1 Description", CphEndDate = DateTime.Today.AddDays(7), CphStartDate = DateTime.Today.AddDays(-7), CphTitle = "Placeholder 1 Title" },
                new CoursePlaceholders() { Recordkey = "2", CphCredits = "4 to 6 credits", CphDescription = "Placeholder 2 Description", CphEndDate = DateTime.Today.AddDays(14), CphTitle = "Placeholder 2 Title" },
                new CoursePlaceholders() { Recordkey = "3", CphDescription = "Placeholder 3 Description", CphStartDate = DateTime.Today.AddDays(-14), CphTitle = "Placeholder 3 Title" },
                new CoursePlaceholders() { Recordkey = "BADDATES", CphDescription = "Placeholder with Start Date > End Date", CphEndDate = DateTime.Today.AddDays(7), CphStartDate = DateTime.Today.AddDays(14), CphTitle = "Placeholder 4 Title" },
                new CoursePlaceholders() { Recordkey = "HAS-REQ-SUBREQ-GRP",
                    CphDescription = "Placeholder associated with a DA Requirement, Subrequirement, and Group",
                    CphStartDate = DateTime.Today.AddDays(-28),
                    CphEndDate = DateTime.Today.AddDays(28),
                    CphTitle = "DA Req/Subreq/Grp",
                    CphAcadReqmt = "REQ1",
                    CphSreqAcadReqmtBlock = "12345",
                    CphGroupAcadReqmtBlock = "12346",
                    CphCredits = "3 credits"
                },
                new CoursePlaceholders() { Recordkey = "BAD-REQ-SUBREQ-GRP", 
                    CphDescription = "Placeholder associated with a DA Requirement, Subrequirement, and Group", 
                    CphStartDate = DateTime.Today.AddDays(-28), 
                    CphEndDate = DateTime.Today.AddDays(28), 
                    CphTitle = "DA Req/Subreq/Grp",
                    CphAcadReqmt = null,
                    CphSreqAcadReqmtBlock = "12345",
                    CphGroupAcadReqmtBlock = "12346",
                    CphCredits = "3 credits"
                }
            };

            MockRecordsAsync("COURSE.PLACEHOLDERS", coursePlaceholderData);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_null_IDs_throws_ArgumentNullException()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(null);
        }

        [TestMethod]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_empty_IDs_returns_no_entities()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>());
            Assert.AreEqual(0, cphs.Count());
        }

        [TestMethod]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_default()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2", "3" });
            Assert.AreEqual(3, cphs.Count());
        }

        [TestMethod]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_cache()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2", "3", "HAS-REQ-SUBREQ-GRP" }, false);
            Assert.AreEqual(4, cphs.Count());
        }

        [TestMethod]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_no_cache()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2", "3", "HAS-REQ-SUBREQ-GRP" }, true);
            Assert.AreEqual(4, cphs.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_corrupt_record()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>() { "1", "2", "3", "BADDATES" }, false);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_corrupt_record_req_subreq_grp()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>() { "BAD-REQ-SUBREQ-GRP" }, false);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CoursePlaceholderRepository_GetCoursePlaceholdersByIdsAsync_nonexistent_record()
        {
            var cphs = await repository.GetCoursePlaceholdersByIdsAsync(new List<string>() { "DOESNOTEXIST" }, false);
        }
    }
}
