namespace Services.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Entities;
    using Services.Contracts;
    using EnvDTE;

    public class WebFormPagesService : IPagesService
    {
        #region Member Variables

        protected readonly IRegexService _regexService;

        #endregion Member Variables

        #region Constructor

        public WebFormPagesService(IRegexService regexService)
        {
            this._regexService = regexService;
        }

        #endregion Constructor

        #region IPagesService

        void IPagesService.ProcessPages(IEnumerable<Page> pagesList)
        {
            foreach (Page page in pagesList)
            {
                page.References = GetReferences(page);
                var parents = GetParents(page.Name, page.Content, pagesList);
                if (page.Parents != null)
                {
                    page.Parents.Concat(parents);
                }
                else
                {
                    page.Parents = parents;
                }
                var children = GetChildren(page.Content, pagesList);
                foreach (var child in children)
                {
                    if (child.Parents == null)
                    {
                        child.Parents = new List<Page>();
                    }

                    child.Parents.Add(page);
                }
            }

            LinkParents(pagesList);
        }

        IEnumerable<Page> IPagesService.GetDuplicatesInSameFile(IEnumerable<Page> pages)
        {
            foreach (var page in pages)
            {
                var duplicates = page.References.Where(p => p.Value != 1).ToDictionary(p => p.Key, p => p.Value);

                if (duplicates.Count != 0)
                {
                    yield return new Page
                    {
                        Content = page.Content,
                        Item = page.Item,
                        Name = page.Name,
                        Parents = page.Parents,
                        References = duplicates
                    };
                }
            }
        }

        IEnumerable<ParentChildDuplicates> IPagesService.GetDuplicatesInLayoutAndFile(IEnumerable<Page> pages)
        {
            var duplicatesList = new List<ParentChildDuplicates>();

            foreach (var child in pages)
            {
                var childReferences = child.References.Keys;

                foreach (var parent in child.Parents)
                {
                    var parentsReferences = parent.References.Keys;
                    var commonReferences = parentsReferences.Intersect(childReferences);

                    if (commonReferences.Count() > 0)
                    {
                        var parentChildDuplicate = new ParentChildDuplicates { Parent = parent, Child = child, Duplicates = commonReferences };
                        duplicatesList.Add(parentChildDuplicate);
                    }
                }
            }

            return duplicatesList;
        }

        #endregion

        #region Protected Methods

        protected void AddToParentsRecursive(Page page, Dictionary<string, Page> parents)
        {
            if (page.Parents.Count == 0)
            {
                return;
            }

            foreach (var item in page.Parents)
            {
                if (!parents.ContainsKey(item.Name))
                {
                    parents.Add(item.Name, item);
                }

                AddToParentsRecursive(item, parents);
            }
        }

        #endregion Protected Methods

        #region Virtual Methods

        protected virtual IEnumerable<Page> FilterPages(IEnumerable<Page> pages)
        {
            return pages.Where(p => p.Name.EndsWith(".ascx") || p.Name.EndsWith(".aspx") || p.Name.EndsWith(".Master"));
        }

        protected virtual void GetMasterPage(string pageName, string pageContent, IEnumerable<Page> pagesList, List<Page> parentList)
        {
            var masterPage = _regexService.GetMasterPageName(pageContent);

            if (!string.IsNullOrEmpty(masterPage))
            {
                masterPage = masterPage.Remove(0, 1);
                masterPage = masterPage.Remove(masterPage.Count() - 1, 1);
                parentList.Add(pagesList.Where(p => p.Name.IndexOf(masterPage, StringComparison.OrdinalIgnoreCase) > -1).First());
            }
        }

        protected virtual IEnumerable<string> GetControls(string pageContent)
        {
            return _regexService.GetWebControls(pageContent);
        }

        protected virtual void LinkParents(IEnumerable<Page> pagesList)
        {
            foreach (Page page in pagesList)
            {
                var parents = new Dictionary<string, Page>();
                AddToParentsRecursive(page, parents);
                page.Parents = parents.Values.ToList();
            }
        }

        #endregion Virtual Methods

        #region Private Methods

        private Dictionary<string, int> GetReferences(Page page)
        {
            var referencesList = _regexService.GetReferences(page.Content);

            var referencesFrequency = from reference in referencesList
                                      group reference by reference into g
                                      orderby g.Key descending
                                      select new
                                      {
                                          Reference = g.Key,
                                          ReferencesCount = g.Count()
                                      };

            return referencesFrequency.ToDictionary(e => e.Reference.Remove(0, 1), e => e.ReferencesCount);
        }

        private IList<Page> GetParents(string pageName, string pageContent, IEnumerable<Page> pagesList)
        {
            var parentList = new List<Page>();

            GetMasterPage(pageName, pageContent, pagesList, parentList);

            return parentList;
        }

        private IEnumerable<Page> GetChildren(string pageContent, IEnumerable<Page> pagesList)
        {
            IEnumerable<string> controls = GetControls(pageContent);

            foreach (var item in controls)
            {
                yield return pagesList.Where(p => p.Name.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1).First();
            }
        }

        #endregion Private Methods
    }
}