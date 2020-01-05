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
            if (MoveItem(item, x, y, width, height, true))
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
        /// <param name="master">主碰撞</param>
        /// <returns>是否更新</returns>
        private bool MoveItem(LayoutItem moveItem, int x, int y, int width, int height, bool master)
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

            var downMove = y - moveItem.Y > 0;

            moveItem.X = x;
            moveItem.Y = y;
            moveItem.Width = width;
            moveItem.Height = height;

            // 这里先排序，决定了块移动的顺序
            IEnumerable<LayoutItem> needMoveItems;

            if (downMove)
            {
                needMoveItems = LayoutItems
                   .OrderBy(p => p, new LayoutItemComparer())
                   .Where(item => moveItem.IntersectsWith(item));
            }
            else
            {
                needMoveItems = LayoutItems
                    .OrderByDescending(p => p, new LayoutItemComparer())
                    .Where(item => moveItem.IntersectsWith(item));
            }

            foreach (var item in needMoveItems)
            {
                // 只有主碰撞的时候，元素向下移动时，判断下方的元素是否可以填充到上方
                if (master && downMove)
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
                        MoveItem(item, fakeItem.X, fakeItem.Y, fakeItem.Width, fakeItem.Height, false);
                        continue;
                    }
                }

                // todo 这里还有个BUG，若item元素下方有元素比item矮，moveItem.Y + moveItem.Height的时候，压缩时会把矮的元素也移动到上方
                MoveItem(item, item.X, moveItem.Y + moveItem.Height, item.Width, item.Height, false);
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
                // 这里先一格一格往上移判断是否有碰撞，若有碰撞，则停留在当前位置
                LayoutItem fisrt = null;

                do
                {
                    item.Y--;
                    fisrt = sortLayout.FirstOrDefault(i => i != item && i.IntersectsWith(item));
                } while (fisrt is null && item.Y >= 0);

                item.Y++;
            }
        }
    }
}

