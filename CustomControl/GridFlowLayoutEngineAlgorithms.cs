using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace CustomControl
{
    /// <summary>
    /// 栅格流布局引擎算法
    /// </summary>
    internal class GridFlowLayoutEngineAlgorithms
    {
        /// <summary>
        /// 布局的块列表
        /// </summary>
        internal IEnumerable<LayoutItem> LayoutItems { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="layoutItems">布局的块列表</param>
        public GridFlowLayoutEngineAlgorithms(IEnumerable<LayoutItem> layoutItems)
        {
            LayoutItems = layoutItems ?? throw new ArgumentNullException(nameof(layoutItems));
        }

        /// <summary>
        /// 初始化布局
        /// 简单的布局，先将块排序，然后依次对排序好的块调用<see cref="Update(LayoutItem, int, int, int, int)"/>方法（挤开碰撞的块）
        /// </summary>
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

        /// <summary>
        /// 更新块布局
        /// </summary>
        /// <param name="item">要更新的块</param>
        /// <param name="x">坐标X</param>
        /// <param name="y">坐标Y</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>是否更新</returns>
        public bool Update(LayoutItem item, int x, int y, int width, int height)
        {
            if (MoveItem(item, x, y, width, height))
            {
                Compact();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移动块
        /// </summary>
        /// <param name="moveItem">要移动的块</param>
        /// <param name="x">坐标X</param>
        /// <param name="y">坐标Y</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>是否更新</returns>
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

        /// <summary>
        /// 将布局紧凑
        /// </summary>
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

