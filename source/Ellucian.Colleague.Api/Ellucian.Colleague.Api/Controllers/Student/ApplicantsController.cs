﻿// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// This controller exposes Colleague Applicant data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ApplicantsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IApplicantService applicantService;
        private readonly ILogger logger;


        /// <summary>
        /// Create an ApplicantsController object
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="applicantService">ApplicantService</param>
        /// <param name="logger">Logger</param>
        public ApplicantsController(IAdapterRegistry adapterRegistry, IApplicantService applicantService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.applicantService = applicantService;
            this.logger = logger;
        }

        /// <summary>
        /// Get an Applicant by id
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <returns>An Applicant DTO</returns>
        public async Task<Applicant> GetApplicantAsync(string applicantId)
        {
            if (string.IsNullOrEmpty(applicantId))
            {
                throw CreateHttpResponseException("applicantId cannot be null or empty");
            }
            try
            {
                return await applicantService.GetApplicantAsync(applicantId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to Applicant resource is forbidden. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("Applicant", applicantId);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting Applicant resource. See log for details");
            }
        }



    }
}