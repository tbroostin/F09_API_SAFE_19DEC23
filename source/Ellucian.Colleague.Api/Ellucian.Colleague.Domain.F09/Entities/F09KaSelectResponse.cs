﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class F09KaSelectResponse
    {
        public List<KaSelectTerms> KATerms { get; set; }
        public List<KaSelectSTC> KAStc { get; set; }

    }
}
