using Microsoft.Practices.Unity;

namespace Ellucian.Web.Http.Bootstrapping
{
    public interface IModuleBootstrapper
    {
        void BootstrapModule(IUnityContainer container);
    }
}
