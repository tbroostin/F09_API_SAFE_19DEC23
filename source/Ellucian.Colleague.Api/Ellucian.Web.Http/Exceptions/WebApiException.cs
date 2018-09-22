using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace Ellucian.Web.Http.Exceptions
{
    /// <summary>
    /// Represents the response information detailing why a request failed.
    /// </summary>
    public class WebApiException
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// List of conflicts that prevented an update operation. Can be null.
        /// </summary>
        public IEnumerable<string> Conflicts { get; set; }

        /// <summary>
        /// Gets a value indicating if this WebApiException is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(Message) &&
                    Conflicts == null)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds a conflict to the list of conflicts. Does not check for duplicates.
        /// </summary>
        /// <param name="conflictMessage"></param>
        public void AddConflict(string conflictMessage)
        {
            if (!string.IsNullOrEmpty(conflictMessage))
            {
                // using late initilization here to allow a null value of the Conflicts property.
                if (Conflicts == null)
                {
                    Conflicts = new List<string>();
                }
                (Conflicts as IList<string>).Add(conflictMessage);
            }
        }

        /// <summary>
        /// Adds a list of conflicts to the conflicts list. Does not check for duplicates.
        /// </summary>
        /// <param name="conflicts"></param>
        public void AddConflicts(IEnumerable<string> conflicts)
        {
            foreach (var conflict in conflicts)
            {
                AddConflict(conflict);
            }
        }

        // constructor
        public WebApiException()
        {
            Message = string.Empty;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Message = \"");
            sb.Append(Message);
            sb.Append("\"");

            if (Conflicts != null)
            {
                sb.Append(", Conflicts = [");
                sb.Append(string.Join<string>(",", Conflicts));
                sb.Append("]");
            }

            return sb.ToString();
        }

    }
}
