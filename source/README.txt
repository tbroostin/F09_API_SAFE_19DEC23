Copyright 2013-2019 Ellucian Company L.P. and its affiliates.   All Rights Reserved.
Use of this material is subject to the terms and conditions of a software license agreement between Ellucian and client.

-----------------
Colleague Web API
-----------------

The purpose of this read me is to provide basic information regarding the contents 
of this download and instructions for unpacking and preparing the source code for 
the Colleague Web API product.


1. File Contents

- ColleagueWebAPI<version>Setup.exe:
  
  This is the binary version of the product and is used to install Colleague
  Web API directly on a web server. See the Setting Up Colleague Web API
  manual for more information.

- Ellucian.Colleague.Api.zip:

  Contains the base source code for Colleague Web API. Do not
  attempt to directly unzip this archive - a PowerShell script has been 
  included that will do this for you.

- Additional .zip archives (Ellucian.Colleague.Api_<module name>.zip):

  For each Web API module you have licensed, you will receive a zip
  archive containing the source items pertaining to that module. Do not
  attempt to directly unzip these archives - a PowerShell script has been 
  included that will do this for you.

- EllucianColleagueApiSourceSetup.ps1:

  PowerShell script that completely automates the unpacking of the zip
  archives and the preparation of the Visual Studio solution. This script
  is unsigned.

- README.txt:

  This readme file.


2. Unpacking and preparing the source code

- Preparing your development PC
  
  The source code must be unpacked onto a development PC with one of the following 
  Visual Studio editions with the required SDKs installed.:
    - 2012 or 2013 Professional, Premium, or Ultimate 
    - 2015, 2017, or 2019 Professional or Enterprise 
  Note: Express versions of Visual Studio are not sufficient to perform code-level
  customization work on the source code for Colleague Web API.
  The Customizing Colleague Web API manual's Preparing for Development section
  provides details all of the steps necessary.

- Running the EllucianColleagueApiSourceSetup.ps1 PowerShell script

  NOTE: To avoid permission problems, you must run the PowerShell script 
        as the administrator (not just a user with administrative rights).

  The EllucianColleagueApiSourceSetup.ps1 PowerShell script must be run in a
  single threaded PowerShell session as it makes heavy use of COM 
  automation when unpacking the source code. In addition, this script is
  not signed, meaning that PowerShell's script execution policy must be
  set to unrestricted. The directions below take these requirements into
  account and allow you to run the script without having to modify your
  user or machine execution policy.

  a. Start Windows PowerShell as the administrator.  You can right-click on the
     PowerShell icon and select "Run as administrator".

  b. Change directories to the location of the 
     EllucianColleagueApiSourceSetup.ps1 script (this will be where you
     downloaded the software from within SA Valet):

     PS C:\Users\user> cd "C:\SAV\Colleague Web API\1.x.x.x"

  c. From the current PowerShell session, start another PowerShell session
     with single threading and an unrestricted execution policy:

     PS C:\SAV\Colleague Web API\1.x.x.x> powershell -Sta -ExecutionPolicy unrestricted

     This accomplishes starting PowerShell with single threading and an
     unrestricted execution policy which are required for the
     EllucianColleagueApiSourceSetup.ps1 script to function.

  d. Run the EllucianColleagueApiSourceSetup.ps1. The script does not have
     any required parameters and by default, the a Ellucian.Colleague.Api
     solution directory will be created in the current working directory;
     you can specify a different destination directory for the 
     Ellucian.Colleague.Api solution directory by specifying the 
     -DestinationDirectory <path> parameter; an example of each is shown
     below:

     PS C:\SAV\Colleague Web API\1.x.x.x> .\EllucianColleagueApiSourceSetup.ps1

     -or- to specify a different destination directory:

     PS C:\SAV\Colleague Web API\1.x.x.x> .\EllucianColleagueApiSourceSetup.ps1 -DestinationDirectory "C:\Ellucian Source"

     The script will first unpack the zip archives and then proceed
     to start an instance of Visual Studio and, through its automation
     interfaces, adjust the solution's items and project references
     so that they reflect the physical items and projects that were
     included in the various zip archives. Once complete, the script
     will save all changes and close Visual Studio. At this point, if
     you were to navigate to the solution directory you would find a
     working solution.
     
     A transcript file, SourceSetupLog.txt, will be created in the 
     current working directory to capture what the script did.
     Subsequent runs append their output to this file.

  e. Add the solution directory to source control