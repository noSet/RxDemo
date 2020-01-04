using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControl
{
    public partial class GridFlowLayoutEngine : LayoutEngine
    {
        public const string AffectedLayout = "Layout";

        private readonly Dictionary<Control, IDisposable[]> _disposables = new Dictionary<Control, IDisposable[]>();

        internal GridFlowLayoutPanel Owner { get; }

        internal Panel Placeholder { get; }

        // todo 因为这里是控件映射，若控件Name属性变更时如果同步
        internal Dictionary<Control, LayoutItem> LayoutItems { get; }

        internal GridFlowLayoutEngineAlgorithms GridFlowLayoutEngineAlgorithms { get; }

        internal Control MovingControl { get; set; }

        public GridFlowLayoutEngine(GridFlowLayoutPanel owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            LayoutItems = new Dictionary<Control, LayoutItem>();
            GridFlowLayoutEngineAlgorithms = new GridFlowLayoutEngineAlgorithms(LayoutItems.Values);
            Placeholder = new Panel { BackColor = Color.Black, Visible = false };
        }

        public override void InitLayout(object child, BoundsSpecified specified)
        {
            base.InitLayout(child, specified);
        }

        public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
        {
            Debug.Assert(object.ReferenceEquals(this.Owner, container));
            Contract.Requires(container != null);
            Contract.Requires(layoutEventArgs != null);

            Debug.WriteLine(layoutEventArgs.AffectedProperty);

            if (layoutEventArgs.AffectedProperty == AffectedLayout)
            {
                try
                {
                    this.Owner.SuspendLayout();

                    foreach (var item in LayoutItems.Values)
                    {
                        Control control = item.ItemRef as Control;

                        control.Location = new Point(item.X * this.Owner.CellWidth + 5, item.Y * this.Owner.CellWidth + 5);
                        control.Size = new Size(item.Width * this.Owner.CellWidth - 10, item.Height * this.Owner.CellWidth - 10);
                    }

                    return false;
                }
                finally
                {
                    this.Owner.ResumeLayout(true);
                }
            }

            //// 初始化
            //if (object.ReferenceEquals(layoutEventArgs.AffectedControl, this.Owner))
            //{
            //    this.Owner.Controls.Add(Placeholder);

            //    for (int i = 0; i < 3; i++)
            //    {
            //        Control control = this.Owner.Controls[i];

            //        if (i == 0)
            //        {
            //            LayoutItems[control] = new LayoutItem { X = 0, Y = 0, Width = 10, Height = 5, Id = control.Name, ItemRef = control };
            //        }
            //        else if (i == 1)
            //        {
            //            LayoutItems[control] = new LayoutItem { X = 0, Y = 5, Width = 10, Height = 5, Id = control.Name, ItemRef = control };
            //        }
            //        else
            //        {
            //            LayoutItems[control] = new LayoutItem { X = 10, Y = 0, Width = 5, Height = 5, Id = control.Name, ItemRef = control };
            //        }
            //    }

            //    foreach (var item in LayoutItems.Values)
            //    {
            //        Control control = item.ItemRef as Control;

            //        control.Location = new Point(item.X * this.Owner.CellWidth + 5, item.Y * this.Owner.CellWidth + 5);
            //        control.Size = new Size(item.Width * this.Owner.CellWidth - 10, item.Height * this.Owner.CellWidth - 10);
            //    }

            //    return false;
            //}

            return base.Layout(container, layoutEventArgs);
        }

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
            Layout(this.Owner, new LayoutEventArgs(this.Owner, AffectedLayout));
        }

        internal void OnRegister(Control control)
        {
            Debug.WriteLine(nameof(OnRegister));

            if (object.ReferenceEquals(control, Placeholder))
            {
                return;
            }

            const int dragRectangle = 10;

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
                .Zip(mouseMove, (one, two) => one);

            // 鼠标左键点击标题部位，并且按住不放移动鼠标，左键弹起结束
            var elementMoving = from start in mouseDown.Select(p => p.EventArgs.Location).Where(p => p.Y <= dragRectangle)
                                from process in mouseMove.TakeUntil(mouseUp)
                                where start != process
                                select new { X = process.X - start.X, Y = process.Y - start.Y };

            // 鼠标移动过程中左键弹起
            var elementEndMove = mouseUp
                .Where(p => p.EventArgs.Location.Y <= dragRectangle)
                .Zip(mouseMove, (one, two) => one);

            var elementBeginResize = mouseDown
                .Where(p => control.ClientRectangle.Right - p.EventArgs.Location.X <= 5 && control.ClientRectangle.Bottom - p.EventArgs.Location.Y <= 5)
                .Zip(mouseMove, (one, two) => one);


            // 鼠标左键点击右下角调整大小并且鼠标在移动
            var elementResize = from start in mouseDown.Select(p => p.EventArgs.Location).Where(p => control.ClientRectangle.Right - p.X <= 5 && control.ClientRectangle.Bottom - p.Y <= 5)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select process;

            var elementEndResize = mouseUp
                .Where(p => control.ClientRectangle.Right - p.EventArgs.Location.X <= 5 && control.ClientRectangle.Bottom - p.EventArgs.Location.Y <= 5)
                .Zip(mouseMove, (one, two) => one);

            _disposables[control] = new[]
            {
                elementBeginMove.Subscribe(p => OnDragStart((Control)p.Sender)),

                elementMoving.Subscribe(p =>
                {
                    var localtion = control.Location;
                    localtion.Offset(p.X, p.Y);
                    control.Location = localtion;

                    OnDrag(control, Round(localtion.X), Round(localtion.Y));
                }),

                elementEndMove.Subscribe(p => OnDragStop((Control)p.Sender)),

                elementBeginResize.Subscribe(p=> OnResizeStart((Control)p.Sender)),

                elementResize.Subscribe(p =>
                {
                    control.Size = new Size(p);

                    OnResize(control, Round(control.Size.Width), Round(control.Size.Height));
                }),

                elementEndResize.Subscribe(p => OnResizeStop((Control) p.Sender)),
            };

            var newItem = new LayoutItem
            {
                Id = control.Name,
                ItemRef = control,
            };

            newItem.Init();

            LayoutItems[control] = newItem;

            int Round(int num)
            {
                var quotient = Math.DivRem(num, Owner.CellWidth, out var remainder);
                return ((double)remainder / Owner.CellWidth) > 0.25 ? quotient + 1 : quotient;
            }

            ChangeItem(newItem, 0, 0, 1, 1);
        }

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
        }

        internal void OnDragStart(Control control)
        {
            Debug.WriteLine(nameof(OnDragStart));

            var moveItem = LayoutItems[control];
            Placeholder.Bounds = control.Bounds;
            Placeholder.Visible = true;
            moveItem.ItemRef = Placeholder;
            control.BringToFront();
            MovingControl = control;
        }

        internal void OnDrag(Control control, int x, int y)
        {
            Debug.WriteLine(nameof(OnDrag));

            if (object.ReferenceEquals(MovingControl, control))
            {
                var moveItem = LayoutItems[control];
                ChangeItem(moveItem, x, y, moveItem.Width, moveItem.Height);
            }
        }

        internal void OnDragStop(Control control)
        {
            Debug.WriteLine(nameof(OnDragStop));

            var item = LayoutItems[control];
            Placeholder.Visible = false;
            item.ItemRef = control;
            MovingControl = null;

            control.Location = new Point(item.X * this.Owner.CellWidth + 5, item.Y * this.Owner.CellWidth + 5);
            control.Size = new Size(item.Width * this.Owner.CellWidth - 10, item.Height * this.Owner.CellWidth - 10);
        }

        internal void OnResizeStart(Control control)
        {
            Debug.WriteLine(nameof(OnResizeStart));

            var item = LayoutItems[control];
            Placeholder.Bounds = control.Bounds;
            Placeholder.Visible = true;
            item.ItemRef = Placeholder;
            control.BringToFront();
            MovingControl = control;
        }

        internal void OnResize(Control control, int width, int height)
        {
            Debug.WriteLine(nameof(OnDrag));

            if (object.ReferenceEquals(MovingControl, control))
            {
                var item = LayoutItems[control];
                ChangeItem(item, item.X, item.Y, width, height);
            }
        }

        internal void OnResizeStop(Control control)
        {
            Debug.WriteLine(nameof(OnDragStop));

            var item = LayoutItems[control];
            Placeholder.Visible = false;
            item.ItemRef = control;
            MovingControl = null;

            control.Location = new Point(item.X * this.Owner.CellWidth + 5, item.Y * this.Owner.CellWidth + 5);
            control.Size = new Size(item.Width * this.Owner.CellWidth - 10, item.Height * this.Owner.CellWidth - 10);
        }

        private void ChangeItem(LayoutItem item, int x, int y, int width, int height)
        {
            x = Math.Max(x, 0);
            y = Math.Max(y, 0);
            width = Math.Max(width, 1);
            height = Math.Max(height, 1);

            if (GridFlowLayoutEngineAlgorithms.Update(item, x, y, width, height))
            {
                Layout(this.Owner, new LayoutEventArgs(this.Owner, AffectedLayout));
            }
        }
    }
}
