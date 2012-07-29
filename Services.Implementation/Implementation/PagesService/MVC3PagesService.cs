using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Services.Contracts;
using EnvDTE;

namespace Services.Implementation
{
    public class MVC3PagesService : WebFormPagesService
    {
        #region Constructor

        public MVC3PagesService(IRegexService regexService)
            : base(regexService)
        {
        }

        #endregion Constructor

        #region Overridden Methods

        protected override IEnumerable<Page> FilterPages(IEnumerable<Page> pages)
        {
            return pages.Where(p => p.Name.EndsWith(".cshtml") || p.Name.EndsWith(".vbhtml"));
        }

        protected override void LinkParents(IEnumerable<Page> pagesList)
        {
            var defaultLayoutPage = GetDefaultLayoutPage(pagesList);

            foreach (Page page in pagesList)
            {
                var parents = new Dictionary<string, Page>();
                AddToParentsRecursive(page, parents);

                if (CheckPageHasDefaultLayout(page, defaultLayoutPage))
                {
                    var defaultLayout = pagesList.Where(p => p.Name.Contains(defaultLayoutPage)).First();
                    parents.Add(defaultLayout.Name, defaultLayout);
                }

                page.Parents = parents.Values.ToList();
            }
        }

        protected override void GetMasterPage(string pageName, string pageContent, IEnumerable<Page> pagesList, List<Page> parentList)
        {
            if (!pageName.Contains("_ViewStart"))
            {
                var multipleLayouts = _regexService.GetMultipleLayouts(pageContent);

                foreach (var layout in multipleLayouts)
                {
                    string name = layout.Remove(0, 1);
                    parentList.Add(pagesList.Where(p => p.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) > -1).First());
                }
            }
        }

        protected override IEnumerable<string> GetControls(string pageContent)
        {
            return _regexService.GetPartialViews(pageContent);
        }

        #endregion Overriden Methods

        #region Private Methods

        private string GetDefaultLayoutPage(IEnumerable<Page> pagesList)
        {
            Page viewStart = pagesList.Where(p => p.Name.Contains("_ViewStart")).SingleOrDefault();

            if (viewStart != null)
            {
                var layoutPageName = _regexService.GetDefaultLayoutPageName(viewStart.Content);

                if (!string.IsNullOrEmpty(layoutPageName))
                {
                    return layoutPageName.Remove(0, 1);
                }
            }

            return string.Empty;
        }

        private bool CheckPageHasDefaultLayout(Page item, string layout)
        {
            bool isMainPage = item.Name.Contains("_ViewStart") || item.Content.Contains("RenderBody");
            return item.Parents.Count == 0 && !isMainPage && !_regexService.CheckPageHasNullLayout(item.Content);
        }

        #endregion Private Methods
    }
}