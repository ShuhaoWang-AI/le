using System.Web.Optimization;

namespace Linko.LinkoExchange.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle(virtualPath: "~/Bundles/css")
                .Include(virtualPath: "~/Content/bootstrap.css")
                //.Include(virtualPath: "~/Content/select2.css")
                .Include(virtualPath: "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css")
                .Include(virtualPath: "~/Content/AdminLTE/css/datepicker3.css")
                .Include(virtualPath: "~/Content/AdminLTE/css/AdminLTE.css")
                .Include(virtualPath: "~/Content/AdminLTE/css/skins/skin-blue-light.css")
                .Include(virtualPath: "~/Content/site.css"));

            bundles.Add(new StyleBundle(virtualPath: "~/Bundles/font-awesome")
                .Include(virtualPath: "~/Content/font-awesome.css"));

            bundles.Add(new StyleBundle(virtualPath: "~/Bundles/icheck")
                .Include(virtualPath: "~/Content/icheck/minimal/blue.css"));

            bundles.Add(new ScriptBundle(virtualPath: "~/Bundles/js")
                .Include(virtualPath: "~/Scripts/bootstrap.js")
                .Include(virtualPath: "~/Scripts/bootstrap-switch.js")
                .Include(virtualPath: "~/Scripts/fastclick.js")
                .Include(virtualPath: "~/Scripts/AdminLTE/jquery.slimscroll.js")
                //.Include(virtualPath: "~/Scripts/select2.full.js")
                //.Include(virtualPath: "~/Scripts/moment.js")
                .Include(virtualPath: "~/Scripts/AdminLTE/bootstrap-datepicker.js")
                .Include(virtualPath: "~/Scripts/jquery.icheck.js")
                .Include(virtualPath: "~/Scripts/AdminLTE/app.js")
                .Include(virtualPath: "~/Scripts/AdminLTE/init.js")
                .Include(virtualPath: "~/Scripts/linkoExchange.js"));

            bundles.Add(new ScriptBundle(virtualPath: "~/bundles/jquery")
                .Include(virtualPath: "~/Scripts/jquery-{version}.js")
                .Include(virtualPath: "~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle(virtualPath: "~/bundles/jqueryval")
                .Include(virtualPath: "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle(virtualPath: "~/bundles/profile")
                .Include(virtualPath: "~/Scripts/profile.js"));


#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif

        }
    }
}