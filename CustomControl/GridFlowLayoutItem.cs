using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomControl
{
    public partial class GridFlowLayoutItem : Panel
    {
        public int TitleHeight { get; set; }

        public GridFlowLayoutItem()
        {
            InitializeComponent();

            Init();
        }

        public GridFlowLayoutItem(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Init();
        }

        private void Init()
        {
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(this, nameof(this.MouseDown))
                .Where(p => p.EventArgs.Button == MouseButtons.Left)
                .Select(p => p.EventArgs.Location);

            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(this, nameof(this.MouseMove))
                .Select(p => p.EventArgs.Location);

            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(this, nameof(this.MouseUp))
                .Where(p => p.EventArgs.Button == MouseButtons.Left)
                .Select(p => p.EventArgs.Location);

            var elementMoves = from start in mouseDown.Where(p => p.Y <= 10)
                               from process in mouseMove.TakeUntil(mouseUp)
                               select new { X = process.X - start.X, Y = process.Y - start.Y };

            elementMoves.Subscribe(p =>
            {
                var localtion = this.Location;
                localtion.Offset(p.X, p.Y);
                this.Location = localtion;
            });

            var elementResize = from start in mouseDown.Where(p => this.ClientRectangle.Right - p.X <= 5 && this.ClientRectangle.Bottom - p.Y <= 5)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select process;

            elementResize.Subscribe(p => this.Size = new Size(p));

            mouseDown
                .Where(p => p.Y <= 10)
                .Zip(mouseMove, (one, two) => one)
                .Subscribe(i => Console.WriteLine("开始拖拽"));

            mouseUp
                .Where(p => p.Y <= 10)
                .Zip(mouseMove, (one, two) => one)
                .Subscribe(i => Console.WriteLine("结束拖拽"));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }
    }
}
