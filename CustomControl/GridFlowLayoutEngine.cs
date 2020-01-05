using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControl
{
    /// <summary>
    /// 栅格流布局引擎
    /// </summary>
    public partial class GridFlowLayoutEngine : LayoutEngine
    {
        public const string AffectedLayout = "Layout";

        /// <summary>
        /// 需要释放的控件资源
        /// </summary>
        private readonly Dictionary<Control, IDisposable[]> _disposables = new Dictionary<Control, IDisposable[]>();

        internal GridFlowLayoutPanel Owner { get; }

        internal Panel Placeholder { get; }

        /// <summary>
        /// 控件和控件位置的映射
        /// todo 因为这里是控件映射，若控件Name属性变更时如果同步
        /// </summary>
        internal Dictionary<Control, LayoutItem> LayoutItems { get; }

        /// <summary>
        /// 栅格流布局引擎算法
        /// </summary>
        internal GridFlowLayoutEngineAlgorithms GridFlowLayoutEngineAlgorithms { get; }

        /// <summary>
        /// 正在更新位置或者大小的控件
        /// </summary>
        internal Control UpdatingControl { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">该布局引擎的所属控件</param>
        public GridFlowLayoutEngine(GridFlowLayoutPanel owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            LayoutItems = new Dictionary<Control, LayoutItem>();
            GridFlowLayoutEngineAlgorithms = new GridFlowLayoutEngineAlgorithms(LayoutItems.Values);
            Placeholder = new Panel { Name = Guid.NewGuid().ToString(), BackColor = Color.Black, Visible = false };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="specified"></param>
        public override void InitLayout(object child, BoundsSpecified specified)
        {
            base.InitLayout(child, specified);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="layoutEventArgs"></param>
        /// <returns></returns>
        public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
        {
            Contract.Requires(container != null);
            Contract.Requires(layoutEventArgs != null);

            Debug.WriteLine(layoutEventArgs.AffectedProperty);

            if (layoutEventArgs.AffectedProperty == AffectedLayout)
            {
                try
                {
                    Owner.SuspendLayout();

                    foreach (var item in LayoutItems.Values)
                    {
                        ApplyLayoutItem(item);
                    }

                    return false;
                }
                finally
                {
                    Owner.ResumeLayout(true);
                }
            }

            // 初始化
            if (object.ReferenceEquals(layoutEventArgs.AffectedControl, Owner))
            {
                Owner.SuspendLayout();
                Owner.Controls.Add(Placeholder);
                Owner.ResumeLayout(false);
            }

            return base.Layout(container, layoutEventArgs);
        }

        /// <summary>
        /// 初始化（加载）布局，这个方法必须在设计器代码执行完后调用，确保<see cref="Control.Name"/>已经被赋值
        /// </summary>
        /// <param name="layoutItems">要加载的布局</param>
        internal void OnInitLayout(IEnumerable<LayoutItem> layoutItems)
        {
            Debug.WriteLine(nameof(OnInitLayout));

            foreach (var kv in LayoutItems)
            {
                var item = kv.Value;

                item.Id = kv.Key.Name;
                item.Init();

                var i = layoutItems.FirstOrDefault(i => i.Id == item.Id);

                if (i != null)
                {
                    item.Cover(i);
                }
            }

            foreach (var item in LayoutItems.Values.Where(i => i.X == -1 && i.Y == -1))
            {
                item.X = 0;
                item.Y = LayoutItems.Values.Max(i => i.Y + i.Height);
            }

            GridFlowLayoutEngineAlgorithms.InitLayout();
            Layout(Owner, new LayoutEventArgs(Owner, AffectedLayout));
        }

        /// <summary>
        /// 控件被添加至<see cref="Owner"/>时调用此方法，此方法注册了控件的移动和调整大小事件，生成控件的<see cref="LayoutItem"/>对象，并且将新加入的控件移动到左上角
        /// </summary>
        /// <param name="control">要注册的控件</param>
        internal void OnRegister(Control control)
        {
            Debug.WriteLine(nameof(OnRegister));

            if (object.ReferenceEquals(control, Placeholder))
            {
                return;
            }

            const int dragRectangle = 10;

            int moving = 0;
            int resizing = 0;

            // 不必要使用Interlocked.CompareExchange，因为都在UI线程上执行

            // 鼠标左键点击
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(control, nameof(control.MouseDown))
                .Where(p => p.EventArgs.Button == MouseButtons.Left);

            // 鼠标移动
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(control, nameof(control.MouseMove))
                .Select(p => p.EventArgs.Location);

            // 鼠标左键弹起
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(control, nameof(control.MouseUp))
                .Where(p => p.EventArgs.Button == MouseButtons.Left);

            // 鼠标左键点击标题部位，并且鼠标有移动
            var elementBeginMove = mouseDown
                .Where(p => p.EventArgs.Location.Y <= dragRectangle)
                .Zip(mouseMove, (one, two) => one)
                .Where(p => Interlocked.CompareExchange(ref moving, 1, 0) == 0);

            // 鼠标左键点击标题部位，并且按住不放移动鼠标，左键弹起结束
            var elementMoving = from start in mouseDown.Select(p => p.EventArgs.Location).Where(p => p.Y <= dragRectangle)
                                from process in mouseMove.TakeUntil(mouseUp)
                                where start != process
                                select new { X = process.X - start.X, Y = process.Y - start.Y };

            // 鼠标移动过程中左键弹起
            var elementEndMove = mouseUp
                .Where(p => Interlocked.CompareExchange(ref moving, 0, 1) == 1);

            // 鼠标左键点击调整大小位置，并且移动鼠标
            var elementBeginResize = mouseDown
                .Where(p => control.ClientRectangle.Right - p.EventArgs.Location.X <= 5 && control.ClientRectangle.Bottom - p.EventArgs.Location.Y <= 5)
                .Zip(mouseMove, (one, two) => one)
                .Where(p => Interlocked.CompareExchange(ref resizing, 1, 0) == 0);

            // 鼠标左键点击右下角调整大小并且鼠标在移动
            var elementResize = from start in mouseDown.Select(p => p.EventArgs.Location).Where(p => control.ClientRectangle.Right - p.X <= 5 && control.ClientRectangle.Bottom - p.Y <= 5)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select process;

            var elementEndResize = mouseUp
                .Where(p => Interlocked.CompareExchange(ref resizing, 0, 1) == 1);

            _disposables[control] = new[]
            {
                elementBeginMove.Subscribe(p => OnDragStart((Control)p.Sender)),

                elementMoving.Subscribe(p =>
                {
                    var x = Math.Max(control.Location.X + p.X, Owner.CellMargin);
                    var y = Math.Max(control.Location.Y + p.Y, Owner.CellMargin);
                    control.Location = new Point(Math.Max(control.Location.X + p.X, Owner.CellMargin), Math.Max(control.Location.Y + p.Y, Owner.CellMargin));

                    OnDrag(control, Round(x), Round(y));
                }),

                elementEndMove.Subscribe(p => OnDragStop((Control)p.Sender)),

                elementBeginResize.Subscribe(p => OnResizeStart((Control)p.Sender)),

                elementResize.Subscribe(p =>
                {
                    var x = Math.Max(p.X, Owner.MinCellWidth * Owner.CellPixel - 2 * Owner.CellMargin);
                    var y = Math.Max(p.Y, Owner.MinCellHeight * Owner.CellPixel - 2 * Owner.CellMargin);
                    control.Size = new Size(x, y);

                    OnResize(control, Round(control.Size.Width), Round(control.Size.Height));
                }),

                elementEndResize.Subscribe(p => OnResizeStop((Control) p.Sender)),
            };

            // 设计器中的控件如果被添加，执行此行代码时候，Name属性为空，通过OnInitLayout方法给LayoutItem.Name赋值
            // 设计器外的控件添加，确保添加的控件已经赋值了Name属性
            var newItem = new LayoutItem
            {
                Id = control.Name,
                ItemRef = control,
            };

            newItem.Init();

            LayoutItems[control] = newItem;

            int Round(int num)
            {
                var quotient = Math.DivRem(num, Owner.CellPixel, out var remainder);
                return ((double)remainder / Owner.CellPixel) > 0.25 ? quotient + 1 : quotient;
            }

            ChangeItem(newItem, 0, 0, Owner.MinCellWidth, Owner.MinCellHeight);
        }

        /// <summary>
        /// 控件从<see cref="Owner"/>注销时调用此方法，此方法注销了控件的移动和调整大小事件，并且注销控件的<see cref="LayoutItem"/>对象，并且重新布局
        /// </summary>
        /// <param name="control">要注册的控件</param>
        internal void OnUnRegister(Control control)
        {
            Debug.WriteLine(nameof(OnUnRegister));

            if (object.ReferenceEquals(control, Placeholder))
            {
                return;
            }

            foreach (var disposable in _disposables[control])
            {
                disposable.Dispose();
            }

            _disposables.Remove(control);

            LayoutItems.Remove(control);

            // 取最后一个，确保这个布局再重置成-1后也能引起变动（因为Update里会判断xywh是否相同，若是第一个元素是个默认布局就不会出发变动）
            var item = LayoutItems.Values.OrderByDescending(p => p, new LayoutItemComparer()).FirstOrDefault();

            if (item != null)
            {
                var x = item.X;
                var y = item.Y;
                var width = item.Width;
                var height = item.Height;

                item.Init();
                ChangeItem(item, x, y, width, height);
            }
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="control"></param>
        internal void OnDragStart(Control control)
        {
            Debug.WriteLine(nameof(OnDragStart));

            var moveItem = LayoutItems[control];
            Placeholder.Bounds = control.Bounds;
            Placeholder.Visible = true;
            moveItem.ItemRef = Placeholder;
            control.BringToFront();
            UpdatingControl = control;
        }

        /// <summary>
        /// 拖拽
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        internal void OnDrag(Control control, int x, int y)
        {
            Debug.WriteLine(nameof(OnDrag));

            if (object.ReferenceEquals(UpdatingControl, control))
            {
                var moveItem = LayoutItems[control];
                ChangeItem(moveItem, x, y, moveItem.Width, moveItem.Height);
            }
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        /// <param name="control"></param>
        internal void OnDragStop(Control control)
        {
            Debug.WriteLine(nameof(OnDragStop));

            var item = LayoutItems[control];
            Placeholder.Visible = false;
            item.ItemRef = control;
            UpdatingControl = null;

            ApplyLayoutItem(item);
        }

        /// <summary>
        /// 开始调整大小
        /// </summary>
        /// <param name="control"></param>
        internal void OnResizeStart(Control control)
        {
            Debug.WriteLine(nameof(OnResizeStart));

            var item = LayoutItems[control];
            Placeholder.Bounds = control.Bounds;
            Placeholder.Visible = true;
            item.ItemRef = Placeholder;
            control.BringToFront();
            UpdatingControl = control;
        }

        /// <summary>
        /// 调整大小
        /// </summary>
        /// <param name="control"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal void OnResize(Control control, int width, int height)
        {
            Debug.WriteLine(nameof(OnResize));

            if (object.ReferenceEquals(UpdatingControl, control))
            {
                var item = LayoutItems[control];
                ChangeItem(item, item.X, item.Y, width, height);
            }
        }

        /// <summary>
        /// 结束调整大小
        /// </summary>
        /// <param name="control"></param>
        internal void OnResizeStop(Control control)
        {
            Debug.WriteLine(nameof(OnResizeStop));

            var item = LayoutItems[control];
            Placeholder.Visible = false;
            item.ItemRef = control;
            UpdatingControl = null;

            ApplyLayoutItem(item);
        }

        /// <summary>
        /// 改变控件位置
        /// </summary>
        /// <param name="item"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void ChangeItem(LayoutItem item, int x, int y, int width, int height)
        {
            x = Math.Max(x, 0);
            y = Math.Max(y, 0);
            width = Math.Max(width, Owner.MinCellWidth);
            height = Math.Max(height, Owner.MinCellHeight);

            if (GridFlowLayoutEngineAlgorithms.Update(item, x, y, width, height))
            {
                Layout(Owner, new LayoutEventArgs(Owner, AffectedLayout));
            }
        }

        /// <summary>
        /// 应用控件大小
        /// </summary>
        /// <param name="item"></param>
        private void ApplyLayoutItem(LayoutItem item)
        {
            if (item.ItemRef is Control control)
            {
                control.Location = new Point(item.X * Owner.CellPixel + Owner.CellMargin, item.Y * Owner.CellPixel + Owner.CellMargin);
                control.Size = new Size(item.Width * Owner.CellPixel - 2 * Owner.CellMargin, item.Height * Owner.CellPixel - 2 * Owner.CellMargin);
            }
        }
    }
}
