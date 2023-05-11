/* Copyright 2019-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain entity for employee leave request
    /// </summary>
    [Serializable]
    public class LeaveRequest : IComparable<LeaveRequest>
    {
        #region Properties 
        /// <summary>
        /// The DB Id of the leave request
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The PerleaveId of the leave request
        /// </summary>
        public string PerLeaveId { get { return perLeaveId; } }
        private readonly string perLeaveId;

        /// <summary>
        /// Identifier of the employee who raised this leave request
        /// </summary>
        public string EmployeeId { get { return employeeId; } }
        private readonly string employeeId;

        /// <summary>
        /// Name of the employee who raised this leave request
        /// </summary>
        public string EmployeeName { get { return employeeName; } }
        private readonly string employeeName;
        /// <summary>
        /// Start date for this leave request
        /// </summary>
        public DateTime? StartDate
        {
            get { return startDate; }
            set
            {
                if (value.HasValue && EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("Start Date cannot be after the EndDate");
                }
                startDate = value;
            }
        }
        private DateTime? startDate;

        /// <summary>
        /// End date for this leave request
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate.Value)
                {
                    throw new ArgumentOutOfRangeException("End Date cannot be before Start Date");
                }
                endDate = value;
            }
        }
        private DateTime? endDate;

        /// <summary>
        /// Identifier of the actioner for this leave request
        /// </summary>
        public string ApproverId { get { return approverId; } }
        private readonly string approverId;

        /// <summary>
        /// Name of the actioner for this leave request
        /// </summary>
        public string ApproverName { get { return approverName; } }
        private readonly string approverName;

        /// <summary>
        /// Flag which indicates whether this LR is Withdrawn or not.
        /// </summary>
        public bool IsWithdrawn { get { return isWithdrawn; } }
        private readonly bool isWithdrawn;

        /// <summary>
        /// The value of HRSS.LR.UNSUBMIT.WDRW option in LVSS form when the Leave Request is Withdrawn.
        /// </summary>
        public string WithdrawOption { get { return withdrawOption; } }
        private readonly string withdrawOption;

        /// <summary>
        /// Current status of this leave request        
        /// </summary>
        public LeaveStatusAction Status { get { return status; } }
        private readonly LeaveStatusAction status;

        /// <summary>
        /// List of all leave request detail object associated wih this leave request
        /// </summary>
        public List<LeaveRequestDetail> LeaveRequestDetails { get; set; }


        /// <summary>
        /// List of all comments object associated wih this leave request
        /// </summary>
        public List<LeaveRequestComment> LeaveRequestComments { get; set; }

        /// <summary>
        /// True when leave request is created by same supervisor user
        /// </summary>
        public bool EnableDeleteForSupervisor { get { return enableDeleteForSupervisor; } }
        private readonly bool enableDeleteForSupervisor;

        /// <summary>
        /// True when a withdraw request has pending supervisor approval
        /// </summary>
        public bool IsWithdrawPendingApproval { get { return isWithdrawPendingApproval; } }
        private readonly bool isWithdrawPendingApproval;
        #endregion

        /// <summary>
        /// Parameterized contructor to instantiate a LeaveRequest object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="perLeaveId"></param>
        /// <param name="employeeId"></param>    
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approverId"></param>
        /// <param name="approverName"></param>
        /// <param name="employeeName"></param>
        /// <param name="status"></param>
        /// <param name="leaveRequestDetails"></param>
        /// <param name="leaveRequestComments"></param>
        /// <param name="isWithdrawPendingApproval"></param>
        /// <param name="isWithdrawn"></param>
        /// <param name="withdrawOption"></param
        /// <param name="enableDeleteForSupervisor">optional</param>
        public LeaveRequest(string id,
            string perLeaveId,
            string employeeId,
            DateTime? startDate,
            DateTime? endDate,
            string approverId,
            string approverName,
            string employeeName,
            LeaveStatusAction status,
            List<LeaveRequestDetail> leaveRequestDetails,
            List<LeaveRequestComment> leaveRequestComments,
            bool isWithdrawPendingApproval,
            bool isWithdrawn = false,
            string withdrawOption = null,
            bool enableDeleteForSupervisor = false)
        {
            if (string.IsNullOrEmpty(perLeaveId))
            {
                throw new ArgumentNullException("perLeaveId");
            }
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            if (startDate == null)
            {
                throw new ArgumentNullException("startDate");
            }
            if (endDate == null)
            {
                throw new ArgumentNullException("endDate");
            }
            if (string.IsNullOrEmpty(approverId))
            {
                throw new ArgumentNullException("approverId");
            }
            if (leaveRequestDetails == null)
            {
                throw new ArgumentNullException("leaveRequestDetails");
            }

            if (!leaveRequestDetails.Any())
            {
                throw new ArgumentException("At least one leaveRequestDetails record is required for a LeaveRequest object.");
            }

            this.id = id;
            this.perLeaveId = perLeaveId;
            this.employeeId = employeeId;
            StartDate = startDate;
            EndDate = endDate;
            this.approverId = approverId;
            this.approverName = approverName;
            this.employeeName = employeeName;
            this.status = status;
            this.isWithdrawn = isWithdrawn;
            this.withdrawOption = withdrawOption;
            LeaveRequestDetails = leaveRequestDetails;
            LeaveRequestComments = leaveRequestComments;
            this.enableDeleteForSupervisor = enableDeleteForSupervisor;
            this.isWithdrawPendingApproval = isWithdrawPendingApproval;
        }

        /// <summary>
        /// Compares this object with another LeaveRequest object.
        /// </summary>
        /// <param name="other">A LeaveRequest object to compare with this object.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings: 
        /// Less than zero
        ///      This object is less than the other parameter.
        /// Zero 
        ///      This object is equal to other in relative terms, but the two objects may not be Equal.
        /// Greater than zero 
        ///      This object is greater than other.</returns>
        public int CompareTo(LeaveRequest other)
        {
            if (EmployeeId != other.EmployeeId)
            {
                return EmployeeId.CompareTo(other.EmployeeId);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// The string representation of this LeaveRequest object using Id, PerLeaveId, EmployeeId, StartDate, EndDate, ApproverId, ApproverName, EmployeeName and Status
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}", Id, PerLeaveId, EmployeeId, StartDate, EndDate, ApproverId, ApproverName, EmployeeName, Status, WithdrawOption);
        }
    }
}
