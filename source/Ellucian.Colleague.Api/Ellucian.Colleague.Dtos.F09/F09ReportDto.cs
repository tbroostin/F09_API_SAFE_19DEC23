﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class dtoF09ReportRequest
    {
        public string Id { get; set; }
        public string Report { get; set; }
        public string JsonRequest{ get; set; }                
    }

    public class dtoF09ReportResponse
    {
        public string HtmlReport { get; set; }
    }
}
