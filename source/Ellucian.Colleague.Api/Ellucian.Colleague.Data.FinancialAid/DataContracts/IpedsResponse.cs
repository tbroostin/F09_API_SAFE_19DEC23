using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
    /// <summary>
    /// This is a custom data contract for the result of an external API call to the
    /// National Center for Education Statistics (NCES) - 
    /// Integrated Postsecondary Education Data System (IPEDS) - 
    /// Post-Secondary Universe Survey 2010 - Directory information - API
    /// 
    /// The JSON response from the API call is deserialized into this DataContract
    /// </summary>
    public class IpedsResponse
    {
        public IpedsResult result { get; set; }
    }

    public class IpedsResult
    {
        public List<IpedsRecord> records { get; set; }
    }

    public class IpedsRecord
    {
        public string unitid { get; set; }
        public string instnm { get; set; }
        public string opeid { get; set; }
    }
}
