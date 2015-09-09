using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GlossaryApp.Print
{
    class Program
    {
        const string MainContentString = @"<div id=""main-content"">";
        static List<Acronym> acronymList = new List<Acronym>();
        static List<CorisGlossary> glossaryList = new List<CorisGlossary>();
        static int Main(string[] args)
        {
            try
            {
                return AsyncContext.Run(() => MainAsync(args));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }

        static async Task<int> MainAsync(string[] args)
        {
            try
            {
                Console.WriteLine("Reading Acronym.xml...");
                var acronymDataTask = GetData<Acronym>(AppConstants.AcronymFileName);
                Console.WriteLine("Reading CorisGlossaries.xml...");
                var glossaryDataTask = GetData<CorisGlossary>(AppConstants.GlossaryFileName);
                acronymList = await acronymDataTask;
                Console.WriteLine("Fetched {0} from Acronym.xml", acronymList.Count);
                glossaryList = await glossaryDataTask;
                Console.WriteLine("Fetched {0} from CorisGlossaries.xml", glossaryList.Count);

                var indexPageHTML = await DownloadString(AppConstants.CoralSiteIndexPage);
                indexPageHTML = Regex.Replace(indexPageHTML, @"\s*(<[^>]+>)\s*", "$1", RegexOptions.Singleline);
                indexPageHTML = indexPageHTML.Replace(@"ng-app=""glossaryArchivecodeApp""", string.Empty);
                indexPageHTML = indexPageHTML.Replace(@"<div class=""scroll-top-wrapper show"" scorll-to-top><span class=""scroll-top-inner""><i class=""fa fa-2x fa-arrow-circle-up""></i></span></div>", string.Empty);
                var mainContentIndex = indexPageHTML.IndexOf(MainContentString, StringComparison.Ordinal);
                Console.WriteLine(mainContentIndex);
                var glossaryAndAcronymData = string.Format("{0}{1}", BuildGlossaryString(glossaryList), BuildAcronymString(acronymList));
                indexPageHTML = indexPageHTML.Insert(mainContentIndex + MainContentString.Length, glossaryAndAcronymData);
                File.WriteAllText(AppConstants.PrintFilePath, indexPageHTML);
                // Console.WriteLine(indexPageHTML);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("completed ...");
                Console.Read();
            }
            return 0;
        }

        static async Task<string> DownloadString(string url)
        {
            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }

        static async Task<List<T>> GetData<T>(string path)
        {
            var listData = new List<T>();
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(List<T>));
                var clientData = await DownloadString(path);
                using (TextReader reader = new StringReader(clientData))
                {
                    listData = (List<T>)xmlSerializer.Deserialize(reader);
                    Console.WriteLine(listData.Count());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listData;
        }

        static string BuildGlossaryString(List<CorisGlossary> coll)
        {
            var ss = @"<div class='card'><div class='card-content row'><div class='col-md-10'><h4 class='media-heading'><span class='card-title'>{0}</span></h4><span>{1}</span></div><div class='media-right col-md-2'><div class='thumbnail'><img style='width: 186px;' class='img-thumbnail' src='images/{2}' alt='{3}' /><small>{4}</small></div></div></div></div>";
            var ssWithOutImage = @"<div class='card'><div class='card-content row'><div class='col-md-10'><h4 class='media-heading'><span class='card-title'>{0}</span></h4><span>{1}</span></div><div class='media-right col-md-2'></div></div></div>";
            var currentChar = string.Empty;
            var output = coll.Aggregate(new StringBuilder("<h3>Glossary of Terminology</h3><hr/>"), (a, b) =>
               {
                   if (!b.Term.Substring(0, 1).ToUpper().Equals(currentChar))
                   {
                       currentChar = b.Term.Substring(0, 1).ToUpper();
                       a.Append(GetAnchorNameTag("glossary-", currentChar));
                       a.Append(HandleFilterMenu("glossary-"));

                   }
                   if ((b.Image != null && string.IsNullOrEmpty(b.Image.Trim())) || string.IsNullOrEmpty(b.Image))
                   {
                       return a.AppendFormat(ssWithOutImage, WebUtility.HtmlDecode(b.Term),
                           WebUtility.HtmlDecode(b.Definition));
                   }
                   return a.AppendFormat(ss, WebUtility.HtmlDecode(b.Term),
                           WebUtility.HtmlDecode(b.Definition), WebUtility.HtmlDecode(b.Image),
                           WebUtility.HtmlDecode(b.Alt_tag), WebUtility.HtmlDecode(b.Caption));
               });
            return output.ToString();
        }

        static string BuildAcronymString(List<Acronym> coll)
        {
            var currentChar = string.Empty;
            //var outerString = @"<div class='list-group'>{0}</div>";
            var ss = "<div class='list-group-item'><b>{0}</b> : {1}</div>";
            var initialString = new StringBuilder();
            initialString.Append("<br /><h3> Acronyms/Abbreviations</h3><hr/>");
            //initialString.Append(HandleFilterMenu("acronym-"));
            var output = coll.Aggregate(initialString, (a, b) =>
            {
                if (!b.acronym.Substring(0, 1).ToUpper().Equals(currentChar))
                {
                    currentChar = b.acronym.Substring(0, 1).ToUpper();
                    if (a.Length > 0)
                    {
                        a.Append("</div>");
                    }
                    a.Append(GetAnchorNameTag("acronym-", currentChar));
                    a.Append(HandleFilterMenu("acronym-"));
                    a.Append("<div class='list-group'>");
                }
                return a.AppendFormat(ss, WebUtility.HtmlDecode(b.acronym), WebUtility.HtmlDecode(b.definition));
            });

            // return string.Format(outerString, output.ToString());
            return output.ToString();
        }

        static string HandleFilterMenu(string type)
        {
            var arr = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z".Split(',');
            var menu = arr.Aggregate(new StringBuilder(), (a, b) =>
            {
                return a.AppendFormat(@"<span><a href='#{0}{1}'>{2}</a><span>&nbsp;|&nbsp;</span></span>", type, b, b);
            });
            return "<div class='row'><div class='col-md-10 col-md-offset-2'>" + menu.ToString() + "</div></div>";
        }

        static string GetAnchorNameTag(string type, string name)
        {
            return string.Format("<a name='{0}'></a><br/>", type + name);
        }
    }
}
