/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using slf4net;
using System.Threading.Tasks;
using System.Net.Http;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// OutsideAwardsController 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class OutsideAwardsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IOutsideAwardsService outsideAwardsService;
        private readonly ILogger logger;

        /// <summary>
        /// Instantiate a new OutsideAwardsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="outsideAwardsService"></param>
        /// <param name="logger"></param>
        public OutsideAwardsController(IAdapterRegistry adapterRegistry, IOutsideAwardsService outsideAwardsService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.outsideAwardsService = outsideAwardsService;
            this.logger = logger;
        }

        /// <summary>
        /// Creates outside award record self-reported by students 
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only"
        /// </accessComments>
        /// <param name="outsideAward"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> CreateOutsideAwardAsync([FromBody]OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw CreateHttpResponseException("Create outsideAward object is required in the request body");
            }
            try
            {
                var newOutsideAward = await outsideAwardsService.CreateOutsideAwardAsync(outsideAward);
                var response = Request.CreateResponse<OutsideAward>(System.Net.HttpStatusCode.Created, newOutsideAward);
                SetResourceLocationHeader("GetOutsideAwards", new { studentId = newOutsideAward.StudentId, year = newOutsideAward.AwardYearCode });
                return response;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Input outside award is invalid. See log for details.");
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Input outside award is invalid. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to create outside award resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered while creating an outside award resource. See log for details.");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred creating OutsideAward resource. See log for details.");
            }
        }

        /// <summary>
        /// Gets all outside awards for the specified student id and award year code
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve the outside awards</param>
        /// <param name="year">award year code</param>
        /// <returns>List of OutsideAward DTOs</returns>
        [HttpGet]
        public async Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync([FromUri]string studentId, [FromUri]string year)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (string.IsNullOrEmpty(year))
            {
                throw CreateHttpResponseException("year cannot be null or empty");
            }

            IEnumerable<OutsideAward> outsideAwards;
            try
            {
                outsideAwards = await outsideAwardsService.GetOutsideAwardsAsync(studentId, year);
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Input studentId and/or year are invalid. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to retrieve outside awards. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred retrieving outside awards.");
            }
            
            return outsideAwards;
        }

        /// <summary>
        /// Deletes outside award record with specified record id
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only"
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="id">record id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteOutsideAwardAsync([FromUri]string studentId, [FromUri]string id)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId is required");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("id is required");
            }
            try
            {
                await outsideAwardsService.DeleteOutsideAwardAsync(studentId, id);
                var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
                return response;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Invalid input arguments. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to delete outside award resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to locate and delete outside award resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered while deleting outside award resource. See log for details.");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred deleting outside award resource. See log for details.");
            }
        }

        /// <summary>
        /// Updates an Outside Award from student entered information.
        /// An Outside Award is defined as an award not given to the student thru the Financial Aid office.
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only"
        /// </accessComments>
        /// <param name="outsideAward">Outside Award Entity</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateOutsideAwardAsync([FromBody]OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw CreateHttpResponseException("Update outsideAward object is required in the request body");
            }
 
            try
            {
                var updatedOutsideAward = await outsideAwardsService.UpdateOutsideAwardAsync(outsideAward);
                var response = Request.CreateResponse<OutsideAward>(System.Net.HttpStatusCode.OK, updatedOutsideAward);
                return response;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Invalid input arguments. See log for details.");
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to update outside award resource. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to locate and update outside award resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered while updating outside award resource. See log for details.");
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred updating outside award resource. See log for details.");
            }
        }

    }
}