using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Web.Http.Bootstrapping;
using Microsoft.Practices.Unity;

namespace Ellucian.Colleague.Data.Finance
{
    public class FinanceModuleBootstrapper : IModuleBootstrapper
    {
        public void BootstrapModule(IUnityContainer container)
        {
            // add rule adapters
            RuleAdapterRegistry rar = container.Resolve<RuleAdapterRegistry>();
            rar.Register<AccountHolder>("PERSON.AR", new AccountHolderRuleAdapter());
            rar.Register<Invoice>("AR.INVOICES", new InvoiceRuleAdapter());
        }
    }
}
