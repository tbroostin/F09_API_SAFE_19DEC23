# Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
# Version 1.29.1 (Delivered with Colleague Web API 1.29.1)

# PURPOSE:
# Warm up the Colleague Web API by pre-loading the most frequently-used and shared API data
# repository caches to reduce initial load time.

# PARAMETERS:
# -webApiBaseUrl: Full URL of the Colleague Web API instance to be warmed-up.
# -userId: Colleague Web username to use when running the warm-up.
# -password: Password for the above Colleague Web username.
# -recycleAppPool: The name of the IIS application pool to be recycled prior to running
#    the warm-up script.
# -runEthosApi : parameter to run Ethos API

# EXAMPLE POWERSHELL COMMANDS:
# PS C:\scripts> .\WarmUp.ps1 "http://serverAddress:1234/ColleagueApi" "loginID" "password"
# PS C:\scripts> .\WarmUp.ps1 "http://serverAddress:1234/ColleagueApi" "loginID" "password" "applicationpoolname" -runEthosApi

# RECOMMENDED USAGE:
# The data caches maintained by Colleague Web API are not retained when its application pool
# is recycled. Accordingly, there can be a noticeable delay in the responsiveness of applications
# that make use of the Colleague Web API. This may result in a poor user experience for whoever 
# happens to access the application first. To alleviate this, IT administrators should consider 
# running this script at least once every 24 hours, during off-peak time or just after daily
# Colleague backup activities. The script can be run more frequently as well.
#
# The script can be used with or without an option that performs a recycle of the Colleague Web
# API's IIS application pool. When using the -recycleAppPool option the traditional recycling
# settings within IIS can be set to not recycle on periodic bases (no regular time interval or 
# specific times) and to never time out (idle time-out) and a scheduled run of this script with 
# the -recycleAppPool can be used instead to ensure that the application pool recycle and 
# warm-up happen at the same time rather than trying to coordinate a scheduled task just after
# a periodic application pool recycle or worse not warming-up due to an idle time-out shutting-
# down the application pool. The suggested usage of the -recycleAppPool option is to create a scheduled task that runs 
# once a day, during off-peak time or just after Colleague backup activities are finished that
# uses the -recycleAppPool option and then, optionally if you wish to run the warm-up periodically
# throughout the day, create another scheduled task that does not use the -recycleAppPool so that
# the application pool is not being recycled during the normal hours of the day.
#
# You can find when and how IIS automatically recycles the Colleague Web API application pool
# by right-clicking on the application pool in IIS Manager and choosing 'Recycling.'
#
# This script could be scheduled using the Windows Task Scheduler as described in Knowledge
# Article 000006250:
#   https://ellucian.force.com/clients/s/article/9304-Colleague-Web-API-Automated-Warm-Up

# NOTES: 
# 1. This script, as delivered by Ellucian, pre-loads some caches that are most frequently used
#    and shared. It does not pre-load all caches.
# 2. The endpoints used in the delivered script do not require special user permission/roles or
#    valid input parameters. Therefore, no modification is necessary prior to running this script.
# 3. You may add to this script more warm up requests for your own endpoints that use caching,
#    or for any other endpoints that you deem necessary to improve performance.
# 4. In order to use the -recycleAppPool option this script MUST be run from the Colleague Web API
#    host web server.

# CAUTIONS:
# 1. You should take special care to select a user ID that does not have broad access to the
#    system (i.e. a user that is assigned many self-service roles), in case the credentials are
#    somehow compromised.
# 2. When adding additional warm up requests to this script, you should ensure the endpoints used
#    and any associated parameters are protected against unauthorized access.
# 3. This file will be overwritten when you perform an upgrade installation. For that reason,
#    it is recommended that you use a separate copy of the script for your customization needs.
# 4. Use of the -recycleAppPool option should only be done during off-peak times, such as in the
#    middle of the night or just after Colleague backup activities are complete. If you run this
#    script periodically throughout the day you should not use this option during those runs.
###

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$webApiBaseUrl,
	
	[Parameter(Mandatory=$True)]
	[string]$userId,
	
	[Parameter(Mandatory=$True)]
	[string]$password,
	
	[Parameter(Mandatory=$false)]
	[string]$recycleAppPool,
	
	[Parameter(Mandatory=$false)]
	[switch]$runEthosApi
)

# Force Tls11, Tls12 support
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls12
Write-Host "Forcing Tls11, Tls12 support"

#various Accept headers
$vndV1 = "application/vnd.ellucian.v1+json"
$vndV2 = "application/vnd.ellucian.v2+json"
$vndV3 = "application/vnd.ellucian.v3+json"
$vndV4 = "application/vnd.ellucian.v4+json"
$vndV5 = "application/vnd.ellucian.v5+json"
$vndStuPlanV1 = "application/vnd.ellucian-planning-student.v1+json"
$vndStuProfV1 = "application/vnd.ellucian-person-profile.v1+json"
$vndHedtechV4 = "application/vnd.hedtech.integration.v4+json"
$vndPilotV1 = "application/vnd.ellucian-pilot.v1+json"
$vndInvoicePayV1 = "application/vnd.ellucian-invoice-payment.v1+json"
$vndIlpV1 = "application/vnd.ellucian-ilp.v1+json"
$vndHrDemoV1 = "application/vnd.ellucian-human-resource-demographics.v1+json"
$vndInstEnrV1 = "application/vnd.ellucian-instant-enrollment.v1+json"
$vndEthosConfigurationSettingsOptions = "application/vnd.hedtech.integration.configuration-settings-options.v1.0.0+json"
$vndEthosCollectionConfigurationSettingsOptions = "application/vnd.hedtech.integration.collection-configuration-settings-options.v1.0.0+json"
$vndEthosCompoundConfigurationSettingsOptions = "application/vnd.hedtech.integration.compound-configuration-settings-options.v1.0.0+json"
$vndEthosDefaultSettingsOptions = "application/vnd.hedtech.integration.default-settings-options.v1.0.0+json"
$vndEthosMappingSettingsOptions = "application/vnd.hedtech.integration.mapping-settings-options.v1.0.0+json"

