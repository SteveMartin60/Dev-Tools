/*

 Copriright Daniele Fontani 

 * User: Zeppa'man
 * Date: 16/12/2005
 * Time: 14.42


 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fuliggine.ColorPickers
{
	/// <summary>
	/// Description of TryForm.
	/// </summary>
	public class TryForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox3;
		private Fuliggine.ColorPickers.ColorBar colorBar1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private Fuliggine.ColorPickers.SquareColorPick squareColorPick1;
		private Fuliggine.ColorPickers.DoubleFrameColor doubleFrameColor1;
		private System.Windows.Forms.Label label2;
		private Fuliggine.ColorPickers.FullColorBar fullColorBar1;
		private Fuliggine.ColorPickers.MonoFrameColor monoFrameColor1;
		private Fuliggine.ColorPickers.ColorGradient colorGradient1;
		private System.Windows.Forms.GroupBox groupBox2;
		public TryForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			
		}
			[STAThread]
		public static void Main(string[] args)
		{
			Application.Run(new TryForm());
		}
	
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.colorGradient1 = new Fuliggine.ColorPickers.ColorGradient();
			this.monoFrameColor1 = new Fuliggine.ColorPickers.MonoFrameColor();
			this.fullColorBar1 = new Fuliggine.ColorPickers.FullColorBar();
			this.label2 = new System.Windows.Forms.Label();
			this.doubleFrameColor1 = new Fuliggine.ColorPickers.DoubleFrameColor();
			this.squareColorPick1 = new Fuliggine.ColorPickers.SquareColorPick();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.colorBar1 = new Fuliggine.ColorPickers.ColorBar();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.doubleFrameColor1);
			this.groupBox2.Location = new System.Drawing.Point(392, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(100, 100);
			this.groupBox2.TabIndex = 12;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "groupBox2";
			// 
			// colorGradient1
			// 
			this.colorGradient1.BaseColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(64)), ((System.Byte)(0)));
			this.colorGradient1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.colorGradient1.Location = new System.Drawing.Point(3, 16);
			this.colorGradient1.Name = "colorGradient1";
			this.colorGradient1.Size = new System.Drawing.Size(210, 197);
			this.colorGradient1.TabIndex = 8;
			// 
			// monoFrameColor1
			// 
			this.monoFrameColor1.BorderSize = 5;
			this.monoFrameColor1.Location = new System.Drawing.Point(16, 32);
			this.monoFrameColor1.Name = "monoFrameColor1";
			this.monoFrameColor1.SelectedColor = System.Drawing.Color.Empty;
			this.monoFrameColor1.Size = new System.Drawing.Size(64, 54);
			this.monoFrameColor1.TabIndex = 5;
			// 
			// fullColorBar1
			// 
			this.fullColorBar1.Location = new System.Drawing.Point(16, 304);
			this.fullColorBar1.Name = "fullColorBar1";
			this.fullColorBar1.SelectedColor = System.Drawing.Color.LightGreen;
			this.fullColorBar1.Size = new System.Drawing.Size(368, 32);
			this.fullColorBar1.TabIndex = 6;
			this.fullColorBar1.OnChangeColor += new Fuliggine.ColorPickers.FullColorBar.OnColorDelegate(this.ColorBar1OnChangeColor);
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(16, 208);
			this.label2.Name = "label2";
			this.label2.TabIndex = 10;
			this.label2.Text = "Simple color bar";
			// 
			// doubleFrameColor1
			// 
			this.doubleFrameColor1.BackSelectedColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(128)), ((System.Byte)(0)));
			this.doubleFrameColor1.BorderSize = 7;
			this.doubleFrameColor1.ForeSelectedColor = System.Drawing.Color.Yellow;
			this.doubleFrameColor1.Location = new System.Drawing.Point(16, 24);
			this.doubleFrameColor1.Name = "doubleFrameColor1";
			this.doubleFrameColor1.Size = new System.Drawing.Size(64, 64);
			this.doubleFrameColor1.TabIndex = 6;
			this.doubleFrameColor1.Click += new System.EventHandler(this.DoubleFrameColor1Click);
			this.doubleFrameColor1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DoubleFrameColor1MouseDown);
			// 
			// squareColorPick1
			// 
			this.squareColorPick1.BaseColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(192)), ((System.Byte)(128)));
			this.squareColorPick1.Location = new System.Drawing.Point(8, 8);
			this.squareColorPick1.Name = "squareColorPick1";
			this.squareColorPick1.SelectedColor = System.Drawing.Color.FromArgb(((System.Byte)(64)), ((System.Byte)(64)), ((System.Byte)(0)));
			this.squareColorPick1.Size = new System.Drawing.Size(376, 192);
			this.squareColorPick1.TabIndex = 8;
			this.squareColorPick1.OnChangeColor += new Fuliggine.ColorPickers.SquareColorPick.OnColorDelegate(this.ColorBar1OnChangeColor);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.colorGradient1);
			this.groupBox1.Location = new System.Drawing.Point(392, 120);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(216, 216);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "color gradient";
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(16, 280);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(264, 16);
			this.label1.TabIndex = 9;
			this.label1.Text = "Advanced color bar with white and black";
			// 
			// colorBar1
			// 
			this.colorBar1.Location = new System.Drawing.Point(16, 240);
			this.colorBar1.Name = "colorBar1";
			this.colorBar1.SelectedColor = System.Drawing.Color.FromArgb(((System.Byte)(0)), ((System.Byte)(255)), ((System.Byte)(251)));
			this.colorBar1.Size = new System.Drawing.Size(368, 32);
			this.colorBar1.TabIndex = 3;
			this.colorBar1.OnChangeColor += new Fuliggine.ColorPickers.ColorBar.OnColorDelegate(this.ColorBar1OnChangeColor);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.monoFrameColor1);
			this.groupBox3.Location = new System.Drawing.Point(504, 16);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(100, 100);
			this.groupBox3.TabIndex = 13;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "groupBox3";
			// 
			// TryForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(608, 350);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.squareColorPick1);
			this.Controls.Add(this.fullColorBar1);
			this.Controls.Add(this.colorBar1);
			this.Name = "TryForm";
			this.Text = "TryForm";
			this.Load += new System.EventHandler(this.TryFormLoad);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
		void SpecialTrackbar1Load(object sender, System.EventArgs e)
		{
			
		}
		
		void ColorBar1OnChangeColor()
		{
			this.monoFrameColor1.SelectedColor=
			this.colorBar1.SelectedColor;
			
			this.doubleFrameColor1.ForeSelectedColor=
			this.colorBar1.SelectedColor;
			
			this.doubleFrameColor1.BackSelectedColor=
			this.squareColorPick1.SelectedColor;
			
			this.squareColorPick1.BaseColor=this.fullColorBar1.SelectedColor;
			
			this.colorGradient1.BaseColor=this.colorBar1.SelectedColor;
		}
		
		void DoubleFrameColor1Click(object sender, System.EventArgs e)
		{
			
		}
		
		void DoubleFrameColor1MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			
		}
		
		void TryFormLoad(object sender, System.EventArgs e)
		{
			this.ColorBar1OnChangeColor();
		}
		
	}
}
