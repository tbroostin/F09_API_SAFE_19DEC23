# Copyright 2013-2019 Ellucian Company L.P. and its affiliates.
param(
[string]$DestinationDirectory,
[switch]$Extract,
[switch]$Prepare,
[switch]$WhatIf
)

$SolutionDirectory = "Ellucian.Colleague.Api"
$SolutionFilename = "Ellucian.Colleague.Api.sln"
$LoggingFilename = "SourceSetupLog.txt"

# --- Function defs ---------------------------------------
function ScriptInformation()
{
    $separator = New-Object string -ArgumentList @('-', 100)
    Write-Host $separator
    "Actions performed by this script and what to expect while it runs:"
    ""
    "This script is designed to completely automate the 1) unpacking and 2) preparation of the source code,"
    "making it ready for custom development. Prior to running this script you should verify that your instance"
    "of Visual Studio is setup correctly for development by reading the Preparing for Development section "
    "of the customization guide."
    ""
    "1) Unpacking the source code"
    ""
    "The source code for the Visual Studio solution is delivered using multiple zip (.zip) archives, allowing"
    "for the appropriate source to be provided based on licensed modules. This script will automatically unpack"
    "those archives and place them in a single solution directory. By default, the solution directory is created"
    "in the working directory of this script, but a different path can be specified by using the"
    "-DestinationDirectory <path> argument. The unpack step will first verify that the target solution directory"
    "is empty, if not, an error will be shown and the script will halt; the destination solution directory must"
    "be empty to ensure proper code unpacking."
    ""
    "2) Preparing the Visual Studio solution"
    ""
    "The solution, as provided in the zip archives, contains references to all solution items (including those"
    "tagged as optional modules). This action of the script utilizes Visual Studio automation in order to"
    "remove those references to missing items without having to do so manually, which is extremely time"
    "consuming."
    ""
    "THIS STEP IS INTERACTIVE! An instance of Visual Studio will be started by this script, and you may have to"
    "answer prompts within Visual Studio before the script is able to proceed. As long as all of the required"
    "development SDKs have been installed and you answer any prompts, the script should be able to run to"
    "completion without issue. If there are any questions regarding the required SDKs and Visual Studio setup,"
    "see the Preparing for Development section of the customization guide. Once this step is complete, the "
    "script will save and close Visual Studio, then you are ready to open the solution and begin modifying code."
    ""
    "This step can be run with the -WhatIf argument in order to see what references will be"
    "removed but without actually removing the references."
    Write-Host $separator
    ""
    Write-Host "Press any key to continue running this script (or ctrl-c to quit)"
    $x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")
}

function AddMessageFilterClass 
{ 
$source = @" 
// http://msdn.microsoft.com/en-us/library/ms228772(VS.80).aspx 
namespace ComUtils{ 
 
using System;
using System.Threading;
using System.Runtime.InteropServices; 
    public class MessageFilter : IOleMessageFilter 
    { 
        public static void Register() 
        { 
            IOleMessageFilter newFilter = new MessageFilter();  
            IOleMessageFilter oldFilter = null;  
            int x = CoRegisterMessageFilter(newFilter, out oldFilter);
            Console.WriteLine(string.Format("Message filter registered ({0})", x));
        } 
        public static void Revoke() 
        { 
            IOleMessageFilter oldFilter = null;  
            int x = CoRegisterMessageFilter(null, out oldFilter); 
            Console.WriteLine(string.Format("Message filter revoked ({0})", x));
        } 
        int IOleMessageFilter.HandleInComingCall(int dwCallType,  
          System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr  
          lpInterfaceInfo)  
        { 
            //Return the flag SERVERCALL_ISHANDLED.		
            return 0; 
        } 
        int IOleMessageFilter.RetryRejectedCall(System.IntPtr  
          hTaskCallee, int dwTickCount, int dwRejectType) 
        { 
            Console.WriteLine("Busy, retry");
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER. 
            { 
                // Retry the thread call immediately if return >=0 &  
                // <100.
                Thread.Sleep(200);
                return 99; 
            } 
            // Too busy; cancel call. 
            return -1; 
        } 
        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,  
          int dwTickCount, int dwPendingType) 
        { 
            //Return the flag PENDINGMSG_WAITDEFPROCESS. 
            return 2;  
        } 
        [DllImport("Ole32.dll")] 
        private static extern int  
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out  
          IOleMessageFilter oldFilter); 
    } 
    [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),  
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)] 
    interface IOleMessageFilter  
    { 
        [PreserveSig] 
        int HandleInComingCall(  
            int dwCallType,  
            IntPtr hTaskCaller,  
            int dwTickCount,  
            IntPtr lpInterfaceInfo); 
        [PreserveSig] 
        int RetryRejectedCall(  
            IntPtr hTaskCallee,  
            int dwTickCount, 
            int dwRejectType); 
        [PreserveSig] 
        int MessagePending(  
            IntPtr hTaskCallee,  
            int dwTickCount, 
            int dwPendingType); 
    } 
} 
"@ 
Add-Type -TypeDefinition $source 
} 

