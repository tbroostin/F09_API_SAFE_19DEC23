using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class F09AdminTrackingSheetResponseDto
    {
        public List<F09AdminTrackingSheetDto> AdminTrackingSheets { get; set; }

        //constructors
        public F09AdminTrackingSheetResponseDto()
        { }

        public F09AdminTrackingSheetResponseDto
        (
            List<F09AdminTrackingSheetDto> adminTrackingSheets
        )
        {
            this.AdminTrackingSheets = adminTrackingSheets;
        }
    }

}
