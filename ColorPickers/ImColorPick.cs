/*

 Copriright Daniele Fontani 

 * User: Zeppa'man
 * Date: 24/11/2005
 * Time: 17.49


 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fuliggine.ColorPickers
{
	
	public class StableColorPicker:PictureBox
		{	
		
		protected   Size Size  = new Size(257,257);
		public Color pColor=Color.Blue;
		//private Point ColorPointer;
		private  HolePointer Curs= new HolePointer();
		 
        public StableColorPicker()
		{	
			Size  = new Size(257,257);
			InitializeComponent();
			
			Curs.Location=new Point(127,127);
			Curs.EndDrag+= new Fuliggine.ColorPickers.HolePointer.OnEndDragDelegate(EndDrag);
			this.Controls.Add(Curs);
			this.SelectByPoint(new Point(127,127));
		
		
			
		}
        public void EndDrag()
        {
        SelectByPoint(this.Curs.Location);
        
        }
	/*	 protected override void OnPaint(PaintEventArgs e)
		{ 	
		 }*/
		 
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		/// 
		
		private void InitializeComponent() 
		{
			
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(StableColorPicker));
			// 
			// StableColorPicker
			// 
			this.Image = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Name = "StableColorPicker";
			
			
			this.Size = new System.Drawing.Size(259, 259);
		}
		#endregion
		
		public delegate void OnColorDelegate();
		public event OnColorDelegate  OnChangeColor;
		//bool CanDrag=false;
		
		
		
		/*protected override void OnMouseMove(MouseEventArgs e)
		{
			if (CanDrag==true)
			{ 
				Curs.Location= ColorPointer;
			
			
			}
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			//CanDrag=false;
		}*/
		
		public void SetColor(Color color)
		{
			SetColor(color.R,color.G,color.B);
		
			
			
		}
		public void SetColor(int R,int G,int B)
		{   //nel centro.
			/*
			if(R>0&&G>0&&B>0)
			{
				
				double teta= Math.Asin((double)(G-127)/(double)(B-127));
		    
			int X=127+Convert.ToInt32( B*Math.Cos(teta));
			int Y=127+Convert.ToInt32(B*Math.Sin(teta));
		    
		    this.Curs.Location=new Point(X,Y);
		    
			}
			*/
			// Bitmap bm=(System.Drawing.Bitmap)this.Image;
			 
			
		}
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			//CanDrag=true;
			SelectByPoint(new Point(e.X,e.Y));
		this.Curs.Location= new Point(e.X,e.Y);
		base.OnClick(e);
		
		
		}
		private void SelectByPoint(Point e)
		
		{
		int x=e.X-(this.Width-2)/2+1;
		int y=e.Y-(this.Width-2)/2+1;
		/*
		this.ColorPointer.X= e.X;
		this.ColorPointer.Y= e.Y;
		*/
		if(e.Y<255&&e.X<255&&(int)Math.Sqrt(x*x+y*y)<255
		   &&e.Y>0&&e.X>0)
		pColor= Color.FromArgb(e.X,e.Y,(int)Math.Sqrt(x*x+y*y));
			
		if (OnChangeColor!=null)
		{
			this.OnChangeColor();
		
		}
		}
	}


public class HolePointer:Control
	{
		public HolePointer()
		{
		
			this.Size= new Size(5,5);
		}
		
		
	
		bool CanDrag=false;
		
		Point position=new Point(0,0);
		
		public delegate void OnEndDragDelegate();
		public event  OnEndDragDelegate  EndDrag;
	
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (CanDrag==true)
			{ 
				if (this.Parent.ClientRectangle.Contains(e.X+this.Left,e.Y+this.Top))
				{
				this.Location = new Point( e.X+this.Left,e.Y+this.Top);
				
				this.Invalidate();
				}
				if(this.EndDrag!=null)
			{
			
				this.EndDrag();
			
			}
				
			}
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			CanDrag=false;
			if(this.EndDrag!=null)
			{
			
				this.EndDrag();
			
			}
			
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			CanDrag=true;
			//position= new Point(e.X,e.Y);
		}
	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.FillRectangle(new SolidBrush(Color.Blue),this.ClientRectangle);
		
		e.Graphics.FillEllipse(new SolidBrush(Color.White),this.ClientRectangle);
	    e.Graphics.DrawEllipse(new Pen(Color.Black,1),this.ClientRectangle);
	
	}
	}
	
}
