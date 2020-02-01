using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class SampleDegreePlanTests
    {
        [TestClass]
        public class SampleDegreePlanConstructor
        {
            SampleDegreePlan track;
            string trackCode;
            string description;
            CourseBlocks block1;
            CourseBlocks block2;
            List<CourseBlocks> blocks;

            [TestInitialize]
            public void Initialize()
            {
                trackCode = "TRACK1";
                description = "ENGL.BA 2010";
                block1 = new CourseBlocks("Block 1", new List<string>() { "1", "2", "3" });
                block2 = new CourseBlocks("Block 2", new List<string>() { "4", "5", "6" });
                blocks = new List<CourseBlocks>() { block1, block2 };
                track = new SampleDegreePlan(trackCode, description, blocks);
            }

            [TestMethod]
            public void Code()
            {
                Assert.AreEqual(trackCode, track.Code);
            }

            [TestMethod]
            public void Description()
            {
                Assert.AreEqual(description, track.Description);
            }

            [TestMethod]
            public void CourseBlockCodes()
            {
                Assert.AreEqual(blocks.ElementAt(0), track.CourseBlocks.ElementAt(0));
                Assert.AreEqual(blocks.ElementAt(1), track.CourseBlocks.ElementAt(1));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionThrownIfCodeNull()
            {
                track = new SampleDegreePlan(null, description, blocks);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionThrownIfCodeEmpty()
            {
                track = new SampleDegreePlan("", description, blocks);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionThrownIfDescriptionNull()
            {
                track = new SampleDegreePlan(trackCode, null, blocks);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionThrownIfDescriptionEmpty()
            {
                track = new SampleDegreePlan(trackCode, "", blocks);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionThrownIfBlockCodesNull()
            {
                blocks = null;
                track = new SampleDegreePlan(trackCode, description, blocks);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionThrownIfBlockCodesEmpty()
            {
                blocks = new List<CourseBlocks>();
                track = new SampleDegreePlan(trackCode, description, blocks);
            }
        }
    }    
}
