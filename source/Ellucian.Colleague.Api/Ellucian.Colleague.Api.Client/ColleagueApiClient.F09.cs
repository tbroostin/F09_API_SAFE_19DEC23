﻿using System;
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

        // F09 added here on 05-20-2019 for Pdf Student Tracking Sheet project
        private static readonly string F09GetPdfStudentTrackingSheet = "f09/pdf-student-tracking-sheet";

        // F09 added here on 04-10-2019
        private static readonly string F09AdminTrackingSheet = "f09/f09AdminTrackingSheet";

        // F09 added here on 05-13-2019
        private static readonly string F09GetStudentAlumniDirectories = "f09/get-student-alumni-directories";
        private static readonly string F09UpdateStudentAlumniDirectories = "f09/update-student-alumni-directories";

        // F09 added here on 05-05-2019 for Demo Reporting Project
        private static readonly string F09GetStudentStatement = "f09/get-student-statement";

        // F09 teresa@toad-code.com 05/21/19
        private static readonly string getF09Ssn = "f09/get-f09Ssn";
        private static readonly string updateF09Ssn = "f09/put-f09Ssn";

        // F09 teresa@toad-code.com 07/17/19
        private static readonly string getF09KaSelect = "f09/get-f09KaSelect";

        // F09 teresa@toad-code.com 06/18/19
        private static readonly string getF09KaGrading = "f09/get-f09KaGrading";

        // F09 teresa@toad-code.com 06/18/19
        private static readonly string updateF09KaGrading = "f09/put-f09KaGrading";

        // F09 teresa@toad-code.com 11/30/19
        private static readonly string getF09Report = "f09/get-f09Report";

        // F09 teresa@toad-code.com 07/30/20
        private static readonly string getF09EvalSelect = "f09/get-f09EvalSelect";
        private static readonly string getF09EvalForm = "f09/get-f09EvalForm";
        private static readonly string updateF09EvalForm = "f09/put-f09EvalForm";

        #region TuitionPaymentPlan
        private static readonly string _getF09Payment = "f09/tuition-payment";
        #endregion TuitionPaymentPlan
    }
}
