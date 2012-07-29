namespace Services.Contracts
{
    using System.Collections.Generic;
    using Entities;
    using EnvDTE;

    public interface IPagesService
    {
        void ProcessPages(IEnumerable<Page> pagesList);

        IEnumerable<Page> GetDuplicatesInSameFile(IEnumerable<Page> pages);

        IEnumerable<ParentChildDuplicates> GetDuplicatesInLayoutAndFile(IEnumerable<Page> pages);
    }
}