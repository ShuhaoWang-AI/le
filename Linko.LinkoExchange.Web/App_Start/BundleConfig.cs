using System.Web.Optimization;

namespace Linko.LinkoExchange.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle(virtualPath: "~/bundles/jquery").Include(virtualPath: "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle(virtualPath: "~/bundles/jqueryval").Include(virtualPath: "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle(virtualPath: "~/bundles/bootstrap").Include(virtualPath: "~/Scripts/bootstrap.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap-theme-flatly.css",
            //          "~/Content/site.css"));

        }
    }
}