//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentDocumentTests
    {
        [TestClass]
        public class StudentDocumentConstructorTests
        {
            private string studentId;
            private string documentCode;
            private DocumentStatus status;
            private string statusDesc;
            private DateTime? dueDate;
            private string instance;

            private StudentDocument studentDocument;


            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                documentCode = "FAC01STX";
                status = DocumentStatus.Waived;
                statusDesc = "Waived";
                dueDate = new DateTime(2014, 1, 6);
                instance = "New Instance";

                studentDocument = new StudentDocument(studentId, documentCode);
            }

            [TestMethod]
            public void StudentId_EqualsTest()
            {
                Assert.AreEqual(studentId, studentDocument.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentId_RequiredTest()
            {
                new StudentDocument("", documentCode);
            }

            [TestMethod]
            public void DocumentCode_EqualsTest()
            {
                Assert.AreEqual(documentCode, studentDocument.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DocumentCode_RequiredTest()
            {
                new StudentDocument(studentId, "");
            }

            [TestMethod]
            public void Status_InitializedTest()
            {
                Assert.AreEqual(DocumentStatus.Incomplete, studentDocument.Status);
            }

            [TestMethod]
            public void Status_GetSetTest()
            {
                studentDocument.Status = status;
                Assert.AreEqual(status, studentDocument.Status);
            }

            [TestMethod]
            public void DueDate_InitializedTest()
            {
                Assert.AreEqual(null, studentDocument.DueDate);
            }

            [TestMethod]
            public void DueDate_GetSetTest()
            {
                studentDocument.DueDate = dueDate;
                Assert.AreEqual(dueDate, studentDocument.DueDate);
            }

            [TestMethod]
            public void Instance_InitializedTest()
            {
                Assert.AreEqual(null, studentDocument.Instance);
            }

            [TestMethod]
            public void Instance_GetSetTest()
            {
                studentDocument.Instance = instance;
                Assert.AreEqual(instance, studentDocument.Instance);
            }

            [TestMethod]
            public void StatusDescription_InitializedTest()
            {
                Assert.AreEqual(null, studentDocument.StatusDescription);
            }

            [TestMethod]
            public void StatusDescription_GetSetTest()
            {
                studentDocument.StatusDescription = statusDesc;
                Assert.AreEqual(statusDesc, studentDocument.StatusDescription);
            }
        }
    }
}
