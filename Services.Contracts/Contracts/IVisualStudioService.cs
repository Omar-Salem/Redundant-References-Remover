namespace Services.Contracts
{
    using System.Collections.Generic;
    using Entities;
    using EnvDTE;

    public interface IVisualStudioService
    {
        WebProject GetWebProject();

        IList<Page> GetPages(ProjectItems item);

        vsSaveStatus RemoveReference(ProjectItem projectItem, string reference, int offset);
    }
}