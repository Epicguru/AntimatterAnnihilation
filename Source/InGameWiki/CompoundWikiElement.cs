using System.Collections.Generic;

namespace InGameWiki
{
    public abstract class CompoundWikiElement : WikiElement
    {
        public List<WikiElement> Elements = new List<WikiElement>();
    }
}