#Restricted Headers list that can be set via property of System.Net.WebRequest 
$RequestHeaderList=@("Accept", "Connection", "ContentLength", "ContentType", "Date", "Expect", "Host", 
					"IfModifiedSince", "KeepAlive", "ProxyConnection", "Range", "Referer", "TransferEncoding", "UserAgent")

# Sends a GET request to the specified endpoint URL using the provided token
function get([string]$url, [string]$token, [string]$accept, [hashtable]$headers) 
{
	$req = [System.Net.WebRequest]::Create($url)
	$req.Method ="GET"
	if ($token) 
	{
		$req.Headers.Add("X-CustomCredentials", $token)
	}
	if ($accept) 
	{
		$req.Accept = $accept
	}
	$req.ContentType = "application/json"
	$req.Timeout = 300000 # max 5 minutes

	#Set the header values
	if ($headers)
	{
		Foreach($item in $headers.GetEnumerator())
		{
			# If the key is a restricted headers, set it via property of System.Net.WebRequest
			if ($RequestHeaderList -contains $item.Key)
			{
				$req.($item.Key) = $item.Value
			}
			else #Add it to Header collection of the request
			{
				$req.Headers.Add($item.Key, $item.Value)
			}
		}
	}

	$resp = $req.GetResponse()
	if ($resp)
	{	
		$reader = new-object System.IO.StreamReader($resp.GetResponseStream())
		$response = $reader.ReadToEnd().Trim()
		return $response
	}
	else
	{
		return ""
	}
}

# Sends a POST request to the specified endpoint URL using the provided token
function post([string]$url, [string]$body, [string]$token, [string]$accept, [hashtable]$headers) 
{
	$req = [System.Net.WebRequest]::Create($url)
	$req.Method ="POST"
	if ($token) 
	{
		$req.Headers.Add("X-CustomCredentials", $token)
	}
	if ($accept) 
	{
		$req.Accept = $accept
	}
	$req.ContentType = "application/json"
	$req.Timeout = 300000 # max 5 minutes
	$Post = $body
	$PostStr = [System.Text.Encoding]::UTF8.GetBytes($Post)
	$req.ContentLength = $PostStr.Length

	if ($headers)
	{
		Foreach($item in $headers.GetEnumerator())
		{
			# If the key is a restricted headers, set it via property of System.Net.WebRequest
			if ($RequestHeaderList -contains $item.Key)
			{
				$req.($item.Key) = $item.Value
			}
			else #Add it to Header collection of the request
			{
				$req.Headers.Add($item.Key, $item.Value)
			}
		}
	}

	$requestStream = $req.GetRequestStream()
	$requestStream.Write($PostStr, 0,$PostStr.length)
	$requestStream.Close()	
	$resp = $req.GetResponse()
	if ($resp)
	{	
		$reader = new-object System.IO.StreamReader($resp.GetResponseStream())
		$response = $reader.ReadToEnd().Trim()
		return $response
	}
	else
	{
		return ""
	}
}

# Recycle application pool if supplied
if (![System.String]::IsNullOrEmpty($recycleAppPool))
{
	Write-Host "Recycling application pool" $recycleAppPool
	$commandPath = [Environment]::GetEnvironmentVariable("systemroot") + "\system32\inetsrv\appcmd.exe"
	if (Test-Path $commandPath)
	{
		$command = "'" + $commandPath + "' recycle apppool " + "'" + $recycleAppPool + "'"
		iex "& $command"
	}
	else
	{
		Write-Host "Cannot recycle because appcmd.exe cannot be found at" $commandPath
	}
}

# Login
Try
{
	Write-Host "Logging in..."
	$token = post ($webApiBaseUrl + "/session/login") ("{'UserId':'" + $userId + "','Password':'" + $password + "'}")
}
Catch [Exception] 
{
	Write-Error "Login failed. Check API URL and user credentials then try again."
	Exit
}

# Send HTTP requests to "warm up" or pre-load the API caches




Write-Host "Getting courses, course sections, terms and associated valcodes..."
$results = post ($webApiBaseUrl + "/Courses/Search?pageSize=10&pageIndex=1") "{'Keywords':'math'}" $token

Write-Host "Getting faculty data..."
Try
{
	# An invalid faculty ID will cause an exception. However, that exception does not
	# mean the faculty cache did not load successfully, so it can be ignored.
	$results = get ($webApiBaseUrl + "/faculty/0000001") $token
}
Catch{}

Write-Host "Getting programs, transcript groupings, credit types..."
$results = get ($webApiBaseUrl + "/programs") $token

Write-Host "Getting academic levels..."
$results = get ($webApiBaseUrl + "/academic-levels") $token $vndV1

Write-Host "Getting CCDs..."
$results = get ($webApiBaseUrl + "/ccds") $token

Write-Host "Getting degrees..."
$results = get ($webApiBaseUrl + "/degrees") $token

Write-Host "Getting majors..."
$results = get ($webApiBaseUrl + "/majors") $token

Write-Host "Getting minors..."
$results = get ($webApiBaseUrl + "/minors") $token

Write-Host "Getting specializations..."
$results = get ($webApiBaseUrl + "/specializations") $token

Write-Host "Getting subjects..."
$results = get ($webApiBaseUrl + "/subjects") $token $vndV1

Write-Host "Getting grades..."
$results = get ($webApiBaseUrl + "/grades") $token

Write-Host "Getting course levels..."
$results = get ($webApiBaseUrl + "/course-levels") $token $vndV1

Write-Host "Getting course types..."
$results = get ($webApiBaseUrl + "/course-types") $token

Write-Host "Getting ImportantNumber records..."
$results = get ($webApiBaseUrl + "/important-numbers") $token

Write-Host "Getting rooms..."
$results = get ($webApiBaseUrl + "/rooms") $token $vndV1

Write-Host "Getting buildings..."
$results = get ($webApiBaseUrl + "/buildings") $token $vndV1

Write-Host "Getting departments..."
$results = get ($webApiBaseUrl + "/departments") $token

Write-Host "Getting locations..."
$results = get ($webApiBaseUrl + "/locations") $token

Write-Host "Getting instructional methods..."
$results = get ($webApiBaseUrl + "/instructional-methods") $token $vndV1

