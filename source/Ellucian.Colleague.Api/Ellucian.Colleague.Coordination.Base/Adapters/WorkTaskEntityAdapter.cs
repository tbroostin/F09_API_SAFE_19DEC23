// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Class for mapping a WorkTask entity to a WorkTask Dto
    /// </summary>
    public class WorkTaskEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>
    {
        public WorkTaskEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a WorkTask entity to a WorkTask Dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Dtos.Base.WorkTask MapToType(Domain.Base.Entities.WorkTask source)
        {
            var workTaskDto = new Dtos.Base.WorkTask()
            {
                Id = source.Id,
                Category = source.Category,
                Description = source.Description,
                TaskProcess = MapProcessCodeToTaskProcess(source.ProcessCode),
                StartDate = source.StartDate,
                ExecState = MapExecutionStateToExecutionState(source.ExecState)
            };
            return workTaskDto;
        }

        /// <summary>
        /// Maps the process code to the correct WorkTaskProcess, if possible
        /// </summary>
        /// <param name="processCode"></param>
        /// <returns></returns>
        private Dtos.Base.WorkTaskProcess MapProcessCodeToTaskProcess(string processCode)
        {
            switch (processCode)
            {
                case "SSHRTA":
                    return Dtos.Base.WorkTaskProcess.TimeApproval;
                case "SSHRLVA":
                    return Dtos.Base.WorkTaskProcess.LeaveRequestApproval;
                default:
                    return Dtos.Base.WorkTaskProcess.None;
            }

        }

        /// <summary>
        /// Maps the ExecutionState to the correct ExecutionState, if possible
        /// </summary>
        /// <param name="executionState"></param>
        /// <returns></returns>
        private Dtos.Base.ExecutionState? MapExecutionStateToExecutionState(Domain.Base.Entities.ExecutionState? executionState)
        {
            switch (executionState)
            {
                case null:
                    return null;
                case Domain.Base.Entities.ExecutionState.NS:
                    return Dtos.Base.ExecutionState.OpenNotStarted;
                case Domain.Base.Entities.ExecutionState.ON:
                    return Dtos.Base.ExecutionState.OpenNotActive;
                case Domain.Base.Entities.ExecutionState.C:
                    return Dtos.Base.ExecutionState.ClosedCompleted;
                default:
                    return null;
            }

        }

    }
}
