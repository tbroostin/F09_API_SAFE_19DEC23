//Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Http.Bootstrapping;
using Microsoft.Practices.Unity;

namespace Ellucian.Colleague.Data.FinancialAid
{
    /// <summary>
    /// Perform any IUnityContainer setup necessary for the financial aid data module.
    /// 
    /// Called (by implementing IModuleBootstrapper) within the main Bootstrapper.cs IUnityContainer
    /// container creation code.
    /// </summary>
    public class FinancialAidModuleBootstrapper : IModuleBootstrapper
    {
        public void BootstrapModule(IUnityContainer container)
        {
            //add rule adapters
            RuleAdapterRegistry rar = container.Resolve<RuleAdapterRegistry>();
            rar.Register<StudentAwardYear>("CS.ACYR", new StudentAwardYearNeedsAnalysisRuleAdapter());
        }
    }
}