Write-Host "Getting races..."
$results = get ($webApiBaseUrl + "/races") $token $vndV1

Write-Host "Getting terms..."
$results = get ($webApiBaseUrl + "/terms") $token

Write-Host "Getting finance configuration..."
$results = get ($webApiBaseUrl + "/configuration") $token

# The endpoints used below are available only with Colleague Web API 1.4 and later.

Write-Host "Getting communication codes..."
$results = get ($webApiBaseUrl + "/communication-codes") $token

Write-Host "Getting convenience fees..."
$results = get ($webApiBaseUrl + "/ecommerce/convenience-fees") $token

Write-Host "Getting denominations..."
$results = get ($webApiBaseUrl + "/denominations") $token

Write-Host "Getting disability types..."
$results = get ($webApiBaseUrl + "/disability-types") $token

Write-Host "Getting interests..."
$results = get ($webApiBaseUrl + "/interests") $token $vndV1

Write-Host "Getting receivable types..."
$results = get ($webApiBaseUrl + "/receivables/receivable-types") $token

Write-Host "Getting deposit types..."
$results = get ($webApiBaseUrl + "/deposits/deposit-types") $token

Write-Host "Getting admitted statuses..."
$results = get ($webApiBaseUrl + "/admitted-statuses") $token

Write-Host "Getting application statuses..."
$results = get ($webApiBaseUrl + "/application-statuses") $token

Write-Host "Getting topic codes..."
$results = get ($webApiBaseUrl + "/topic-codes") $token

Write-Host "Getting prefixes..."
$results = get ($webApiBaseUrl + "/prefixes") $token

Write-Host "Getting suffixes..."
$results = get ($webApiBaseUrl + "/suffixes") $token

Write-Host "Getting ImportantNumber categories..."
$results = get ($webApiBaseUrl + "/important-number-categories") $token

Write-Host "Getting building types..."
$results = get ($webApiBaseUrl + "/building-types") $token

Write-Host "Getting degree types..."
$results = get ($webApiBaseUrl + "/degree-types") $token

Write-Host "Getting citizen types..."
$results = get ($webApiBaseUrl + "/citizen-types") $token

Write-Host "Getting academic-programs..."
$results = get ($webApiBaseUrl + "/academic-programs") $token $vndV1

Write-Host "Getting institution types..."
$results = get ($webApiBaseUrl + "/institution-types") $token

Write-Host "Getting languages..."
$results = get ($webApiBaseUrl + "/languages") $token

Write-Host "Getting marital-statuses..."
$results = get ($webApiBaseUrl + "/marital-statuses") $token $vndV1

Write-Host "Getting prospect sources..."
$results = get ($webApiBaseUrl + "/prospect-sources") $token

Write-Host "Getting visa-types..."
$results = get ($webApiBaseUrl + "/visa-types") $token

Write-Host "Getting application influences..."
$results = get ($webApiBaseUrl + "/application-influences") $token

Write-Host "Getting application status categories..."
$results = get ($webApiBaseUrl + "/application-status-categories") $token

Write-Host "Getting career goals..."
$results = get ($webApiBaseUrl + "/career-goals") $token

Write-Host "Getting external transcript statuses..."
$results = get ($webApiBaseUrl + "/external-transcript-statuses") $token

Write-Host "Getting student loads..."
$results = get ($webApiBaseUrl + "/student-loads") $token

Write-Host "Getting transcript categories..."
$results = get ($webApiBaseUrl + "/transcript-categories") $token

Write-Host "Getting noncourse statuses..."
$results = get ($webApiBaseUrl + "/noncourse-statuses") $token

Write-Host "Getting time management configuration..."
Try
{
	$results = get ($webApiBaseUrl + "/time-management-configuration") $token
}
Catch{}

Write-Host "Getting pay cycles..."
Try
{
	$results = get ($webApiBaseUrl + "/pay-cycles") $token
}
Catch{}
Write-Host "Getting positions..."
Try
{
	$results = get ($webApiBaseUrl + "/positions") $token
}
Catch{}

# Added Ethos Endpoints to the script

