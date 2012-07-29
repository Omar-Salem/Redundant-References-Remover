using System.Collections.Generic;

namespace Entities
{
    public class ParentChildDuplicates
    {
        public Page Parent { get; set; }

        public Page Child { get; set; }

        public IEnumerable<string> Duplicates { get; set; }
    }
}