function CheckDependencies()
{
    # check for STA threading
    if([System.Threading.Thread]::CurrentThread.ApartmentState -ne "STA")
    {
        Write-Host "Powershell is not running with -Sta option. This script can only run reliably using STA threading." -ForegroundColor Red
        Break #exit script!
    }
}

function CheckArguments()
{
    if ([string]::IsNullOrEmpty($DestinationDirectory))
    {
        # use current working directory
        Set-Variable -Name DestinationDirectory -Value ($PWD.ToString().TrimEnd('\\')) -Scope script
    }
    
    if (!$Extract -and !$Prepare)
    {
        Set-Variable -Name Extract -Value $True -Scope script
        Set-Variable -Name Prepare -Value $True -Scope script
    }
}

function ExtractZipFiles()
{
    param(
    [Parameter(Mandatory=$true)]
    [string]$sourceFolder, 

    [Parameter(Mandatory=$true)]
    [string]$destinationFolder
    )

    Write-Host ([String]::Format("Looking for source zip files in '{0}'", $sourceFolder))
    Write-Host ([String]::Format("Writing zip contents to '{0}'", $destinationFolder))

    if(!(Test-Path $sourceFolder))
    {
        Write-Error ([String]::Format("Directory '{0}' not found", $sourceFolder))
    }

    if(!(Test-Path $destinationFolder))
    {
        $result = [System.IO.Directory]::CreateDirectory($destinationFolder)
        Write-Host ([String]::Format("Directory '{0}' not found, created", $destinationFolder))
    }

    $zipFiles = [System.IO.Directory]::GetFiles($sourceFolder, "*.zip", [System.IO.SearchOption]::TopDirectoryOnly)
    if($zipFiles -ne $null -and $zipFiles.Length -gt 0)
    {
        $shellApp = New-Object -ComObject shell.application
        $shellDestFolder = $shellApp.Namespace($destinationFolder)
        foreach($zipFileName in $zipFiles)
        {
            Write-Host ("Unzipping file " + $zipFileName)
            $zipFile = $shellApp.Namespace($zipFileName)
            $shellDestFolder.CopyHere($zipFile.Items(), 0x14)
        }   
    }
}

function StartLogging()
{
    StopLogging
    Start-Transcript -path $LoggingFilename -append
}

function StopLogging()
{
    $ErrorActionPreference="SilentlyContinue"
    Stop-Transcript | out-null
    $ErrorActionPreference = "Continue"
}

function OpenSolution()
{
    $DTE.MainWindow | %{$_.GetType.Invoke().InvokeMember("Visible","SetProperty",$null,$_,$true)}
    $DTE.Solution.Open($solutionFilename)
    do
    {
        Start-Sleep -Seconds 2
    }
    while($DTE.StatusBar.Text -ne "Ready")
}

function CloseSolution()
{
    if(!$WhatIf)
    {
        $DTE.ExecuteCommand("File.SaveAll")
        Write-Host "Solution Saved"
    }
    $DTE.Solution.Close()
    $DTE.Quit()
}

function WriteMessage()
{
    param(
    [Parameter(Mandatory=$true)]
    $action,
    [Parameter(Mandatory=$true)]
    $message
    )
    $spacerCount = 15 - $action.Length
    if ($spacerCount -lt 0)
    {
        $spacerCount = 0
    }
    $spacer = New-Object string -ArgumentList @('.', $spacerCount)
    Write-Host ([String]::Format("{0}{1}: {2}", $action, $spacer, $message))
}