if ($runEthosApi)
{
	Write-Host "Ethos API Run Flag is set..."
	
	Write-Host "Getting Ethos academic-catalogs..."
	$results = get ($webApiBaseUrl + "/academic-catalogs") $token 

	Write-Host "Getting Ethos academic-credentials..."
	$results = get ($webApiBaseUrl + "/academic-credentials") $token

	Write-Host "Getting Ethos alternative-credential-types..."
	$results = get ($webApiBaseUrl + "/alternative-credential-types") $token

	Write-Host "Getting Ethos academic-disciplines..."
	$results = get ($webApiBaseUrl + "/academic-disciplines") $token 

	Write-Host "Getting Ethos academic-honors..."
	$results = get ($webApiBaseUrl + "/academic-honors") $token

	Write-Host "Getting Ethos academic levels..."
	$results = get ($webApiBaseUrl + "/academic-levels") $token

	Write-Host "Getting Ethos academic-period-enrollment-statuses..."
	$results = get ($webApiBaseUrl + "/academic-period-enrollment-statuses") $token 

	Write-Host "Getting Ethos academic-periods..."
	$results = get ($webApiBaseUrl + "/academic-periods") $token 

	Write-Host "Getting Ethos academic-programs..."
	$results = get ($webApiBaseUrl + "/academic-programs") $token 

	Write-Host "Getting Ethos academic-standings..."
	$results = get ($webApiBaseUrl + "/academic-standings") $token	

	Write-Host "Getting Ethos account-receivable-types..."
	$results = get ($webApiBaseUrl + "/account-receivable-types") $token

	Write-Host "Getting Ethos accounting-code-categories..."
	$results = get ($webApiBaseUrl + "/accounting-code-categories") $token
	
	Write-Host "Getting Ethos accounting-codes..."
	$results = get ($webApiBaseUrl + "/accounting-codes") $token

	Write-Host "Getting Ethos accounting-string-components..."
	$results = get ($webApiBaseUrl + "/accounting-string-components") $token

	Write-Host "Getting Ethos accounting-string-formats..."
	$results = get ($webApiBaseUrl + "/accounting-string-formats") $token

	Write-Host "Getting Ethos accounting-string-subcomponents..."
	$results = get ($webApiBaseUrl + "/accounting-string-subcomponents") $token

	Write-Host "Getting Ethos accounting-string-subcomponent-values..."
	$results = get ($webApiBaseUrl + "/accounting-string-subcomponent-values") $token

	Write-Host "Getting Ethos accounts-payable-sources..."
	$results = get ($webApiBaseUrl + "/accounts-payable-sources") $token

	Write-Host "Getting Ethos addresses..."
	$results = get ($webApiBaseUrl + "/addresses?offset=0&limit=100") $token 

	Write-Host "Getting Ethos accounting-string-component-values..."
	$results = get ($webApiBaseUrl + "/accounting-string-component-values?offset=0&limit=1") $token 
	
	Write-Host "Getting Ethos administrative-instructional-methods..."
	$results = get ($webApiBaseUrl + "/administrative-instructional-methods?offset=0&limit=1") $token 

	Write-Host "Getting Ethos administrative-periods..."
	$results = get ($webApiBaseUrl + "/administrative-periods?offset=0&limit=1") $token 

	Write-Host "Getting Ethos admission-application-supporting-items..."
	$results = get ($webApiBaseUrl + "/admission-application-supporting-items?offset=0&limit=1") $token 
	
	Write-Host "Getting Ethos address-types..."
	$results = get ($webApiBaseUrl + "/address-types") $token 

	Write-Host "Getting Ethos admission-application-sources..."
	$results = get ($webApiBaseUrl + "/admission-application-sources") $token

	Write-Host "Getting Ethos admission-application-influences..."
	$results = get ($webApiBaseUrl + "/admission-application-influences") $token

	Write-Host "Getting Ethos admission-application-status-types..."
	$results = get ($webApiBaseUrl + "/admission-application-status-types") $token

	Write-Host "Getting Ethos admission-application-supporting-item-statuses..."
	$results = get ($webApiBaseUrl + "/admission-application-supporting-item-statuses") $token 

	Write-Host "Getting Ethos admission-application-supporting-item-types..."
	$results = get ($webApiBaseUrl + "/admission-application-supporting-item-statuses") $token 

	Write-Host "Getting Ethos admission-application-types..."
	$results = get ($webApiBaseUrl + "/admission-application-types") $token 

	Write-Host "Getting Ethos admission-application-withdrawal-reasons..."
	$results = get ($webApiBaseUrl + "/admission-application-withdrawal-reasons") $token 

	Write-Host "Getting Ethos admission-decision-types..."
	$results = get ($webApiBaseUrl + "/admission-decision-types") $token

	Write-Host "Getting Ethos admission-populations..."
	$results = get ($webApiBaseUrl + "/admission-populations") $token 

	Write-Host "Getting Ethos admission-residency-types..."
	$results = get ($webApiBaseUrl + "/admission-residency-types") $token

	Write-Host "Getting Ethos advisor-types..."
	$results = get ($webApiBaseUrl + "/advisor-types") $token

	Write-Host "Getting Ethos aptitude-assessment-sources..."
	$results = get ($webApiBaseUrl + "/aptitude-assessment-sources") $token 

	Write-Host "Getting Ethos aptitude-assessment-types..."
	$results = get ($webApiBaseUrl + "/aptitude-assessment-types") $token 

	Write-Host "Getting Ethos aptitude-assessments..."
	$results = get ($webApiBaseUrl + "/aptitude-assessments") $token

	Write-Host "Getting Ethos assessment-calculation-methods..."
	$results = get ($webApiBaseUrl + "/assessment-calculation-methods") $token 

	Write-Host "Getting Ethos assessment-percentile-types..."
	$results = get ($webApiBaseUrl + "/assessment-percentile-types") $token 

	Write-Host "Getting Ethos assessment-special-circumstances..."
	$results = get ($webApiBaseUrl + "/assessment-special-circumstances") $token 

	Write-Host "Getting Ethos attendance-categories ..."
	$results = get ($webApiBaseUrl + "/attendance-categories ") $token 

	Write-Host "Getting Ethos bargaining-units..."
	$results = get ($webApiBaseUrl + "/bargaining-units") $token 

	Write-Host "Getting Ethos beneficiary-preference-types..."
	$results = get ($webApiBaseUrl + "/beneficiary-preference-types") $token

	Write-Host "Getting Ethos billing-override-reasons..."
	$results = get ($webApiBaseUrl + "/billing-override-reasons") $token

	Write-Host "Getting Ethos budget-codes..."
	$results = get ($webApiBaseUrl + "/budget-codes") $token

	Write-Host "Getting Ethos budget-phases..."
	$results = get ($webApiBaseUrl + "/budget-phases") $token

	Write-Host "Getting Ethos buildings..."
	$results = get ($webApiBaseUrl + "/buildings") $token

	Write-Host "Getting Ethos building-wings..."
	$results = get ($webApiBaseUrl + "/building-wings") $token

	Write-Host "Getting Ethos buyers..."
	$results = get ($webApiBaseUrl + "/buyers") $token

	Write-Host "Getting Ethos campus-involvement-roles..."
	$results = get ($webApiBaseUrl + "/campus-involvement-roles") $token

	Write-Host "Getting Ethos campus-organizations..."
	$results = get ($webApiBaseUrl + "/campus-organizations") $token

	Write-Host "Getting Ethos campus-organization-types..."
	$results = get ($webApiBaseUrl + "/campus-organization-types") $token 

	Write-Host "Getting Ethos career-goals..."
	$results = get ($webApiBaseUrl + "/career-goals") $token 

	Write-Host "Getting Ethos charge-assessment-methods..."
	$results = get ($webApiBaseUrl + "/charge-assessment-methods") $token 

	Write-Host "Getting Ethos cip-codes..."
	$results = get ($webApiBaseUrl + "/cip-codes") $token
	
	Write-Host "Getting Ethos citizenship-statuses..."
	$results = get ($webApiBaseUrl + "/citizenship-statuses") $token

	Write-Host "Getting Ethos collection-configuration-settings..."
	$results = get ($webApiBaseUrl + "/collection-configuration-settings") $token

	Write-Host "Getting Ethos collection-configuration-settings-options..."
	$results = get ($webApiBaseUrl + "/collection-configuration-settings") $token $vndEthosCollectionConfigurationSettingsOptions

	Write-Host "Getting Ethos comment-subject-area..."
	$results = get ($webApiBaseUrl + "/comment-subject-area") $token

	Write-Host "Getting Ethos commerce-tax-codes..."
	$results = get ($webApiBaseUrl + "/commerce-tax-codes") $token 

	Write-Host "Getting Ethos commerce-tax-code-rates..."
	$results = get ($webApiBaseUrl + "/commerce-tax-code-rates") $token 

	Write-Host "Getting Ethos commodity-codes..."
	$results = get ($webApiBaseUrl + "/commodity-codes") $token 

	Write-Host "Getting Ethos commodity-unit-types..."
	$results = get ($webApiBaseUrl + "/commodity-unit-types") $token 
	
	Write-Host "Getting Ethos compound-configuration-settings..."
	$results = get ($webApiBaseUrl + "/compound-configuration-settings") $token

	Write-Host "Getting Ethos compound-configuration-settings-options..."
	$results = get ($webApiBaseUrl + "/compound-configuration-settings") $token $vndEthosCompoundConfigurationSettingsOptions

	Write-Host "Getting Ethos configuration-settings..."
	$results = get ($webApiBaseUrl + "/configuration-settings") $token

	Write-Host "Getting Ethos configuration-settings-options..."
	$results = get ($webApiBaseUrl + "/configuration-settings") $token $vndEthosConfigurationSettingsOptions

	Write-Host "Getting Ethos contract-types..."
	$results = get ($webApiBaseUrl + "/contract-types") $token

	Write-Host "Getting Ethos cost-calculation-methods..."
	$results = get ($webApiBaseUrl + "/cost-calculation-methods") $token

	Write-Host "Getting Ethos countries..."
	$results = get ($webApiBaseUrl + "/countries") $token

	Write-Host "Getting Ethos country-iso-codes..."
	$results = get ($webApiBaseUrl + "/country-iso-codes") $token

	Write-Host "Getting Ethos course-categories..."
	$results = get ($webApiBaseUrl + "/course-categories") $token

	Write-Host "Getting Ethos course-levels..."
	$results = get ($webApiBaseUrl + "/course-levels") $token

	Write-Host "Getting Ethos course-statuses..."
	$results = get ($webApiBaseUrl + "/course-statuses") $token

	Write-Host "Getting Ethos course-title-types..."
	$results = get ($webApiBaseUrl + "/course-title-types") $token

	Write-Host "Getting Ethos course-topics..."
	$results = get ($webApiBaseUrl + "/course-topics") $token

	Write-Host "Getting Ethos course-transfer-statuses..."
	$results = get ($webApiBaseUrl + "/course-transfer-statuses") $token

	Write-Host "Getting Ethos courses..."
	$results = get ($webApiBaseUrl + "/courses") $token

	Write-Host "Getting Ethos credit categories..."
	$results = get ($webApiBaseUrl + "/credit-categories") $token

	Write-Host "Getting Ethos currencies..."
	$results = get ($webApiBaseUrl + "/currencies") $token

	Write-Host "Getting Ethos currency-iso-codes..."
	$results = get ($webApiBaseUrl + "/currency-iso-codes") $token

	Write-Host "Getting Ethos deduction-categories..."
	$results = get ($webApiBaseUrl + "/deduction-categories") $token	
	
	Write-Host "Getting Ethos deduction-types..."
	$results = get ($webApiBaseUrl + "/deduction-types") $token

	Write-Host "Getting Ethos default-settings..."
	$results = get ($webApiBaseUrl + "/default-settings") $token

	Write-Host "Getting Ethos default-settings-options..."
	$results = get ($webApiBaseUrl + "/default-settings") $token $vndEthosDefaultSettingsOptions

	Write-Host "Getting Ethos earning-types..."
	$results = get ($webApiBaseUrl + "/earning-types") $token

	Write-Host "Getting Ethos educational-goals..."
	$results = get ($webApiBaseUrl + "/educational-goals") $token
	
	Write-Host "Getting Ethos educational-institution-units..."
	$results = get ($webApiBaseUrl + "/educational-institution-units") $token

	Write-Host "Getting Ethos email-types..."
	$results = get ($webApiBaseUrl + "/email-types") $token 

	Write-Host "Getting Ethos emergency-contact-types..."
	$results = get ($webApiBaseUrl + "/emergency-contact-types?offset=0&limit=1") $token

	Write-Host "Getting Ethos emergency-contact-phone-availabilities..."
	$results = get ($webApiBaseUrl + "/emergency-contact-phone-availabilities?offset=0&limit=1") $token

	Write-Host "Getting Ethos employee-leave-plans..."
	$results = get ($webApiBaseUrl + "/employee-leave-plans?offset=0&limit=1") $token

	Write-Host "Getting Ethos employment-classifications..."
	$results = get ($webApiBaseUrl + "/employment-classifications") $token 

	Write-Host "Getting Ethos employment-departments..."
	$results = get ($webApiBaseUrl + "/employment-departments") $token 

	Write-Host "Getting Ethos employment-frequencies..."
	$results = get ($webApiBaseUrl + "/employment-frequencies") $token 

	Write-Host "Getting Ethos employment-leave-of-absence-reasons..."
	$results = get ($webApiBaseUrl + "/employment-leave-of-absence-reasons") $token

	Write-Host "Getting Ethos employment-performance-review-ratings..."
	$results = get ($webApiBaseUrl + "/employment-performance-review-ratings") $token 

	Write-Host "Getting Ethos employment-performance-review-types..."
	$results = get ($webApiBaseUrl + "/employment-performance-review-types") $token 

	Write-Host "Getting Ethos employment-proficiencies..."
	$results = get ($webApiBaseUrl + "/employment-proficiencies") $token 

	Write-Host "Getting Ethos employment-termination-reasons..."
	$results = get ($webApiBaseUrl + "/employment-termination-reasons") $token

	Write-Host "Getting Ethos employment-vocations..."
	$results = get ($webApiBaseUrl + "/employment-vocations") $token

	Write-Host "Getting Ethos enrollment-statuses..."
	$results = get ($webApiBaseUrl + "/enrollment-statuses") $token	

	Write-Host "Getting Ethos ethnicities..."
	$results = get ($webApiBaseUrl + "/ethnicities") $token

	Write-Host "Getting Ethos external-employment-positions..."
	$results = get ($webApiBaseUrl + "/external-employment-positions") $token

	Write-Host "Getting Ethos external-employment-statuses..."
	$results = get ($webApiBaseUrl + "/external-employment-statuses") $token

	Write-Host "Getting Ethos financial-aid-award-periods..."
	$results = get ($webApiBaseUrl + "/financial-aid-award-periods") $token

	Write-Host "Getting Ethos financial-aid-funds..."
	$results = get ($webApiBaseUrl + "/financial-aid-funds") $token

	Write-Host "Getting Ethos financial-aid-fund-categories..."
	$results = get ($webApiBaseUrl + "/financial-aid-fund-categories") $token

	Write-Host "Getting Ethos financial-aid-fund-classifications..."
	$results = get ($webApiBaseUrl + "/financial-aid-fund-classifications") $token

	Write-Host "Getting Ethos financial-aid-offices..."
	$results = get ($webApiBaseUrl + "/financial-aid-offices") $token

	Write-Host "Getting Ethos financial-aid-years..."
	$results = get ($webApiBaseUrl + "/financial-aid-years") $token

	Write-Host "Getting Ethos financial-document-types..."
	$results = get ($webApiBaseUrl + "/financial-document-types") $token

	Write-Host "Getting Ethos fiscal-years..."
	$results = get ($webApiBaseUrl + "/fiscal-years") $token

	Write-Host "Getting Ethos fiscal-periods..."
	$results = get ($webApiBaseUrl + "/fiscal-periods") $token

	Write-Host "Getting Ethos fixed-asset-categories..."
	$results = get ($webApiBaseUrl + "/fixed-asset-categories") $token

	Write-Host "Getting Ethos fixed-asset-designations..."
	$results = get ($webApiBaseUrl + "/fixed-asset-designations") $token
		
	Write-Host "Getting Ethos fixed-asset-types..."
	$results = get ($webApiBaseUrl + "/fixed-asset-types") $token

	Write-Host "Getting Ethos floor-characteristics..."
	$results = get ($webApiBaseUrl + "/floor-characteristics") $token

	Write-Host "Getting Ethos free-on-board-types..."
	$results = get ($webApiBaseUrl + "/free-on-board-types") $token

	Write-Host "Getting Ethos geographic-areas..."
	$results = get ($webApiBaseUrl + "/geographic-areas") $token 

	Write-Host "Getting Ethos geographic-area-types..."
	$results = get ($webApiBaseUrl + "/geographic-area-types") $token 

	Write-Host "Getting Ethos grade-change-reasons..."
	$results = get ($webApiBaseUrl + "/grade-change-reasons") $token 

	Write-Host "Getting Ethos grade-definitions..."
	$results = get ($webApiBaseUrl + "/grade-definitions") $token

	Write-Host "Getting Ethos grade schemes..."
	$results = get ($webApiBaseUrl + "/grade-schemes") $token

	Write-Host "Getting Ethos grants..."
	$results = get ($webApiBaseUrl + "/grants") $token

	Write-Host "Getting Ethos gender-identities..."
	$results = get ($webApiBaseUrl + "/gender-identities") $token

	Write-Host "Getting Ethos housing-resident-types..."
	$results = get ($webApiBaseUrl + "/housing-resident-types") $token

	Write-Host "Getting Ethos identity-document-types..."
	$results = get ($webApiBaseUrl + "/identity-document-types") $token 

	Write-Host "Getting Ethos instructional methods..."
	$results = get ($webApiBaseUrl + "/instructional-methods") $token

	Write-Host "Getting Ethos instructional-platforms..."
	$results = get ($webApiBaseUrl + "/instructional-platforms") $token 

	Write-Host "Getting Ethos instructor-categories..."
	$results = get ($webApiBaseUrl + "/instructor-categories") $token

	Write-Host "Getting Ethos instructor-staff-types..."
	$results = get ($webApiBaseUrl + "/instructor-staff-types") $token

	Write-Host "Getting Ethos instructor-tenure-types..."
	$results = get ($webApiBaseUrl + "/instructor-tenure-types") $token

	Write-Host "Getting Ethos interest-areas..."
	$results = get ($webApiBaseUrl + "/interest-areas") $token 

	Write-Host "Getting Ethos interests..."
	$results = get ($webApiBaseUrl + "/interests") $token

	Write-Host "Getting Ethos job-change-reasons..."
	$results = get ($webApiBaseUrl + "/job-change-reasons") $token

	Write-Host "Getting Ethos language-iso-codes..."
	$results = get ($webApiBaseUrl + "/language-iso-codes") $token

	Write-Host "Getting Ethos languages..."
	$results = get ($webApiBaseUrl + "/languages") $token

	Write-Host "Getting Ethos leave-plans..."
	$results = get ($webApiBaseUrl + "/leave-plans") $token

	Write-Host "Getting Ethos leave-types..."
	$results = get ($webApiBaseUrl + "/leave-types") $token

	Write-Host "Getting Ethos mapping-settings..."
	$results = get ($webApiBaseUrl + "/mapping-settings") $token

	Write-Host "Getting Ethos mapping-settings-options..."
	$results = get ($webApiBaseUrl + "/mapping-settings") $token $vndEthosMappingSettingsOptions

	Write-Host "Getting Ethos marital-statuses..."
	$results = get ($webApiBaseUrl + "/marital-statuses") $token

	Write-Host "Getting Ethos meal-plans..."
	$results = get ($webApiBaseUrl + "/meal-plans") $token

	Write-Host "Getting Ethos meal-plan-rates..."
	$results = get ($webApiBaseUrl + "/meal-plan-rates") $token

	Write-Host "Getting Ethos meal-types..."
	$results = get ($webApiBaseUrl + "/meal-types") $token

	Write-Host "Getting Ethos pay-classess..."
	$results = get ($webApiBaseUrl + "/pay-classes") $token

	Write-Host "Getting Ethos pay-cycles..."
	$results = get ($webApiBaseUrl + "/pay-cycles") $token

	Write-Host "Getting Ethos pay-classifications..."
	$results = get ($webApiBaseUrl + "/pay-classifications") $token

	Write-Host "Getting Ethos pay-periods..."
	$results = get ($webApiBaseUrl + "/pay-periods") $token

	Write-Host "Getting Ethos pay-scales..."
	$results = get ($webApiBaseUrl + "/pay-scales") $token

	Write-Host "Getting Ethos payroll-deduction-arrangement-change-reasons..."
	$results = get ($webApiBaseUrl + "/payroll-deduction-arrangement-change-reasons") $token

	Write-Host "Getting Ethos personal-pronouns..."
	$results = get ($webApiBaseUrl + "/personal-pronouns") $token

	Write-Host "Getting Ethos personal-relationship-statuses..."
	$results = get ($webApiBaseUrl + "/personal-relationship-statuses") $token

	Write-Host "Getting Ethos personal-relationship-types..."
	$results = get ($webApiBaseUrl + "/personal-relationship-types") $token 

	Write-Host "Getting Ethos person-filters..."
	$results = get ($webApiBaseUrl + "/person-filters") $token 

	Write-Host "Getting Ethos person-hold-types..."
	$results = get ($webApiBaseUrl + "/person-hold-types") $token 

	Write-Host "Getting Ethos person-name-types..."
	$results = get ($webApiBaseUrl + "/person-name-types") $token 

	Write-Host "Getting Ethos person-sources..."
	$results = get ($webApiBaseUrl + "/person-sources") $token 

	Write-Host "Getting Ethos phone-types..."
	$results = get ($webApiBaseUrl + "/phone-types") $token 

	Write-Host "Getting Ethos privacy-statuses..."
	$results = get ($webApiBaseUrl + "/privacy-statuses") $token

	Write-Host "Getting Ethos position-classifications..."
	$results = get ($webApiBaseUrl + "/position-classifications") $token

	Write-Host "Getting Ethos races..."
	$results = get ($webApiBaseUrl + "/races") $token

	Write-Host "Getting Ethos rehire-types..."
	$results = get ($webApiBaseUrl + "/rehire-types") $token 

	Write-Host "Getting Ethos relationship-statuses..."
	$results = get ($webApiBaseUrl + "/relationship-statuses") $token 

	Write-Host "Getting Ethos relationship-types..."
	$results = get ($webApiBaseUrl + "/relationship-types") $token 

	Write-Host "Getting Ethos religions..."
	$results = get ($webApiBaseUrl + "/religions") $token

	Write-Host "Getting Ethos residency-types..."
	$results = get ($webApiBaseUrl + "/residency-types") $token 

	Write-Host "Getting Ethos resources..."
	$results = get ($webApiBaseUrl + "/resources") $token 

	Write-Host "Getting Ethos room-characteristics..."
	$results = get ($webApiBaseUrl + "/room-characteristics") $token

	Write-Host "Getting Ethos room-rates..."
	$results = get ($webApiBaseUrl + "/room-rates") $token

	Write-Host "Getting Ethos room-types..."
	$results = get ($webApiBaseUrl + "/room-types") $token 

	Write-Host "Getting Ethos roommate-characteristics..."
	$results = get ($webApiBaseUrl + "/roommate-characteristics") $token

	Write-Host "Getting Ethos rooms..."
	$results = get ($webApiBaseUrl + "/rooms") $token

	Write-Host "Getting Ethos section-description-types..."
	$results = get ($webApiBaseUrl + "/section-description-types") $token 
	
	Write-Host "Getting Ethos section-grade-types..."
	$results = get ($webApiBaseUrl + "/section-grade-types") $token 

	Write-Host "Getting Ethos section-registration-statuses..."
	$results = get ($webApiBaseUrl + "/section-registration-statuses") $token 

	Write-Host "Getting Ethos section-statuses..."
	$results = get ($webApiBaseUrl + "/section-statuses") $token 

	Write-Host "Getting Ethos section-title-types..."
	$results = get ($webApiBaseUrl + "/section-title-types") $token

	Write-Host "Getting Ethos sections..."
	$results = get ($webApiBaseUrl + "/sections") $token

	Write-Host "Getting Ethos ship-to-destinations..."
	$results = get ($webApiBaseUrl + "/ship-to-destinations") $token

	Write-Host "Getting Ethos shipping-methods..."
	$results = get ($webApiBaseUrl + "/shipping-methods") $token

	Write-Host "Getting Ethos sites..."
	$results = get ($webApiBaseUrl + "/sites") $token

	Write-Host "Getting Ethos social-media-types..."
	$results = get ($webApiBaseUrl + "/social-media-types") $token 

	Write-Host "Getting Ethos sources..."
	$results = get ($webApiBaseUrl + "/sources") $token 

	Write-Host "Getting Ethos source-contexts..."
	$results = get ($webApiBaseUrl + "/source-contexts") $token 

	Write-Host "Getting Ethos student-classifications..."
	$results = get ($webApiBaseUrl + "/student-classifications") $token 

	Write-Host "Getting Ethos student-cohorts..."
	$results = get ($webApiBaseUrl + "/student-cohorts") $token
	
	Write-Host "Getting Ethos student-course-transfers..."
	$results = get ($webApiBaseUrl + "/student-course-transfers?offset=0&limit=1") $token

	Write-Host "Getting Ethos student-residential-categories..."
	$results = get ($webApiBaseUrl + "/student-residential-categories") $token

	Write-Host "Getting Ethos student-statuses..."
	$results = get ($webApiBaseUrl + "/student-statuses") $token 

	Write-Host "Getting Ethos student-types..."
	$results = get ($webApiBaseUrl + "/student-types") $token 

	Write-Host "Getting Ethos subjects..."
	$results = get ($webApiBaseUrl + "/subjects") $token

	Write-Host "Getting Ethos visa-types..."
	$results = get ($webApiBaseUrl + "/visa-types") $token 

	Write-Host "Getting Ethos vendor-classifications..."
	$results = get ($webApiBaseUrl + "/vendor-classifications") $token 

	Write-Host "Getting Ethos vendor-hold-reasons..."
	$results = get ($webApiBaseUrl + "/vendor-hold-reasons") $token 

	Write-Host "Getting Ethos vendor-payment-terms..."
	$results = get ($webApiBaseUrl + "/vendor-payment-terms") $token

	Write-Host "Getting Ethos veteran-statuses..."
	$results = get ($webApiBaseUrl + "/veteran-statuses") $token 

	Write-Host "Getting Ethos financial-aid-academic-progress-types..."
	$results = get ($webApiBaseUrl + "/financial-aid-academic-progress-types") $token

	Write-Host "Getting Ethos financial-aid-academic-progress-statuses..."
	$results = get ($webApiBaseUrl + "/financial-aid-academic-progress-statuses") $token
}

