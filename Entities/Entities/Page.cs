using System.Collections.Generic;
using EnvDTE;

namespace Entities
{
    public class Page
    {
        public string Name { get; set; }

        public ProjectItem Item { get; set; }

        public string Content { get; set; }

        public IList<Page> Parents { get; set; }

        public Dictionary<string, int> References { get; set; }
    }
}