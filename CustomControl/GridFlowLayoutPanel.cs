using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace CustomControl
{
    public partial class GridFlowLayoutPanel : Panel
    {
        private GridFlowLayoutEngine _layoutEngine;

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

            _layoutEngine.OnRegister(e.Control);
            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            Contract.Requires(e != null);

            _layoutEngine.OnUnRegister(e.Control);
            base.OnControlRemoved(e);
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
