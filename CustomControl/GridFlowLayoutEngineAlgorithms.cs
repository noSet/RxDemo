using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace CustomControl
{
    internal class GridFlowLayoutEngineAlgorithms
    {
        internal IEnumerable<LayoutItem> LayoutItems { get; }

        public GridFlowLayoutEngineAlgorithms(IEnumerable<LayoutItem> layoutItems)
        {
            LayoutItems = layoutItems ?? throw new ArgumentNullException(nameof(layoutItems));
        }

        public void InitLayout()
        {
            foreach (var item in LayoutItems.OrderBy(p => p, new LayoutItemComparer()))
            {
                int x = item.X;
                int y = item.Y;
                int width = item.Width;
                int height = item.Height;

                item.Init();

                Update(item, x, y, width, height);
            }
        }

        public bool Update(LayoutItem item, int x, int y, int width, int height)
        {
            if (MoveItem(item, x, y, width, height))
            {
                Compact();
                return true;
            }

            return false;
        }

        private bool MoveItem(LayoutItem moveItem, int x, int y, int width, int height)
        {
            if (moveItem is null)
            {
                throw new ArgumentNullException(nameof(moveItem));
            }

            // 坐标相同不移动
            if (moveItem.X == x && moveItem.Y == y && moveItem.Width == width && moveItem.Height == height)
            {
                return false;
            }

            moveItem.X = x;
            moveItem.Y = y;
            moveItem.Width = width;
            moveItem.Height = height;

            // 这里先排序，决定了块移动的顺序
            IEnumerable<LayoutItem> needMoveItems = LayoutItems
                .OrderBy(p => p, new LayoutItemComparer())
                .Where(item => moveItem.IntersectsWith(item));

            foreach (var item in needMoveItems)
            {
                var fakeItem = new LayoutItem()
                {
                    X = item.X,
                    Y = Math.Max(moveItem.Y - item.Height, 0),
                    Width = item.Width,
                    Height = item.Height,
                    Id = string.Empty
                };

                if (!LayoutItems.Any(i => fakeItem.IntersectsWith(i)))
                {
                    MoveItem(item, fakeItem.X, fakeItem.Y, fakeItem.Width, fakeItem.Height);
                }
                else
                {
                    MoveItem(item, item.X, moveItem.Y + moveItem.Height, item.Width, item.Height);
                }
            }

            return true;
        }

        private void Compact()
        {
            var sortLayout = LayoutItems.OrderBy(i => i, new LayoutItemComparer()).ToArray();

            foreach (var item in sortLayout)
            {
                item.Y = 0;

                // 查找第一个碰撞的元素（排除和自己碰撞）
                var first = LayoutItems.FirstOrDefault(i => i != item && i.IntersectsWith(item));

                while (first != null)
                {
                    item.Y = first.Y + first.Height;

                    first = LayoutItems.FirstOrDefault(i => i != item && i.IntersectsWith(item));
                }
            }
        }
    }
}

