using System.Collections.Generic;

namespace Services.Contracts
{
    public interface IRegexService
    {
        IEnumerable<string> GetReferences(string pageContent);

        string GetDefaultLayoutPageName(string content);

        IEnumerable<string> GetMultipleLayouts(string content);

        IEnumerable<string> GetPartialViews(string content);

        IEnumerable<string> GetWebControls(string content);

        bool CheckPageHasNullLayout(string content);

        string GetMasterPageName(string content);
    }
}