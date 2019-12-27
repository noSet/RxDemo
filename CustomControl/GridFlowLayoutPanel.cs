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
        public int CellWidth { get; set; } = 200;

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

            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(control, nameof(control.MouseDown))
                .Where(p => p.EventArgs.Button == MouseButtons.Left);

            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(control, nameof(control.MouseMove))
                .Select(p => p.EventArgs.Location);

            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(control, nameof(control.MouseUp))
                .Where(p => p.EventArgs.Button == MouseButtons.Left);

            var elementBeginMove = mouseDown
                .Where(p => p.EventArgs.Location.Y <= 10)
                .Zip(mouseMove, (one, two) => one);

            var elementMoving = from start in mouseDown.Select(p => p.EventArgs.Location).Where(p => p.Y <= 10)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select new { X = process.X - start.X, Y = process.Y - start.Y };

            var elementEndMove = mouseUp
                .Where(p => p.EventArgs.Location.Y <= 10)
                .Zip(mouseMove, (one, two) => one);

            var elementResize = from start in mouseDown.Select(p => p.EventArgs.Location).Where(p => control.ClientRectangle.Right - p.X <= 5 && control.ClientRectangle.Bottom - p.Y <= 5)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select process;

            _disposables[control] = new[]
            {
                elementBeginMove.Subscribe(p => _layoutEngine.BeginDrag((Control)p.Sender)),

                elementMoving.Subscribe(p =>
                {
                    var localtion = control.Location;
                    localtion.Offset(p.X, p.Y);
                    control.Location = localtion;
                }),

                elementEndMove.Subscribe(p => _layoutEngine.EndDrag((Control)p.Sender)),

                elementResize.Subscribe(p => control.Size = new Size(p)),
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
        public GridFlowLayoutPanel Owner { get; }

        //public SortedSet<LayoutItem> Layouts { get; }

        private readonly Dictionary<string, LayoutItem> _controls = new Dictionary<string, LayoutItem>();

        public GridFlowLayoutEngine(GridFlowLayoutPanel owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
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

            Console.WriteLine(layoutEventArgs.AffectedProperty);

            //if (layoutEventArgs.AffectedProperty != "Bounds")
            //{
            //    return true;
            //}


            // 初始化
            if (object.ReferenceEquals(layoutEventArgs.AffectedControl, this.Owner))
            {
                for (int i = 0; i < this.Owner.Controls.Count; i++)
                {
                    Control control = this.Owner.Controls[i];

                    if (i == 0)
                    {
                        _controls[control.Name] = new LayoutItem { X = 0, Y = 0, Width = 2, Height = 1, Id = control.Name, ItemRef = control };
                    }
                    else if (i == 1)
                    {
                        _controls[control.Name] = new LayoutItem { X = 0, Y = 1, Width = 2, Height = 1, Id = control.Name, ItemRef = control };
                    }
                    else
                    {
                        _controls[control.Name] = new LayoutItem { X = 2, Y = 0, Width = 1, Height = 1, Id = control.Name, ItemRef = control };
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

            // 移动
            Control control1 = layoutEventArgs.AffectedControl;
            control1.BringToFront();

            if (control1.Name == "")
            {
                control1.Location = new Point(880, 880);
                return false;
            }

            var moveToItem = _controls[control1.Name];
            MoveItem(moveToItem, (int)Math.Round((double)control1.Location.X / this.Owner.CellWidth, 0), (int)Math.Round((double)control1.Location.Y / this.Owner.CellWidth));

            moveToItem.Moving = true;
            Compact();
            moveToItem.Moving = false;

            foreach (var item in _controls.Values)
            {
                Control control = item.ItemRef as Control;

                if (object.ReferenceEquals(control, control1))
                {
                    continue;
                }

                control.Location = new Point(item.X * this.Owner.CellWidth, item.Y * this.Owner.CellWidth);
                control.Size = new Size(item.Width * this.Owner.CellWidth, item.Height * this.Owner.CellWidth);
            }

            return false;
        }

        private readonly string _placeholder = Guid.NewGuid().ToString();

        public void BeginDrag(Control control)
        {
            Console.WriteLine(nameof(BeginDrag));
            var moveItem = _controls[control.Name];

            _controls[_placeholder] = moveItem.FakeItem(0, 0);
            _controls[_placeholder].ItemRef = new Panel { BackColor = Color.Black, Size = control.Size, Location = control.Location };
            this.Owner.Controls.Add(_controls[_placeholder].ItemRef as Panel);
        }

        public void EndDrag(Control control)
        {
            Console.WriteLine(nameof(EndDrag));
            this.Owner.Controls.Remove(_controls[_placeholder].ItemRef as Panel);
            _controls.Remove(_placeholder);
        }

        public void MoveItem(LayoutItem moveItem, int x, int y, bool isUserAction = true)
        {
            if (moveItem is null)
            {
                throw new ArgumentNullException(nameof(moveItem));
            }

            // 坐标相同不移动
            if (moveItem.X == x && moveItem.Y == y)
            {
                return;
            }

            int oldX = moveItem.X;
            int oldY = moveItem.Y;

            moveItem.X = x;
            moveItem.Y = y;
            moveItem.Moving = true;

            var needMoveItems = _controls.Values.Where(item => moveItem.IntersectsWith(item)).ToArray();

            foreach (var item in needMoveItems)
            {
                if (item.Moving)
                {
                    return;
                }

                if (isUserAction)
                {
                    var fakeItem = new LayoutItem()
                    {
                        X = item.X,
                        Y = Math.Max(moveItem.Y + moveItem.Height, 0),
                        Width = item.Width,
                        Height = item.Height,
                        Id = string.Empty
                    };

                    if (_controls.Values.FirstOrDefault(i => fakeItem.IntersectsWith(i)) != default(LayoutItem))
                    {
                        MoveItem(item, item.X, fakeItem.Y, false);
                        continue;
                    }
                }

                MoveItem(item, item.X, moveItem.Y + moveItem.Height, false);
            }

            moveItem.Moving = false;
        }

        public void Compact()
        {
            var sortLayout = _controls.Values.OrderBy(i => i, new LayoutItemComparer()).ToArray();
            foreach (var item in sortLayout)
            {
                if (item.Moving)
                {
                    continue;
                }

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
