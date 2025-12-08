/*

 Copriright Daniele Fontani 

 * User: Zeppa'man
 * Date: 18/12/2005
 * Time: 14.28


 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fuliggine.ColorPickers
{
	/// <summary>
	/// Description of MonoFrameColor.
	/// </summary>
	public class MonoFrameColor : System.Windows.Forms.UserControl
	{   
		private System.Windows.Forms.Panel inPanel;
		private System.Windows.Forms.Panel outPanel;
		int _bordersize=15;
		Color _SelectedColor;
		
		public Color SelectedColor
		{
			get{return _SelectedColor;}
			set{
			_SelectedColor=value;
			this.inPanel.BackColor=value;
			}
		
		}
		
		public Color BackColor
		{
			get{return this.outPanel.BackColor;}
			set{
			this.outPanel.BackColor=value;
			
			}
		
		}	
		public Color ForeColor
		{
			get{return this.outPanel.ForeColor;}
			set{
			this.outPanel.ForeColor=value;
			this.inPanel.ForeColor=value;
			
			}
		
		}	
		public int BorderSize
		{
			get{return _bordersize;}
			set{_bordersize=value;}
		
		
		}
			
		
		public MonoFrameColor()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			
			this.outPanel.BackColor= SystemColors.ControlDark;
			this.inPanel.BackColor= SystemColors.Control;
			this.outPanel.ForeColor= SystemColors.ControlDarkDark;
			this.inPanel.ForeColor= SystemColors.ControlDarkDark;
		doResize();
		
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			this.outPanel = new System.Windows.Forms.Panel();
			this.inPanel = new System.Windows.Forms.Panel();
			this.outPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// outPanel
			// 
			this.outPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.outPanel.Controls.Add(this.inPanel);
			this.outPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outPanel.Location = new System.Drawing.Point(0, 0);
			this.outPanel.Name = "outPanel";
			this.outPanel.Size = new System.Drawing.Size(136, 120);
			this.outPanel.TabIndex = 0;
			// 
			// inPanel
			// 
			this.inPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.inPanel.Location = new System.Drawing.Point(8, 8);
			this.inPanel.Name = "inPanel";
			this.inPanel.Size = new System.Drawing.Size(120, 100);
			this.inPanel.TabIndex = 0;
			// 
			// MonoFrameColor
			// 
			this.Controls.Add(this.outPanel);
			this.Name = "MonoFrameColor";
			this.Size = new System.Drawing.Size(136, 120);
			this.outPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
		protected override void OnResize(EventArgs e)
		{
		
			doResize();
		
		
		}
		public void doResize()
		{
		inPanel.Size= new Size(this.Width-2*_bordersize,this.Height-2*_bordersize);
			
		inPanel.Location=new Point(_bordersize-1,_bordersize-1);
		
		}
	
	}


}
