using System.Web;
using System.Web.Optimization;

namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// A central place where all Script and Style bundles shall be registered.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Allows the application to register any resources that should be bundled.
        /// </summary>
        /// <param name="bundles">the bundle collection to which bundles should be added</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.min.css",
                        "~/Content/themes/base/jquery-ui.structure.min.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new ScriptBundle("~/bundles/globalscripts").Include(
                        "~/Scripts/knockout-3.4.0.js",
                        "~/Scripts/knockout.validation.js",
                        "~/Scripts/global.js", 
                        "~/Scripts/jquery.ui.plugin_responsive-table.js"));
        }
    }
}