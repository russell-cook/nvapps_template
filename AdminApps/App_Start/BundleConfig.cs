using System.Web.Optimization;

namespace AdminApps
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*",
                        "~/Scripts/dhtmlxgantt.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap.custom.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/bootstrap-datepicker.js",
                      "~/Scripts/DatePickerReady.js",
                      "~/Scripts/jquery.timepicker.js",
                      "~/Scripts/TimePickerReady.js"
                      //"~/Scripts/init-gantt.js"
            ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/themes/nvapps/bootstrap.css",
                      "~/Content/bootstrap.custom.css",
                      "~/Content/bootstrap-datepicker3.css",
                      "~/Content/jquery.timepicker.css",
                      "~/Content/site.css",
                      // load theme for dhtmlx gantt chart. multiple themes are available
                      "~/Content/dhtmlxgantt/dhtmlxgantt_terrace.css"));
        }
    }
}
