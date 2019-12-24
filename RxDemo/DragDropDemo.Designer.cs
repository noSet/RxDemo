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
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.gridFlowLayoutPanel1 = new CustomControl.GridFlowLayoutPanel(this.components);
            this.Red = new CustomControl.GridFlowLayoutItem(this.components);
            this.Blue = new CustomControl.GridFlowLayoutItem(this.components);
            this.Yellow = new System.Windows.Forms.Panel();
            this.gridFlowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(119, 976);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 3;
            this.button6.Text = "添加";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(3, 3);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 4;
            this.button5.Text = "button5";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(3, 32);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(3, 61);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(3, 90);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 119);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(236, 976);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 4;
            this.button7.Text = "移除";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // gridFlowLayoutPanel1
            // 
            this.gridFlowLayoutPanel1.CellWidth = 200;
            this.gridFlowLayoutPanel1.Controls.Add(this.Red);
            this.gridFlowLayoutPanel1.Controls.Add(this.Blue);
            this.gridFlowLayoutPanel1.Controls.Add(this.Yellow);
            this.gridFlowLayoutPanel1.Location = new System.Drawing.Point(95, 12);
            this.gridFlowLayoutPanel1.Name = "gridFlowLayoutPanel1";
            this.gridFlowLayoutPanel1.Size = new System.Drawing.Size(1208, 877);
            this.gridFlowLayoutPanel1.TabIndex = 2;
            // 
            // Red
            // 
            this.Red.BackColor = System.Drawing.Color.Red;
            this.Red.Location = new System.Drawing.Point(0, 0);
            this.Red.Name = "Red";
            this.Red.Size = new System.Drawing.Size(400, 200);
            this.Red.TabIndex = 3;
            this.Red.TitleHeight = 0;
            // 
            // Blue
            // 
            this.Blue.BackColor = System.Drawing.Color.Blue;
            this.Blue.Location = new System.Drawing.Point(0, 200);
            this.Blue.Name = "Blue";
            this.Blue.Size = new System.Drawing.Size(400, 200);
            this.Blue.TabIndex = 2;
            this.Blue.TitleHeight = 0;
            // 
            // Yellow
            // 
            this.Yellow.BackColor = System.Drawing.Color.Yellow;
            this.Yellow.Location = new System.Drawing.Point(400, 0);
            this.Yellow.Name = "Yellow";
            this.Yellow.Size = new System.Drawing.Size(200, 200);
            this.Yellow.TabIndex = 1;
            // 
            // DragDropDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1647, 1083);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.gridFlowLayoutPanel1);
            this.Name = "DragDropDemo";
            this.Text = "Form1";
            this.gridFlowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel Yellow;
        private CustomControl.GridFlowLayoutPanel gridFlowLayoutPanel1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private CustomControl.GridFlowLayoutItem Blue;
        private CustomControl.GridFlowLayoutItem Red;
    }
}

