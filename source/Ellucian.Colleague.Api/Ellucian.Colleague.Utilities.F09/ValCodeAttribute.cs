using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Utilities.F09
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ValCodeAttribute : Attribute
    {
        public string ValCodeID { get; }

        public Enums.ValConvert Convert { get; set; }
        public string[] ValidCodes { get; set; }

        public ValCodeAttribute(string valcodeId, params string[] validCodes)
        {
            this.ValCodeID = valcodeId;
            Convert = Enums.ValConvert.Code;
            ValidCodes = validCodes;
        }
    }
}