# The endpoint used below is available only with Colleague Web API 1.5 and later.

Write-Host "Getting payment plan templates..."
$results = get ($webApiBaseUrl + "/payment-plans/templates") $token


# The endpoint used below is available only with Colleague Web API 1.6 and later.

Write-Host "Getting project data..."
Try
{
	# An invalid project ID will cause an exception. However, that exception does not
	# mean the GL configuration cache did not load successfully, so it can be ignored.
	$results = get ($webApiBaseUrl + "/projects/0") $token
}
Catch{}

Write-Host "Getting financial aid external hyperlinks..."
$results = get ($webApiBaseUrl + "/financial-aid-links") $token

Write-Host "Getting financial aid offices..."
Try
{	
	# No FA.SYS.PARAM record will cause an exception. We are logging a message
	# to notify users it cannot be accessed. If a client does not use FA, this can be ingnored
	$results = get ($webApiBaseUrl + "/financial-aid-offices") $token $vndV1
}
Catch{}

Write-Host "Getting award year definitions..."
$results = get ($webApiBaseUrl + "/award-years") $token

Write-Host "Getting awards..."
$results = get ($webApiBaseUrl + "/awards") $token

Write-Host "Getting award statuses..."
$results = get ($webApiBaseUrl + "/award-statuses") $token

