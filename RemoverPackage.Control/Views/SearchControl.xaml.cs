using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using RemoverPackage.Control.ViewModels;
using Services.Contracts;

namespace RemoverPackage.Control
{
    public partial class SearchControl : UserControl
    {
        public SearchControl(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var regexService = ContainerProvider.Container.GetExportedValue<IRegexService>();
            ContainerProvider.Container.ComposeExportedValue<IRegexService>("regexService", regexService);
            ContainerProvider.Container.ComposeExportedValue<IServiceProvider>("serviceProvider", serviceProvider);

            var visualStudioService = ContainerProvider.Container.GetExportedValue<IVisualStudioService>();

            this.DataContext = new SearchViewModel(visualStudioService, regexService);
        }
    }
}