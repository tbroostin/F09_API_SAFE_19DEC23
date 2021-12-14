// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Course section meeting information
    /// </summary>
    [Serializable]
    public class PortalSectionMeeting
    {
        /// <summary>
        /// Building where this meeting time occurs.
        /// </summary>
        public string Building { get; private set; }

        /// <summary>
        /// Room number where this meeting time occurs.
        /// </summary>
        public string Room { get; private set; }

        /// <summary>
        /// Meeting time instructional method (such as lecture, lab).
        /// </summary>
        public string InstructionalMethod { get; private set; }

        /// <summary>
        /// List of Days of the week (enumerable) when this meeting time occurs.
        /// </summary>
        public List<DayOfWeek> DaysOfWeek { get; private set; }

        /// <summary>
        /// Meeting time start time
        /// </summary>
        public DateTimeOffset? StartTime { get; private set; }

        /// <summary>
        /// Meeting time end time 
        /// </summary>
        public DateTimeOffset? EndTime { get; private set; }

        /// <summary>
        /// Portal Meeting constructor. 
        /// </summary>
        /// <param name="building">section meeting building</param>
        /// <param name="room">section meeting room</param>
        /// <param name="instructionalMethod">Type of instruction</param>
        /// <param name="daysOfWeek">meeting days of the week</param>
        /// <param name="startTime">section meeting start time</param>
        /// <param name="endTime">section meeeting end time</param>
        public PortalSectionMeeting(string building, string room, string instructionalMethod, List<DayOfWeek> daysOfWeek, DateTimeOffset? startTime, DateTimeOffset? endTime)
        {
            Building = building;
            Room = room;
            InstructionalMethod = instructionalMethod;
            DaysOfWeek = daysOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }

    }
}
