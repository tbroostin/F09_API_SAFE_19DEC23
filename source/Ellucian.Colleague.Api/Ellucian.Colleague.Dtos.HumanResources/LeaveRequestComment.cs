using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for leave request comment
    /// </summary>
    public class LeaveRequestComment
    {
        /// <summary>
        /// The database ID of the Comments object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The ID of the leave request this Comment is associated to. 
        /// </summary>
        public string LeaveRequestId { get; set; }

        /// <summary>
        /// The actual Comment contents
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// the ID of the employee this Comment is associated to
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// The name of the person who created the comment
        /// </summary>
        public string CommentAuthorName { get; set; }

        /// <summary>
        /// A timestamp of when this Comment object was created and changed in the database. Required if the Comment database record
        /// this object represents already exists. Not required if this is a new Comment
        /// </summary>
        public Timestamp Timestamp { get; set; }

       

    }
}