Write-Host "Getting award periods..."
$results = get ($webApiBaseUrl + "/award-periods") $token

Write-Host "Getting ipeds institutions..."
try
{
	$results = post ($webApiBaseUrl + "/ipeds-institutions") "['00000001']" $token
}
Catch{}

Write-Host "Getting restriction-types..."
$results = get ($webApiBaseUrl + "/restriction-types") $token

# The endpoints used below are available only with Colleague Web API 1.7 and later.

Write-Host "Getting institutions..."
$results = get ($webApiBaseUrl + "/institutions") $token

Write-Host "Getting financial aid budget components..."
$results = get ($webApiBaseUrl + "/financial-aid-budget-components") $token

Write-Host "Getting financial aid checklist items..."
$results = get ($webApiBaseUrl + "/financial-aid-checklist-items") $token

# The endpoints used below are available only with Colleague Web API 1.8 and later.

Write-Host "Getting academic progress status items..."
$results = get ($webApiBaseUrl + "/academic-progress-statuses") $token

Write-Host "Getting petition-statuses..."
$results = get ($webApiBaseUrl + "/petition-statuses") $token

Write-Host "Getting countries..."
$results = get ($webApiBaseUrl + "/countries") $token

Write-Host "Getting states..."
$results = get ($webApiBaseUrl + "/states") $token

