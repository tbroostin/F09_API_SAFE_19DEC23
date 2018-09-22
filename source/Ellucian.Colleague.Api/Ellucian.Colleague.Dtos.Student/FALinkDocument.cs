// Copyright 2017 Ellucian Company L.P. and its affiliates
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// FA Link Document DTO
    /// </summary>
    public class FALinkDocument
    {
        /// <summary>
        /// An anonymous JSON document acting as the container of both request and response contents from and to TrimData's FALink service.
        /// </summary>
        /*
          This document allows this API endpoint to be maintenance-free from Ellucian's perspective. If TrimData needs to add anything to 
          the transaction, they will only need to modify the JSON doc and the Envision code that handles it.
        */
        public JObject Document { get; set; }
    }
}
