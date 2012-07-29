namespace RemoverPackage.Control
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using Services.Contracts;
    using Services.Implementation;

    internal static class ContainerProvider
    {
        private static CompositionContainer container;

        public static CompositionContainer Container
        {
            get
            {
                if (container == null)
                {
                    List<AssemblyCatalog> catalogList = new List<AssemblyCatalog>();
                    catalogList.Add(new AssemblyCatalog(typeof(IPagesService).Assembly));
                    catalogList.Add(new AssemblyCatalog(typeof(WebFormPagesService).Assembly));
                    container = new CompositionContainer(new AggregateCatalog(catalogList));
                }

                return container;
            }
        }
    }
}