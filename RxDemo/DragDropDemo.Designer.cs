namespace RxDemo
{
    partial class DragDropDemo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gridFlowLayoutPanel1 = new CustomControl.GridFlowLayoutPanel(this.components);
            this.Red = new System.Windows.Forms.Panel();
            this.Blue = new System.Windows.Forms.Panel();
            this.Yellow = new System.Windows.Forms.Panel();
            this.button8 = new System.Windows.Forms.Button();
            this.gridFlowLayoutPanel1.SuspendLayout();
            this.Yellow.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridFlowLayoutPanel1
            // 
            this.gridFlowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridFlowLayoutPanel1.AutoScroll = true;
            this.gridFlowLayoutPanel1.CellPixel = 75;
            this.gridFlowLayoutPanel1.Controls.Add(this.Red);
            this.gridFlowLayoutPanel1.Controls.Add(this.Blue);
            this.gridFlowLayoutPanel1.Controls.Add(this.Yellow);
            this.gridFlowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.gridFlowLayoutPanel1.MinCellWidth = 2;
            this.gridFlowLayoutPanel1.Name = "gridFlowLayoutPanel1";
            this.gridFlowLayoutPanel1.Size = new System.Drawing.Size(1051, 445);
            this.gridFlowLayoutPanel1.TabIndex = 2;
            // 
            // Red
            // 
            this.Red.BackColor = System.Drawing.Color.Red;
            this.Red.Location = new System.Drawing.Point(5, 155);
            this.Red.Margin = new System.Windows.Forms.Padding(5);
            this.Red.Name = "Red";
            this.Red.Padding = new System.Windows.Forms.Padding(10);
            this.Red.Size = new System.Drawing.Size(65, 65);
            this.Red.TabIndex = 3;
            // 
            // Blue
            // 
            this.Blue.BackColor = System.Drawing.Color.Blue;
            this.Blue.Location = new System.Drawing.Point(5, 80);
            this.Blue.Margin = new System.Windows.Forms.Padding(5);
            this.Blue.Name = "Blue";
            this.Blue.Padding = new System.Windows.Forms.Padding(10);
            this.Blue.Size = new System.Drawing.Size(65, 65);
            this.Blue.TabIndex = 2;
            // 
            // Yellow
            // 
            this.Yellow.BackColor = System.Drawing.Color.Yellow;
            this.Yellow.Controls.Add(this.button8);
            this.Yellow.Location = new System.Drawing.Point(5, 5);
            this.Yellow.Margin = new System.Windows.Forms.Padding(5);
            this.Yellow.Name = "Yellow";
            this.Yellow.Size = new System.Drawing.Size(65, 65);
            this.Yellow.TabIndex = 1;
            // 
            // button8
            // 
            this.button8.Dock = System.Windows.Forms.DockStyle.Top;
            this.button8.Location = new System.Drawing.Point(0, 0);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(65, 23);
            this.button8.TabIndex = 0;
            this.button8.Text = "button8";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // DragDropDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1051, 500);
            this.Controls.Add(this.gridFlowLayoutPanel1);
            this.Name = "DragDropDemo";
            this.Text = "Form1";
            this.gridFlowLayoutPanel1.ResumeLayout(false);
            this.Yellow.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel Yellow;
        private CustomControl.GridFlowLayoutPanel gridFlowLayoutPanel1;
        private System.Windows.Forms.Panel Blue;
        private System.Windows.Forms.Panel Red;
        private System.Windows.Forms.Button button8;
    }
}

