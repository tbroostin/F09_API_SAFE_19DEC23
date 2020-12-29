using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    
    public class F09KaSelectResponseDto
    {
        public List<KaSelectTermsDto> KATerms { get; set; }
        public List<KaSelectSTCDto> KAStc { get; set; }



        //constructors
        public F09KaSelectResponseDto()
        { }

        public F09KaSelectResponseDto
        (
            List<KaSelectTermsDto> kaTerms,
            List<KaSelectSTCDto> kaStc
        )
        {
            this.KATerms = kaTerms;
            this.KAStc = kaStc;
        }
    }
}