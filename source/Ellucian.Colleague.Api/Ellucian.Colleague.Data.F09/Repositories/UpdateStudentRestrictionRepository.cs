using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.F09.Transactions;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    [RegisterType]
    class UpdateStudentRestrictionRepository : BaseColleagueRepository, IUpdateStudentRestrictionRepository
    {
        public UpdateStudentRestrictionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<UpdateStudentRestrictionResponse> GetStudentRestrictionAsync(string personId)
        {
            var request = new ctxUpdateStuRestrictionRequest();
            request.Id = personId;
            //request.RequestType = "GET";

            UpdateStudentRestrictionResponse profile;

            try
            {
                ctxUpdateStuRestrictionResponse response = await transactionInvoker.ExecuteAsync<ctxUpdateStuRestrictionRequest, ctxUpdateStuRestrictionResponse>(request);
                profile = this.CreateStudentRestrictionObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetStudentRestrictionAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetStudentRestrictionAsync': " + String.Join("\n", ex.Message));
            }

            return profile;
        }

        public async Task<UpdateStudentRestrictionResponse> UpdateStudentRestrictionAsync(UpdateStudentRestrictionRequest studentRequest)
        {
            var request = new ctxUpdateStuRestrictionRequest();
            request.Id = studentRequest.Id;
            request.Restriction = studentRequest.Restriction;
            request.Action = studentRequest.Action;
            request.StartDate = studentRequest.StartDate;
            request.EndDate = studentRequest.EndDate;
            request.Comments = studentRequest.Comments;
            request.Options = studentRequest.Options;

            UpdateStudentRestrictionResponse profile;

            try
            {
                ctxUpdateStuRestrictionResponse response = await transactionInvoker.ExecuteAsync<ctxUpdateStuRestrictionRequest, ctxUpdateStuRestrictionResponse>(request);
                profile = this.CreateStudentRestrictionObject(response);
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateStudentRestrictionAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateStudentRestrictionAsync': " + String.Join("\n", ex.Message));
            }

            return profile;
        }

        private UpdateStudentRestrictionResponse CreateStudentRestrictionObject(ctxUpdateStuRestrictionResponse response)
        {
            UpdateStudentRestrictionResponse student = new UpdateStudentRestrictionResponse();
            student.Error = response.Error;
            student.Message = response.Msg;

            return student;
        }
    }
}
