// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class TaxFormStatementTests
    {
        public TaxFormStatement statement;
        public TaxFormStatementBuilder statementBuilder = new TaxFormStatementBuilder();

        [TestMethod]
        public void TaxFormStatement_Success_W2()
        {
            statement = statementBuilder.Build();
            Assert.AreEqual(statementBuilder.PersonId, statement.PersonId);
            Assert.AreEqual(statementBuilder.TaxYear, statement.TaxYear);
            Assert.AreEqual(statementBuilder.TaxForm, statement.TaxForm);
            Assert.AreEqual(TaxFormNotations.None, statement.Notation);
        }

        [TestMethod]
        public void TaxFormStatement_Success_1095C()
        {
            // Change the form from the template that uses W-2 to 1095-C
            statement = statementBuilder.WithTaxForm(TaxForms.Form1095C).Build();
            Assert.AreEqual(statementBuilder.PersonId, statement.PersonId);
            Assert.AreEqual(statementBuilder.TaxYear, statement.TaxYear);
            Assert.AreEqual(statementBuilder.TaxForm, statement.TaxForm);
            Assert.AreEqual(TaxFormNotations.None, statement.Notation);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPersonId()
        {
            statement = statementBuilder.WithPersonId(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyPersonId()
        {
            statement = statementBuilder.WithPersonId(string.Empty).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxYear()
        {
            statement = statementBuilder.WithTaxYear(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyTaxYear()
        {
            statement = statementBuilder.WithTaxYear(string.Empty).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPdfRecordId()
        {
            statement = statementBuilder.WithRecordId(null).Build();
        }

    }
}
