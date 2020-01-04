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
        public int CellWidth { get; set; } = 50;

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