# The endpoints used below are available only with Colleague Web API 1.10 and later.

Write-Host "Getting relationship types..."
$results = get ($webApiBaseUrl + "/relationship-types") $token

# The endpoints used below are available only with Colleague Web API 1.11 and later.

try
{
	Write-Host "Getting banks..."
	$results = get ($webApiBaseUrl + "/banks/211691185") $token
}
catch {}

Write-Host "Getting banking information configuration..."
$results = get ($webApiBaseUrl + "/banking-information-configuration") $token

# The endpoints used below are available only with Colleague Web API 1.14 and later.

Write-Host "Getting General Ledger Major Components..."
$results = get ($webApiBaseUrl + "/configuration/general-ledger") $token


# The endpoints used below are available only with Colleague Web API 1.15 and later.

# The endpoints used below are available only with Colleague Web API 1.17 and later.


Write-Host "Getting Pay Statement Configuration..."
$results = get ($webApiBaseUrl + "/pay-statement-configuration") $token;

try
{
	
	$year = Get-Date -Format "yyyy"
	$startDate = Get-Date -Year ($year - 1) -Month 1 -Day 1 -Hour 0 -Minute 0 -Second 0 -Millisecond 0
	Write-Host "Building PayStatements cache for non-conseting employees only, starting on " + $startDate
	$results = get ($webApiBaseUrl + "/pay-statements?hasOnlineConsent=False&startDate="+$startDate) $token
}
catch {}

# The endpoints used below are available only with Colleague Web API 1.18 and later.

Write-Host "Getting nonacademic attendance event types..."
$results = get ($webApiBaseUrl + "/nonacademic-attendance-event-types") $token

# The endpoints used below are available only with Colleague Web API 1.20 and later.
Write-Host "Getting earnings type groups..."
$results = get ($webApiBaseUrl + "/earnings-type-groups") $token

Write-Host "Getting campus calendars..."
$results = get ($webApiBaseUrl + "/campus-calendars") $token

# The endpoints used below are available only with Colleague Web API 1.25 and later.
Write-Host "Getting agreement periods..."
$results = get ($webApiBaseUrl + "/agreement-periods") $token

# The endpoints used below are available only with Colleague Web API 1.28 and later.
Write-Host "Getting instant enrollment course sections..."
$results = post ($webApiBaseUrl + "/Courses/Search?pageSize=10&pageIndex=1") "{'Keywords':'math'}" $token $vndInstEnrV1


# Add additional warm up requests for your custom APIs below.

# Logout
Write-Host "Logging out..."
post ($webApiBaseUrl + "/session/logout") "" $token
Write-Host "Done."
