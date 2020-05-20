using UnityEngine;

namespace InGameWiki.Internal
{
    public static class Extensions
    {
        public static Rect GetInner(this Rect rect, float margin = 5f)
        {
            return new Rect(rect.x + margin, rect.y + margin, rect.width - margin * 2, rect.height - margin * 2);
        }
    }
}
