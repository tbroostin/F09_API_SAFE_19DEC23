using System.Web.Mvc;

namespace Ellucian.Colleague.Api.Areas.Planning
{
    /// <summary>
    /// Handle area registration for planning.
    /// </summary>
    public class PlanningAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// Gets the area name.
        /// </summary>
        public override string AreaName
        {
            get
            {
                return "Planning";
            }
        }

        /// <summary>
        /// Performs the area registration specific to this area.
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Planning_default",
                "Planning/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
