using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControl
{
    public partial class GridFlowLayoutPanel : Panel
    {
        private GridFlowLayoutEngine _layoutEngine;
        private readonly Dictionary<Control, IDisposable[]> _disposables = new Dictionary<Control, IDisposable[]>();

        [Browsable(true)]
        [DefaultValue(50)]
        public int CellWidth { get; set; } = 2;

        public GridFlowLayoutPanel()
        {
            InitializeComponent();
        }

        public GridFlowLayoutPanel(IContainer container)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.Add(this);

            InitializeComponent();
        }

        public override LayoutEngine LayoutEngine
        {
            get
            {
                _layoutEngine ??= new GridFlowLayoutEngine(this);

                return _layoutEngine;
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            Contract.Requires(e != null);
            base.OnControlAdded(e);
            Register(e.Control);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            Contract.Requires(e != null);
            base.OnControlRemoved(e);
            UnRegister(e.Control);
        }

        private void Register(Control control)
        {
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
                elementBeginMove.Subscribe(p => _layoutEngine.OnDragStart((Control)p.Sender)),

                elementMoving.Subscribe(p =>
                {
                    var localtion = control.Location;
                    localtion.Offset(p.X, p.Y);
                    control.Location = localtion;

                    _layoutEngine.OnDrag(control, localtion.X/ CellWidth, localtion.Y / CellWidth);
                }),

                elementEndMove.Subscribe(p => _layoutEngine.OnDragStop((Control)p.Sender)),

                elementBeginResize.Subscribe(p=> _layoutEngine.OnResizeStart((Control)p.Sender)),

                elementResize.Subscribe(p =>
                {
                    control.Size = new Size(p);

                    _layoutEngine.OnResize(control, control.Size.Width / CellWidth, control.Size.Height / CellWidth);
                }),

                elementEndResize.Subscribe(p => _layoutEngine.OnDragStop((Control)p.Sender)),
            };
        }

        private void UnRegister(Control control)
        {
            foreach (var disposable in _disposables[control])
            {
                disposable.Dispose();
            }

            _disposables.Remove(control);
        }
    }

    // This class demonstrates a simple custom layout engine.
    public class GridFlowLayoutEngine : LayoutEngine
    {
        public const string AffectedLayout = "Layout";

        public GridFlowLayoutPanel Owner { get; }

        public Panel Placeholder { get; }

        //public SortedSet<LayoutItem> Layouts { get; }

        private readonly Dictionary<string, LayoutItem> _controls = new Dictionary<string, LayoutItem>();

        public GridFlowLayoutEngine(GridFlowLayoutPanel owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
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
                foreach (var item in _controls.Values)
                {
                    Control control = item.ItemRef as Control;

                    control.Location = new Point(item.X * this.Owner.CellWidth, item.Y * this.Owner.CellWidth);
                    control.Size = new Size(item.Width * this.Owner.CellWidth, item.Height * this.Owner.CellWidth);
                }

                return false;
            }

            // 初始化
            if (object.ReferenceEquals(layoutEventArgs.AffectedControl, this.Owner))
            {
                this.Owner.Controls.Add(Placeholder);

                for (int i = 0; i < 3; i++)
                {
                    Control control = this.Owner.Controls[i];

                    if (i == 0)
                    {
                        _controls[control.Name] = new LayoutItem { X = 0, Y = 0, Width = 20, Height = 10, Id = control.Name, ItemRef = control };
                    }
                    else if (i == 1)
                    {
                        _controls[control.Name] = new LayoutItem { X = 0, Y = 10, Width = 20, Height = 10, Id = control.Name, ItemRef = control };
                    }
                    else
                    {
                        _controls[control.Name] = new LayoutItem { X = 20, Y = 0, Width = 5, Height = 5, Id = control.Name, ItemRef = control };
                    }
                }

                foreach (var item in _controls.Values)
                {
                    Control control = item.ItemRef as Control;

                    control.Location = new Point(item.X * this.Owner.CellWidth, item.Y * this.Owner.CellWidth);
                    control.Size = new Size(item.Width * this.Owner.CellWidth, item.Height * this.Owner.CellWidth);
                }

                return false;
            }

            return base.Layout(container, layoutEventArgs);
        }

        internal Control MovingControl { get; set; }

        internal void OnDragStart(Control control)
        {
            Debug.WriteLine(nameof(OnDragStart));

            var moveItem = _controls[control.Name];
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
                var moveItem = _controls[control.Name];
                if (MoveItem(moveItem, x, y, moveItem.Width, moveItem.Height))
                {
                    Compact();
                    Layout(this.Owner, new LayoutEventArgs(this.Owner, AffectedLayout));
                }
            }
        }

        internal void OnDragStop(Control control)
        {
            Debug.WriteLine(nameof(OnDragStop));

            var moveItem = _controls[control.Name];
            Placeholder.Visible = false;
            moveItem.ItemRef = control;
            MovingControl = null;

            control.Location = new Point(moveItem.X * this.Owner.CellWidth, moveItem.Y * this.Owner.CellWidth);
            control.Size = new Size(moveItem.Width * this.Owner.CellWidth, moveItem.Height * this.Owner.CellWidth);
        }

        internal void OnResizeStart(Control control)
        {
            Debug.WriteLine(nameof(OnResizeStart));

            var item = _controls[control.Name];
            Placeholder.Bounds = control.Bounds;
            Placeholder.Visible = true;
            item.ItemRef = Placeholder;
            control.BringToFront();
            MovingControl = control;
        }

        internal void OnResize(Control control, int width, int heigth)
        {
            Debug.WriteLine(nameof(OnDrag));

            if (object.ReferenceEquals(MovingControl, control))
            {
                var item = _controls[control.Name];
                if (MoveItem(item, item.X, item.Y, width, heigth))
                {
                    Compact();
                    Layout(this.Owner, new LayoutEventArgs(this.Owner, AffectedLayout));
                }
            }
        }

        internal void OnResizeStop(Control control)
        {
            Debug.WriteLine(nameof(OnDragStop));

            var item = _controls[control.Name];
            Placeholder.Visible = false;
            item.ItemRef = control;
            MovingControl = null;

            control.Location = new Point(item.X * this.Owner.CellWidth, item.Y * this.Owner.CellWidth);
            control.Size = new Size(item.Width * this.Owner.CellWidth, item.Height * this.Owner.CellWidth);
        }

        public bool MoveItem(LayoutItem moveItem, int x, int y, int width, int height)
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
            IEnumerable<LayoutItem> needMoveItems = _controls.Values
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

                if (!_controls.Values.Any(i => fakeItem.IntersectsWith(i)))
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

        public void Compact()
        {
            var sortLayout = _controls.Values.OrderBy(i => i, new LayoutItemComparer()).ToArray();
            foreach (var item in sortLayout)
            {
                var fakeItem = item.FakeItem(item.X, 0);

                while (_controls.Values.FirstOrDefault(i => i != item && i.IntersectsWith(fakeItem)) != null)
                {
                    fakeItem.Y++;
                }

                item.CoverXY(fakeItem);
            }
        }
    }
}
