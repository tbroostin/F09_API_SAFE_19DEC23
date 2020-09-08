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
      purpose..............: matches build that is currently deployed to PROJDB18
      Colleague.Api Version: 1.28.1.5
      deployed date........: 09/05/20
      deployed commit......: see server for actual commit (this is inside source control)
      zipped code..........: C:\Program Files (x86)\Ellucian\test18_1.26\F09ColleagueApi\FGU-WebApi-FGU-dev.zip

  master
      purpose..............: matches build that is currently deployed to PRODUCTION
      Colleague.Api Version: 1.26.0.8
      deployed date........: 02/26/20
      deployed commit......: 65a5571451e0a6070f7ef8b5ad319af52bf9cddc
      zipped code..........: C:\Program Files (x86)\Ellucian\Live18_1.26\F09ColleagueApi\FGU-WebApi-master.zip

**-----------------------------------------------------------------
          Deployment Log
**-----------------------------------------------------------------
  02/03/20 PROJDB18 v1.26.0.8 65a5571451e0a6070f7ef8b5ad319af52bf9cddc -upgrade API to v1.26
  02/26/20 LIVE     v1.26.0.8 65a5571451e0a6070f7ef8b5ad319af52bf9cddc -upgrade API to v1.26
  07/30/20 TEST18   v1.26.0.8 118526a29b19d7e66b250d5e175486165538124d -Student Evals Project
  09/05/20 TEST18   v1.28.1.5 (see server)                             -upgrade API to v1.28

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
 
     
 