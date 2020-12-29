using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Coordination.F09.Adapters
{
    /// <summary>
    /// Maps the Ellucian delivered PayableDepositDirective domain to dto
    /// </summary>
    // Teresa discussed an issue with me in the SS -- HumanResources/BankingInformation
    // The issue gave this error:
            // Unknown exception occurred
            // AutoMapper.AutoMapperMappingException: Missing type map configuration or unsupported mapping.
            // 
            // Mapping types:
            // Timestamp -> Timestamp
            // Ellucian.Colleague.Domain.Base.Entities.Timestamp -> Ellucian.Colleague.Dtos.Base.Timestamp
            // 
            // Destination path:
            // PayableDepositDirective.Timestamp
            // 
            // Source value:
            // Ellucian.Colleague.Domain.Base.Entities.Timestamp
            //    at AutoMapper.MappingEngine.AutoMapper.IMappingEngineRunner.Map(ResolutionContext context)
            //    at AutoMapper.Mappers.TypeMapObjectMapperRegistry.PropertyMapMappingStrategy.MapPropertyValue(ResolutionContext context, IMappingEngineRunner mapper, Object mappedObject, PropertyMap propertyMap)
            //    at AutoMapper.Mappers.TypeMapObjectMapperRegistry.PropertyMapMappingStrategy.Map(ResolutionContext context, IMappingEngineRunner mapper)
            //    at AutoMapper.Mappers.TypeMapMapper.Map(ResolutionContext context, IMappingEngineRunner mapper)
            //    at AutoMapper.MappingEngine.AutoMapper.IMappingEngineRunner.Map(ResolutionContext context)
            //    at AutoMapper.MappingEngine.Map[TSource,TDestination](TSource source)
            //    at Ellucian.Web.Adapters.AutoMapperAdapter`2.MapToType(SourceType Source)
            //    at Ellucian.Colleague.Coordination.Base.Services.PayableDepositDirectiveService.<GetPayableDepositDirectivesAsync>d__4.MoveNext() in \Ellucian.Colleague.Api\Ellucian.Colleague.Coordination.Base\Services\PayableDepositDirectiveService.cs:line 75
            // --- End of stack trace from previous location where exception was thrown ---
            //    at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
            //    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
            //    at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
            //    at Ellucian.Colleague.Api.Controllers.Base.PayableDepositDirectivesController.<GetPayableDepositDirectivesAsync>d__4.MoveNext() in \Ellucian.Colleague.Api\Ellucian.Colleague.Api\Controllers\Base\PayableDepositDirectivesController.cs:line 56
    // Upon inspection it looks like the Adapter from the domain to dto doesn't exist
    // the adapter the other way from dto to domain does exist however
    // not sure what is missing so (Roger) created this
    // it may be missing some of the features, but it at least seems to rectify the error
    [RegisterType]
    public class F09PayableDepositDirectiveAdapter : AutoMapperAdapter<
        Ellucian.Colleague.Domain.Base.Entities.PayableDepositDirective,
        Ellucian.Colleague.Dtos.Base.PayableDepositDirective>
    {
        public F09PayableDepositDirectiveAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(
            adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.BankAccountType, Dtos.Base.BankAccountType>();
            AddMappingDependency<Domain.Base.Entities.Timestamp, Dtos.Base.Timestamp>();
        }
    }
}
