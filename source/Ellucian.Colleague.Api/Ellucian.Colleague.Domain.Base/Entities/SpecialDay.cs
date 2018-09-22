/* Copyright 2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// SpecialDay is used to identify dates and date ranges that have meaning campus wide, such as holidays,
    /// snow days, etc.
    /// </summary>
    [Serializable]
    public class SpecialDay : IComparable, IComparer<SpecialDay>
    {
        /// <summary>
        /// The Id of the Special Day
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// A Description of the Special Day
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The Id of the CampusCalendar to which this special day belongs
        /// </summary>
        public string CampusCalendarId { get; private set; }

        /// <summary>
        /// The code indicating the type of special day
        /// </summary>
        public string SpecialDayTypeCode { get; private set; }

        /// <summary>
        /// Whether this special day is a Holiday
        /// </summary>
        public bool IsHoliday { get; private set; }

        /// <summary>
        /// Whether this special day is a full day or not.
        /// </summary>
        public bool IsFullDay { get; private set; }

        /// <summary>
        /// The Date and Time this special day starts. This will only contain the Date portion if this is a full day.
        /// </summary>
        public DateTimeOffset StartDateTime { get; private set; }

        /// <summary>
        /// The Date and Time this special day ends. This will only contain the Date portion if this is a full day.
        /// </summary>
        public DateTimeOffset EndDateTime { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="campusCalendarId"></param>
        /// <param name="specialDayTypeCode"></param>
        /// <param name="isHoliday"></param>
        /// <param name="isFullDay"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        public SpecialDay(string id, 
            string description, 
            string campusCalendarId, 
            string specialDayTypeCode, 
            bool isHoliday, 
            bool isFullDay,
            DateTimeOffset startDateTime,
            DateTimeOffset endDateTime)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("description");
            }
            if (string.IsNullOrWhiteSpace(campusCalendarId))
            {
                throw new ArgumentNullException("campusCalendarId");
            }
            if (string.IsNullOrWhiteSpace(specialDayTypeCode))
            {
                throw new ArgumentNullException("specialDayTypeCode");
            }
            if (startDateTime == default(DateTimeOffset))
            {
                throw new ArgumentNullException("startDateTime", "Start time may not be the default date/time");
            }
            if (endDateTime == default(DateTimeOffset))
            {
                throw new ArgumentNullException("endDateTime", "End time may not be the default date/time");
            }    
            if (startDateTime > endDateTime)
            {
                throw new ArgumentException("Start time cannot be later than end time");
            }

            Id = id;
            Description = description;
            CampusCalendarId = campusCalendarId;
            SpecialDayTypeCode = specialDayTypeCode;
            IsHoliday = isHoliday;
            IsFullDay = isFullDay;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        /// <summary>
        /// SpecialDays are equal when their Ids are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var specialDay = obj as SpecialDay;

            return Id == specialDay.Id;
        }

        /// <summary>
        /// Hashcode computed from the Id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// String representation of the Special day
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", Id, Description);
        }

        /// <summary>
        /// Compare this Special Day with another.
        /// This method First compares the two StartDateTime,
        /// Then compares the Description,
        /// Then compares the Ids.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>a Value less than Zero if this special day is less than obj. 
        /// A value equal to Zero if this special day is the same as obj.
        /// A value greater than Zero if this special day is greater than obj</returns>
        public int CompareTo(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return -1;
            }

            return Compare(this, (SpecialDay)obj);
        }

        /// <summary>
        /// Compare two Special Days with one another.
        /// This method First compares the two StartDateTime,
        /// Then compares the Description,
        /// Then compares the Ids.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>a Value less than Zero if x is less than y. 
        /// A value equal to Zero if x is the same as y.
        /// A value greater than Zero if x is greater than y</returns>
        public int Compare(SpecialDay x, SpecialDay y)
        {
            if (x != null && y == null)
            {
                return -1;
            }
            else if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return 1;
            }
            else
            {
                if (x.StartDateTime != y.StartDateTime)
                {
                    return x.StartDateTime.CompareTo(y.StartDateTime);
                }
                else if (x.Description != y.Description)
                {
                    return x.Description.CompareTo(y.Description);
                }
                else
                {
                    return x.Id.CompareTo(y.Id);
                }
            }
        }
    }
}
