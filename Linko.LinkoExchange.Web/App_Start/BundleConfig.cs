using System.Web.Optimization;

namespace Linko.LinkoExchange.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(bundle:new StyleBundle(virtualPath:"~/bundles/css")
                            .Include(virtualPath:"~/Content/bootstrap.css")

                            //.Include(virtualPath: "~/Content/select2.css")
                            .Include(virtualPath:"~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css")
                            .Include(virtualPath:"~/Content/animate.css")
                            .Include(virtualPath:"~/Content/AdminLTE/css/datepicker3.css")
                            .Include(virtualPath:"~/Content/AdminLTE/css/dataTables.bootstrap.css")
                            .Include(virtualPath:"~/Content/AdminLTE/css/dataTables.responsive.css")

                            // .Include(virtualPath: "~/Content/AdminLTE/css/AdminLTE.css") // Don't include this file in bundling as it fails to load fonts
                            //.Include(virtualPath: "~/Content/AdminLTE/css/skins/skin-blue-light.css")
                            .Include(virtualPath:"~/Content/site.css",
                                     transforms:new IItemTransform[] {new CssRewriteUrlTransform()}));

            bundles.Add(bundle:new StyleBundle(virtualPath:"~/bundles/font-awesome")
                            .Include(virtualPath:"~/Content/font-awesome.css"));

            bundles.Add(bundle:new StyleBundle(virtualPath:"~/bundles/icheck")
                            .Include(virtualPath:"~/Content/icheck/minimal/blue.css",
                                     transforms:new IItemTransform[] {new CssRewriteUrlTransform()}));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/js")
                            .Include(virtualPath:"~/Scripts/bootstrap.js")
                            .Include(virtualPath:"~/Scripts/bootstrap-switch.js")
                            .Include(virtualPath:"~/Scripts/bootstrap-notify.js")
                            .Include(virtualPath:"~/Scripts/fastclick.js")
                            .Include(virtualPath:"~/Scripts/AdminLTE/jquery.slimscroll.js")

                            //.Include(virtualPath: "~/Scripts/select2.full.js")
                            //.Include(virtualPath: "~/Scripts/moment.js")
                            .Include(virtualPath:"~/Scripts/AdminLTE/bootstrap-datepicker.js")
                            .Include(virtualPath:"~/Scripts/jquery.icheck.js")
                            .Include(virtualPath:
                                     "~/Scripts/AdminLTE/jquery.dataTables.js") // jquery.dataTables.js needs to be include before dataTables.bootstrap.js
                            .Include(virtualPath:"~/Scripts/AdminLTE/dataTables.bootstrap.js")
                            .Include(virtualPath:"~/Scripts/AdminLTE/dataTables.responsive.js")
                            .Include(virtualPath:"~/Scripts/AdminLTE/app.js")
                            .Include(virtualPath:"~/Scripts/AdminLTE/init.js")
                            .Include(virtualPath:"~/Scripts/linkoExchange.js"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/js_icheck")
                            .Include(virtualPath:"~/Scripts/jquery.icheck.js"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/jquery")
                            .Include(virtualPath:"~/Scripts/jquery-{version}.js")
                            .Include(virtualPath:"~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/jqueryval")
                            .Include(virtualPath:"~/Scripts/jquery.validate*"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/inputmask")
                            .Include(virtualPath:"~/Scripts/jquery.inputmask.bundle.js"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/profile")
                            .Include(virtualPath:"~/Scripts/profile.js"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/auditlogs")
                            .Include(virtualPath:"~/Scripts/auditlogs.js"));

            bundles.Add(bundle:new ScriptBundle(virtualPath:"~/bundles/pendingInvitations")
                            .Include(virtualPath:"~/Scripts/pendingInvitation.js"));

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
                BundleTable.EnableOptimizations = true;
#endif
        }
    }
}