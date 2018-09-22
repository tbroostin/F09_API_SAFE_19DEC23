// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Api.Client.Core
{
    /// <summary>
    /// Provides a single container for API response errors.
    /// </summary>
    /// <remarks>
    /// This class is the client-side version of Ellucian.Web.Http.Exceptions.WebApiException used for deserialization.
    /// The duplicate class was created to prevent adding the dependency to the containing assembly as it has
    /// a whole bunch of other dependencies of its own.
    /// </remarks>
    [Serializable]
    public class ColleagueApiErrorDetail
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a list of conflicts that occurred during a HTTP PUT request.
        /// </summary>
        public List<string> Conflicts { get; set; }

        /// <summary>
        /// Returns an string summarizing the ColleagueApiErrorDetail.
        /// </summary>
        /// <returns>A string summarizing the ColleagueApiErrorDetail</returns>
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
