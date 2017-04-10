using System.Web;
using System.Web.Optimization;

namespace ToDo
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-1.12.1.min.js"                                                
                        ));

            bundles.Add(new ScriptBundle("~/bundles/themejs").Include(
                "~/Scripts/waypoints.min.js",
                "~/Scripts/jquery.countdown.js",
                "~/Scripts/jquery.nav.js",
                "~/Scripts/main.js"                               
                ));



                  bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //DataTables Bundles
            bundles.Add(new ScriptBundle("~/bundles/table").Include(
               "~/Scripts/DataTables/jquery.dataTables.js",
               "~/Scripts/DataTables/dataTables.buttons.min.js",
               "~/Scripts/DataTables/buttons.flash.min.js",
               "~/Scripts/DataTables/dataTables.fixedColumns.min.js",
               "~/Scripts/DataTables/dataTables.fixedHeader.min.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                       "~/Content/main.css",
                       "~/Content/plugins.css",
                       "~/Content/site.css",
                       "~/Content/font-awesome.min.css",
                        "~/Content/DataTables/css/jquery.dataTables.css",
                         "~/Content/DataTables/images",
                         "~/Content/DataTables/css/buttons.dataTables.min.css",
                         "~/Content/DataTables/css/fixedColumns.dataTables.min.css",
                         "~/Content/colors-css/"                     
                         ));
        }
    }
}