**==F09==========================================================**
**-----------------------------------------------------------------
**
**        Fielding Ellucian Web Student (Self Service)
**        -------------------------------------------
**        https://github.com/TOAD-CODE/FGU-WebApi
**
**-----------------------------------------------------------------

**-----------------------------------------------------------------
          Summary of Branches
**-----------------------------------------------------------------
  
  FGU-dev
      purpose..............: matches build that is currently deployed to TEST18
      Colleague.Api Version: 1.37.1.2
      deployed date........: 01/14/21
      deployed commit......: see server for actual commit (this is inside source control)

  master
      purpose..............: matches build that is currently deployed to PRODUCTION
      Colleague.Api Version: 1.33.1.6
      deployed date........: 12/13/21
      deployed commit......: see server for actual commit (this is inside source control)

**-----------------------------------------------------------------
          Deployment Log
**-----------------------------------------------------------------
  02/03/20 PROJDB18 v1.26.0.8 65a5571451e0a6070f7ef8b5ad319af52bf9cddc -upgrade API to v1.26
  02/26/20 LIVE     v1.26.0.8 65a5571451e0a6070f7ef8b5ad319af52bf9cddc -upgrade API to v1.26
  07/30/20 TEST18   v1.26.0.8 118526a29b19d7e66b250d5e175486165538124d -Student Evals Project
  09/05/20 TEST18   v1.28.1.5 aed9a805f070469a39ec82e434acf594001ee950 -upgrade API to v1.28
  09/05/20 PROJDB18 v1.28.1.5 aed9a805f070469a39ec82e434acf594001ee950 -upgrade API to v1.28
  12/15/20 TEST18   v1.29.1.9                                          -upgrade API to v1.29
  12/15/20 PROJDB18 v1.29.1.9                                          -upgrade API to v1.29
  12/15/20 LIVE     v1.29.1.9                                          -upgrade API to v1.29
  12/08/21 TEST18   v1.33.1.6                                          -upgrade API to v1.33
  12/13/21 TEST18   v1.33.1.6                                          -upgrade API to v1.33
  01/14/22 TEST18                                                      -override reg dates cache timer
  01/19/22 LIVE                                                        -override reg dates cache timer
  12/03/22 TEST18   v1.37.1.2                                          -upgrade API to v1.37

**-----------------------------------------------------------------
          Summary of Custom CTX Transactions
**-----------------------------------------------------------------
  XCTX.GET.ACTIVE.RESTRICTIONS   alias: ctxGetActiveRestrictions -10/08/18, leroy (teresa on ERP side) <--this is the opt out of paper statments project
  XCTX.UPDATE.STU.RESTRICTION    alias: ctxUpdateStuRestrictions -10/03/18, leroy (teresa on ERP side)
  XCTX.F09.SS.FAAPP              alias: F09_ScholarshipApplication -03/29/19, leroy (teresa on ERP side)     
  XCTX.F09.STU.TRACKING.SHEET    alias: ctxF09StuTrackingSheet   -03/29/19, teresa
  XCTX.F09.ADMIN.TRACKING.SHEET  alias: ctxF09AdminTrackingSheet -04/20/19, teresa
  XCTX.F09.PDF.TRACKING.SHEET    alias: ctxF09PdfTrackingSheet   -06/07/19, leroy
  XCTX.F09.SS.DIRT               alias: ctxDirectories           -05/11/19, leroy (teresa on ERP side)
  XCTX.F09.SS.SSN                alias: ctxF09Ssn                -05/21/19, teresa
  XCTX.F09.SS.KA.SELECT          alias: ctxF09KaSelect           -06/17/19, teresa 
  XCTX.F09.SS.KA.GRADING         alias: ctxF09KaGrading          -07/07/19, teresa
  XCTX.F09.SS.PAY.PLAN.SIGNUP    alias: ctxF09PayPlanSignup      -08/18/19, roger (teresa on ERP side)
  XCTX.F09.REPORT                alias: ctxF09Report             -12/03/19, teresa
  XCTX.F09.SS.EVAL.FORM          alias: ctxF09EvalForm           -07/30/20, teresa
  XCTX.F09.SS.EVAL.SELECT        alias: ctxF09EvalSelect         -07/30/20, teresa


**-----------------------------------------------------------------
          Modification Log - Important notes
**-----------------------------------------------------------------
 f09 teresa@toad-code.com 07/30/30
 Important note: after generating the ctxF09EvalForm transaction
 delete "public class Questions"
 because KA grading already defines that class, we simply need to reuse it
 
 F09 teresa@toad-code.com 05/18/21, 12/08/21
 -change PDF report column header from "Acadmic Program" to "Program (Catalog)"
 in:
 \source\Ellucian.Colleague.Api\Ellucian.Colleague.Api\Reports\F09\StudentTrackingSheet.rdlc 

 f09 teresa@toad-code.com 01/14/22
 background: Waitlist students are not "given permission to register" until after the "standard" section registration window is closed.
 To make this work,  the registrar uses RGUC to change the "registration ADD end-date"  for the section.
 Then they give the waitlist student permission-to-register, and the student has 2 days to register. 
 The trouble occurs when the student goes to SS to register.The section "registration ADD end-date" is on a 24 hour cache.

 solution: for the first month of each term (Jan, May, Sept), there will only be a few students registering,
 in those months, override the cache timer to 1 minute (instead of 24 hours)

 modified:
  source/Ellucian.Colleague.Api/Ellucian.Colleague.Data.Student/Repositories/RegistrationGroupRepository.cs
 