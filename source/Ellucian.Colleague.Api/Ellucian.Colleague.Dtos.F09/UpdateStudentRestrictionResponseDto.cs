using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class UpdateStudentRestrictionResponseDto
    {
        public string Error { get; set; }

        public string Message { get; set; }

        // Constructors
        public UpdateStudentRestrictionResponseDto()
        { }

        public UpdateStudentRestrictionResponseDto
        (
            string error,
            string message
        )
        {
            this.Error = error;
            this.Message = message;
        }
    }
}
