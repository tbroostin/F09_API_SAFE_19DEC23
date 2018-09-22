//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Links Controller is used to get links for the Financial Aid Homepage
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FALinkController : BaseCompressedApiController
    {
        private readonly ILogger Logger;
        private readonly IFALinkRepository FaLinkRepository;

        /// <summary>
        /// FA Link Controller constructor
        /// </summary>
        /// <param name="faLinkRepository">FA Link repository</param>
        /// <param name="logger">Logger</param>
        public FALinkController(ILogger logger, IFALinkRepository faLinkRepository)
        {
            Logger = logger;
            FaLinkRepository = faLinkRepository;

        }

        /// <summary>
        /// Send the input FA Link document to Colleague to be processed by Trimdata's CTX/subroutines.
        /// </summary>
        /// <param name="inputDocument">FA Link input Document</param>
        /// <returns>Output FA Link document</returns>
        [HttpPost]
        public async Task<FALinkDocument> PostAsync([FromBody]FALinkDocument inputDocument)
        {
            try
            {
                string outputDocString = await FaLinkRepository.PostFALinkDocumentAsync(inputDocument.Document.ToString());
                JObject outputDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(outputDocString);
                FALinkDocument returnDto = new FALinkDocument();
                returnDto.Document = outputDoc;
                return returnDto;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error occurred processing FA Link document.");
            }
        }
    }
}