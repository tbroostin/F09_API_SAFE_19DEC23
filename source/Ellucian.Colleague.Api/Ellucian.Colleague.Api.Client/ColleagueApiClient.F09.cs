using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client
{
    partial class ColleagueApiClient
    {
        private static readonly string F09ActiveRestrictions = "f09/active-restrictions";
        private static readonly string F09StudentRestriction = "f09/student-restriction";

        // F09 added here on 03-14-2019
        private static readonly string F09GetScholarshipApplication = "f09/get-scholarship-application";
        private static readonly string F09UpdateScholarshipApplication = "f09/update-scholarship-application";

        // F09 added here on 04-01-2019
        private static readonly string F09StuTrackingSheet = "f09/f09StuTrackingSheet";
        private static readonly string F09AdminTrackingSheet = "f09/f09AdminTrackingSheet";
    }
}
