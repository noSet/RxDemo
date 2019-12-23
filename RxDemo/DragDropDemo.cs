﻿using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace RxDemo
{
    public partial class DragDropDemo : Form
    {
        public const int Cell = 50;

        public DragDropDemo()
        {
            InitializeComponent();

            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Yellow, nameof(Yellow.MouseDown))
                .Where(p => p.EventArgs.Button == MouseButtons.Left)
                .Select(p => p.EventArgs.Location);

            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Yellow, nameof(Yellow.MouseMove))
                .Select(p => p.EventArgs.Location);

            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Yellow, nameof(Yellow.MouseUp))
                .Where(p => p.EventArgs.Button == MouseButtons.Left);

            var elementMoves = from start in mouseDown.Where(p => p.Y <= 10)
                               from process in mouseMove.TakeUntil(mouseUp)
                               select new { X = process.X - start.X, Y = process.Y - start.Y };

            elementMoves.Subscribe(p =>
            {
                var localtion = Yellow.Location;
                localtion.Offset(p.X, p.Y);
                Yellow.Location = localtion;
            });

            var elementResize = from start in mouseDown.Where(p => Yellow.ClientRectangle.Right - p.X <= 5 && Yellow.ClientRectangle.Bottom - p.Y <= 5)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select process;

            elementResize.Subscribe(p => Yellow.Size = new Size(p));

            var sizeChanged = Observable.FromEventPattern<EventArgs>(Yellow, nameof(Yellow.LocationChanged));
            var endAutoSize = Observable.Empty<EventArgs>();

            var autoSize = from stop in mouseUp
                           from changed in sizeChanged.TakeUntil(endAutoSize)
                           select changed;

            mouseUp.Subscribe(p =>
            {
                var oldLoction = Yellow.Location;

                var x = oldLoction.X % Cell > (Cell / 2) ? Cell * (oldLoction.X / Cell + 1) : Cell * (oldLoction.X / Cell);
                var y = oldLoction.Y % Cell > (Cell / 2) ? Cell * (oldLoction.Y / Cell + 1) : Cell * (oldLoction.Y / Cell);

                Yellow.Location = new Point(x, y);
                endAutoSize.StartWith(p.EventArgs);
            });
        }

        private void button6_Click(object sender, EventArgs e)
        {
            gridFlowLayoutPanel1.Controls.Add(new Button() { Text = "abc" });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            gridFlowLayoutPanel1.Controls.RemoveAt(0);
        }
    }
}