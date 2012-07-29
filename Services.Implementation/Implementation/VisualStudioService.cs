namespace Services.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Entities;
    using EnvDTE;
    using Services.Contracts;

    [Export(typeof(IVisualStudioService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class VisualStudioService : IVisualStudioService
    {
        #region Member Variables

        private readonly IServiceProvider _serviceProvider;
        private DTE _dte;

        #endregion Member Variables

        #region Constructor

        [ImportingConstructor]
        public VisualStudioService([Import("serviceProvider", typeof(IServiceProvider))]IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        #endregion Constructor

        #region IVisualStudioService

        WebProject IVisualStudioService.GetWebProject()
        {
            this._dte = (DTE)_serviceProvider.GetService(typeof(DTE));

            foreach (Project project in _dte.Solution.Projects)
            {
                var projectTypeGuid = GetProjectTypeGuids(project);
                var webProject = new WebProject { Project = project };
                webProject.ProjectType = GetProjectType(projectTypeGuid);

                if (webProject.ProjectType != ProjectType.Other)
                {
                    return webProject;
                }
            }

            return null;
        }

        IList<Page> IVisualStudioService.GetPages(ProjectItems projectItems)
        {
            List<Page> pages = new List<Page>();

            foreach (ProjectItem item in projectItems)
            {
                pages.AddRange(GetPages(item));
            }

            return pages;
        }

        vsSaveStatus IVisualStudioService.RemoveReference(ProjectItem projectItem, string reference, int offset)
        {
            if (!projectItem.IsOpen)
            {
                projectItem.Open();
            }

            projectItem.Document.Activate();
            var document = _dte.ActiveDocument.Object("TextDocument");
            EditPoint start = document.StartPoint.CreateEditPoint();
            start.MoveToAbsoluteOffset(offset);
            EditPoint end = document.EndPoint.CreateEditPoint();
            string pattern = string.Format(@"\<{{script|link}}.+{0}.*{{/\>|script\>}}", reference);//<(script|link)\b(.*?){0}(.*?)(script>|/>)
            start.ReplacePattern(end, pattern, " ", (int)vsFindOptions.vsFindOptionsRegularExpression);
            var saveResult = this._dte.ActiveDocument.Save();
            this._dte.ActiveDocument.Close();
            return saveResult;
        }

        #endregion IVisualStudioService

        #region Private Methods

        private ProjectType GetProjectType(string projectTypeGuid)
        {
            if (projectTypeGuid.Contains("E53F8FEA-EAE0-44A6-8774-FFD645390401"))
            {
                return ProjectType.MVC3;
            }
            else if (projectTypeGuid.Contains("F85E285D-A4E0-4152-9332-AB1D724D3325"))
            {
                return ProjectType.MVC2;
            }
            else if (projectTypeGuid.Contains("349c5851-65df-11da-9384-00065b846f21"))
            {
                return ProjectType.WebForm;
            }
            else
            {
                return ProjectType.Other;
            }
        }

        private string GetProjectTypeGuids(Project proj)
        {
            string projectTypeGuids = "";
            int result = 0;

            object service = GetService(proj.DTE, typeof(Microsoft.VisualStudio.Shell.Interop.IVsSolution));
            var solution = (Microsoft.VisualStudio.Shell.Interop.IVsSolution)service;
            Microsoft.VisualStudio.Shell.Interop.IVsHierarchy hierarchy = null;

            result = solution.GetProjectOfUniqueName(proj.UniqueName, out hierarchy);

            if (result == 0)
            {
                Microsoft.VisualStudio.Shell.Interop.IVsAggregatableProject aggregatableProject = (Microsoft.VisualStudio.Shell.Interop.IVsAggregatableProject)hierarchy;
                result = aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
            }

            return projectTypeGuids;
        }

        private object GetService(object serviceProvider, System.Type type)
        {
            return GetService(serviceProvider, type.GUID);
        }

        private object GetService(object serviceProviderObject, System.Guid guid)
        {
            object service = null;
            IntPtr serviceIntPtr;
            int hr = 0;

            Guid SIDGuid = guid;
            Guid IIDGuid = SIDGuid;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)serviceProviderObject;
            hr = serviceProvider.QueryService(ref SIDGuid, ref  IIDGuid, out  serviceIntPtr);

            if (hr != 0)
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            }
            else if (!serviceIntPtr.Equals(IntPtr.Zero))
            {
                service = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(serviceIntPtr);
                System.Runtime.InteropServices.Marshal.Release(serviceIntPtr);
            }

            return service;
        }

        private IList<Page> GetPages(ProjectItem item)
        {
            var files = new List<Page>();

            if (item.ProjectItems.Count == 0)
            {
                if (CheckIsPage(item))//maybe its an empty folder?
                {
                    GetPageData(item, files);
                }
            }
            else
            {
                if (CheckIsPage(item))//aspx page ?
                {
                    GetPageData(item, files);
                }
                else
                {
                    foreach (ProjectItem currentItem in item.ProjectItems)
                    {
                        files.AddRange(GetPages(currentItem));
                    }
                }
            }

            return files;
        }

        private void GetPageData(ProjectItem item, List<Page> files)
        {
            var fileContents = GetDocumentText(item);
            files.Add(new Page { Name = FormatPath(item), Content = fileContents, Item = item });
        }

        private bool CheckIsPage(ProjectItem item)
        {
            return item.Name.EndsWith(".aspx") || item.Name.EndsWith(".ascx") || item.Name.EndsWith(".Master") || item.Name.EndsWith(".cshtml") || item.Name.EndsWith(".vbhtml");
        }

        private string GetDocumentText(ProjectItem page)
        {
            if (!page.IsOpen)
            {
                page.Open();
            }

            var document = (TextDocument)page.Document.Object();
            var startPoint = document.StartPoint.CreateEditPoint();
            var endPoint = document.EndPoint.CreateEditPoint();
            return startPoint.GetText(endPoint);
        }

        private string FormatPath(ProjectItem item)
        {
            string projectName = item.ContainingProject.Name;
            string pathName = item.FileNames[0];
            int idx = pathName.IndexOf(projectName);
            return pathName.Substring(idx);
        }

        #endregion Private Methods
    }
}