function WalkSubProject()
{
    param(
    [Parameter(Mandatory=$true)]
    $project
    )
    
    Write-Host ([String]::Format("{1}Scanning project '{0}'", $project.Name, [Environment]::NewLine))
    $projectItems = $project.ProjectItems
    foreach($projItem in $projectItems)
    {
        VerifyProjectItem $projItem
    }
}

function VerifyProjectItem()
{
    param(
    [Parameter(Mandatory=$true)]
    $projectItem
    )
    
    $removeprojectItem = 0

    if ($projectItem.FileCount -gt 0)
    {
        For ($i=1; $i -le $projectItem.FileCount; $i++)
        {
            $removeFile = 0
            if ($projectItem.FileNames($i))
            {
                $attribs = 0
                try
                {
                    $attribs = [System.IO.File]::GetAttributes($projectItem.FileNames($i))
                }
                catch
                {
                    if (![System.IO.File]::Exists($projectItem.FileNames($i)))
                    {
                        WriteMessage "Remove" $projectItem.FileNames($i)
                        $removeFile = 1
                    }
                }
                if ($attribs -gt 0)
                {
                    if(($attribs -band 0x16) -gt 0)
                    {
                        WriteMessage "Inspecting" ([String]::Format("{0}", $projectItem.FileNames($i)))
                        $projectItems = $projectItem.ProjectItems
                        foreach($projItem in $projectItems)
                        {
                            VerifyProjectItem $projItem
                        }
                        if ($projectItems.Count -eq 0)
                        {
                            WriteMessage "Remove, empty" $projectItem.FileNames($i)
                            $removeFile = 1
                        }
                    }
                    else
                    {
                        WriteMessage "Verified" $projectItem.FileNames($i)
                    }
                }
            }

            if ($projectItem.SubProject)
            {
                WalkSubProject $projectItem.SubProject
            }
        }
    }
    
    if(($projectItem.ProjectItems -eq $null) -and ($projectItem.SubProject -eq $null) -and ($projectItem.FileNames(1) -eq $null))
    {
        WriteMessage "Missing project" $projectItem.Name
        $removeFile = 1
    }   
    
    if($removeFile -and (!$WhatIf))
    {
        Write-Host "                 Removing item" -ForegroundColor Magenta
        $projectItem.Delete()
    }
}

function VerifySolutionItems()
{
    $solutionProjects = $DTE.Solution.Projects
    foreach ($solutionProject in $solutionProjects)
    {
        Write-Host ([String]::Format("{1}Scanning project '{0}'", $solutionProject.Name, [Environment]::NewLine))
        $projectItems = $solutionProject.ProjectItems
        foreach($projItem in $projectItems)
        {
            VerifyProjectItem $projItem
        }
    }
}

function getDTE()
{
    $devEnv = $null
    $supportedVersions = @{
            "VS 2019" = "VisualStudio.DTE.16.0"
            "VS 2017" = "VisualStudio.DTE.15.0"
            "VS 2015" = "VisualStudio.DTE.14.0"
            "VS 2013" = "VisualStudio.DTE.12.0"
            "VS 2012" = "VisualStudio.DTE.11.0"}
    foreach($versionToTry in $supportedVersions.GetEnumerator() | Sort-Object Key -descending)
    {
        try
        {
            $devEnv = New-Object -com $versionToTry.Value
            break
        }
        catch{}
    }
    if ($devEnv -eq $null)
    {
        Write-Host Unable to load a supported version of Visual Studio!
        Break #exit script!
    }
    
    return $devEnv
}

