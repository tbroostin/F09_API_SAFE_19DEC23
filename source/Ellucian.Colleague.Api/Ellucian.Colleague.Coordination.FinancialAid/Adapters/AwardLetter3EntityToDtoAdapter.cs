//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// A custom AwardLetter3EntityToDtoAdapter that maps AwardLetter3 and Person domain entities to an AwardLetter3 DTO
    /// </summary>
    public class AwardLetter3EntityToDtoAdapter
    {
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;

        /// <summary>
        /// Instantiate the adapter
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="logger">logger</param>
        public AwardLetter3EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Map an AwardLetter3, a list of StudentAwards, and a Person domain objects to an AwardLetter3 DTO.
        /// </summary>
        /// <param name="sourceAwardLetter">AwardLetter3 Domain object</param>
        /// <returns>an AwardLetter3 DTO</returns>       
        public Dtos.FinancialAid.AwardLetter3 MapToType(Domain.FinancialAid.Entities.AwardLetter3 sourceAwardLetter)
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
                singleAward.AwRenewableFlag = annualAward.AwRenewableFlag;
                singleAward.AwRenewableText = annualAward.AwRenewableText;

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

            var awardLetterHistoryCosts = new List<Dtos.FinancialAid.AwardLetterHistoryCost>();
            if (sourceAwardLetter.AlhDirectCostDesc != null)
            {
                for (var i = 0; i < sourceAwardLetter.AlhDirectCostDesc.Count(); i++)
                {
                    var directCost = new Dtos.FinancialAid.AwardLetterHistoryCost();
                    directCost.Description = sourceAwardLetter.AlhDirectCostDesc[i];
                    directCost.Amount = sourceAwardLetter.AlhDirectCostAmount[i];
                    directCost.CostType = "Direct Costs";
                    awardLetterHistoryCosts.Add(directCost);
                }
            }
            if (sourceAwardLetter.AlhIndirectCostDesc != null)
            {
                for (var j = 0; j < sourceAwardLetter.AlhIndirectCostDesc.Count(); j++)
                {
                    var indirectCost = new Dtos.FinancialAid.AwardLetterHistoryCost();
                    indirectCost.Description = sourceAwardLetter.AlhIndirectCostDesc[j];
                    indirectCost.Amount = sourceAwardLetter.AlhIndirectCostAmount[j];
                    indirectCost.CostType = "Indirect Costs";
                    awardLetterHistoryCosts.Add(indirectCost);
                }
            }

            var offerLetterHousingEnrollmentItems = new List<Dtos.FinancialAid.OfferLetterHousingEnrollmentItem>();
            if (sourceAwardLetter.AlhHousingDesc != null)
            {
                for (var i = 0; i < sourceAwardLetter.AlhHousingDesc.Count(); i++)
                {
                    var housingEnrollmentDesc = new Dtos.FinancialAid.OfferLetterHousingEnrollmentItem();
                    housingEnrollmentDesc.AlhHousingDesc = sourceAwardLetter.AlhHousingDesc[i];
                    offerLetterHousingEnrollmentItems.Add(housingEnrollmentDesc);
                }
            }
            if (sourceAwardLetter.AlhEnrollmentStatus != null && offerLetterHousingEnrollmentItems != null && offerLetterHousingEnrollmentItems.Any())
            {
                for (var j = 0; j < sourceAwardLetter.AlhEnrollmentStatus.Count(); j++)
                {
                    if (offerLetterHousingEnrollmentItems.Count() < sourceAwardLetter.AlhEnrollmentStatus.Count())
                    {
                        var newEnrollmentDesc = new Dtos.FinancialAid.OfferLetterHousingEnrollmentItem();
                        offerLetterHousingEnrollmentItems.Add(newEnrollmentDesc);
                    }
                    offerLetterHousingEnrollmentItems[j].AlhEnrollmentDesc = sourceAwardLetter.AlhEnrollmentStatus[j];
                }
            }
            else if(sourceAwardLetter.AlhEnrollmentStatus != null)
            {
                for (var j = 0; j < sourceAwardLetter.AlhEnrollmentStatus.Count(); j++)
                {
                    var enrollmentDesc = new Dtos.FinancialAid.OfferLetterHousingEnrollmentItem();
                    enrollmentDesc.AlhEnrollmentDesc = sourceAwardLetter.AlhEnrollmentStatus[j];
                    offerLetterHousingEnrollmentItems.Add(enrollmentDesc);
                }
            }

            // Get distinct award periods for a year
            var awardPeriodsForYear = sourceAwardLetter.AwardLetterAnnualAwards.Any() ? sourceAwardLetter.AwardLetterAnnualAwards.SelectMany(a => a.AwardLetterAwardPeriods).ToList() : new List<Domain.FinancialAid.Entities.AwardLetterAwardPeriod>();
            var distinctAwardPeriodColumnsForYear = awardPeriodsForYear.Any() ? awardPeriodsForYear.Select(ap => ap.ColumnNumber).Distinct().OrderBy(cn => cn).ToList() : new List<int>();
            var awardPeriodTotals = new List<Dtos.FinancialAid.AwardPeriodTotal>();
            // Calculate totals for total row for pdf
            foreach (var columnNumber in distinctAwardPeriodColumnsForYear)
            {
                var columnGroupPeriods = awardPeriodsForYear.Where(ap => ap.ColumnNumber == columnNumber).ToList();
                var updatedColumnGroupPeriods = new List<Domain.FinancialAid.Entities.AwardLetterAwardPeriod>();
                var awardPeriodTotal = new Dtos.FinancialAid.AwardPeriodTotal();
                foreach (var period in columnGroupPeriods)
                {
                    if (period.GroupNumber != 4)
                    {
                        updatedColumnGroupPeriods.Add(period);
                    }
                }
                var totalAmountForPeriod = updatedColumnGroupPeriods.Any() ? updatedColumnGroupPeriods.Sum(p => p.AwardPeriodAmount) : null;
                var nonNullAmount = totalAmountForPeriod.GetValueOrDefault();
                awardPeriodTotal.TotalAmount = nonNullAmount;
                awardPeriodTotals.Add(awardPeriodTotal);
            }

            //create the DTO
            var awardLetterReportDto = new Dtos.FinancialAid.AwardLetter3()
            {
                AwardLetterYear = sourceAwardLetter.AwardLetterYear,
                AwardYearDescription = sourceAwardLetter.AwardYearDescription,
                StudentId = sourceAwardLetter.StudentId,
                StudentName = sourceAwardLetter.StudentName,
                StudentAddress = studentAddress,

                AwardLetterParameterId = sourceAwardLetter.AwardLetterParameterId,
                Id = sourceAwardLetter.Id,
                StudentOfficeCode = sourceAwardLetter.StudentOfficeCode,
                AwardLetterHistoryType = sourceAwardLetter.AwardLetterHistoryType,

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
                PreAwardText = sourceAwardLetter.PreAwardText,
                PostAwardText = sourceAwardLetter.PostAwardText,
                PostClosingText = sourceAwardLetter.PostClosingText,
                AcceptedDate = sourceAwardLetter.AcceptedDate,
                CreatedDate = sourceAwardLetter.CreatedDate,
                AlhEnrollmentStatus = sourceAwardLetter.AlhEnrollmentStatus,
                AlhHousingInd = sourceAwardLetter.AlhHousingInd,
                AlhHousingDesc = sourceAwardLetter.AlhHousingDesc,
                OfferLetterHousingEnrollmentItem = offerLetterHousingEnrollmentItems,

                // Direct cost assignment
                AlhDirectCostAmount = sourceAwardLetter.AlhDirectCostAmount,
                AlhDirectCostDesc = sourceAwardLetter.AlhDirectCostDesc,
                AlhDirectCostComp = sourceAwardLetter.AlhDirectCostComp,
                AwardLetterHistoryCosts = awardLetterHistoryCosts,

                // Indirect cost assignment
                AlhIndirectCostAmount = sourceAwardLetter.AlhIndirectCostAmount,
                AlhIndirectCostDesc = sourceAwardLetter.AlhIndirectCostDesc,
                AlhIndirectCostComp = sourceAwardLetter.AlhIndirectCostComp,

                // Pell entitlement assignment
                AlhPellEntitlementList = sourceAwardLetter.AlhPellEntitlementList,

                //Total amount for period for PDF
                AwardPeriodTotals = awardPeriodTotals,
                AlhZeroAwardFlg = sourceAwardLetter.AlhZeroAwardFlg,
                ZeroAwardPeriods = sourceAwardLetter.AlhZeroAwpds
            };

            return awardLetterReportDto;
        }
    }
}
