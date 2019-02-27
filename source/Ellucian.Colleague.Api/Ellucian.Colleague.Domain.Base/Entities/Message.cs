using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{

    [Serializable]
    public class Message
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
        public string ProcessLink { get { return _processLink; } }
        private readonly string _processLink;

        /// <summary>
        /// Create a work task for a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        public Message(string id, string category, string description, string processLink)
        {
            _id = id;
            _category = category;
            _description = description;
            _processLink = processLink;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var message = obj as Message;

            return message.Id.Equals(this.Id);
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