function VerifySolutionItemsManual()
{
    Add-Type -AssemblyName System.Web

    $newSolutionContent = @()
    $removedProjectGuids = @()
    $validProjectPaths = @()

    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    # Solution file processing
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    $solutionContent = Get-Content $SolutionFilename
    $includeLine = $true
    $inProject = $false
    $solutionItems = $false

    WriteMessage "Verifying content in " $SolutionFilename

    foreach($line in $solutionContent) 
    {
        $compareLine = $line.Replace("`t", "")
    
        if (($includeLine -eq $false) -and ($inProject -eq $true))
        {
            # Prior line was a project that we excluded; don't include the accompanying EndProject line, either
            $includeLine = $false
        }
        else
        {
            # Otherwise, default to include
            $includeLine = $true
        }
    
        if ($solutionItems -eq $true)
        {
            ############################
            # SolutionItems section
            ############################

            # Set include back to true, since we test these items one-by-one
            $includeLine = $true

            if ($compareLine -eq "EndProjectSection")
            {
                # End of "SolutionItems" section
                $solutionItems = $false
            }
            else
            {
                # Check the solution item
                $solutionItemParts = $compareLine -Split ' = '
                $solutionItemRelativePath = $solutionItemParts[1].Trim()            
                $solutionItemAbsolutePath = [System.IO.Path]::Combine($SolutionDirectory, $solutionItemRelativePath)
                if (!(Test-Path $solutionItemAbsolutePath))
                {
                    # Exclude it
                    $includeLine = $false
                    WriteMessage "Remove" $solutionItemAbsolutePath
                }
            }
        } 
        elseif (($inProject -eq $true) -and ($compareLine.StartsWith("ProjectSection(SolutionItems)")))
        {
            # Enable the solution items check flag
            $solutionItems = $true
        }
        elseif ($compareLine.StartsWith("Project("))
        {
            ############################
            # Project group section
            ############################
            $inProject = $true
        
            $projectParts = $compareLine -Split '[,=]' | ForEach-Object { $_.Trim('[ "]') };

            $projectInfo = New-Object PSObject -Property @{
                Name = $projectParts[1];
                File = $projectParts[2];
                Guid = $projectParts[3]
            }

            # Only check project files (e.g. Name and File properties do not match)
            if ($projectInfo.Name -ne $projectInfo.File)
            {
                $projectAbsolutePath = [System.IO.Path]::Combine($SolutionDirectory, $projectInfo.File)
            
                if (Test-Path $projectAbsolutePath)
                {
                    # While we're here, add the valid csproj path to our list; will loop through and
                    # clean those up after the entire solution has been cleaned
                    $validProjectPaths += $projectAbsolutePath
                    WriteMessage "Verified" $projectInfo.Name
                }
                else
                {
                    # Project does not exist; exclude it
                    $includeLine = $false
                    $removedProjectGuids += $projectInfo.Guid
                    WriteMessage "Missing project" $projectInfo.Name
                }
            }
        }
        elseif (($compareLine.StartsWith("EndProject")) -and (-not ($compareLine.StartsWith("EndProjectSection"))))
        {
            # Exit project group
            $inProject = $false
        }
    
        if ($includeLine -eq $true)
        {
            # Add the current line to the new solution file we're building
            $newSolutionContent += $line
        }
    }

    # Now, loop through the modified solution file one more time, removing any
    # references to projects that we have removed on the first pass
    $solutionContent = $newSolutionContent
    $newSolutionContent = @()
    foreach($line in $solutionContent) 
    {
        $includeLine = $true
        foreach($removedProjectGuid in $removedProjectGuids)
        {
            if ($line.IndexOf($removedProjectGuid) -ge 0)
            {
                # This line references a removed project; do not include
                $includeLine = $false
                break
            }
        }
    
        if ($includeLine -eq $true)
        {
            # Add the current line to the new solution file we're building
            $newSolutionContent += $line
        }
    }

    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    # csproj file processing
    #~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    # New, loop through all valid project files that we gathered along the way, cleaning those up, as well
    foreach($csprojPath in $validProjectPaths)
    {
        # Read the project file and clean it up
        $csprojParent = Split-Path -Path $csprojPath -Parent
    
        [xml]$csprojXml = Get-Content $csprojPath
    
        # Default to false until we know we have changed to write out
        $csprojChanged = $false
    
        foreach ($itemGroupChild in $csprojXml.Project.ItemGroup.ChildNodes)
        {
            $removeItem = $false

            if ($itemGroupChild.LocalName -eq "Compile")
            {
                $compileItemXml = [xml]$itemGroupChild.OuterXml
                $compilePath = [System.Web.HttpUtility]::UrlDecode($compileItemXml.Compile.Include)
                $compileAbsolutePath = [System.IO.Path]::Combine($csprojParent, $compilePath)
                if (!(Test-Path $compileAbsolutePath -PathType Leaf))
                {
                    WriteMessage "Remove item" $compileAbsolutePath
                    $removeItem = $true

                    if ($itemGroupChild.ParentNode.ChildNodes.Count -eq 1)
                    {
                        # Only a single item in this ItemGroup; delete the entire ItemGroup node
                        $itemGroupChild.ParentNode.ParentNode.RemoveChild($itemGroupChild.ParentNode)
                        $csprojChanged = $true
                    }
                    else
                    {
                        # Remove only this child from the ItemGroup
                        $itemGroupChild.ParentNode.RemoveChild($itemGroupChild)
                        $csprojChanged = $true
                    }
                }
            }
            elseif ($itemGroupChild.LocalName -eq "Content")
            {
                $contentItemXml = [xml]$itemGroupChild.OuterXml
                $contentPath = [System.Web.HttpUtility]::UrlDecode($contentItemXml.Content.Include)
                $contentAbsolutePath = [System.IO.Path]::Combine($csprojParent, $contentPath)
                if (!(Test-Path $contentAbsolutePath -PathType Leaf))
                {
                    WriteMessage "Remove item" $contentAbsolutePath
                    $removeItem = $true

                    if ($itemGroupChild.ParentNode.ChildNodes.Count -eq 1)
                    {
                        # Only a single item in this ItemGroup; delete the entire ItemGroup node
                        $itemGroupChild.ParentNode.ParentNode.RemoveChild($itemGroupChild.ParentNode)
                        $csprojChanged = $true
                    }
                    else
                    {
                        # Remove only this child from the ItemGroup
                        $itemGroupChild.ParentNode.RemoveChild($itemGroupChild)
                        $csprojChanged = $true
                    }
                }
            }
            elseif ($itemGroupChild.LocalName -eq "Folder")
            {
                $folderItemXml = [xml]$itemGroupChild.OuterXml
                $folderPath = [System.Web.HttpUtility]::UrlDecode($folderItemXml.Folder.Include)
                $folderAbsolutePath = [System.IO.Path]::Combine($csprojParent, $folderPath)
                if (!(Test-Path $folderAbsolutePath -PathType Container))
                {
                    WriteMessage "Remove folder" $folderAbsolutePath
                    $removeItem = $true

                    if ($itemGroupChild.ParentNode.ChildNodes.Count -eq 1)
                    {
                        # Only a single item in this ItemGroup; delete the entire ItemGroup node
                        $itemGroupChild.ParentNode.ParentNode.RemoveChild($itemGroupChild.ParentNode)
                        $csprojChanged = $true
                    }
                    else
                    {
                        # Remove only this child from the ItemGroup
                        $itemGroupChild.ParentNode.RemoveChild($itemGroupChild)
                        $csprojChanged = $true
                    }
                }
            }
            elseif ($itemGroupChild.LocalName -eq "None")
            {
                $noneItemXml = [xml]$itemGroupChild.OuterXml
                $nonePath = [System.Web.HttpUtility]::UrlDecode($noneItemXml.None.Include)
                $noneAbsolutePath = [System.IO.Path]::Combine($csprojParent, $nonePath)
                if (!(Test-Path $noneAbsolutePath -PathType Leaf))
                {
                    WriteMessage "Remove item" $noneAbsolutePath
                    $removeItem = $true

                    if ($itemGroupChild.ParentNode.ChildNodes.Count -eq 1)
                    {
                        # Only a single item in this ItemGroup; delete the entire ItemGroup node
                        $itemGroupChild.ParentNode.ParentNode.RemoveChild($itemGroupChild.ParentNode)
                        $csprojChanged = $true
                    }
                    else
                    {
                        # Remove only this child from the ItemGroup
                        $itemGroupChild.ParentNode.RemoveChild($itemGroupChild)
                        $csprojChanged = $true
                    }
                }
            }
            elseif ($itemGroupChild.LocalName -eq "WCFMetadata")
            {
                $wcfItemXml = [xml]$itemGroupChild.OuterXml
                $wcfPath = [System.Web.HttpUtility]::UrlDecode($wcfItemXml.WCFMetadata.Include)
                $wcfAbsolutePath = [System.IO.Path]::Combine($csprojParent, $wcfPath)
                if (!(Test-Path $wcfAbsolutePath -PathType Container))
                {
                    WriteMessage "Remove folder" $wcfAbsolutePath
                    $removeItem = $true

                    if ($itemGroupChild.ParentNode.ChildNodes.Count -eq 1)
                    {
                        # Only a single item in this ItemGroup; delete the entire ItemGroup node
                        $itemGroupChild.ParentNode.ParentNode.RemoveChild($itemGroupChild.ParentNode)
                        $csprojChanged = $true
                    }
                    else
                    {
                        # Remove only this child from the ItemGroup
                        $itemGroupChild.ParentNode.RemoveChild($itemGroupChild)
                        $csprojChanged = $true
                    }
                }
            }
            elseif ($itemGroupChild.LocalName -eq "ProjectReference")
            {
                $projectReferenceItemXml = [xml]$itemGroupChild.OuterXml
                $projectPath = [System.Web.HttpUtility]::UrlDecode($projectReferenceItemXml.ProjectReference.Include)
                $projectAbsolutePath = [System.IO.Path]::Combine($csprojParent, $projectPath)
                if (!(Test-Path $projectAbsolutePath -PathType Leaf))
                {
                    WriteMessage "Remove project" $projectAbsolutePath
                    $removeItem = $true

                    # This referenced project does not exist; remove it
                    $itemGroupChild.ParentNode.RemoveChild($itemGroupChild)
                    $csprojChanged = $true
                }
            }

            if ($removeItem -and (!$WhatIf))
            {
                Write-Host "                 Removing item" -ForegroundColor Magenta
            }
        }
    
        if ($csprojChanged -eq $true)
        {
            if (!$WhatIf)
            {
                # Write out the changes
                $csprojXml.Save($csprojPath)
            }
        }
    }

    if (!$WhatIf)
    {
        # Write out the cleaned solution file
        Set-Content -Path $SolutionFilename -Value $newSolutionContent
    }
}

