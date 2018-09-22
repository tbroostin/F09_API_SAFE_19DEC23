// Copyright 2015 - 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using slf4net;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base
{
    public static class RecurrenceUtility 
    {
        /// <summary>
        /// GetRecurrenceDates
        /// </summary>
        /// <param name="recurrence"></param>
        /// <param name="timePeriod"></param>
        /// <param name="frequency"></param>
        /// <param name="campusCalendar"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetRecurrenceDates(Recurrence3 recurrence, Ellucian.Colleague.Domain.Base.Entities.FrequencyType frequency, CampusCalendar campusCalendar)
        {
            if (recurrence == null)
                throw new ArgumentNullException("recurrence", "At least one recurrence pattern must be provided.");
            
            return GetRecurrenceDates(recurrence.RepeatRule, recurrence.TimePeriod, frequency, campusCalendar);
        }

        /// <summary>
        /// Get all meeting dates
        /// </summary>
        /// <param name="repeatRule"></param>
        /// <param name="timePeriod"></param>
        /// <param name="frequency"></param>
        /// <param name="campusCalendar"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetRecurrenceDates(IRepeatRule repeatRule, RepeatTimePeriod2 timePeriod, Ellucian.Colleague.Domain.Base.Entities.FrequencyType frequency, CampusCalendar campusCalendar, string colleagueTimeZone = "")
        {
            if (repeatRule == null)
                throw new ArgumentNullException("repeatRule", "At least one repeatRule must be specified on the recurrence pattern.");

            if (repeatRule.Type == null)
                throw new ArgumentNullException("repeatRule", "The type must be specified on the repeat rule recurrence pattern.");

            if (timePeriod == null)
                throw new ArgumentNullException("timePeriod", "At least one timePeriod must be specified on the recurrence pattern.");

            if (timePeriod.EndOn == null)
                throw new ArgumentNullException("timePeriod.EndDate", "The end date must be specified on the recurrence pattern.");

            if (timePeriod.StartOn == null)
                throw new ArgumentNullException("timePeriod.StartDate", "The start date must be specified on the recurrence pattern.");


            IEnumerable<DateTime> meetingDates = null;
            var repeatRuleType = repeatRule.Type;
            DateTime endDate = timePeriod.EndOn.Value.Date;

            if (repeatRuleType == Dtos.FrequencyType2.Daily)
            {
                var repeatRuleDaily = (RepeatRuleDaily)repeatRule;

                if (repeatRuleDaily.Ends == null)
                    throw new ArgumentNullException("repeatRuleDaily.Ends", "The RepeatRuleDaily Ends must be provided on the recurrence pattern.");
                
                var interval = (repeatRuleDaily.Interval == 0) ? 1 : (int)repeatRuleDaily.Interval;

                 List<HedmDayOfWeek?> days = Enum.GetValues(typeof(HedmDayOfWeek)).Cast<HedmDayOfWeek?>().ToList();  
                 
                meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);

              
            }
            else if (repeatRuleType == Dtos.FrequencyType2.Weekly)
            {
                var repeatRuleWeekly = (RepeatRuleWeekly)repeatRule;

                if (repeatRuleWeekly.Ends == null)
                    throw new ArgumentNullException("repeatRuleWeekly.Ends", "The RepeatRuleWeekly Ends must be provided on the recurrence pattern.");
                
                var interval = (repeatRuleWeekly.Interval == 0) ? 1 : (int)repeatRuleWeekly.Interval;

                List<HedmDayOfWeek?> days = repeatRuleWeekly.DayOfWeek;
                meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);

            }
            else if (repeatRuleType == Dtos.FrequencyType2.Monthly)
            {
                var repeatRuleMonthly = (RepeatRuleMonthly)repeatRule;
                if (repeatRuleMonthly == null)
                    throw new ArgumentNullException("repeatRuleMonthly", "RepeatRuleMonthly must be provided on the recurrence pattern.");

                var interval = repeatRuleMonthly.Interval;

                if (repeatRuleMonthly.Ends == null)
                    throw new ArgumentNullException("repeatRuleMonthly.Ends", "The RepeatRuleMonthly Ends must be provided on the recurrence pattern.");
                var ends = repeatRuleMonthly.Ends;

                int? repetitions;
                if (ends.Repetitions != null)
                    repetitions = ends.Repetitions;

                if (repeatRuleMonthly.RepeatBy == null)
                    throw new ArgumentNullException("repeatRuleMonthly.RepeatBy", "The RepeatRuleMonthly RepeatBy rule must be provided on the recurrence pattern.");

                if (repeatRuleMonthly.RepeatBy.DayOfMonth != null && repeatRuleMonthly.RepeatBy.DayOfMonth != 0)
                {
                    var dayOfMonth = repeatRuleMonthly.RepeatBy.DayOfMonth;
                    if (dayOfMonth > 31)
                        throw new ArgumentException("If providing a dayOfMonth to the RepeatRuleMonthly RepeatBy rule must, it must be a valid number.", "dayOfMonth");

                    List<HedmDayOfWeek?> days = new List<HedmDayOfWeek?>() { (HedmDayOfWeek)timePeriod.StartOn.Value.DayOfWeek };
                    meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);
                }
                else if (repeatRuleMonthly.RepeatBy.DayOfWeek != null)
                {
                    List<DateTime> meetingDays = new List<DateTime>();
                    var occurence = repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence;
                    DateTime counter = timePeriod.StartOn.Value.DateTime;
                    var lastDateOfEndMonth = new DateTime(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month, DateTime.DaysInMonth(timePeriod.EndOn.Value.DateTime.Year, timePeriod.EndOn.Value.DateTime.Month));
                    while (DateTime.Compare(counter, lastDateOfEndMonth) <= 0)
                    {
                        meetingDays.Add(GetDateByOrdinalDay(counter.Year, counter.Month, (DayOfWeek)repeatRuleMonthly.RepeatBy.DayOfWeek.Day, occurence));
                        counter = counter.AddMonths(1);
                    }
                    meetingDates = meetingDays.Distinct().OrderBy(d => d).ToList();
                }
                else
                    throw new ArgumentNullException("repeatRuleMonthly.RepeatBy", "The RepeatRuleMonthly RepeatBy rule must contain either a DayOfMonth or DayOfWeek.");
                }
            else if (repeatRuleType == Dtos.FrequencyType2.Yearly)
            {
                var repeatRuleYearly = (RepeatRuleYearly)repeatRule;
                var interval = (repeatRuleYearly.Interval == 0) ? 1 : (int)repeatRuleYearly.Interval;
                

                List<HedmDayOfWeek?> days = new List<HedmDayOfWeek?>() { (HedmDayOfWeek)timePeriod.StartOn.Value.DayOfWeek };
                meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);


                
            }

            return meetingDates.Select(x => x.Date).Distinct();
        }


        /// <summary>
        /// Get all meeting dates
        /// </summary>
        /// <param name="repeatRule"></param>
        /// <param name="timePeriod"></param>
        /// <param name="frequency"></param>
        /// <param name="campusCalendar"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetRecurrenceDates2(IRepeatRule repeatRule, RepeatTimePeriod2 timePeriod, Ellucian.Colleague.Domain.Base.Entities.FrequencyType frequency, CampusCalendar campusCalendar, string colleagueTimeZone = "")
        {
            if (repeatRule == null)
                throw new ArgumentNullException("repeatRule", "At least one repeatRule must be specified on the recurrence pattern.");

            if (timePeriod == null)
                throw new ArgumentNullException("timePeriod", "At least one timePeriod must be specified on the recurrence pattern.");

            if (timePeriod.StartOn == null)
                throw new ArgumentNullException("timePeriod.StartDate", "The start date must be specified on the recurrence pattern.");


            IEnumerable<DateTime> meetingDates = null;
            var repeatRuleType = repeatRule.Type;
            DateTime endDate = timePeriod.EndOn.Value.Date;

            if (repeatRuleType == Dtos.FrequencyType2.Daily)
            {
                var repeatRuleDaily = (RepeatRuleDaily)repeatRule;
                if (repeatRuleDaily == null)
                    throw new ArgumentException("timePeriod", "Repeat rule daily cant not be extracted.");

                var interval = (repeatRuleDaily.Interval == 0) ? 1 : (int)repeatRuleDaily.Interval;

                if ((endDate == default(DateTime)) && (repeatRuleDaily.Ends == null))
                    throw new ArgumentException("timePeriod", "Must provide either an endOn date or a repeat rule.");
                if ((repeatRuleDaily.Ends != null)
                    && (!repeatRuleDaily.Ends.Date.HasValue) && (!repeatRuleDaily.Ends.Repetitions.HasValue))
                    throw new ArgumentException("timePeriod", "Repeat rule must contain an end date, or a repetition interval.");
                if ((endDate == default(DateTime)) && (repeatRuleDaily.Ends != null) && (repeatRuleDaily.Ends.Date.HasValue))
                    endDate = Convert.ToDateTime(repeatRuleDaily.Ends.Date);
                if ((endDate == default(DateTime)) && (repeatRuleDaily.Ends != null)
                    && (repeatRuleDaily.Ends.Repetitions.HasValue) && (repeatRuleDaily.Ends.Repetitions != 0))
                {
                    int daysToAdd = interval * Convert.ToInt16(repeatRuleDaily.Ends.Repetitions);
                    endDate = Convert.ToDateTime(timePeriod.StartOn.Value.Date.AddDays(daysToAdd - 1));
                }
                if (endDate == default(DateTime))
                    throw new ArgumentException("timePeriod", "Unable to determine endDate.");

                var days = Enum.GetValues(typeof(HedmDayOfWeek)).Cast<HedmDayOfWeek?>().ToList();

                meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);
            }
            else if (repeatRuleType == Dtos.FrequencyType2.Weekly)
            {
                var repeatRuleWeekly = (RepeatRuleWeekly)repeatRule;
                if (repeatRuleWeekly == null)
                    throw new ArgumentException("timePeriod", "Repeat rule weekly cant not be extracted.");

                if (repeatRuleWeekly.DayOfWeek == null)
                    throw new ArgumentNullException("repeatRuleWeekly.DayOfWeek", "The RepeatRuleWeekly DayOfWeek must be provided on the recurrence pattern.");
                if ((endDate == default(DateTime)) && (repeatRuleWeekly.Ends == null))
                    throw new ArgumentException("timePeriod", "Must provide either an endOn date or a repeat rule.");
                if ((repeatRuleWeekly.Ends != null) && (!repeatRuleWeekly.Ends.Date.HasValue) && (!repeatRuleWeekly.Ends.Repetitions.HasValue))
                    throw new ArgumentException("timePeriod", "Repeat rule must contain an end date, or a repetition interval.");

                var interval = (repeatRuleWeekly.Interval == 0) ? 1 : (int)repeatRuleWeekly.Interval;

                var days = repeatRuleWeekly.DayOfWeek;

                if ((endDate == default(DateTime)) && (repeatRuleWeekly.Ends != null) && (repeatRuleWeekly.Ends.Date.HasValue))
                    endDate = Convert.ToDateTime(repeatRuleWeekly.Ends.Date);
                if ((endDate == default(DateTime)) && (repeatRuleWeekly.Ends != null) &&
                    (repeatRuleWeekly.Ends.Repetitions.HasValue) && (repeatRuleWeekly.Ends.Repetitions != 0))
                {
                    int daysToAdd = interval * 7 * Convert.ToInt16(repeatRuleWeekly.Ends.Repetitions);
                    endDate = Convert.ToDateTime(timePeriod.StartOn.Value.Date.AddDays(daysToAdd - 1));
                }
                if (endDate == default(DateTime))
                    throw new ArgumentException("timePeriod", "Unable to determine endDate.");

                meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);
            }
            else if (repeatRuleType == Dtos.FrequencyType2.Monthly)
            {
                var meetingDays = new List<DateTime>();
                var repeatRuleMonthly = (RepeatRuleMonthly)repeatRule;
                if (repeatRuleMonthly == null)
                    throw new ArgumentNullException("repeatRuleMonthly", "RepeatRuleMonthly must be provided on the recurrence pattern.");
                if (repeatRuleMonthly.RepeatBy == null)
                    throw new ArgumentNullException("repeatRuleMonthly.RepeatBy", "The RepeatRuleMonthly RepeatBy rule must be provided on the recurrence pattern.");
                if ((endDate == default(DateTime)) && (repeatRuleMonthly.Ends != null) &&
                   (!repeatRuleMonthly.Ends.Date.HasValue) && (!repeatRuleMonthly.Ends.Repetitions.HasValue))
                    throw new ArgumentException("timePeriod", "Must provide either an endOn date , a repeat rule end date, or a repetition interval.");

                var interval = (repeatRuleMonthly.Interval == 0) ? 1 : (int)repeatRuleMonthly.Interval;
                var startDate = timePeriod.StartOn.Value.Date;
                if ((endDate == default(DateTime)) && (repeatRuleMonthly.Ends != null) && (repeatRuleMonthly.Ends.Date.HasValue))
                    endDate = Convert.ToDateTime(repeatRuleMonthly.Ends.Date);

                if (repeatRuleMonthly.RepeatBy != null && repeatRuleMonthly.RepeatBy.DayOfMonth != null && repeatRuleMonthly.RepeatBy.DayOfMonth != 0)
                {
                    var dayOfMonth = Convert.ToInt32(repeatRuleMonthly.RepeatBy.DayOfMonth);
                    if (dayOfMonth > 31)
                        throw new ArgumentException("If providing a dayOfMonth to the RepeatRuleMonthly, then the RepeatBy rule must be a valid number.", "dayOfMonth");
                   
                    var newStartDate = new DateTime(startDate.Year, startDate.Month, dayOfMonth);
                    //  if the startOn date is after the first calculated date, then add a month
                    if (DateTime.Compare(startDate, newStartDate) > 0)
                    {
                        newStartDate = newStartDate.AddMonths(1);
                    }
                    var range = 1;
                    if (repeatRuleMonthly.Ends != null && repeatRuleMonthly.Ends.Repetitions.HasValue)
                        range = Convert.ToInt32(repeatRuleMonthly.Ends.Repetitions) * interval;
                    else if (endDate != default(DateTime))
                    {
                        range = Math.Abs(((newStartDate.Year - endDate.Year) * 12) + newStartDate.Month - endDate.Month) + 1;
                    }
                    int i = 0;
                    var current = newStartDate;
                    while (i < range)
                    {
                        meetingDays.Add(new DateTime(current.Year, current.Month, dayOfMonth));
                        current = current.AddMonths(interval);
                        i += interval;
                    }

                }
                else if (repeatRuleMonthly.RepeatBy != null && repeatRuleMonthly.RepeatBy.DayOfWeek != null)
                {
                    var occurence = repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence;
                    var dayOfWeek = (DayOfWeek)repeatRuleMonthly.RepeatBy.DayOfWeek.Day;
                   
                    var monthlyRange = 1;
                    if (repeatRuleMonthly.Ends != null && repeatRuleMonthly.Ends.Repetitions.HasValue)
                        monthlyRange = Convert.ToInt32(repeatRuleMonthly.Ends.Repetitions) * interval;
                    else if (endDate != default(DateTime))
                    {
                        monthlyRange = Math.Abs(((startDate.Year - endDate.Year) * 12) + startDate.Month - endDate.Month) + 1;
                    }
                    int i = 0;
                    var current = startDate;
                    while (i < monthlyRange)
                    {
                        meetingDays.Add(GetDateByOrdinalDay(current.Year, current.Month, dayOfWeek, occurence));
                        current = current.AddMonths(interval);
                        i += interval;
                    }
                    meetingDates = meetingDays.Distinct().OrderBy(d => d).ToList();
                }
                else
                {
                    throw new ArgumentNullException("repeatRuleMonthly.RepeatBy", "The RepeatRuleMonthly RepeatBy rule must contain either a DayOfMonth or DayOfWeek.");
                }
                meetingDates = meetingDays.Distinct().OrderBy(d => d).ToList();
            }
            else if (repeatRuleType == Dtos.FrequencyType2.Yearly)
            {
                var repeatRuleYearly = (RepeatRuleYearly)repeatRule;
                if (repeatRuleYearly == null)
                    throw new ArgumentNullException("repeatRuleYearly", "RepeatRuleYearly must be provided on the recurrence pattern.");
                if ((endDate == default(DateTime)) && (repeatRuleYearly.Ends == null))
                    throw new ArgumentException("timePeriod", "Must provide either an endOn date or a repeat rule.");
                if ((repeatRuleYearly.Ends != null) && (!repeatRuleYearly.Ends.Date.HasValue) && (!repeatRuleYearly.Ends.Repetitions.HasValue))
                    throw new ArgumentException("timePeriod", "Repeat rule must contain an end date, or a repetition interval.");

                var interval = (repeatRuleYearly.Interval == 0) ? 1 : (int)repeatRuleYearly.Interval;

                if ((endDate == default(DateTime)) && (repeatRuleYearly.Ends != null) && (repeatRuleYearly.Ends.Date.HasValue))
                    endDate = Convert.ToDateTime(repeatRuleYearly.Ends.Date);
                if ((endDate == default(DateTime)) && (repeatRuleYearly.Ends != null)
                    && (repeatRuleYearly.Ends.Repetitions.HasValue) && (repeatRuleYearly.Ends.Repetitions != 0))
                {
                    int yearsToAdd = interval * Convert.ToInt16(repeatRuleYearly.Ends.Repetitions);
                    endDate = Convert.ToDateTime(timePeriod.StartOn.Value.Date.AddYears(yearsToAdd - 1));
                }
                if (endDate == default(DateTime))
                    throw new ArgumentException("timePeriod", "Unable to determine endDate.");

                var days = new List<HedmDayOfWeek?>() { (HedmDayOfWeek)timePeriod.StartOn.Value.DayOfWeek };
                meetingDates = BuildDateList(timePeriod.StartOn.Value.Date, endDate, frequency, interval, days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);
            }

            return meetingDates.Select(x => x.Date).Distinct();
        }
        /// <summary>
        /// Build a list of dates for a given date range and recurrence patterns
        /// </summary>
        /// <param name="startDate">Earliest Date that can be included in the list</param>
        /// <param name="endDate">Latest Date that can be included in the list</param>
        /// <param name="frequency">Frequency of recurrence</param>
        /// <param name="interval">Interval at which recurrence occurs</param>
        /// <param name="recurrenceDays">Days of week on which recurrence pattern occurs</param>
        /// <param name="specialDays">Special days to be removed from the final list of dates</param>        
        /// <param name="backDatingLimit">Number of days in past that back-dating is permitted</param>
        /// <returns>List of Dates</returns>
        public static IEnumerable<DateTime> BuildDateList(DateTime startDate, DateTime endDate, Ellucian.Colleague.Domain.Base.Entities.FrequencyType frequency, int interval, IEnumerable<HedmDayOfWeek?> recurrenceDays, IEnumerable<DateTime> specialDays, int backDatingLimit)
        {
            if (recurrenceDays == null || recurrenceDays.Count() == 0)
            {
                throw new ArgumentNullException("recurrenceDays", "At least one day of the week must be specified on the recurrence pattern.");
            }
            if (interval <= 0)
            {
                throw new ArgumentOutOfRangeException("interval", "Recurrence interval must be greater than zero.");
            }
            if (backDatingLimit < 0)
            {
                throw new ArgumentOutOfRangeException("backDatingLimit", "Back dating limit cannot be less than zero.");
            }
            if (specialDays == null)
            {
                specialDays = new List<DateTime>();
            }

            var dates = new List<DateTime>();
            foreach (var day in recurrenceDays)
            {
                // Get the "true" start date using the supplied start date and the back-dating limit
                var trueStartDate = new DateTime(Math.Max(startDate.Ticks, DateTime.Today.AddDays(-backDatingLimit).Ticks));
                if (trueStartDate.DayOfWeek != (DayOfWeek)day)
                {
                    int diff = (int)day - (int)trueStartDate.DayOfWeek;
                    if (diff < 0)
                    {
                        diff += 7;
                    }
                    trueStartDate = trueStartDate.AddDays(diff);
                }


                var date = trueStartDate;
                while (date <= endDate)
                {
                    if (!specialDays.Contains(date)
                        && !dates.Contains(date)
                        && (frequency != Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly || (frequency == Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly && (HedmDayOfWeek)date.DayOfWeek == day)))
                    {
                        if (((frequency == Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Daily || frequency == Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly) && (HedmDayOfWeek)date.DayOfWeek == day)
                            || (frequency == Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly || frequency == Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly))
                        {
                            dates.Add(date);
                        }
                    }
                    switch (frequency)
                    {
                        case Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly:
                            date = date.AddDays(7 * interval);
                            break;
                        case Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly:
                            date = date.AddMonths(1);
                            break;
                        case Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly:
                            date = date.AddYears(1); 
                            break;
                        default:
                            date = date.AddDays(1); 
                            break;
                    }
                }
            }
            if (frequency == Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly)
                return dates.Distinct().OrderBy(d => d).ToList();
            else
                return dates.Distinct().OrderBy(d => d).Where((elem, idx) => idx % interval == 0).ToList();

        }

        /// <summary>
        /// Gets the date of the specified instance of the specified day of week within 
        /// the specified month for the specified year
        /// </summary>
        /// <param name="dt">The parent object of this method</param>
        /// <param name="year">The year to calculate for</param>
        /// <param name="month">The month to calculate for</param>
        /// <param name="day">The day of week to find</param>
        /// <param name="ordinal">The instance of the day of week to find</param>
        /// <returns>The resulting DateTime</returns>
        public static DateTime GetDateByOrdinalDay(int year, int month, DayOfWeek dayOfWeek, int ordinal)
        {
            // normalize some values to make sure we're within acceptable ranges
            month = Math.Min(Math.Max(month, 1), 12);
            // retrun value - default to first day of month
            var workingDate = new DateTime(year, month, 1);
            //determine the last day of the month
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            //set variable for last day of month
            var maxDays = lastDay.Day;
            var gap = 0;

            if (ordinal < 0)
            {
                // start out on the last date of the specified month and of the current year
                workingDate = new DateTime(year, month, maxDays);

                // if the the day of week of the last day is NOT the specified day of week
                if (workingDate.DayOfWeek != dayOfWeek)
                {
                    // determine the number of days between the first of the month and 
                    // the first instance of the specified day of week
                    gap = (int)dayOfWeek - (int)workingDate.DayOfWeek;
                    gap = (gap < 0) ? Math.Abs(gap) : 7 - gap;

                    // and set the date to the first instance of the specified day of week
                    workingDate = workingDate.AddDays(-gap);
                }

                // if we want something later than the first instance
                if (Math.Abs(ordinal) > 1)
                {
                    // determine how many days we're going to subtract from the working date to 
                    // satisfy the specified ordinal
                    var daysToSubtract = 7 * (Math.Abs(ordinal) - 1);

                    // now adjust back, just in  case the specified ordinal - this loop 
                    // should only iterate once or twice
                    while (daysToSubtract + gap > maxDays - 1)
                    {
                        daysToSubtract -= 7;
                    }

                    // finally we adjust the date by the number of days to add
                    workingDate = workingDate.AddDays(-daysToSubtract);
                }
            }
            else
            {
                // normalize some values to make sure we're within acceptable ranges
                ordinal = Math.Min(Math.Max(ordinal, 1), 5);

                // if the the day of week of the first day is NOT the specified day of week
                if (workingDate.DayOfWeek != dayOfWeek)
                {
                    // determine the number of days between the first of the month and 
                    // the first instance of the specified day of week
                    gap = (int)workingDate.DayOfWeek - (int)dayOfWeek;
                    gap = (gap < 0) ? Math.Abs(gap) : 7 - gap;

                    // and set the date to the first instance of the specified day of week
                    workingDate = workingDate.AddDays(gap);
                }

                // if we want something later than the first instance
                if (ordinal > 1)
                {
                    // determine how many days we're going to add to the working date to 
                    // satisfy the specified ordinal
                    var daysToAdd = 7 * (ordinal - 1);

                    // now adjust back, just in  case the specified ordinal - this loop 
                    // should only iterate once or twice
                    while (daysToAdd + gap > maxDays - 1)
                    {
                        daysToAdd -= 7;
                    }

                    // finally we adjust the date by the number of days to add
                    workingDate = workingDate.AddDays(daysToAdd);
                }
            }
            // and return the date to the calling method
            return workingDate;
        }
    }
}
