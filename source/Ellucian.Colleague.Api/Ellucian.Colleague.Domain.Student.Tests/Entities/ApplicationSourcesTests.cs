//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class ApplicationSourcesTests
    {
        [TestClass]
        public class ApplicationSourcesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private ApplicationSource admissionApplicationSources;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                admissionApplicationSources = new ApplicationSource(guid, code, desc);
            }

            [TestMethod]
            public void ApplicationSources_Code()
            {
                Assert.AreEqual(code, admissionApplicationSources.Code);
            }

            [TestMethod]
            public void ApplicationSources_Description()
            {
                Assert.AreEqual(desc, admissionApplicationSources.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationSources_GuidNullException()
            {
                new ApplicationSource(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationSources_CodeNullException()
            {
                new ApplicationSource(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationSources_DescNullException()
            {
                new ApplicationSource(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationSourcesGuidEmptyException()
            {
                new ApplicationSource(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationSourcesCodeEmptyException()
            {
                new ApplicationSource(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationSourcesDescEmptyException()
            {
                new ApplicationSource(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class ApplicationSources_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private ApplicationSource admissionApplicationSources1;
            private ApplicationSource admissionApplicationSources2;
            private ApplicationSource admissionApplicationSources3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                admissionApplicationSources1 = new ApplicationSource(guid, code, desc);
                admissionApplicationSources2 = new ApplicationSource(guid, code, "Second Year");
                admissionApplicationSources3 = new ApplicationSource(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ApplicationSourcesSameCodesEqual()
            {
                Assert.IsTrue(admissionApplicationSources1.Equals(admissionApplicationSources2));
            }

            [TestMethod]
            public void ApplicationSourcesDifferentCodeNotEqual()
            {
                Assert.IsFalse(admissionApplicationSources1.Equals(admissionApplicationSources3));
            }
        }

        [TestClass]
        public class ApplicationSources_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private ApplicationSource admissionApplicationSources1;
            private ApplicationSource admissionApplicationSources2;
            private ApplicationSource admissionApplicationSources3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                admissionApplicationSources1 = new ApplicationSource(guid, code, desc);
                admissionApplicationSources2 = new ApplicationSource(guid, code, "Second Year");
                admissionApplicationSources3 = new ApplicationSource(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ApplicationSourcesSameCodeHashEqual()
            {
                Assert.AreEqual(admissionApplicationSources1.GetHashCode(), admissionApplicationSources2.GetHashCode());
            }

            [TestMethod]
            public void ApplicationSourcesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(admissionApplicationSources1.GetHashCode(), admissionApplicationSources3.GetHashCode());
            }
        }
    }
}
