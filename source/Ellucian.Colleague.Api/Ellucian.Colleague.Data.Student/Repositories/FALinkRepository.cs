// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// FALinkRepository class exposes database access to Colleague Award Letters. It
    /// gathers data from numerous tables based on student data and creates AwardLetter
    /// objects.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FALinkRepository : BaseColleagueRepository, IFALinkRepository
    {
        public FALinkRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Send the FALink JSON document to Colleague for processing. Returns the output JSON document.
        /// </summary>
        /// <param name="FALinkDocument">The input FA Link document</param>
        /// <returns></returns>
        public async Task<string> PostFALinkDocumentAsync(string FALinkDocument)
        {
            try
            {
                FlnkFppInterfaceRequest request = new FlnkFppInterfaceRequest();
                request.Document = FALinkDocument;
                FlnkFppInterfaceResponse response = await transactionInvoker.ExecuteAsync<FlnkFppInterfaceRequest, FlnkFppInterfaceResponse>(request);
                return response.Document;
            }
            catch (ColleagueTransactionException e)
            {
                string message = "Error occurred executing FALink transaction.";
                logger.Error(e, message);
                throw new ApplicationException(message, e);
            }
            catch (Exception e)
            {
                string message = "Unexpected exception occurred executing FALink transaction.";
                logger.Error(e, message);
                throw new ApplicationException(message, e);
            }
        }
    }
}
