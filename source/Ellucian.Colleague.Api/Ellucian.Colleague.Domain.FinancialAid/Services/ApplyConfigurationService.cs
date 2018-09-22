//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Services
{
    public class ApplyConfigurationService
    {
        /// <summary>
        /// Filters StudentAwardYears to a list of active StudentAwardYears
        /// </summary>
        /// <param name="studentAwardYears"></param>
        /// <returns></returns>
        public static IEnumerable<StudentAwardYear> FilterStudentAwardYears(IEnumerable<StudentAwardYear> studentAwardYears)
        {
            return studentAwardYears.Where(y => (y.CurrentConfiguration != null && y.CurrentConfiguration.IsSelfServiceActive)).AsEnumerable();
        }

        /// <summary>
        /// Filter StudentAwards based on the current configuration of each StudentAward and the overrides passed in. This method returns only
        /// StudentAward objects that are Viewable or have an AwardId contained in the list of award id overrides. This method also filters the
        /// StudentAwardPeriods objects that are Viewable or have an AwardPeriodId contained in the list of award period overrides.
        /// </summary>
        /// <param name="studentAwards">List of StudentAward objects to filter</param>
        /// <param name="includeAwardIdsOverride">List of override award ids that will force this method to return StudentAwards with these award ids regardless
        /// of the visibility.</param>
        /// <param name="includeAwardPeriodIdsOverride">List of override award period ids that will force this method to return StudentAwardPeriods with 
        /// these award period ids regardless of whether or not they are viewable.</param>
        /// <returns></returns>
        public static IEnumerable<StudentAward> FilterStudentAwards(IEnumerable<StudentAward> studentAwards, IEnumerable<string> includeAwardIdsOverride, IEnumerable<string> includeAwardPeriodIdsOverride)
        {
            if (includeAwardIdsOverride == null) includeAwardIdsOverride = new List<string>();
            if (includeAwardPeriodIdsOverride == null) includeAwardPeriodIdsOverride = new List<string>();

            //filter the StudentAwards.
            //StudentAward must be 
            //  Viewable and at least one award period must be viewable
            //  or contained in awardId overrides
            var filteredStudentAwards = studentAwards.Where(sa => (sa.IsViewable && sa.StudentAwardPeriods.Any(p => p.IsViewable)) || includeAwardIdsOverride.Contains(sa.Award.Code)).AsEnumerable();

            //now loop through each filtered student award and filter out the StudentAwardPeriods
            foreach (var studentAward in filteredStudentAwards)
            {
                var filteredStudentAwardPeriods = studentAward.StudentAwardPeriods.Where(p => p.IsViewable || includeAwardPeriodIdsOverride.Contains(p.AwardPeriodId));
                studentAward.StudentAwardPeriods = filteredStudentAwardPeriods.ToList();
            }

            //lastly, remove any student awards in which all the award periods were removed
            return filteredStudentAwards.Where(s => s.StudentAwardPeriods.Count() > 0).AsEnumerable();
        }

        /// <summary>
        /// Filter StudentAwards based on the current configuration of each StudentAward. This method returns only StudentAward objects
        /// that are Viewable. This method also sets the StudentAwardPeriods attribute of each StudentAward to only
        /// those StudentAwardPeriods that are Viewable.
        /// </summary>
        /// <param name="studentAwards"></param>
        /// <returns>Return the studentAwardYear records that are active.</returns>
        public static IEnumerable<StudentAward> FilterStudentAwards(IEnumerable<StudentAward> studentAwards)
        {
            return FilterStudentAwards(studentAwards, null, null);
        }

        /// <summary>
        /// Filters a single student award based on its IsViewable attribute. This method also filters
        /// the StudentAwardPeriods of the given StudentAward that are Viewable. This method returns null
        /// if the StudentAward is not Viewable or if none of the StudentAwardPeriods are viewable
        /// </summary>
        /// <param name="studentAward"></param>
        /// <returns></returns>
        public static StudentAward FilterStudentAwards(StudentAward studentAward)
        {
            return FilterStudentAwards(new List<StudentAward>() { studentAward }).FirstOrDefault();
        }

        /// <summary>
        /// Determine if this award letter is active for the years office
        /// </summary>
        /// <param name="awardLetter"></param>
        /// <returns></returns>
        public static AwardLetter FilterAwardLetters(AwardLetter awardLetter)
        {
            return FilterAwardLetters(new List<AwardLetter>() { awardLetter }).FirstOrDefault();
        }

        /// <summary>
        /// Exclude award letters that are not active for this years office 
        /// </summary>
        /// <param name="awardLetters"></param>
        /// <returns></returns>
        public static IEnumerable<AwardLetter> FilterAwardLetters(IEnumerable<AwardLetter> awardLetters)
        {
            return awardLetters.Where(a => a.AwardYear.CurrentConfiguration.IsAwardLetterActive);
        }

        /// <summary>
        /// This method filters the awards that are to be displayed on the award letter
        /// and shopping sheet
        /// </summary>
        /// <param name="studentAwards"></param>
        /// <returns>Filtered awards for the award letter/shopping sheet</returns>
        public static IEnumerable<StudentAward> FilterAwardsForAwardLetterAndShoppingSheetDisplay(IEnumerable<StudentAward> studentAwards)
        {
            // loop through each student award and filter out the StudentAwardPeriods based on excluded award status categories, award codes,
            //award period codes, and award categories 
            foreach (var studentAward in studentAwards)
            {
                var filteredStudentAwardPeriods = studentAward.StudentAwardPeriods.Where(p => p.IsViewableOnAwardLetterAndShoppingSheet);
                studentAward.StudentAwardPeriods = filteredStudentAwardPeriods.ToList();

            }

            // return only studentawards that have remaining studentAwardPeriods and that have award amounts greater than zero.
            return studentAwards.Where(s => s.StudentAwardPeriods.Count() > 0 && s.StudentAwardPeriods.Sum(p => p.AwardAmount ?? 0) > 0).AsEnumerable();
        }
    }
}
