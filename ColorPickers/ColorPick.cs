/*

 Copriright Daniele Fontani 

 * User: Zeppa'man
 * Date: 23/11/2005
 * Time: 16.20


 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fuliggine.ColorPickers
	
{
public class SquareColorPick:UserControl
{
     public bool AnimationRadial=false;
     public Color pColor;
     public Color _selectedcolor;
       public Color SelectedColor
     {
     	get{return _selectedcolor ;}
     	set{_selectedcolor=value; 
     		this.Invalidate();
     	   }
     
     
     }
     public Color BaseColor
     {
     	get{return pColor ;}
     	set{pColor=value;
     		this.Invalidate();
     	}
     
     
     }
  
		 HolePointer Curs=new HolePointer();
		
		 
	 public SquareColorPick()
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
		 public void EndDrag()
        {
        this._selectedcolor=ColorByTop(this.Curs.Top);
        
        this.Invalidate();
        
        if (OnChangeColor!=null)
		{
			this.OnChangeColor();
		
		}
        
        }
		 
			protected override void OnResize(EventArgs e)
		{
		
			Curs.Location=new Point(this.Width/2,this.Height/2);
			
			this.Invalidate();
			
			//this.EndDrag();
		
		
		} 
		 
		public Color ColorByTop(int left)
		{
			
			int step= 255 / (this.Height);
			
			
			
		//	if(this.Height>left && left>=0)
			{
			
			int r=pColor.R-(left*step);
	   		if(r<0)
	   		{
	   		r=0;
	   		}
	   		
	   		
	   		int g=pColor.G-(left*step);
	   		if(g<0)
	   		{
	   		g=0;
	   		}
	   		
	   		
	   		int b=pColor.B-(left*step);
	   		if(b<0)
	   		{
	   		b=0;
	   		}
				
				
				
	   		
				return Color.FromArgb(r,g,b );
			
			
			
			}
				 return Color.Black;
			
		 
		}	
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		
		void InitializeComponent() {
			// 
			// SquareColorPick
			// 
			this.Name = "SquareColorPick";
			this.Size = new System.Drawing.Size(255, 255);
		}
		#endregion
		
		protected override void OnPaint(PaintEventArgs e)
		{int ScaleY=255/this.Height;

	   	for ( int y = 0 ; y<this.Height;y++)
		{	
	   		int r=pColor.R-(y*ScaleY);
	   		if(r<0)
	   		{
	   		r=0;
	   		}
	   			   		
	   		int g=pColor.G-(y*ScaleY);
	   		if(g<0)
	   		{
	   		g=0;
	   		}
	   			   		
	   		int b=pColor.B-(y*ScaleY);
	   		if(b<0)
	   		{
	   		b=0;
	   		}
	   		e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(r,g,b)),1),0,y,this.Width,y);
	    		}
		}
		public delegate void OnColorDelegate();
		public event OnColorDelegate  OnChangeColor;
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			
		int x=e.X-(this.Width-2)/2+1;
		int y=e.Y-(this.Width-2)/2+1;
		/*this.ColorPointer.X= e.X;
		this.ColorPointer.Y= e.Y;*/
		if(e.Y<255&&e.X<255&&(int)Math.Sqrt(x*x+y*y)<255
		   &&e.Y>0&&e.X>0)
		pColor= Color.FromArgb(e.X,e.Y,(int)Math.Sqrt(x*x+y*y));
			
		if (OnChangeColor!=null)
		{
			this.OnChangeColor();
		
		}
		
		base.OnClick(e);
		
		
		}
		
	}	


public class ColorGradient:UserControl
{
     public bool AnimationRadial=false;
     public Color pColor;
     public Color BaseColor
     {
     	get{return pColor ;}
     	set{pColor=value;
     		this.Invalidate();
     	}
     
     
     }
   //  private Point ColorPointer;
	 public ColorGradient()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//ColorPointer=new Point(this.ClientRectangle.Width/2,this.ClientRectangle.Width/2);
			
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		
		void InitializeComponent() {
			// 
			// SquareColorPick
			// 
			this.Name = "SquareColorPick";
			this.Size = new System.Drawing.Size(255, 255);
		}
		#endregion
			
