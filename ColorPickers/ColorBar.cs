/*

 Copriright Daniele Fontani 

 * User: Zeppa'man
 * Date: 18/12/2005
 * Time: 13.43


 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fuliggine.ColorPickers
{
	/// <summary>
	/// Description of ColorBar.
	/// </summary>
	public class ColorBar : System.Windows.Forms.UserControl
	{
		
		
	    public delegate void OnColorDelegate();
		public event OnColorDelegate  OnChangeColor;
	
		Color _selCol=Color.AliceBlue;
		public Color SelectedColor
		{
			get{return _selCol ;}
			set{_selCol=value;  }
		
		
		}
		HolePointer Curs=new HolePointer();
		public ColorBar()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			Curs.Location=new Point(this.Width/2,this.Height/2);
			Curs.EndDrag+= new Fuliggine.ColorPickers.HolePointer.OnEndDragDelegate(EndDrag);
			this.Controls.Add(Curs);
			
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(aMouseDown);
			
			
		}
		
		private void aMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
		
		Curs.Location=new Point(e.X,e.Y);
		this.EndDrag();
		
		}
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			// 
			// ColorBar
			// 
			this.Name = "ColorBar";
			this.Size = new System.Drawing.Size(292, 32);
		}
		#endregion
	
		 public void EndDrag()
        {
        this._selCol=ColorByLeft(this.Curs.Left);
        
        if (OnChangeColor!=null)
		{
			this.OnChangeColor();
		
		}
        
        }
		public Color ColorByLeft(int left)
		{
			int section= left*6/(this.Width);
			int step=255*6/(this.Width);
			left=left  %  ( this.Width/6);
			
			switch (section)
			{		

                     //							r     G     b
					case 0:return Color.FromArgb(255,0,left*step );
					case 1:return Color.FromArgb(255-left*step,0,255 );
					case 2:return Color.FromArgb(0,left*step,255 );
					case 3:return Color.FromArgb(0,255,255-left*step );
				//	case 4:return Color.FromArgb(255,0,left*step );
					case 4:return Color.FromArgb(left*step,255,0 );
					case 5:return Color.FromArgb(255,255-left*step,0 );
				    default:return Color.Black;
			
			
			
			
			
			}
		 
		}
		protected override void OnResize(EventArgs e)
		{
		
			Curs.Location=new Point(this.Width/2,this.Height/2);
			
			this.Invalidate();
			
			this.EndDrag();
		
		
		}
		protected override void OnPaint(PaintEventArgs e)
		{  int R=255;
		   int G=0;
		   int B=0;
		   //orizzontal...
		   int colorstep=255/ ( this.Width/6 );
		   int i=0;
		   //the color step x pixel
		   
			for(B=0;B<256;B+=colorstep ,i++)
			{
				e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(R,G,B)),1),i,0,i,this.Height);
			
			}
			B=255;
			for(R=255;R>0;R-=colorstep,i++)
			{
			
				e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(R,G,B)),1),i,0,i,this.Height);
			
			}
			R=0;
		    for(G=0;G<256;G+=colorstep,i++)
			{
		    	e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(R,G,B)),1),i,0,i,this.Height);
			
			
			}
		    G=255;
		    		 
		  
			for(B=255;B>0;B-=colorstep,i++)
			{ 
				e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(R,G,B)),1),i,0,i,this.Height);
		
			}
			
			B=0;
			 
			for(R=0;R<256;R+=colorstep,i++)
			{
				e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(R,G,B)),1),i,0,i,this.Height);
			
			
			}
			R=255;
			B=0;
			for(G=255;G>0;G-=colorstep,i++)
			{
				e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(R,G,B)),1),i,0,i,this.Height);
			
			
			}
			
		}
	
	
	}


}
