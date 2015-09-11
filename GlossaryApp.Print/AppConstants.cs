using System.Configuration;

namespace GlossaryApp.Print
{
    class AppConstants
    {
        public static string CoralSiteUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["CoralSiteUrl"];
            }
        }

        public static string CoralSiteIndexPage
        {
            get
            {
                return ConfigurationManager.AppSettings["CoralSiteIndexPage"];
            }
        }


        public static string GlossaryFileName
        {
            get
            {
                return string.Format("{0}{1}", CoralSiteUrl, ConfigurationManager.AppSettings["GlossaryFileName"]);
            }
        }

        public static string AcronymFileName
        {
            get
            {
                return string.Format("{0}{1}", CoralSiteUrl, ConfigurationManager.AppSettings["AcronymFileName"]);
            }
        }

        public static string PrintFilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["PrintFilePath"];
            }
        }

        public static string GlossaryPrintFileName
        {
            get
            {
                return ConfigurationManager.AppSettings["GlossaryPrintFileName"];
            }
        }

        public static string AcronymPrintFileName
        {
            get
            {
                return ConfigurationManager.AppSettings["AcronymPrintFileName"];
            }
        }
    }
}
