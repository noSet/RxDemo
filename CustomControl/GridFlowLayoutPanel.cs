using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControl
{
    public partial class GridFlowLayoutPanel : Panel
    {
        private GridFlowLayoutEngine _layoutEngine;

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
                _layoutEngine ??= new GridFlowLayoutEngine(this, new LayoutItem[0]);

                return _layoutEngine;
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
        }
    }

    // This class demonstrates a simple custom layout engine.
    public class GridFlowLayoutEngine : LayoutEngine
    {
        public GridFlowLayoutPanel Owner { get; }

        public SortedSet<LayoutItem> Layouts { get; }

        private Dictionary<Control, LayoutItem> _controls = new Dictionary<Control, LayoutItem>();

        public GridFlowLayoutEngine(GridFlowLayoutPanel owner, IEnumerable<LayoutItem> layouts)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));

            if (layouts is null)
            {
                throw new ArgumentNullException(nameof(layouts));
            }

            Layouts = new SortedSet<LayoutItem>(layouts, new LayoutItemComparer());
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

            // 初始化
            if (object.ReferenceEquals(layoutEventArgs.AffectedControl, this.Owner))
            {
                for (int i = 0; i < this.Owner.Controls.Count; i++)
                {
                    Control control = this.Owner.Controls[i];

                    if (i == 0)
                    {
                        Layouts.Add(new LayoutItem { X = 0, Y = 0, Width = 2, Height = 1, Id = control.Name, ItemRef = control });
                    }
                    else if (i == 1)
                    {
                        Layouts.Add(new LayoutItem { X = 0, Y = 1, Width = 2, Height = 1, Id = control.Name, ItemRef = control });
                    }
                    else
                    {
                        Layouts.Add(new LayoutItem { X = 2, Y = 0, Width = 1, Height = 1, Id = control.Name, ItemRef = control });
                    }
                }

                foreach (var item in Layouts)
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

            MoveItem(Layouts.SingleOrDefault(i => i.Id == control1.Name), (int)Math.Round((double)control1.Location.X / this.Owner.CellWidth, 0), (int)Math.Round((double)control1.Location.Y / this.Owner.CellWidth));

            foreach (var item in Layouts)
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

            var needMoveItems = Layouts.Where(item => moveItem.IntersectsWith(item)).ToArray();

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

                    if (Layouts.FirstOrDefault(i => fakeItem.IntersectsWith(i)) != default(LayoutItem))
                    {
                        MoveItem(item, item.X, fakeItem.Y, false);
                        continue;
                    }
                }

                MoveItem(item, item.X, moveItem.Y + moveItem.Height, false);
            }

            moveItem.Moving = false;
        }
    }
}
