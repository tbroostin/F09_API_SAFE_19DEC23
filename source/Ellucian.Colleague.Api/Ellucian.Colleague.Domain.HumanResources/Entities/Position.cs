/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class Position
    {
        /// <summary>
        /// The database Guid
        /// </summary>
        public string Guid { get; set; }
        private string guid;

        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// A long form title. Required
        /// </summary>
        public string Title { 
            get { return title; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                title = value;
            }
        }
        private string title;

        /// <summary>
        /// A short form title
        /// </summary>
        public string ShortTitle {
            get { return shortTitle; }
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                shortTitle = value;
            }
        }
        private string shortTitle;

        /// <summary>
        /// Whether this is an Exempt position or non-exempt position, meaning the position
        /// is exempt or not exempt from the Fair Labor Standards Act overtime rules. Most
        /// salaried positions are considered exempt. If true, the position is exempt.
        /// </summary>
        public bool IsExempt { get; set; }

        /// <summary>
        /// Whether this is a salaried or hourly position. If true, the position is salaried
        /// </summary>
        public bool IsSalary { get; set; }

        /// <summary>
        /// The Id of the Position considered the supervising position of this position
        /// </summary>
        public string SupervisorPositionId { get; set; }

        /// <summary>
        /// The Id of the Position considered the alternate supervising position of this position
        /// </summary>
        public string AlternateSupervisorPositionId { get; set; }

        /// <summary>
        /// The type of timecard to be applied to this position
        /// </summary>
        public TimecardType TimecardType { get; set; }

        /// <summary>
        /// The Date this position becomes active
        /// </summary>
        public DateTime StartDate { 
            get { return startDate; }
            set 
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("StartDate cannot be after EndDate");
                }
                startDate = value;
            }
        }
        private DateTime startDate;

        /// <summary>
        /// The Date this position becomes inactive. Can be null
        /// </summary>
        public DateTime? EndDate { 
            get { return endDate; }
            set 
            { 
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("EndDate cannot be before StartDate");
                }
                endDate = value;
            }
        }
        private DateTime? endDate;

        /// <summary>
        /// The Id of the Workweek Schedule.
        /// </summary>
        //public string WorkWeekScheduleId { get; set; }

        /// <summary>
        /// A list of Ids of the Pay Schedules associated to this position
        /// </summary>
        public List<string> PositionPayScheduleIds { get; set; }

        /// <summary>
        /// Position Authorized Date
        /// </summary>
        public DateTime? PositionAuthorizedDate { get; set; }

        /// <summary>
        /// Position Class
        /// </summary>
        public string PositionClass { get; set; }

        /// <summary>
        /// Position Dept
        /// </summary>
        public string PositionDept
        {
            get { return positionDept; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                positionDept = value;
            }
        }
        private string positionDept;

        /// <summary>
        /// Position Location
        /// </summary>
        public string PositionLocation { get; set; }

        /// <summary>
        /// Position Job Description
        /// </summary>
        public string PositionJobDesc { get; set; }

        /// <summary>
        /// Instantiate a Position object
        /// </summary>
        /// <param name="id">The Id of the position. Can be null or empty if creating the object to add to the database</param>
        /// <param name="title">The long form title of the position. Required.</param>
        /// <param name="shortTitle">The short form title of the position. Required</param>
        /// <param name="startDate">The date the position becomes active</param>
        /// <param name="isSalary">True: position is salaried. False: position is hourly</param>
        public Position(string id, string title, string shortTitle, string positionDept, DateTime startDate, bool isSalary)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException("title");
            }
            if (string.IsNullOrWhiteSpace(shortTitle))
            {
                throw new ArgumentNullException("shortTitle");
            }
            if (string.IsNullOrWhiteSpace(positionDept))
            {
                throw new ArgumentNullException("positionDept");
            }

            this.id = id;
            this.title = title;
            this.shortTitle = shortTitle;
            this.positionDept = positionDept;
            this.startDate = startDate;
            this.IsSalary = isSalary;

            PositionPayScheduleIds = new List<string>();
        }

        /// <summary>
        /// Two positions are equal when their ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var position = obj as Position;

            return position.Id == this.Id;
        }

        /// <summary>
        /// Hashcode representation of Position (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// String representation of Position - ShortTitle and Id
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ShortTitle + "-" + Id;
        }
    }
}
