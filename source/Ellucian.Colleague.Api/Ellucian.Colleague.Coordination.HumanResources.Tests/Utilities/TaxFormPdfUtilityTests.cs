// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.HumanResources.Utilities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Utilities
{
	[TestClass]
	public class TaxFormPdfUtilityTests
	{
		private Dictionary<string, string> pdfData;
		private Form1095cCoveredIndividualsPdfData coveredIndividual;

		[TestInitialize]
		public void Initialize()
		{
			pdfData = new Dictionary<string, string>();
			coveredIndividual = new Form1095cCoveredIndividualsPdfData()
			{
				 Covered12Month = true,
				 CoveredJanuary = false,
				 CoveredFebruary = false,
				 CoveredMarch = false,
				 CoveredApril = false,
				 CoveredMay = false,
				 CoveredJune = false,
				 CoveredJuly = false,
				 CoveredAugust = false,
				 CoveredSeptember = false,
				 CoveredOctober = false,
				 CoveredNovember = false,
				 CoveredDecember = false,
				 CoveredIndividualDateOfBirth = new DateTime(1984, 07, 18),
				 CoveredIndividualFirstName = "Emma",
				 CoveredIndividualMiddleName = "Joyce",
				 CoveredIndividualLastName = "Kleehammer",
				 CoveredIndividualSsn = "000-00-0001",
				 IsEmployeeItself = false,
			};
		}

		[TestCleanup]
		public void Cleanup()
		{
			pdfData = null;
		}

		[TestMethod]
		public void AddDependentRowToDictionary_FullyCovered()
		{
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual(coveredIndividual.CoveredIndividualName(), pdfData["Box" + rowNumber.ToString() + "Name"]);
			Assert.AreEqual(coveredIndividual.CoveredIndividualSsn, pdfData["Box" + rowNumber.ToString() + "SSN"]);
			Assert.AreEqual(coveredIndividual.CoveredIndividualDateOfBirth.Value.ToString("yyyy-MM-dd"), pdfData["Box" + rowNumber.ToString() + "DOB"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredJanuary()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredJanuary = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredFebruary()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredFebruary = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredMarch()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredMarch = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredApril()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredApril = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredMay()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredMay = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredJune()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredJune = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredJuly()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredJuly = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredAugust()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredAugust = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredSeptember()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredSeptember = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredOctober()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredOctober = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredNovember()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredNovember = true;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}

		[TestMethod]
		public void AddDependentRowToDictionary_CoveredDecember()
		{
			coveredIndividual.Covered12Month = false;
			coveredIndividual.CoveredDecember = true;
			coveredIndividual.CoveredIndividualDateOfBirth = null;
			int rowNumber = 17;
			TaxFormPdfUtility.Populate1095CDependentRow(ref pdfData, coveredIndividual, rowNumber);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "All"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Jan"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Feb"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Mar"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Apr"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "May"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "June"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "July"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Aug"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Sept"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Oct"]);
			Assert.AreEqual("", pdfData["Box" + rowNumber.ToString() + "Nov"]);
			Assert.AreEqual("X", pdfData["Box" + rowNumber.ToString() + "Dec"]);
		}
	}
}