using System;
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

            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(panel1, nameof(panel1.MouseDown))
                .Select(p => p.EventArgs.Location);

            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(panel1, nameof(panel1.MouseMove))
                .Select(p => p.EventArgs.Location);

            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(panel1, nameof(panel1.MouseUp));

            var elementMoves = from start in mouseDown.Where(p => p.Y <= 10)
                               from process in mouseMove.TakeUntil(mouseUp)
                               select new { X = process.X - start.X, Y = process.Y - start.Y };

            elementMoves.Subscribe(p =>
            {
                var localtion = panel1.Location;
                localtion.Offset(p.X, p.Y);
                panel1.Location = localtion;
            });

            var elementResize = from start in mouseDown.Where(p => panel1.ClientRectangle.Right - p.X <= 5 && panel1.ClientRectangle.Bottom - p.Y <= 5)
                                from process in mouseMove.TakeUntil(mouseUp)
                                select process;

            elementResize.Subscribe(p => panel1.Size = new Size(p));

            var sizeChanged = Observable.FromEventPattern<EventArgs>(panel1, nameof(panel1.LocationChanged));
            var endAutoSize = Observable.Empty<EventArgs>();

            var autoSize = from stop in mouseUp
                           from changed in sizeChanged.TakeUntil(endAutoSize)
                           select changed;

            mouseUp.Subscribe(p =>
            {
                var oldLoction = panel1.Location;

                var x = oldLoction.X % Cell > (Cell / 2) ? Cell * (oldLoction.X / Cell + 1) : Cell * (oldLoction.X / Cell);
                var y = oldLoction.Y % Cell > (Cell / 2) ? Cell * (oldLoction.Y / Cell + 1) : Cell * (oldLoction.Y / Cell);

                panel1.Location = new Point(x, y);
                endAutoSize.StartWith(p.EventArgs);
            });
        }
    }
}
