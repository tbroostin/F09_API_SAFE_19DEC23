using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class GetF09StuTrackingSheetResponseDto
    {
        public string html { get; set; }


        // Constructors
        public GetF09StuTrackingSheetResponseDto()
        { }

        public GetF09StuTrackingSheetResponseDto
        (
            string html
        )
        {
            this.html = html;
        }
    }
}
