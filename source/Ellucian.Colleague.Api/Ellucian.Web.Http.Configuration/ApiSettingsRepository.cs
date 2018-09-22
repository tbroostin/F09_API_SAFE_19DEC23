// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration.Transactions;
using slf4net;

namespace Ellucian.Web.Http.Configuration
{
    public class ApiSettingsRepository : BaseColleagueRepository, IApiSettingsRepository
    {
        public ApiSettingsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Returns a list of all WEB.API.CONFIG items in Colleague, by name
        /// </summary>
        /// <returns>A list of configuration names</returns>
        public IEnumerable<string> GetNames()
        {
            var selectString = "SAVING WAC.CONFIGURATION.NAME";
            var result = new string[] { };
            try
            {
                result = DataReader.Select("WEB.API.CONFIG", selectString);
            }
            catch (Exception ex)
            {
                var errorText = "Data Reader Error: " + ex.Message;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            if (result == null)
            {
                return new List<string>();
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Gets a colleague WEB.API.CONFIG record by name and returns an ApiSettings object
        /// </summary>
        /// <param name="name">Name (not Id) of the colleague WEB.API.CONFIG record</param>
        /// <returns><see cref="ApiSettings"/>ApiSettings object</returns>
        /// <remarks>
        /// Uses anonymous data reader.
        /// </remarks>
        public ApiSettings Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Name must be specified");
            }

            // Attempt to select and read from Colleague using the given name, throw errors if not found or invalid record read
            var selectString = "WITH WAC.CONFIGURATION.NAME EQ \"" + name + "\" BY @ID";
            var result = new string[] { };
            try
            {
                result = DataReader.Select("WEB.API.CONFIG", selectString);
            }
            catch (Exception ex)
            {
                var errorText = "Data Reader Error: " + ex.Message;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }

            if (result == null || result.Length == 0)
            {
                throw new ArgumentException("Configuration not found with the given name.");
            }

            var apiConfig = new DataContracts.WebApiConfig();
            try
            {
                apiConfig = DataReader.ReadRecord<DataContracts.WebApiConfig>("WEB.API.CONFIG", result[0]);
            }
            catch (Exception ex)
            {
                var errorText = "Data Reader Error: " + ex.Message;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }

            if (apiConfig == null)
            {
                throw new InvalidOperationException("Error reading the colleague api configuration record");
            }

            // Build an ApiSettings object from the WEB.API.CONFIG data read from Colleague
            var apiSettings = new ApiSettings(int.Parse(apiConfig.Recordkey), apiConfig.WacConfigurationName, int.Parse(apiConfig.WacVersion))
            {
                PhotoType = apiConfig.WacPhotoType,
                PhotoURL = apiConfig.WacPhotoUrl,
                PhotoHeaders = new Dictionary<string, string>(),
                ReportLogoPath = apiConfig.WacLogoPath,
                UnofficialWatermarkPath = apiConfig.WacUnofficlWatermarkPath,
                CacheProvider = apiConfig.WacCacheProvider
            };

            // try to retrieve Colleague time zone (defined on UI form CTZS)
            // Note that UT.PARMS/TIME.ZONE.SETTINGS and the code file TIME.ZONES need to be defined as public on WSPD.
            try
            {
                bool tzUndefined = false;
                var timeZoneSettings = DataReader.ReadRecord<DataContracts.TimeZoneSettings>("UT.PARMS", "TIME.ZONE.SETTINGS");
                if (timeZoneSettings != null && !string.IsNullOrEmpty(timeZoneSettings.ColleagueTimeZone))
                {
                    var colleagueTimeZone = DataReader.ReadRecord<DataContracts.TimeZones>(timeZoneSettings.ColleagueTimeZone);
                    if (colleagueTimeZone != null && !string.IsNullOrEmpty(colleagueTimeZone.TimeZonesStandardName))
                    {
                        try
                        {
                            // time zone is valid .Net standard zone name if FindSystemTimeZoneById() doesn't throw an exception.
                            TimeZoneInfo.FindSystemTimeZoneById(colleagueTimeZone.TimeZonesStandardName);
                            // Colleague time zone looks good. Overwrite the default time zone with it in apiSettings
                            apiSettings.ColleagueTimeZone = colleagueTimeZone.TimeZonesStandardName;
                        }
                        catch
                        {
                            logger.Warn("The time zone \"" + colleagueTimeZone.TimeZonesStandardName
                                + "\" defined in Colleague is not a valid .NET time zone. The API server's time zone \""
                                + apiSettings.ColleagueTimeZone + "\" will be used.");
                        }
                    }
                    else
                    {
                        tzUndefined = true;
                    }
                }
                else
                {
                    tzUndefined = true;
                }
                if (tzUndefined)
                {
                    logger.Warn("Time zone is not defined in Colleague (check UI form CTZS). The API server's time zone \""
                        + apiSettings.ColleagueTimeZone + "\" will be used.");
                }
            }
            catch (Exception e)
            {
                logger.Warn("Exception occurred retrieving time zone from Colleague: " + e.Message + "\n" + e.StackTrace
                    + "\nThe API server's time zone \"" + apiSettings.ColleagueTimeZone + "\" will be used.");
            }

            // Use in-process (HTTP) caching
            apiSettings.CacheProvider = ApiSettings.INPROC_CACHE;

            foreach (var header in apiConfig.WebApiConfigPhotoHdrEntityAssociation)
            {
                apiSettings.PhotoHeaders.Add(header.WacPhotoHeaderNameAssocMember, header.WacPhotoHeaderValueAssocMember);
            }

            // Get cache-related valcode entries
            apiSettings.SupportedCacheProviders = GetSupportedCacheProviders();
            apiSettings.DebugTraceLevels = GetDebugTraceLevels();

            return apiSettings;
        }

        public bool Update(ApiSettings settings)
        {
            // If this is a request for a new config record, first select to make sure the specified name does
            // not already exist
            string[] result = null;
            if (settings.Id == 0)
            {
                var selectString = "WITH WAC.CONFIGURATION.NAME EQ \"" + settings.Name + "\"";
                try
                {
                    result = DataReader.Select("WEB.API.CONFIG", selectString);
                }
                catch (Exception ex)
                {
                    var errorText = "Data Reader Error: " + ex.Message;
                    logger.Error(errorText);
                    throw new InvalidOperationException(errorText);
                }

                if (result != null && result.Length > 0)
                {
                    throw new ArgumentException("Configuration already exists with the given name. Update not attempted");
                }
            }

            // Save in-process caching to web.api.config
            settings.CacheProvider = ApiSettings.INPROC_CACHE;
            settings.CacheHost = string.Empty;
            settings.CachePort = null;

            // Build the request
            var request = new UpdateWebAPIConfigRequest()
            {
                ApiConfigId = settings.Id.ToString(),
                ConfigurationName = settings.Name,
                Version = settings.Version,
                PhotoUrl = settings.PhotoURL,
                PhotoType = settings.PhotoType,
                PhotoHeaders = new List<PhotoHeaders>(),
                LogoPath = settings.ReportLogoPath,
                UnofficialWatermarkPath = settings.UnofficialWatermarkPath,
                CacheProvider = settings.CacheProvider,
                CacheHost = settings.CacheHost,
                CachePort = settings.CachePort
            };

            if (settings.PhotoHeaders != null)
            {
                foreach (var hdr in settings.PhotoHeaders)
                {
                    var header = new PhotoHeaders();
                    header.PhotoHeaderName = hdr.Key;
                    header.PhotoHeaderValue = hdr.Value;
                    request.PhotoHeaders.Add(header);
                }
            }

            // Attempt update.
            var updateResponse = new Transactions.UpdateWebAPIConfigResponse();
            try
            {
                updateResponse = transactionInvoker.Execute<Transactions.UpdateWebAPIConfigRequest, Transactions.UpdateWebAPIConfigResponse>(request);
            }
            catch (Exception ex)
            {
                var errorText = "Transaction Invoker Execute Error: " + ex.Message;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }

            // If response is not complete or returns an unsuccessful response or returns an error message, throw an error
            if ((updateResponse == null) || !updateResponse.UpdateSuccessful || !string.IsNullOrEmpty(updateResponse.ErrorMessage))
            {
                string error = string.Empty;
                if (updateResponse == null || string.IsNullOrEmpty(updateResponse.ErrorMessage))
                {
                    error = "Web API Configuration update request did not return a successful response";
                }
                else
                {
                    error = "API Settings Repository Update error: " + updateResponse.ErrorMessage;
                }
                logger.Error(error);
                throw new InvalidOperationException(error);
            }

            // Return true if update completed without error returned.
            return true;
        }

        /// <summary>
        /// Gets the list of code/descriptions of all cache providers from Colleague.
        /// </summary>
        /// <returns></returns>
        private List<KeyValuePair<string, string>> GetSupportedCacheProviders()
        {
            List<KeyValuePair<string, string>> supportedCacheProviders = new List<KeyValuePair<string, string>>();

            ApplValcodes cacheProviders = null;
            cacheProviders = GetOrAddToCache<ApplValcodes>("CacheProviders",
                    () =>
                    {
                        string cacheProviderValcodeKey = "WEBAPI.CACHE.PROVIDERS";
                        ApplValcodes cacheProvidersValcode = DataReader.ReadRecord<ApplValcodes>("UT.VALCODES", cacheProviderValcodeKey);

                        if (cacheProvidersValcode == null)
                        {
                            var errorMessage = "Unable to access " + cacheProviderValcodeKey + " valcode table.";
                            logger.Info(errorMessage);
                            throw new Exception(errorMessage);
                        }
                        return cacheProvidersValcode;
                    }
                    );

            for (int codeIdx = 0; codeIdx < cacheProviders.ValInternalCode.Count; codeIdx++)
            {
                supportedCacheProviders.Add(new KeyValuePair<string, string>(cacheProviders.ValExternalRepresentation[codeIdx], cacheProviders.ValInternalCode[codeIdx]));
            }

            return supportedCacheProviders;
        }

        /// <summary>
        /// Gets the debug trace levels valcode from Colleague.
        /// </summary>
        /// <returns></returns>
        private List<KeyValuePair<string, TraceLevel?>> GetDebugTraceLevels()
        {
            List<KeyValuePair<string, TraceLevel?>> supportedDebugTraceLevels = new List<KeyValuePair<string, TraceLevel?>>();

            ApplValcodes debugTraceLevels = null;
            debugTraceLevels = GetOrAddToCache<ApplValcodes>("DebugTraceLevels",
                () =>
                {
                    string debugTraceLevelsValcodeKey = "DEBUG.TRACE.LEVELS";
                    ApplValcodes debugTraceLevelsValcode = DataReader.ReadRecord<ApplValcodes>("UT.VALCODES", debugTraceLevelsValcodeKey);

                    if (debugTraceLevelsValcode == null)
                    {
                        var errorMessage = "Unable to access " + debugTraceLevelsValcodeKey + " valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return debugTraceLevelsValcode;
                }
                );

            for (int codeIdx = 0; codeIdx < debugTraceLevels.ValInternalCode.Count; codeIdx++)
            {
                string traceLevelExternalRepresentation = debugTraceLevels.ValExternalRepresentation[codeIdx];
                string traceLevelInternalCode = debugTraceLevels.ValInternalCode[codeIdx];

                TraceLevel traceLevelSetting = TranslateTraceLevelFromValcode(traceLevelInternalCode);

                supportedDebugTraceLevels.Add(new KeyValuePair<string, TraceLevel?>(traceLevelExternalRepresentation, traceLevelSetting));
            }

            return supportedDebugTraceLevels;
        }

        /// <summary>
        /// Given a trace level string code from the Colleague trace level valcode, return the associated TraceLevel setting.
        /// </summary>
        /// <param name="traceLevelCodeId">The trace level code identifier from Colleague to translate.</param>
        /// <returns></returns>
        private TraceLevel TranslateTraceLevelFromValcode(string traceLevelCodeId)
        {
            TraceLevel returnTraceLevel;

            switch (traceLevelCodeId.ToUpper())
            {
                case "OFF":
                    returnTraceLevel = TraceLevel.Off;
                    break;

                case "ERROR":
                    returnTraceLevel = TraceLevel.Error;
                    break;

                case "WARNING":
                    returnTraceLevel = TraceLevel.Warning;
                    break;

                case "INFO":
                    returnTraceLevel = TraceLevel.Info;
                    break;

                case "VERBOSE":
                    returnTraceLevel = TraceLevel.Verbose;
                    break;

                default:
                    returnTraceLevel = TraceLevel.Off;
                    break;
            }

            return returnTraceLevel;
        }

        /// <summary>
        /// Given a TraceLevel setting from .NET, return the associated Colleague trace level valcode string value.
        /// </summary>
        /// <param name="traceLevel">The trace level setting to translate.</param>
        /// <returns></returns>
        private string TranslateInternalCodeFromTraceLevel(TraceLevel traceLevel)
        {
            string returnTraceLevelCode;

            switch (traceLevel)
            {
                case TraceLevel.Off:
                    returnTraceLevelCode = "OFF";
                    break;

                case TraceLevel.Error:
                    returnTraceLevelCode = "ERROR";
                    break;

                case TraceLevel.Warning:
                    returnTraceLevelCode = "WARNING";
                    break;

                case TraceLevel.Info:
                    returnTraceLevelCode = "INFO";
                    break;

                case TraceLevel.Verbose:
                    returnTraceLevelCode = "VERBOSE";
                    break;

                default:
                    returnTraceLevelCode = "OFF";
                    break;
            }

            return returnTraceLevelCode;
        }
    }
}
