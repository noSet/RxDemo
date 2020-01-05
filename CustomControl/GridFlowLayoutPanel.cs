using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
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

        public void InitLayout(IEnumerable<LayoutItem> layoutItems)
        {
            _layoutEngine.OnInitLayout(layoutItems);
        }

        public IEnumerable<LayoutItem> GetLayoutItems()
        {
            return _layoutEngine.LayoutItems.Values;
        }
    }
}
