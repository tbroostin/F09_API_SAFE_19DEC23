﻿//Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// A custom AwardLetter2EntityToDtoAdapter that maps AwardLetter2 and Person domain entities to an AwardLetter2 DTO
    /// </summary>
    public class AwardLetter2EntityToDtoAdapter
    {
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;

        /// <summary>
        /// Instantiate the adapter
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="logger">logger</param>
        public AwardLetter2EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Map an AwardLetter, a list of StudentAwards, and a Person domain objects to an AwardLetter DTO.
        /// </summary>
        /// <param name="sourceAwardLetter">AwardLetter2 Domain object</param>
        /// <returns>an AwardLetter DTO</returns>       
        public Dtos.FinancialAid.AwardLetter2 MapToType(Domain.FinancialAid.Entities.AwardLetter2 sourceAwardLetter)       
        {
            //create the office address
            var officeAddress = new List<Dtos.FinancialAid.AwardLetterAddress>();
            if (sourceAwardLetter.ContactAddress != null)
            {
                officeAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = sourceAwardLetter.ContactName });
                foreach (var line in sourceAwardLetter.ContactAddress)
                {
                    officeAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = line });
                }
            }
            if (sourceAwardLetter.ContactPhoneNumber != null)
            {
                officeAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = sourceAwardLetter.ContactPhoneNumber });
            }

            //create the student's address
            var studentAddress = new List<Dtos.FinancialAid.AwardLetterAddress>();

            if (sourceAwardLetter.StudentAddress.Any())
            {
                foreach (var line in sourceAwardLetter.StudentAddress)
                {
                    studentAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = line });
                }
            }

            //Add Award Letter Award category Group information used for this letter
            var awardCategoryGroupData = new List<Dtos.FinancialAid.AwardLetterGroup>();
            foreach (var groupRecord in sourceAwardLetter.AwardLetterGroups)
            {
                var singleGroup = new Dtos.FinancialAid.AwardLetterGroup();
                singleGroup.GroupName = groupRecord.GroupName;
                singleGroup.GroupNumber = groupRecord.GroupNumber;
                singleGroup.GroupType = Dtos.FinancialAid.GroupType.AwardCategories;

                awardCategoryGroupData.Add(singleGroup);
            }

            // Add in the Annual Award Information
            var awardLetterAnnualAwards = new List<Dtos.FinancialAid.AwardLetterAnnualAward>();

            foreach (var annualAward in sourceAwardLetter.AwardLetterAnnualAwards)
            {
                var singleAward = new Dtos.FinancialAid.AwardLetterAnnualAward();
                singleAward.AwardId = annualAward.AwardId;
                singleAward.AnnualAwardAmount = annualAward.AnnualAnnualAmount;
                singleAward.AwardDescription = annualAward.AwardDescription;
                singleAward.GroupName = annualAward.GroupName;
                singleAward.GroupNumber = annualAward.GroupNumber;

                // Add in the Award Period Information
                var awardLetterAwardPeriodRecords = new List<Dtos.FinancialAid.AwardLetterAwardPeriod>();
                
                foreach (var awardPeriodRecord in annualAward.AwardLetterAwardPeriods)
                {
                    var awpRecord = new Dtos.FinancialAid.AwardLetterAwardPeriod();
                    awpRecord.AwardId = awardPeriodRecord.AwardId;
                    awpRecord.AwardDescription = awardPeriodRecord.AwardDescription;
                    awpRecord.AwardPeriodAmount = awardPeriodRecord.AwardPeriodAmount;
                    awpRecord.GroupName = awardPeriodRecord.GroupName;
                    awpRecord.GroupNumber = awardPeriodRecord.GroupNumber;
                    awpRecord.ColumnName = awardPeriodRecord.ColumnName;
                    awpRecord.ColumnNumber = awardPeriodRecord.ColumnNumber;
                    awardLetterAwardPeriodRecords.Add(awpRecord);
                }

                singleAward.AwardLetterAwardPeriods = awardLetterAwardPeriodRecords;
                awardLetterAnnualAwards.Add(singleAward);

            }

            //create the DTO
            var awardLetterReportDto = new Dtos.FinancialAid.AwardLetter2()
            {
                AwardLetterYear = sourceAwardLetter.AwardLetterYear,
                AwardYearDescription = sourceAwardLetter.AwardYearDescription,
                StudentId = sourceAwardLetter.StudentId,
                StudentName = sourceAwardLetter.StudentName,
                StudentAddress = studentAddress,

                AwardLetterParameterId = sourceAwardLetter.AwardLetterParameterId,
                Id = sourceAwardLetter.Id,
                StudentOfficeCode = sourceAwardLetter.StudentOfficeCode,

                ContactName = sourceAwardLetter.ContactName,
                ContactAddress = officeAddress,
              
                AwardLetterGroups = awardCategoryGroupData,                
                AwardLetterAnnualAwards = awardLetterAnnualAwards,

                HousingCode = (Colleague.Dtos.FinancialAid.HousingCode?)sourceAwardLetter.HousingCode,
                BudgetAmount = sourceAwardLetter.BudgetAmount,
                EstimatedFamilyContributionAmount = sourceAwardLetter.EstimatedFamilyContributionAmount,
                NeedAmount = sourceAwardLetter.NeedAmount,

                OpeningParagraph = sourceAwardLetter.OpeningParagraph,
                ClosingParagraph = sourceAwardLetter.ClosingParagraph,
                AcceptedDate = sourceAwardLetter.AcceptedDate,
                CreatedDate = sourceAwardLetter.CreatedDate,
            };

            return awardLetterReportDto;
        }
    }
}