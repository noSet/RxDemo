using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControl
{
    public class GridFlowLayoutPanel : Panel
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private IContainer _components = new Container();

        private GridFlowLayoutEngine _layoutEngine;
        private readonly MouseHook _hook;

        [Browsable(true)]
        [DefaultValue(50)]
        [DisplayName("栅格单元像素")]
        public int CellPixel { get; set; } = 50;

        [Browsable(true)]
        [DefaultValue(1)]
        [DisplayName("最小栅格宽度")]
        public int MinCellWidth { get; set; } = 1;

        [Browsable(true)]
        [DefaultValue(1)]
        [DisplayName("最小栅格高度")]
        public int MinCellHeight { get; set; } = 1;

        [Browsable(true)]
        [DefaultValue(5)]
        [DisplayName("栅格间距")]
        public int CellMargin { get; set; } = 5;

        internal Dictionary<IntPtr, Control> InternalControls { get; } = new Dictionary<IntPtr, Control>();

        public GridFlowLayoutPanel()
        {
            _hook = new MouseHook(this);
        }

        public GridFlowLayoutPanel(IContainer container)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.Add(this);

            _hook = new MouseHook(this);
        }

        public override LayoutEngine LayoutEngine
        {
            get
            {
                _layoutEngine ??= new GridFlowLayoutEngine(this);

                return _layoutEngine;
            }
        }

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _hook.Dispose();
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            Contract.Requires(e != null);

            InternalControls.Add(e.Control.Handle, e.Control);
            _layoutEngine.OnRegister(e.Control);
            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            Contract.Requires(e != null);

            InternalControls.Remove(e.Control.Handle);
            _layoutEngine.OnUnRegister(e.Control);
            base.OnControlRemoved(e);
        }

        private int Round(int num)
        {
            var quotient = Math.DivRem(num, CellPixel, out var remainder);
            return ((double)remainder / CellPixel) > 0.25 ? quotient + 1 : quotient;
        }

        internal void OnDragStart(Control control)
        {
            Debug.WriteLine(nameof(OnDragStart));

            // todo 开放事件
            _layoutEngine.OnDragStart(control);
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

            x = Math.Max(x, CellMargin);
            y = Math.Max(y, CellMargin);
            control.Location = new Point(x, y);

            // todo 开放事件
            _layoutEngine.OnDrag(control, Round(x), Round(y));
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        /// <param name="control"></param>
        internal void OnDragStop(Control control)
        {
            Debug.WriteLine(nameof(OnDragStop));

            // todo 开放事件
            _layoutEngine.OnDragStop(control);
        }

        /// <summary>
        /// 开始调整大小
        /// </summary>
        /// <param name="control"></param>
        internal void OnResizeStart(Control control)
        {
            Debug.WriteLine(nameof(OnResizeStart));

            // todo 开放事件
            _layoutEngine.OnResizeStart(control);
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

            width = Math.Max(width, MinCellWidth * CellPixel - 2 * CellMargin);
            height = Math.Max(height, MinCellHeight * CellPixel - 2 * CellMargin);
            control.Size = new Size(width, height);

            // todo 开放事件
            _layoutEngine.OnResize(control, Round(control.Size.Width), Round(control.Size.Height));
        }

        /// <summary>
        /// 结束调整大小
        /// </summary>
        /// <param name="control"></param>
        internal void OnResizeStop(Control control)
        {
            Debug.WriteLine(nameof(OnResizeStop));

            // todo 开放事件
            _layoutEngine.OnResizeStop(control);
        }

        /// <summary>
        /// 使用默认的布局方式初始化栅格布局
        /// </summary>
        public void InitLayoutItems()
        {
            InitLayoutItems(Enumerable.Empty<LayoutItem>());
        }

        /// <summary>
        /// 初始化栅格布局
        /// </summary>
        /// <param name="layoutItems"></param>
        public void InitLayoutItems(IEnumerable<LayoutItem> layoutItems)
        {
            if (layoutItems is null)
            {
                throw new ArgumentNullException(nameof(layoutItems));
            }

            _layoutEngine.OnInitLayout(layoutItems);
        }

        /// <summary>
        /// 获取栅格布局
        /// </summary>
        /// <returns>栅格布局</returns>
        public IEnumerable<LayoutItem> GetLayoutItems()
        {
            return _layoutEngine.LayoutItems.Values;
        }
    }
}
