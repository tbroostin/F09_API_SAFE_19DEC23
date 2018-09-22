//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{

    /// <summary>
    /// A custom AwardLetterEntityToDtoAdapter that maps AwardLetter, StudentAward and Person domain entities to an AwardLetter DTO
    /// </summary>
    public class AwardLetterEntityToDtoAdapter
    {
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;

        /// <summary>
        /// Instantiate the adapter
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="logger">logger</param>
        public AwardLetterEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Map an AwardLetter, a list of StudentAwards, and a Person domain objects to an AwardLetter DTO.
        /// </summary>
        /// <param name="sourceAwardLetter">AwardLetter Domain object</param>
        /// <param name="sourceStudentAwards">List of StudentAward domain objects that constitute the award table on the letter</param>
        /// <param name="sourcePerson">Person domain object. Used for demographic information.</param>
        /// <returns>an AwardLetter DTO</returns>
        public Dtos.FinancialAid.AwardLetter MapToType(Domain.FinancialAid.Entities.AwardLetter sourceAwardLetter, IEnumerable<Domain.FinancialAid.Entities.StudentAward> sourceStudentAwards, Domain.Base.Entities.Person sourcePerson)
        {
            //create the office address
            var officeAddress = new List<Dtos.FinancialAid.AwardLetterAddress>();
            if (sourceAwardLetter.IsContactBlockActive)
            {
                officeAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = sourceAwardLetter.ContactName });
                foreach (var line in sourceAwardLetter.ContactAddress)
                {
                    officeAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = line });
                }
                officeAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = sourceAwardLetter.ContactPhoneNumber });
            }

            //create the student's address
            var studentAddress = new List<Dtos.FinancialAid.AwardLetterAddress>();
            studentAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = sourcePerson.PreferredName });
            foreach (var line in sourcePerson.PreferredAddress)
            {
                studentAddress.Add(new Dtos.FinancialAid.AwardLetterAddress() { AddressLine = line });
            }

            var awardTableRows = new List<Dtos.FinancialAid.AwardLetterAward>();
            var assignedAwards = new List<Domain.FinancialAid.Entities.StudentAward>();

            //Put awards in assigned categories in one list and create award table rows for each
            for (var i = 0; i < sourceAwardLetter.AwardCategoriesGroups.Count; i++)
            {
                var group = sourceAwardLetter.AwardCategoriesGroups[i];
                var selectedAwards = sourceStudentAwards.Where(sa => group.Members.Any(m => sa.Award.AwardCategory != null && m == sa.Award.AwardCategory.Code));
                assignedAwards.AddRange(selectedAwards);
                foreach (var award in selectedAwards)
                {
                    awardTableRows.Add(new Dtos.FinancialAid.AwardLetterAward()
                    {
                        GroupNumber = group.SequenceNumber,
                        GroupName = group.Title,
                        AwardId = award.Award.Code,
                        AwardDescription = award.Award.Description
                    });
                }
            }

            //All the remaining awards not in other cats go into the last category
            var remainingAwards = sourceStudentAwards.Where(ssa => assignedAwards.All(sca => sca.Award.Code != ssa.Award.Code));
            foreach (var award in remainingAwards)
            {
                awardTableRows.Add(new Dtos.FinancialAid.AwardLetterAward()
                {
                    GroupNumber = sourceAwardLetter.NonAssignedAwardsGroup.SequenceNumber,
                    GroupName = sourceAwardLetter.NonAssignedAwardsGroup.Title,
                    AwardId = award.Award.Code,
                    AwardDescription = award.Award.Description
                });
            }

            //Max capacity of this array is six since that is the max number of award period columns in ALPA
            var awardPeriodColumnsArray = new List<List<Domain.FinancialAid.Entities.StudentAwardPeriod>>(6)
            {
                new List<Domain.FinancialAid.Entities.StudentAwardPeriod>(),
                new List<Domain.FinancialAid.Entities.StudentAwardPeriod>(),
                new List<Domain.FinancialAid.Entities.StudentAwardPeriod>(),
                new List<Domain.FinancialAid.Entities.StudentAwardPeriod>(),
                new List<Domain.FinancialAid.Entities.StudentAwardPeriod>(),
                new List<Domain.FinancialAid.Entities.StudentAwardPeriod>()
            };

            //All student award periods for all student awards for the year
            var studentAwardPeriods = sourceStudentAwards.SelectMany(sca => sca.StudentAwardPeriods);
            var awardPeriodColumnGroupsCount = sourceAwardLetter.AwardPeriodColumnGroups.Count;

            var awardPeriodColumnIndicies = new List<int>();

            //Put each award period in the appropriate period column
            foreach (var period in studentAwardPeriods)
            {
                // we use 6 as cut off for the loop since the index for the awardPeriodColumnsGroups goes from 0 to 5
                // for the total possible groups count of 6.
                for (var i = 0; i < awardPeriodColumnGroupsCount; i++)
                {
                    if (sourceAwardLetter.AwardPeriodColumnGroups[i].Members.Contains(period.AwardPeriodId))
                    {
                        awardPeriodColumnsArray[i].Add(period);
                        awardPeriodColumnIndicies.Add(i);
                    }
                }
                var awardTableRow = awardTableRows.FirstOrDefault(atr => atr.AwardId == period.Award.Code);
                if (awardTableRow != null)
                {
                    awardTableRow.Total += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                }
                
            }

            awardPeriodColumnIndicies = awardPeriodColumnIndicies.Distinct().OrderBy(i => i).ToList();

            //Get only those award period columns that contain some award periods
            var awardPeriodColumns = awardPeriodColumnsArray.Where(apc => apc.Count > 0).ToList();
            var awardPeriodsCount = awardPeriodColumns.Count;

            //Assign amounts to the correct award table row, period column
            for (var i = 0; i < awardPeriodsCount; i++)
            {
                var periods = awardPeriodColumns[i];
                foreach (var period in periods)
                {
                    var awardTableRow = awardTableRows.FirstOrDefault(atr => atr.AwardId == period.Award.Code);
                    if (awardTableRow != null)
                    {
                        switch (i)
                        {
                            case 0:
                                awardTableRow.Period1Amount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                                break;
                            case 1:
                                awardTableRow.Period2Amount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                                break;
                            case 2:
                                awardTableRow.Period3Amount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                                break;
                            case 3:
                                awardTableRow.Period4Amount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                                break;
                            case 4:
                                awardTableRow.Period5Amount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                                break;
                            case 5:
                                awardTableRow.Period6Amount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                                break;
                        }                        
                    }
                }
            }

            //finally add a total row
            awardTableRows.Add(new Dtos.FinancialAid.AwardLetterAward()
            {
                GroupName = "",
                GroupNumber = sourceAwardLetter.NonAssignedAwardsGroup.SequenceNumber + 1, //the non-assigned awards group is the last category group, +1 to create the final Total group
                AwardId = "",
                AwardDescription = "Total",
                Period1Amount = awardTableRows.Sum(r => r.Period1Amount),
                Period2Amount = awardTableRows.Sum(r => r.Period2Amount),
                Period3Amount = awardTableRows.Sum(r => r.Period3Amount),
                Period4Amount = awardTableRows.Sum(r => r.Period4Amount),
                Period5Amount = awardTableRows.Sum(r => r.Period5Amount),
                Period6Amount = awardTableRows.Sum(r => r.Period6Amount),
                Total = awardTableRows.Sum(r => r.Total)
            });

            var awardYearEntity = sourceAwardLetter.AwardYear;
            //create the DTO
            var awardLetterReportDto = new Dtos.FinancialAid.AwardLetter()
            {
                AwardYearCode = awardYearEntity.Code,
                AwardYearDescription = awardYearEntity.Description,
                StudentId = sourceAwardLetter.StudentId,
                StudentName = sourcePerson.PreferredName,
                IsContactBlockActive = sourceAwardLetter.IsContactBlockActive,
                ContactAddress = officeAddress,
                StudentAddress = studentAddress,
                Date = DateTime.Today,
                OpeningParagraph = sourceAwardLetter.OpeningParagraph,
                IsNeedBlockActive = sourceAwardLetter.IsNeedBlockActive,
                IsHousingCodeActive = sourceAwardLetter.IsHousingCodeActive,
                HousingCode = (Colleague.Dtos.FinancialAid.HousingCode?)sourceAwardLetter.HousingCode,
                BudgetAmount = sourceAwardLetter.BudgetAmount,
                EstimatedFamilyContributionAmount = sourceAwardLetter.EstimatedFamilyContributionAmount,
                NeedAmount = sourceAwardLetter.NeedAmount,
                AwardColumnHeader = sourceAwardLetter.AwardNameTitle,
                TotalColumnHeader = sourceAwardLetter.AwardTotalTitle,
                AwardPeriod1ColumnHeader = awardPeriodColumnIndicies.Count() > 0 ? sourceAwardLetter.AwardPeriodColumnGroups[awardPeriodColumnIndicies[0]].Title : string.Empty,
                AwardPeriod2ColumnHeader = awardPeriodColumnIndicies.Count() > 1 ? sourceAwardLetter.AwardPeriodColumnGroups[awardPeriodColumnIndicies[1]].Title : string.Empty,
                AwardPeriod3ColumnHeader = awardPeriodColumnIndicies.Count() > 2 ? sourceAwardLetter.AwardPeriodColumnGroups[awardPeriodColumnIndicies[2]].Title : string.Empty,
                AwardPeriod4ColumnHeader = awardPeriodColumnIndicies.Count() > 3 ? sourceAwardLetter.AwardPeriodColumnGroups[awardPeriodColumnIndicies[3]].Title : string.Empty,
                AwardPeriod5ColumnHeader = awardPeriodColumnIndicies.Count() > 4 ? sourceAwardLetter.AwardPeriodColumnGroups[awardPeriodColumnIndicies[4]].Title : string.Empty,
                AwardPeriod6ColumnHeader = awardPeriodColumnIndicies.Count() > 5 ? sourceAwardLetter.AwardPeriodColumnGroups[awardPeriodColumnIndicies[5]].Title : string.Empty,
                NumberAwardPeriodColumns = awardPeriodsCount,
                AwardTableRows = awardTableRows,
                ClosingParagraph = sourceAwardLetter.ClosingParagraph,
                AcceptedDate = sourceAwardLetter.AcceptedDate
            };

            return awardLetterReportDto;
        }
    }
}
