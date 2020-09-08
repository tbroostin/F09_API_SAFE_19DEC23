/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Entity for Leave Request Comment 
    /// </summary>
    [Serializable]
     public class LeaveRequestComment
    {
        /// <summary>
        /// DB id of this leave request comment object
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Identifier of the leave request object
        /// </summary>
        public string LeaveRequestId { get { return leaveRequestId; } }
        private readonly string leaveRequestId;


        /// <summary>
        /// The actual Comment 
        /// </summary>
        public string Comments { get { return comments; } }
        private readonly string comments;


        /// <summary>
        /// The ID of the employee this Comment is associated to
        /// </summary>
        public string EmployeeId { get { return employeeId; } }
        private readonly string employeeId;

        /// <summary>
        /// The name of the person who created the comment
        /// </summary>
        public string CommentAuthorName { get { return commentAuthorName; } }
        private readonly string commentAuthorName;

        /// <summary>
        /// Timestamp metadata
        /// </summary>
         public Timestamp Timestamp { get; set; }

      
        //public LeaveRequestComment(string id, string leaveRequestId, string employeeId, string comments,string commentAuthorName,Timestamp timestamp)
        //{
        //    if (leaveRequestId == null)
        //    {
        //        throw new ArgumentNullException("leaveRequestId");
        //    }
        //    this.id = id;
        //    this.leaveRequestId = leaveRequestId;
        //    this.employeeId = employeeId;
        //    this.comments = comments;
        //    this.commentAuthorName = commentAuthorName;
        //    this.Timestamp = timestamp;
        //}

        public LeaveRequestComment(string id, string leaveRequestId, string employeeId, string comments, string commentAuthorName)
        {
            if (leaveRequestId == null)
            {
                throw new ArgumentNullException("leaveRequestId");
            }
            this.id = id;
            this.leaveRequestId = leaveRequestId;
            this.employeeId = employeeId;
            this.comments = comments;
            this.commentAuthorName = commentAuthorName;
           
        }

    }
}