# --- End Function defs -----------------------------------

#--- Where the real work starts ---
CheckDependencies # check script dependencies
CheckArguments # validate arguments
ScriptInformation # show user what this script does
StartLogging # start a transcript

# transform these vars into full paths
$SolutionDirectory = [System.IO.Path]::Combine($DestinationDirectory, $SolutionDirectory)
$SolutionFilename = [System.IO.Path]::Combine($SolutionDirectory, $SolutionFilename)

Write-Host ([String]::Format("Preparing source solution '{0}'", $SolutionFilename))

# Add and register the MessageFilter class for better COM stability
AddMessageFilterClass 
[ComUtils.MessageFilter]::Register()

# Extract zip archives
if($Extract)
{
    Write-Host "Extracting zip files..."
    # check to see if the target solution dir exists, if it does, tell the user to remove it and exit the script.
    if ([System.IO.Directory]::Exists($SolutionDirectory))
    {
        $directoryInfo = Get-ChildItem $SolutionDirectory -Force | Measure-Object
        if($directoryInfo.Count -ne 0)
        {
            Write-Host ([String]::Format("The destination directory '{0}' is not empty. Please remove all items from this directory and run again. This script is not able to automatically remove these items.", $SolutionDirectory)) -ForegroundColor Red
            Break #exit script!
        }
    }
    ExtractZipFiles $PWD $DestinationDirectory
}

# Prepare solution (Visual Studio automation)
if ($Prepare)
{	
    Write-Host "Preparing solution..."
    $DTE = getDTE
    if ($WhatIf)
    {
        Write-Host "Running as 'What-If'"
    }
    if ($DTE.Version.StartsWith("15") -or $DTE.Version.StartsWith("16"))
    {
        # Use the non-DTE approach for preparation for VS 2017
        VerifySolutionItemsManual

        # After cleaning up the solution/csproj files manually,
        # open and close the solution using the selected DTE,
        # just to confirm it prepped for use
        OpenSolution
        CloseSolution
    }
    else
    {
        # Use the found DTE for solution preparation
        OpenSolution
        VerifySolutionItems
        CloseSolution
    }
}

# remove message filter
[ComUtils.MessageFilter]::Revoke()

StopLogging # stop the transcript