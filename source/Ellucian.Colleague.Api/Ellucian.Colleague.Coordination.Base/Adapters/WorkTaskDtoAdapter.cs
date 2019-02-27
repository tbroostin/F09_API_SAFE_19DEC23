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
    /// Class for mapping a WorkTask Dto to a WorkTask Entity
    /// </summary>
    public class WorkTaskDtoAdapter : AutoMapperAdapter<Dtos.Base.WorkTask, Domain.Base.Entities.WorkTask>
    {
        public WorkTaskDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a WorkTask Dto to a WorkTask Entity
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Domain.Base.Entities.WorkTask MapToType(Dtos.Base.WorkTask source)
        {
            var workTaskEntity = new Domain.Base.Entities.WorkTask(source.Id, source.Category, source.Description, MapWorkTaskProcessToProcessCode(source.TaskProcess), source.StartDate, MapExecutionStateToExecutionState(source.ExecState));
            
            return workTaskEntity;
        }

        /// <summary>
        /// Maps the WorkTaskProcess to the correct WorkTaskProcess, if possible
        /// </summary>
        /// <param name="workTaskProcess"></param>
        /// <returns></returns>
        private string MapWorkTaskProcessToProcessCode(Dtos.Base.WorkTaskProcess workTaskProcess)
        {
            switch (workTaskProcess)
            {
                case Dtos.Base.WorkTaskProcess.None:
                    return string.Empty;
                case Dtos.Base.WorkTaskProcess.LeaveRequestApproval:
                    return "SSHRLVA";
                case Dtos.Base.WorkTaskProcess.TimeApproval:
                    return "SSHRTA";
                default:
                    return string.Empty;
            }

        }

        /// <summary>
        /// Maps the ExecutionState to the correct ExecutionState, if possible
        /// </summary>
        /// <param name="executionState"></param>
        /// <returns></returns>
        private Domain.Base.Entities.ExecutionState? MapExecutionStateToExecutionState(Dtos.Base.ExecutionState? executionState)
        {
            switch (executionState)
            {
                case null:
                    return null;
                case Dtos.Base.ExecutionState.OpenNotStarted:
                    return Domain.Base.Entities.ExecutionState.NS;
                case Dtos.Base.ExecutionState.OpenNotActive:
                    return Domain.Base.Entities.ExecutionState.ON;
                case Dtos.Base.ExecutionState.ClosedCompleted:
                    return Domain.Base.Entities.ExecutionState.C;
                default:
                    return null;
            }

        }
    }

}
