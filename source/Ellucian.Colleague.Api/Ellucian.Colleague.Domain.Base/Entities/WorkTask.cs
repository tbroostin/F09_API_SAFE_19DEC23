﻿// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class WorkTask
    {
        /// <summary>
        /// Unique ID of this task
        /// </summary>
        public string Id { get { return _id; } }
        private readonly string _id;
        /// <summary>
        /// Category for this task, user-defined, used for grouping like tasks
        /// </summary>
        public string Category { get { return _category; } }
        private readonly string _category;
        /// <summary>
        /// Detailed task descriptions
        /// </summary>
        public string Description { get { return _description; } }
        private readonly string _description;
        /// <summary>
        /// Process code to aid in routing task link
        /// </summary>
        public string ProcessCode { get { return _processCode; } }
        private readonly string _processCode;

        /// <summary>
        /// Exec state
        /// </summary>
        public ExecutionState? ExecState { get { return _execState; } }
        private readonly ExecutionState? _execState;

        /// <summary>
        /// Start date and time
        /// </summary>
        public DateTimeOffset? StartDate { get { return _startDate; } }
        private readonly DateTimeOffset? _startDate;

        /// <summary>
        /// Create a work task for a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        /// <param name="processCode"></param>
        /// <param name="startDate"></param>
        /// <param name="execState"></param>
        public WorkTask(string id, string category, string description, string processCode, DateTimeOffset? startDate=null, ExecutionState? execState = null)
        {
            _id = id;
            _category = category;
            _description = description;
            _processCode = processCode;
            _execState = execState;
            _startDate = startDate;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var workTask = obj as WorkTask;

            return workTask.Id.Equals(this.Id);
        }

        /// <summary>
        /// HashCode is based on the Id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
