/* Copyright 2021-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for leave request comment
    /// </summary>
    [DataContract]
    public class LeaveRequestComment
    {
        /// <summary>
        /// The database ID of the Comments object
        /// </summary>
        [JsonProperty("leaveRequestCommentId")]
        [Metadata("LEAVE.REQ.COMMENTS.ID", DataDescription = "Id of this leave request comments object.")]
        public string Id { get; set; }

        /// <summary>
        /// The ID of the leave request this Comment is associated to. 
        /// </summary>
        [JsonProperty("leaveRequestId")]
        [Metadata("LEAVE.REQ.COMMENTS.ID", DataDescription = "Id of this leave request comments object.")]
        public string LeaveRequestId { get; set; }

        /// <summary>
        /// The actual Comment contents
        /// </summary>
        [JsonProperty("comments")]
        [Metadata("LRC.COMMENTS", DataDescription = "The actual Comment contents.")]
        public string Comments { get; set; }

        /// <summary>
        /// the ID of the employee this Comment is associated to
        /// </summary>
        [JsonProperty("employeeId")]
        [Metadata("LRC.EMPLOYEE.ID", DataDescription = "The ID of the employee this Comment is associated to.")]
        public string EmployeeId { get; set; }

        /// <summary>
        /// The name of the person who created the comment
        /// </summary>
        [JsonProperty("commentAuthorName")]
        [Metadata("LRC.ADD.OPERNAME", DataDescription = "The name of the person who created the comment.")]
        public string CommentAuthorName { get; set; }

        /// <summary>
        /// A timestamp of when this Comment object was created and changed in the database. Required if the Comment database record
        /// this object represents already exists. Not required if this is a new Comment
        /// </summary>
        [JsonProperty("timestamp")]
        [Metadata( DataDescription = "A timestamp of when this Comment object was created and changed in the database.")]
        public Timestamp Timestamp { get; set; }

       

    }
}