		protected override void OnResize(EventArgs e)
		{
		
			
			this.Invalidate();
			
			
		
		
		}
		protected override void OnPaint(PaintEventArgs e)
		{
		
			
		int Scalex=255/this.Width;
		int ScaleY=255/this.Height;

		
		
	  

	   // for ( int x = 1 ; x <this.Width; x++)
	  //  { 
	   	
	   	for ( int y = 0 ; y<this.Height;y++)
		{	
	   		int r=pColor.R-(y*ScaleY);
	   		if(r<0)
	   		{
	   		r=0;
	   		}
	   		
	   		
	   		int g=pColor.G-(y*ScaleY);
	   		if(g<0)
	   		{
	   		g=0;
	   		}
	   		
	   		
	   		int b=pColor.B-(y*ScaleY);
	   		if(b<0)
	   		{
	   		b=0;
	   		}
	   		e.Graphics.DrawLine(new Pen(new SolidBrush(Color.FromArgb(r,g,b)),1),0,y,this.Width,y);
	    	
	   		
		
		}
	   	
	   
		
	//	}
		
		}
		public delegate void OnColorDelegate();
		public event OnColorDelegate  OnChangeColor;
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			
		int x=e.X-(this.Width-2)/2+1;
		int y=e.Y-(this.Width-2)/2+1;
		/*this.ColorPointer.X= e.X;
		this.ColorPointer.Y= e.Y;*/
		if(e.Y<255&&e.X<255&&(int)Math.Sqrt(x*x+y*y)<255
		   &&e.Y>0&&e.X>0)
		pColor= Color.FromArgb(e.X,e.Y,(int)Math.Sqrt(x*x+y*y));
			
		if (OnChangeColor!=null)
		{
			this.OnChangeColor();
		
		}
		
		base.OnClick(e);
		
		
		}
		
	}	

public class RoundColorPick:UserControl
{
     public bool AnimationRadial=false;
     public Color pColor;
     private Point ColorPointer;
	 public RoundColorPick()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			ColorPointer=new Point(this.ClientRectangle.Width/2,this.ClientRectangle.Width/2);
			
		}
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		
		void InitializeComponent() 
		{
			// 
			// ColorPick
			// 
			this.Name = "ColorPick";
			this.Size = new System.Drawing.Size(255, 255);
		}
		#endregion
		
		protected override void OnPaint(PaintEventArgs e)
		{
		//set some usefull var
		
		int offsett= 1;
		int width=this.ClientRectangle.Width-2;
		int height=this.ClientRectangle.Height-2;
		int rx= width/2;		
		int x=10;
		int y=10;
		int cx=offsett+rx;
		double teta=0 ;			
		//refresh the background
	e .Graphics.FillRectangle(new SolidBrush(this.BackColor),this.ClientRectangle);
	 
	//The core of painting
	if ( AnimationRadial==true)
	{
	for (  teta = 0 ; teta <2*Math.PI; teta=teta + 0.003)
		{for ( double ray = 0 ; ray<rx ;ray= ray+1)
		{	
		  	
	   	x= cx+ Convert.ToInt32(ray* Math.Cos(teta));
		y= cx - Convert.ToInt32(ray* Math.Sin(teta));
		Rectangle rect= new Rectangle(x,y,1,1);

		e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(x,y,(int)(ray/rx*255))),rect);
	       
		}}}
		else
		{
		for ( double ray = 0 ; ray<rx ;ray= ray+1)
		{
	     for (  teta = 0 ; teta <2*Math.PI; teta=teta + 0.003)
		{ 	   	
	   	x= cx+ Convert.ToInt32(ray* Math.Cos(teta));
		y= cx - Convert.ToInt32(ray* Math.Sin(teta));
		Rectangle rect= new Rectangle(x,y,1,1);
    	e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(x,y,(int)(ray/rx*255))),rect);
	    }}}}
		
		public delegate void OnColorDelegate();
		public event OnColorDelegate  OnChangeColor;
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			
		int x=e.X-(this.Width-2)/2+1;
		int y=e.Y-(this.Width-2)/2+1;
		this.ColorPointer.X= e.X;
		this.ColorPointer.Y= e.Y;
		if(e.Y<255&&e.X<255&&(int)Math.Sqrt(x*x+y*y)<255
		   &&e.Y>0&&e.X>0)
		pColor= Color.FromArgb(e.X,e.Y,(int)Math.Sqrt(x*x+y*y));
			
		if (OnChangeColor!=null)
		{
			this.OnChangeColor();
		
		}
		
		base.OnClick(e);
		
		
		}
		
	}
}
