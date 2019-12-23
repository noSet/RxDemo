using System.Collections.Generic;

namespace CustomControl
{
    public class LayoutItemComparer : IComparer<LayoutItem>
    {
        public LayoutItemComparer()
        {
        }

        public int Compare(LayoutItem x, LayoutItem y)
        {
            if (x.Y > y.Y || (x.Y == y.Y && x.X > y.X))
            {
                return 1;
            }
            else if (x.Y == y.Y && x.X == y.X)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
