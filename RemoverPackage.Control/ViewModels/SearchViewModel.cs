namespace RemoverPackage.Control.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Entities;
    using RemoverPackage.Control.Commands;
    using Services.Contracts;
    using Services.Implementation;
    using EnvDTE;
using System.Collections.Generic;

    public class SearchViewModel : ViewModelBase
    {
        #region Member Variables

        private readonly IVisualStudioService _visualStudioService;
        private readonly IRegexService _regexService;
        private ObservableCollection<Page> _duplicatesInSameFile;
        private ObservableCollection<ParentChildDuplicates> _duplicatesInLayoutAndFile;

        #endregion Member Variables

        #region Constructor

        public SearchViewModel(IVisualStudioService visualStudioService, IRegexService regexService)
        {
            this._visualStudioService = visualStudioService;
            this._regexService = regexService;
            DuplicatesInLayoutAndFile = new ObservableCollection<ParentChildDuplicates>();
            DuplicatesInSameFile = new ObservableCollection<Page>();
        }

        #endregion Constructor

        #region Public Properties

        public ObservableCollection<Page> DuplicatesInSameFile
        {
            get { return _duplicatesInSameFile; }
            set
            {
                if (_duplicatesInSameFile != value)
                {
                    _duplicatesInSameFile = value;
                    OnPropertyChanged("DuplicatesInSameFile");
                }
            }
        }

        public ObservableCollection<ParentChildDuplicates> DuplicatesInLayoutAndFile
        {
            get { return _duplicatesInLayoutAndFile; }
            set
            {
                if (_duplicatesInLayoutAndFile != value)
                {
                    _duplicatesInLayoutAndFile = value;
                    OnPropertyChanged("DuplicatesInLayoutAndFile");
                }
            }
        }

        #endregion Public Properties

        #region Commands

        public ICommand FindDuplicates
        {
            get { return new RelayCommand(FindDuplicatesExecute, CanFindDuplicatesExecute); }
        }

        public ICommand RemoveDuplicates
        {
            get { return new RelayCommand(RemoveDuplicatesExecute, CanRemoveDuplicatesExecute); }
        }

        #endregion Commands

        #region Commands Methods

        private void FindDuplicatesExecute()
        {
            var webProject = _visualStudioService.GetWebProject();

            if (webProject == null)
            {
                return;
            }


            var pagesService = GetPageService(webProject);
            var pages = _visualStudioService.GetPages(webProject.Project.ProjectItems);
            pagesService.ProcessPages(pages);


            var pagesWithDuplicatedReferences = pagesService.GetDuplicatesInSameFile(pages);
            DuplicatesInSameFile = new ObservableCollection<Page>(pagesWithDuplicatedReferences);

            var pagesWithDuplicatesInLayout = pagesService.GetDuplicatesInLayoutAndFile(pages);
            DuplicatesInLayoutAndFile = new ObservableCollection<ParentChildDuplicates>(pagesWithDuplicatesInLayout);
        }

        private bool CanFindDuplicatesExecute()
        {
            return true;
        }

        private void RemoveDuplicatesExecute()
        {
            foreach (var item in _duplicatesInSameFile)
            {
                foreach (var reference in item.References.Keys)
                {
                    int idx = item.Content.IndexOf(reference);
                    var result = _visualStudioService.RemoveReference(item.Item, reference, idx + 1);
                }
            }

            foreach (var item in _duplicatesInLayoutAndFile)
            {
                foreach (var reference in item.Duplicates)
                {
                    var result = _visualStudioService.RemoveReference(item.Child.Item, reference, 1);
                }
            }

            DuplicatesInSameFile.Clear();
            DuplicatesInLayoutAndFile.Clear();
        }

        private bool CanRemoveDuplicatesExecute()
        {
            return DuplicatesInSameFile.Count != 0 || DuplicatesInLayoutAndFile.Count != 0;
        }

        #endregion Commands Methods

        #region Private Methods

        private IPagesService GetPageService(WebProject webProject)
        {
            IPagesService pageService = null;

            switch (webProject.ProjectType)
            {
                case ProjectType.MVC2:
                    pageService = new MVC2PagesService(_regexService);
                    break;

                case ProjectType.MVC3:
                    pageService = new MVC3PagesService(_regexService);
                    break;

                case ProjectType.WebForm:
                    pageService = new WebFormPagesService(_regexService);
                    break;
            }

            return pageService;
        }

        #endregion Private Methods
    }
}