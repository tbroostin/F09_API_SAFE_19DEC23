using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestCdmIntegrationRepository
    {
        private static Collection<CdmIntegration> _cdmIntegrationResponses = new Collection<CdmIntegration>();
        
        public static Collection<CdmIntegration> CdmIntegrationResponses
        {
            get
            {
                if (_cdmIntegrationResponses.Count == 0)
                {
                    GenerateCdmIntegrationDataContracts();
                }
                return _cdmIntegrationResponses;
            }
        }

        /// <summary>
        /// Performs data setup for international parameters to be used in tests
        /// </summary>
        private static void GenerateCdmIntegrationDataContracts()
        {
            string[,] _cdmIntegrationData = GetCdmIntegrationData();
            int cdmIntegrationCount = _cdmIntegrationData.Length / 23;
            for (int i = 0; i < cdmIntegrationCount; i++)
            {
                // Parse out the data
                string id = _cdmIntegrationData[i, 0].Trim();
                string baseUrl = _cdmIntegrationData[i, 1].Trim();
                string amqpUsername = _cdmIntegrationData[i, 2].Trim();
                string amqpPassword = _cdmIntegrationData[i, 3].Trim();
                string exchangeName = _cdmIntegrationData[i, 4].Trim();
                string exchangeQueue = _cdmIntegrationData[i, 5].Trim();
                string outboundExchange = _cdmIntegrationData[i, 6].Trim();
                string inboundExchange = _cdmIntegrationData[i, 7].Trim();
                string inboundQueue = _cdmIntegrationData[i, 8].Trim();
                string apiUsername = _cdmIntegrationData[i, 9].Trim();
                string apiPassword = _cdmIntegrationData[i, 10].Trim();
                string apiErpName = _cdmIntegrationData[i, 11].Trim();
                string virtualHost = _cdmIntegrationData[i, 12].Trim();
                int timeout = int.Parse(_cdmIntegrationData[i, 13].Trim());
                string autoRecover = _cdmIntegrationData[i, 14].Trim();
                int heartbeat = int.Parse(_cdmIntegrationData[i, 15].Trim());
                //bool useIntegrationHub = bool.Parse(_cdmIntegrationData[i, 16].Trim());
                string useIntegrationHub = _cdmIntegrationData[i, 16].Trim();
                string apiKey = _cdmIntegrationData[i, 17].Trim();
                string tokenUrl = _cdmIntegrationData[i, 18].Trim();
                string publishUrl = _cdmIntegrationData[i, 19].Trim();
                string subscribeUrl = _cdmIntegrationData[i, 20].Trim();
                string errorUrl = _cdmIntegrationData[i, 21].Trim();
                string mediaType = _cdmIntegrationData[i, 22].Trim();

                CdmIntegration resp = new CdmIntegration()
                {
                    Recordkey = id,
                    CintServerBaseUrl = baseUrl,
                    CintServerUsername = amqpUsername,
                    CintServerPassword = amqpPassword,
                    CintBusEventExchange = exchangeName,
                    CintBusEventQueue = exchangeQueue,
                    CintOutboundExchange = outboundExchange,
                    CintInboundExchange = inboundExchange,
                    CintInboundQueue = inboundQueue,
                    CintApiUsername = apiUsername,
                    CintApiPassword = apiPassword,
                    CintApiErp = apiErpName,
                    CintServerVirtHost = virtualHost,
                    CintServerTimeout = timeout,
                    //CintServerAutorecoverFlag = autoRecover == "Y",
                    CintServerAutorecoverFlag = autoRecover,
                    CintServerHeartbeat = heartbeat,
                    CintUseIntegrationHub = useIntegrationHub,
                    CintHubApiKey = apiKey,
                    CintHubTokenUrl = tokenUrl,
                    CintHubPublishUrl = publishUrl,
                    CintHubSubscribeUrl = subscribeUrl,
                    CintHubErrorUrl = errorUrl,
                    CintHubMediaType = mediaType

                };
                resp.ApiBusEventMapEntityAssociation = new List<CdmIntegrationApiBusEventMap>();

                if (resp.Recordkey == "TEST2")
                {
                    string[,] _cdmIntegrationMappingData = GetCdmIntegrationMappingData();
                    int cdmIntegrationMappingCount = _cdmIntegrationMappingData.Length / 4;
                    for (int j = 0; j < cdmIntegrationMappingCount; j++)
                    {
                        string resourceName = _cdmIntegrationMappingData[j, 0].Trim();
                        string version = _cdmIntegrationMappingData[j, 1].Trim();
                        string path = _cdmIntegrationMappingData[j, 2].Trim();
                        string eventName = _cdmIntegrationMappingData[j, 3].Trim();
                        resp.ApiBusEventMapEntityAssociation.Add(new CdmIntegrationApiBusEventMap()
                        {
                            CintApiResourceAssocMember = resourceName,
                            CintApiRsrcSchemaSemVerAssocMember = version,
                            CintApiPathAssocMember = path,
                            CintApiBusEventsAssocMember = eventName
                        });
                    }

                    resp.CintInboundRoutingKeys = new List<string>() { "abc", "def" };
                    resp.CintBusEventRoutingKeys = new List<string>() { "123", "456" };
                }
                _cdmIntegrationResponses.Add(resp);
            }
        }

        /// <summary>
        /// Gets CDM integration raw data
        /// </summary>
        /// <returns>String array of cdm integration data</returns>
        private static string[,] GetCdmIntegrationData()
        {
            string[,] cdmIntegrationData =   {   //ID                  Date Format    Delimiter
                                                    {"TEST", "http://www.sdw2k3app1:7575", "amqpAdmin", "amqpPassword", "BusinessEventExchange", "EventQueue", "Outbound", "Inbound", "InboundQueue", "apiAdmin", "apiPassword", "Colleague", "http://www.amazon.com", "30", "Y", "60" , "Y", "APIKEY1", "http://server:80", "http://server:801", "http://server:802", "http://server:803","application/vnd.hedtech.change-notifications.v2+json"},
                                                    {"TEST2", "https://www.sdw2k3app1:7576", "amqpAdmin2", "amqpPassword2", "BusinessEventExchange2", "EventQueue2", "Outbound2", "Inbound2", "InboundQueue2", "apiAdmin2", "apiPassword2", "Colleague", "http://www.amazon.com", "30", "N", "60", "N", "APIKEY2", "http://server:800", "http://server:8001", "http://server:8002", "http://server:8003", "application/vnd.hedtech.change-notifications.v2+json"}
                                             };
            return cdmIntegrationData;
        }

        /// <summary>
        /// Gets CDM integration mapping raw data
        /// </summary>
        /// <returns>String array of cdm integration mapping data</returns>
        private static string[,] GetCdmIntegrationMappingData()
        {
            string[,] cdmIntegrationMappingData =  {
                                                       {"course","1","/courses/","CDM-COURSES"},
                                                       {"person","16.0.0","/person/","CDM-PERSON"}
                                                   };
            return cdmIntegrationMappingData;
        }
    }
}
