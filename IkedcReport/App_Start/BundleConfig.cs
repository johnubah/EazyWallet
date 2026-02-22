using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace WalletReport.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-2.1.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-1.11.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            //bundles.Add(new ScriptBundle("~/bundles/DashboardSupport").Include(
            //          "~/Scripts/libs/lo-dash.js",
            //          "~/Scripts/mylibs/jquery.idle-timer.js",
            //          "~/Scripts/mylibs/jquery.plusplus.js",
            //          "~/Scripts/mylibs/jquery.jgrowl.js",
            //          "~/Scripts/mylibs/jquery.scrollTo.js",
            //          "~/Scripts/mylibs/jquery.ui.touch-punch.js",
            //          "~/Scripts/mylibs/jquery.ui.multiaccordion.js",
            //          "~/Scripts/mylibs/number-functions.js"));

            //bundles.Add(new ScriptBundle("~/bundles/DashboardGeneral").Include(
            //           "~/Scripts/mylibs/jquery.hashchange.js"));

            //bundles.Add(new ScriptBundle("~/bundles/DashboardForm").Include(
            //    "~/Scripts/mylibs/forms/jquery.autosize.js",
            //    "~/Scripts/mylibs/forms/jquery.checkbox.js",
            //    "~/Scripts/mylibs/forms/jquery.chosen.js",
            //    "~/Scripts/mylibs/forms/jquery.cleditor.js",
            //    "~/Scripts/mylibs/forms/jquery.colorpicker.js",
            //    "~/Scripts/mylibs/forms/jquery.ellipsis.js",
            //    "~/Scripts/mylibs/forms/jquery.fileinput.js",
            //    "~/Scripts/mylibs/forms/jquery.fullcalendar.js",
            //    "~/Scripts/mylibs/forms/jquery.maskedinput.js",
            //    "~/Scripts/mylibs/forms/jquery.mousewheel.js",
            //    "~/Scripts/mylibs/forms/jquery.placeholder.js",
            //    "~/Scripts/mylibs/forms/jquery.pwdmeter.js",
            //    "~/Scripts/mylibs/forms/jquery.ui.datetimepicker.js",
            //    "~/Scripts/mylibs/forms/jquery.ui.spinner.js",
            //    "~/Scripts/mylibs/forms/jquery.validate.js",
            //     "~/Scripts/mylibs/charts/jquery.flot.js",
            //    "~/Scripts/mylibs/charts/jquery.flot.orderBars.js",
            //    "~/Scripts/mylibs/charts/jquery.flot.pie.js",
            //    "~/Scripts/mylibs/charts/jquery.flot.resize.js",
            //    "~/Scripts/mylibs/explorer/jquery.elfinder.js",
            //    "~/Scripts/mylibs/forms/uploader/plupload.js",
            //    "~/Scripts/mylibs/forms/uploader/plupload.html5.js",
            //    "~/Scripts/mylibs/forms/uploade/plupload.html4.js",
            //    "~/Scripts/mylibs/forms/uploader/plupload.flash.js",
            //     "~/Scripts/mylibs/forms/uploader/jquery.plupload.queue/jquery.plupload.queue.js"));


            //bundles.Add(new ScriptBundle("~/bundles/DTV").Include("~/Scripts/mylibs/forms/uploader/plupload.html4.js",
            //    "~/Scripts/mylibs/fullstats/jquery.css-transform.js",
            //    "~/Scripts/mylibs/fullstats/jquery.animate-css-rotate-scale.js",
            //    "~/Scripts/mylibs/fullstats/jquery.sparkline.js",
            //    "~/Scripts/mylibs/syntaxhighlighter/shCore.js",
            //    "~/Scripts/mylibs/syntaxhighlighter/shAutoloader.js",
            //    "~/Scripts/mylibs/dynamic-tables/jquery.dataTables.js",
            //    "~/Scripts/mylibs/dynamic-tables/jquery.dataTables.tableTools.zeroClipboard.js",
            //    "~/Scripts/mylibs/dynamic-tables/jquery.dataTables.tableTools.js",
            //    "~/Scripts/mylibs/gallery/jquery.fancybox.js",
            //    "~/Scripts/mylibs/tooltips/jquery.tipsy.js",
            //    "~/Scripts/mango.js",
            //    "~/Scripts/plugins.js",
            //    "~/Scripts/script.js",
            //    "~/Scripts/app.js",
            //    "~/Scripts/mylibs/polyfills/respond.js",
            //    "~/Scripts/mylibs/polyfills/matchmedia.js"));


            ////        <script src="js/mylibs/polyfills/modernizr-2.6.1.min.js"></script>







            //// Use the development version of Modernizr to develop with and learn from. Then, when you're
            //// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));


            ////bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            ////bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            ////            "~/Content/themes/base/jquery.ui.core.css",
            ////            "~/Content/themes/base/jquery.ui.resizable.css",
            ////            "~/Content/themes/base/jquery.ui.selectable.css",
            ////            "~/Content/themes/base/jquery.ui.accordion.css",
            ////            "~/Content/themes/base/jquery.ui.autocomplete.css",
            ////            "~/Content/themes/base/jquery.ui.button.css",
            ////            "~/Content/themes/base/jquery.ui.dialog.css",
            ////            "~/Content/themes/base/jquery.ui.slider.css",
            ////            "~/Content/themes/base/jquery.ui.tabs.css",
            ////            "~/Content/themes/base/jquery.ui.datepicker.css",
            ////            "~/Content/themes/base/jquery.ui.progressbar.css",
            ////            "~/Content/themes/base/jquery.ui.theme.css"));

            //bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/style.css",
            //    "~/Content/grid.css",
            //    "~/Content/layout.css",
            //    "~/Content/icons.css",
            //    "~/Content/fonts/font-awesome.css",
            //    "~/Content/external/jquery-ui-1.8.21.custom.css",
            //    "~/Content/external/jquery.chosen.css",
            //    "~/Content/external/jquery.cleditor.css",
            //    "~/Content/external/jquery.colorpicker.css",
            //    "~/Content/external/jquery.elfinder.css",
            //    "~/Content/external/jquery.fancybox.css",
            //    "~/Content/external/jquery.jgrowl.css",
            //    "~/Content/external/jquery.plupload.queue.css",
            //    "~/Content/external/syntaxhighlighter/shCore.css",
            //    "~/Content/external/syntaxhighlighter/shThemeDefault.css",
            //    "~/Content/elements.css",
            //    "~/Content/forms.css",
            //    "~/Content/print-invoice.css",
            //    "~/Content/typographics.css",
            //    "~/Content/media-queries.css",
            //    "~/Content/ie-fixes.css"));


        }
    }
}