using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Services.Contracts;

namespace Services.Implementation
{
    [Export(typeof(IRegexService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RegexService : IRegexService
    {
        #region IRegexService

        IEnumerable<string> IRegexService.GetReferences(string pageContent)
        {
            var matches = GetMatches(pageContent, @"(src|href)=""[^/]*/(.*?)\.(css|js)");

            foreach (var item in matches)
            {
                yield return GetMatches(item, @"/(?!.*/)(.*?)\.(css|js)").First();
            }
        }

        string IRegexService.GetDefaultLayoutPageName(string content)
        {
            return GetDefaultLayoutPageName(content);
        }

        IEnumerable<string> IRegexService.GetMultipleLayouts(string content)
        {
            var layouts = GetMatches(content, @"Layout[\n|\s]*=[\n|\s]*""[^""]*");

            foreach (var item in layouts)
            {
                yield return GetDefaultLayoutPageName(item);
            }
        }

        IEnumerable<string> IRegexService.GetPartialViews(string content)
        {
            var partialViews = GetMatches(content, @"RenderPartial\(""[^""]*");

            foreach (var item in partialViews)
            {
                yield return item.Replace(@"RenderPartial(""", string.Empty);
            }
        }

        IEnumerable<string> IRegexService.GetWebControls(string content)
        {
            var webControls = GetMatches(content, @"""(.*?).ascx");

            foreach (var item in webControls)
            {
                yield return item.Replace(@"""", string.Empty);
            }
        }

        string IRegexService.GetMasterPageName(string content)
        {
            return GetMatches(content, @"/[^/]*.master""").FirstOrDefault();
        }

        bool IRegexService.CheckPageHasNullLayout(string content)
        {
            Regex regex = new Regex(@"Layout[\n|\s]*=[\n|\s]*null");//Layout=null
            return regex.IsMatch(content);
        }

        #endregion IRegexService

        #region Private Methods

        private IEnumerable<string> GetMatches(string documentText, string pattern)
        {
            var matchesList = Regex.Matches(documentText, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matchesList)
            {
                foreach (Capture capture in match.Captures)
                {
                    yield return capture.Value;
                }
            }
        }

        private string GetDefaultLayoutPageName(string content)
        {
            return GetMatches(content, @"/[^/]*.[cs|vb]html").First();
        }

        #endregion Private Methods
    }
}