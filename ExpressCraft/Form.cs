﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Html5;
using Bridge.jQuery2;

namespace ExpressCraft
{

	public class Form : Control
	{
		public static HTMLDivElement WindowHolder { get; set; }
		public static HTMLDivElement WindowManager { get; set; }
		public static HTMLDivElement WindowManagerStart { get; set; }
		public static TextInput WindowManagerSearch { get; set; }

		public static int ResizeCorners { get; set; } = 2;
		public static Form MovingForm = null;
		public static HTMLElement Parent = null;
		public static bool Mouse_Down { get; set; } = false;
		public static int FadeLength { get; set; } = 100;
        public static HTMLElement FormOverLay;
		
        public bool AllowSizeChange = true;
		public bool AllowMoveChange = true;

        public jQuery Self;

        protected bool _IsDialog = false;

        public bool IsDialog()
        {
            return _IsDialog;
        }

		private List<Control> Children = new List<Control>();

		public void LinkchildToForm(Control child)
		{
			if(child == null)
				return;
			Children.Add(child);
		}

		public void LinkchildrenToForm(params Control[] children)
		{
			if(children == null || children.Length == 0)
				return;
			Children.AddRange(children);
		}

		public static List<FormCollection> FormCollections = new List<FormCollection>();        

		public FormStartPosition StartPosition = FormStartPosition.WindowsDefaultLocation;

		public static bool ShowBodyOverLay { get; set; } = false;

		public static int Window_DefaultHeight { get; set; } = 480;
		public static int Window_DefaultWidth { get; set; } = 640;

		private static Form _ActiveForm;
		private static Form _PrevActiveForm;
		private static MouseMoveAction MoveAction = MouseMoveAction.Move;
        
		public bool TopMost = false;

        public DialogResultEnum DialogResult = DialogResultEnum.None;

		public static int WindowHolderSelectionBoxX;
		public static int WindowHolderSelectionBoxY;

		public static int WindowHolderSelectionBoxXOff;
		public static int WindowHolderSelectionBoxYOff;
		
		protected HTMLDivElement Heading { get; set; }
		protected HTMLDivElement ButtonClose { get; set; }
		protected HTMLDivElement ButtonExpand { get; set; }
		protected HTMLDivElement ButtonMinimize { get; set; }
		protected HTMLSpanElement HeadingTitle { get; set; }

		public HTMLDivElement Body { get; set; }
		public HTMLDivElement BodyOverLay { get; set; }
		public HTMLElement Owner { get; set; } = null;

		public int prev_px;
		public int prev_py;

		private int prev_width;
		private int prev_height;

		private int prev_top;
		private int prev_left;

		public int MinWidth { get; set; } = 200;
		public int MinHeight { get; set; } = 50;

		public void Resizing()
		{
			if(OnResize != null)
				OnResize(this);
			OnResizing();

			for(int i = 0; i < Children.Count; i++)
			{
				if(Children[i] != null && Children[i].OnResize != null)
				{
					Children[i].OnResize(Children[i]);
				}
			}
		}

		protected virtual void OnResizing()
		{
			
		}

		public WindowState windowState { get; set; }

		public static bool MidleOfAction()
		{
			return  MovingForm != null; // WindowHolderSelectionBox != null ||
		}

		public bool IsContentVisible()
		{
			return Content != null && Content.Style.Visibility == Visibility.Visible;
		}

		protected virtual void Initialise()
		{

		}

		protected virtual void OnShowing()
		{

		}

		protected virtual void OnShowed()
		{

		}

		protected virtual void OnClosing()
		{

		}

        protected virtual void OnClosed()
        {

        }

        public enum WindowState
		{
			Normal,
			Minimized,
			Maximized
		}
		
		public enum FormStartPosition
		{
			Manual,
			Center,
			WindowsDefaultLocation						
		}

		private enum MouseMoveAction
		{
			None,
			Move,
			TopLeftResize,
			LeftResize,
			BottomLeftResize,
			BottomResize,
			BottomRightResize,
			RightResize,
			TopResize,
			TopRightResize
		}

        public static FormCollection GetActiveFormCollection()
        {
            for (int i = FormCollections.Count - 1; i >= 0; i--)
            {
                var frmCol = FormCollections[i];
                if (frmCol.FormOwner == null)
                {
                    for (int x = 0; x < frmCol.VisibleForms.Count; x++)
                    {
                        if (frmCol.VisibleForms[x] != null)
                        {
                            frmCol.VisibleForms[x].Close();
                        }
                    }
                    FormCollections.RemoveAt(i);
                }else
                {
                    return frmCol;
                }
            }
            
            return null;
        }

		public static void SetBodyOverLay()
		{
            var ActiveCollection = GetActiveFormCollection();
            if (ActiveCollection == null)
                return;

            ActiveCollection.FormOwner.ShowBodyOverLayStyle();

            var VisibleForms = ActiveCollection.VisibleForms;            

			for(int i = 0; i < VisibleForms.Count; i++)
			{
				var form = VisibleForms[i];
				if(form != null)
				{
                    form.ShowBodyOverLayStyle();
                }
			}
		}


        public void ShowBodyOverLayStyle()
        {
            if (BodyOverLay != null &&
                BodyOverLay.Style.Visibility == Visibility.Collapse)
                BodyOverLay.Style.Visibility = Visibility.Visible;
        }

        public static Form ActiveForm
		{
			get { return _ActiveForm; }
			set
			{
				if(_ActiveForm != value)
				{
					_PrevActiveForm = _ActiveForm;

					if(_ActiveForm != null)
					{
						if(_ActiveForm.Content != null)
						{
							_ActiveForm.BodyOverLay.Style.Visibility = Visibility.Visible;							
						}
					}
					_ActiveForm = value;
					if(_ActiveForm != null)
					{
						if(_ActiveForm.Content != null)
						{
							_ActiveForm.BodyOverLay.Style.Visibility = Visibility.Collapse;							
							_ActiveForm.BringToFront();
						}
					}
				}

			}
		}
		
		public static void ChangeStateTextSelection(HTMLElement element, bool state)
		{
			if(state)
			{
				jQuery.Select(element).Css("user-select", "text");
			}
			else
			{
				jQuery.Select(element).Css("user-select", "none");
			}
		}

		public static void DisableStateDrag(HTMLElement element)
		{
			if(element is HTMLImageElement)
			{
				element.As<HTMLImageElement>().OnDragStart = (ev) =>
				{
					ev.PreventDefault();
				};
			}
			else
			{
				jQuery.Select(element).Css("user-drag:", "none");
			}
		}		

		public static void SetupHideElementsOnView()
		{
			Window.OnBlur = (ev) =>
			{
				if(Document.Body.ChildNodes.Contains(WindowHolder))
					Document.Body.RemoveChild(WindowHolder);
			};

			Window.OnFocus = (ev) =>
			{
				if(!Document.Body.ChildNodes.Contains(WindowHolder))
					Document.Body.AppendChild(WindowHolder);
			};
		}

		public static void SetupWindowManager()
		{
			if(Parent == null || WindowHolder == null)
				return;

			if(Settings.WindowManagerVisible)
			{
				WindowHolder.Style.Height = "calc(100% - 40px)";
				if(!Parent.Children.Contains(WindowManager))
					Parent.AppendChild(WindowManager);
			}
			else
			{
				WindowHolder.Style.Height = "100%";
				if(Parent.Children.Contains(WindowManager))
					Parent.RemoveChild(WindowManager);
			}			
		}

		public static void Setup(HTMLElement parent = null)
		{
			StyleController.Setup();
			
			if(parent == null)
				Parent = Document.Body;
			else
				Parent = parent;            

			WindowHolder = Div("form-container");
			WindowManager = Div("form-manager");
			WindowManagerStart = Div("form-manager-start");

			WindowManagerSearch = new TextInput();
			WindowManagerSearch.Content.ClassList.Add("form-manager-search");

            FormOverLay = Div("system-form-collection-overlay");			
			FormOverLay.OnMouseDown = (ev) =>
			{
				if(Document.ActiveElement != null)
				{
					Document.ActiveElement.Focus();
					ev.PreventDefault();
				}
			};
            FormOverLay.OnClick = (ev) => {                

                if(ActiveForm != null)
                {
                    var form = ActiveForm;
                    form.Heading.ClassList.Add("form-heading-flash");
                    Global.SetTimeout(() =>
                    {
                        form.Heading.ClassList.Remove("form-heading-flash");
                    }, 800);
                }
            };
            FormOverLay.OnContextMenu = (ev) => {
                ev.StopPropagation();
                ev.PreventDefault();
            };
            FormOverLay.Style.Visibility = Visibility.Visible;

			Window.OnBeforeUnload = (ev) =>
			{
				Script.Write("return 'Would you like to close this application?'");
			};
			Window.OnResize = (ev) => {
				if(FormCollections == null)
					return;

				for(int i = 0; i < FormCollections.Count; i++)
				{
					if(FormCollections[i] == null)
						continue;
					var fc = FormCollections[i];
					if(fc.FormOwner != null)
						fc.FormOwner.Resizing();
					for(int x = 0; x < fc.VisibleForms.Count; x++)
					{
						if(fc.VisibleForms[x] != null)
							fc.VisibleForms[x].Resizing();
					}
				}
			};
			Window.OnMouseMove = (ev) =>
			{

				var mev = ev.As<MouseEvent>();

				if(MovingForm != null)
				{
					ev.PreventDefault();
					ev.StopImmediatePropagation();
					ev.StopPropagation();

					if(MovingForm.BodyOverLay.Style.Visibility == Visibility.Collapse)
					{
						MovingForm.BodyOverLay.Style.Visibility = Visibility.Visible;
						MovingForm.Heading.Focus();
					}

					var Y = (mev.PageY + MovingForm.prev_py);
					var X = (mev.PageX + MovingForm.prev_px);

					if(MovingForm.windowState == WindowState.Maximized && MoveAction == MouseMoveAction.Move)
					{
						MovingForm.changeWindowState();
						X = mev.PageX - (MovingForm.prev_width / 2);

						MovingForm.prev_px = X - mev.PageX;
					}

					var obj = MovingForm.Self;

					int X1;
					int Y1;

					int W;
					int H;

					if(Y < 0)
						Y = 1;
					if(X < 0)
						X = 1;
					
					switch(MoveAction)
					{
						case MouseMoveAction.Move:
							MovingForm.Content.SetLocation(X, Y);

							break;
						case MouseMoveAction.TopLeftResize:
							Rectange.SetBounds(out X1, out Y1, out W, out H, obj);

							W -= X - X1;
							H -= Y - Y1;

							if(W < MovingForm.MinWidth)
							{
								X -= MovingForm.MinWidth - W;
								W = MovingForm.MinWidth;
							}

							if(H < MovingForm.MinHeight)
							{
								Y -= MovingForm.MinHeight - H;
								H = MovingForm.MinHeight;
							}

							MovingForm.Content.SetBounds(X, Y, W, H);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.TopResize:
							Y1 = Global.ParseInt(obj.Css("top"));
							H = Global.ParseInt(obj.Css("height"));

							H -= Y - Y1;

							if(H < MovingForm.MinHeight)
							{
								Y -= MovingForm.MinHeight - H;
								H = MovingForm.MinHeight;
							}

							obj.Css("top", Y).
								Css("height", H);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.TopRightResize:
							Rectange.SetBounds(out X1, out Y1, out W, out H, obj);

							H -= Y - Y1;
							W = mev.PageX - X1;

							if(H < MovingForm.MinHeight)
							{
								Y -= MovingForm.MinHeight - H;
								H = MovingForm.MinHeight;
							}

							if(W < MovingForm.MinWidth)
							{
								W = MovingForm.MinWidth;
							}

							obj.Css("top", Y).Css("height", H).Css("width", W);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.LeftResize:
							X1 = Global.ParseInt(obj.Css("left"));
							W = Global.ParseInt(obj.Css("width"));

							W -= mev.PageX - X1;

							if(W < MovingForm.MinWidth)
							{
								X -= MovingForm.MinWidth - W;
								W = MovingForm.MinWidth;
							}

							obj.Css("left", X).Css("width", W);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.BottomLeftResize:
							Rectange.SetBounds(out X1, out Y1, out W, out H, obj);

							W -= X - X1;
							H = mev.PageY - Y1;

							if(W < MovingForm.MinWidth)
							{
								X -= MovingForm.MinWidth - W;
								W = MovingForm.MinWidth;
							}

							if(H < MovingForm.MinHeight)
							{
								H = MovingForm.MinHeight;
							}

							obj.Css("left", X).Css("width", W).Css("height", H);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.BottomResize:
							Y1 = Global.ParseInt(obj.Css("top"));
							H = Global.ParseInt(obj.Css("height"));

							H = mev.PageY - Y1;

							if(H < MovingForm.MinHeight)
							{
								H = MovingForm.MinHeight;
							}

							obj.Css("height", H);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.RightResize:
							X1 = Global.ParseInt(obj.Css("left"));
							W = Global.ParseInt(obj.Css("width"));

							W = mev.PageX - X1;

							if(W < MovingForm.MinWidth)
							{
								W = MovingForm.MinWidth;
							}

							obj.Css("width", W);

							MovingForm.Resizing();

							break;
						case MouseMoveAction.BottomRightResize:
							Rectange.SetBounds(out X1, out Y1, out W, out H, obj);

							W = mev.PageX - X1;

							H = mev.PageY - Y1;

							if(H < MovingForm.MinHeight)
							{
								H = MovingForm.MinHeight;
							}
							if(W < MovingForm.MinWidth)
							{
								W = MovingForm.MinWidth;
							}

							obj.Css("width", W).Css("height", H);

							MovingForm.Resizing();

							break;
						default:
							break;
					}
				}
			};
			Window.OnMouseUp = (ev) =>
			{
				if(MovingForm != null)
				{
					MovingForm.BodyOverLay.Style.Visibility = Visibility.Collapse;
				}

				MovingForm = null;
				Mouse_Down = false;
				MoveAction = MouseMoveAction.Move;
			};
			
			WindowHolder.AppendChild(FormOverLay);
			WindowManager.AppendChildren(WindowManagerStart, WindowManagerSearch);

			Parent.AppendChild(WindowHolder);

			SetupWindowManager();

		//	Parent.AppendChild(TaskBar);

			//TaskBar.AppendChild(ButtonStart);
			//TaskBar.AppendChild(InputStartSearch);

			//Window_Desktop = new FileExplorer(WindowHolder) { NodeViewType = NodeViewType.Medium_Icons, Path = FileExplorer.DesktopPath };
		}
		private enum FormButtonType
		{
			Close,
			Maximize,
			Minimize,
			Restore,
			Help
		}

		public void SetWindowState(WindowState State)
		{    
            if(!AllowSizeChange)
				return;
            
			if((windowState = State) == WindowState.Normal)
			{
				this.SetBounds(prev_left, prev_top, prev_width, prev_height);				
				Resizing();
			}
			else if(windowState == WindowState.Maximized)
			{                				
				Rectange.SetBounds(out prev_left, out prev_top, out prev_width, out prev_height, Self);				
				this.SetBounds("0", "0", "calc(100% - 2px)", "calc(100% - 2px)");				
			}
			Resizing();
		}

		private void changeWindowState()
		{
			if(windowState == WindowState.Maximized)
			{
				SetWindowState(WindowState.Normal);
			}
			else
			{
				SetWindowState(WindowState.Maximized);
			}
		}

		private HTMLDivElement CreateFormButton(FormButtonType Type)
		{
			var butt = Div("form-heading-button");
			
			switch(Type)
			{
				case FormButtonType.Close:
					butt.ClassList.Add("form-heading-button-close");					
					butt.Style.Left = StyleController.Calc(100, 45);					
					butt.InnerHTML = "X";
		
					butt.OnMouseDown = (ev) =>
					{
						if(MovingForm != null) //  || WindowHolderSelectionBox != null
							return;
						Mouse_Down = true;

						ev.StopPropagation();
						ev.PreventDefault();						

						ActiveForm = this;
					};

					butt.OnMouseUp = (ev) =>
					{
						if(MovingForm != null) //|| WindowHolderSelectionBox != null
							return;

						ev.StopPropagation();
						ev.PreventDefault();

						Close();
					};

					butt.OnMouseEnter = (ev) =>
					{
						if(MovingForm != null) //  || WindowHolderSelectionBox != null
							return;

						SetCursor(Cursor.Default);
					};

					butt.OnMouseLeave = (ev) =>
					{
						if(MovingForm != null) //  || WindowHolderSelectionBox != null
							return;					
					};

					break;
				case FormButtonType.Maximize:
					butt.Style.Left = StyleController.Calc(100, 91);				
					butt.InnerHTML = "&#9633;";
			
					butt.OnMouseUp = (ev) =>
					{
						if(MovingForm != null) //  || WindowHolderSelectionBox != null
							return;

						ev.StopPropagation();
						ev.PreventDefault();

						Mouse_Down = false;

						changeWindowState();
					};

					break;
				case FormButtonType.Minimize:
					butt.Style.Left = StyleController.Calc(100, 137);
					
					butt.InnerHTML = "-";

					butt.OnMouseUp = (ev) =>
					{
						if(MovingForm != null) // || WindowHolderSelectionBox != null
							return;

						ev.StopPropagation();
						ev.PreventDefault();

						Mouse_Down = false;
						windowState = WindowState.Minimized;
					};

					break;
				case FormButtonType.Restore:
					break;
				case FormButtonType.Help:
					break;
				default:
					butt.OnMouseUp = (ev) =>
					{
						if(MovingForm != null) //  || WindowHolderSelectionBox != null
							return;

						ev.StopPropagation();
						ev.PreventDefault();

						Mouse_Down = false;
					};
					break;
			}

			butt.OnMouseMove = (ev) =>
			{
				if(MovingForm != null) //  || WindowHolderSelectionBox != null
					return;

				ev.StopImmediatePropagation();
				ev.PreventDefault();
			};

			if(Type != FormButtonType.Close)
			{
				butt.OnMouseDown = (ev) =>
				{
					if(MovingForm != null) // || WindowHolderSelectionBox != null
						return;

					Mouse_Down = true;

					ev.StopPropagation();
					ev.PreventDefault();
				
					ActiveForm = this;
				};
			}

			return butt;
		}
        
        public void SetCursor(Cursor cur)
        {
            Content.Style.Cursor = cur;
            Heading.Style.Cursor = cur;
        }		

		public Form() : base("form-base")
		{		
			Heading = Div("form-heading");

            Heading.OnContextMenu = (ev) => {
                ev.StopPropagation();
                ev.PreventDefault();
            };
			
			HeadingTitle = Span("form-heading-title");		
				
			Body = Div("form-body");

			Body.OnContextMenu = (ev) => {
				if(ev.Target == Body)
				{
					ev.StopPropagation();
					ev.PreventDefault();
				}
			};

			BackColor = "#F0F0F0";

            BodyOverLay = Div("form-body-overlay");

			BodyOverLay.Style.Opacity = ShowBodyOverLay ? "0.5" : "0";

			ButtonClose = CreateFormButton(FormButtonType.Close);
			ButtonExpand = CreateFormButton(FormButtonType.Maximize);
			ButtonMinimize = CreateFormButton(FormButtonType.Minimize);

			BodyOverLay.Style.Visibility = Visibility.Collapse;

            Self = jQuery.Select(Content);

            Content.AddEventListener(EventType.MouseDown, (ev) => {
                if (!IsActiveFormCollection())
                    return;

				var mev = ev.As<MouseEvent>();

                mev.StopPropagation();

                Mouse_Down = true;
				
				MovingForm = this;
				ActiveForm = this;
				
				SetBodyOverLay();
				
				prev_px = Global.ParseInt(Self.Css("left")) - mev.PageX;
				prev_py = Global.ParseInt(Self.Css("top")) - mev.PageY;

				var width = Content.ClientWidth;
				var height = Content.ClientHeight;
				Point mouse = new Point(mev.PageX - Content.OffsetLeft, mev.PageY - Content.OffsetTop);				

				if(windowState == WindowState.Maximized)
				{
					SetCursor(Cursor.Default);
					MoveAction = MouseMoveAction.Move;                    
				}else
				{
					if(HeadingTitle != null && ev.Target == HeadingTitle)
					{
						SetCursor(Cursor.Default);						
						MoveAction = MouseMoveAction.Move;						
					}else
					{
						if(AllowSizeChange)
						{
							if(mouse.X <= ResizeCorners && mouse.Y <= ResizeCorners)
							{
								SetCursor(Cursor.NorthWestSouthEastResize);
								MoveAction = MouseMoveAction.TopLeftResize;
							}
							else if(mouse.Y <= ResizeCorners && mouse.X >= width - ResizeCorners)
							{
								SetCursor(Cursor.NorthEastSouthWestResize);
								MoveAction = MouseMoveAction.TopRightResize;
							}
							else if(mouse.Y <= ResizeCorners)
							{
								SetCursor(Cursor.NorthResize);
								MoveAction = MouseMoveAction.TopResize;
							}
							else if(mouse.X <= ResizeCorners && mouse.Y >= height - ResizeCorners)
							{
								SetCursor(Cursor.NorthEastSouthWestResize);
								MoveAction = MouseMoveAction.BottomLeftResize;
							}
							else if(mouse.Y >= height - ResizeCorners && mouse.X >= width - ResizeCorners)
							{
								SetCursor(Cursor.NorthWestSouthEastResize);
								MoveAction = MouseMoveAction.BottomRightResize;
							}
							else if(mouse.Y >= height - ResizeCorners)
							{
								SetCursor(Cursor.SouthResize);							
								MoveAction = MouseMoveAction.BottomResize;
							}
							else if(mouse.X <= ResizeCorners)
							{
								SetCursor(Cursor.WestResize);
								MoveAction = MouseMoveAction.LeftResize;

							}
							else if(mouse.X >= width - ResizeCorners)
							{
								SetCursor(Cursor.EastResize);
								MoveAction = MouseMoveAction.RightResize;
							}
							else
							{
								SetCursor(Cursor.Default);
								MoveAction = MouseMoveAction.Move;
							}
						}												
					}
				}

				if(!AllowMoveChange && MoveAction == MouseMoveAction.Move)
				{
					MoveAction = MouseMoveAction.None;
				}
			});

			Heading.AddEventListener(EventType.DblClick, (ev) => {
                if(AllowSizeChange)
                {
                    changeWindowState();
                }				
				ev.PreventDefault();
				ev.StopPropagation();
			});

			Content.AddEventListener(EventType.MouseMove, (ev) => {
				if(ev.Target == HeadingTitle)
					return;
				var mev = ev.As<MouseEvent>();

				var width = Content.ClientWidth;
				var height = Content.ClientHeight;
				Point mouse = new Point(mev.PageX - Content.OffsetLeft, mev.PageY - Content.OffsetTop);				

				if(MovingForm != null && MoveAction == MouseMoveAction.Move)
				{
					SetCursor(Cursor.Default);
					return;
				}
				else if(windowState == WindowState.Maximized)
				{
					SetCursor(Cursor.Default);
					return;
				}
                if(AllowSizeChange)
                {
                    if (MoveAction == MouseMoveAction.TopLeftResize || mouse.X <= ResizeCorners && mouse.Y <= ResizeCorners)
                    {
                        SetCursor(Cursor.NorthWestSouthEastResize);
                    }
                    else if (MoveAction == MouseMoveAction.TopRightResize || mouse.Y <= ResizeCorners && mouse.X >= width - ResizeCorners)
                    {
                        SetCursor(Cursor.NorthEastSouthWestResize);
                    }
                    else if (mouse.Y <= ResizeCorners || MoveAction == MouseMoveAction.TopResize)
                    {
                        SetCursor(Cursor.NorthResize);
                    }
                    else if (MoveAction == MouseMoveAction.BottomLeftResize || mouse.X <= ResizeCorners && mouse.Y >= height - ResizeCorners)
                    {
                        SetCursor(Cursor.NorthEastSouthWestResize);
                    }
                    else if (MoveAction == MouseMoveAction.BottomRightResize || mouse.Y >= height - ResizeCorners && mouse.X >= width - ResizeCorners)
                    {
                        SetCursor(Cursor.NorthWestSouthEastResize);
                    }
                    else if (MoveAction == MouseMoveAction.BottomResize || mouse.Y >= height - ResizeCorners)
                    {
                        SetCursor(Cursor.SouthResize);
                    }
                    else if (MoveAction == MouseMoveAction.LeftResize || mouse.X <= ResizeCorners)
                    {
                        SetCursor(Cursor.WestResize);
                    }
                    else if (MoveAction == MouseMoveAction.RightResize || mouse.X >= width - ResizeCorners)
                    {
                        SetCursor(Cursor.EastResize);
                    }
                    else
                    {
                        SetCursor(Cursor.Default);
                    }
                }else
                {
                    SetCursor(Cursor.Default);
                }
				
			});
						
			Heading.AddEventListener(EventType.MouseDown, (ev) => {
				SetBodyOverLay();
                if (!IsActiveFormCollection())
                    return;

				if(windowState == WindowState.Maximized)
				{
					MovingForm = this;
					SetCursor(Cursor.Default);

					MoveAction = MouseMoveAction.Move;
				}
				else
				{
					MovingForm = this;
				}

				ActiveForm = this;
			});												

			Body.AddEventListener(EventType.MouseDown, (ev) => {
                if (!IsActiveFormCollection())
                    return;

				ActiveForm = this;
				MovingForm = null;
				SetCursor(Cursor.Default);
				ev.StopPropagation();
			});

			Body.AddEventListener(EventType.MouseMove, (ev) => {
				if(MovingForm == null)
				{
                    if (!IsActiveFormCollection())
                        return;

					SetCursor(Cursor.Default);
					ev.StopPropagation();
				}
			});			

			BodyOverLay.AddEventListener(EventType.MouseDown, (ev) =>
			{
                if (!IsActiveFormCollection())
                    return;
				BodyOverLay.Style.Visibility = Visibility.Collapse;
				ActiveForm = this;
			});
			
			Body.AddEventListener(EventType.MouseLeave, (ev) =>
			{
				if(MovingForm == null)
				{
					SetBodyOverLay();
				}
			});

			BodyOverLay.AddEventListener(EventType.MouseEnter, (ev) =>
			{
				if(MovingForm == null && IsActiveFormCollection()) // WindowHolderSelectionBox == null && 
				{
					BodyOverLay.Style.Visibility = Visibility.Collapse;
				}
				else
				{
					BodyOverLay.Style.Visibility = Visibility.Visible;
				}
			});

			jQuery.Select(Content)
				.Css("width", Window_DefaultWidth)
				.Css("height", Window_DefaultHeight);

			Content.AppendChild(Heading);
			Content.AppendChild(Body);
			Content.AppendChild(BodyOverLay);

			Heading.AppendChild(HeadingTitle);
			Heading.AppendChild(ButtonClose);
			Heading.AppendChild(ButtonExpand);
			Heading.AppendChild(ButtonMinimize);

			Initialise();
		}

		public int TitleBarHeight()
		{
			return Heading.ClientHeight;
		}

		public int TitleBarWidth()
		{
			return Heading.ClientWidth;
		}

		public int ClientX()
		{
			return Body.ClientLeft;
		}

		public int ClientY()
		{
			return Body.ClientTop;
		}

		public string Height
		{
			get { return Content.Style.Height; }
			set { Content.Style.Height = value; }
		}

		public string Width
		{
			get { return Content.Style.Width; }
			set { Content.Style.Width = value; }
		}

		public string Left
		{
			get { return Content.Style.Left; }
			set { Content.Style.Left = value; }
		}

		public string Top
		{
			get { return Content.Style.Top; }
			set { Content.Style.Top = value; }
		}

		public string Text
		{
			get { return HeadingTitle.InnerHTML; }
			set { HeadingTitle.InnerHTML = value; }
		}

		public string BackColor
		{
			get { return Body.Style.BackgroundColor; }
			set { Body.Style.BackgroundColor = value; }
		}

		public string ForeColor
		{
			get { return Body.Style.Color; }
			set { Body.Style.Color = value; }
		}

        public List<DialogResult> DialogResults = new List<DialogResult>();

        public FormCollection GetFormCollectionFromForm(Form form)
        {
            for (int i = 0; i < FormCollections.Count; i++)
            {
                if (this == FormCollections[i].FormOwner)
                    return FormCollections[i];
                var visibleForms = FormCollections[i].VisibleForms;
                for (int x = 0; x < visibleForms.Count; x++)
                {
                    if (visibleForms[x] == this)
                        return FormCollections[i];
                }
            }
            return null;
        }

        public bool IsActiveFormCollection()
        {
            return GetFormCollectionFromForm(this) == GetActiveFormCollection();        
        }

        public bool IsVisible()
        {
            return GetFormCollectionFromForm(this) != null;            
        }

        public void ShowStartNewLevel(HTMLElement owner = null)
        {
            if (IsVisible())
            {
                // Already Open???
                throw new Exception("Invalid request to open form as a dialog that is already visible!");
            }
            AddFormToParentElement(owner);
            
            Body.Focus();

            FormCollections.Add(new FormCollection(this));

            CalculateZOrder();

            OnShowed();

            ActiveForm = this;
        }		

		public void ShowDialog(params DialogResult[] dialogResults)
		{
			InDialogResult = false;

			if (ButtonMinimize != null)
                ButtonMinimize.Delete();
            if (ButtonExpand != null)
                ButtonExpand.Delete();
            if (ButtonClose != null)
                ButtonClose.Delete();

            if (dialogResults != null && dialogResults.Length > 0)
                DialogResults.AddRange(dialogResults);
            StartPosition = FormStartPosition.Center;

            _IsDialog = true;                        

            ShowStartNewLevel(null);

            CentreForm();

            ActiveForm = this;
        }

		private float MinZero(float input)
		{
			return input < 0 ? 0 : input;
		}
		private int MinZero(int input)
		{
			return input < 0 ? 0 : input;
		}

		public void CentreForm()
        {
            if (Owner == null)
                return;
		
			Self
            .Css("left", MinZero((Owner.ClientWidth / 2) - (Global.ParseInt(this.Width) / 2)))
            .Css("top", MinZero((Owner.ClientHeight / 2) - (Global.ParseInt(this.Height) / 2)));
        }

        protected void AddFormToParentElement(HTMLElement owner = null)
        {
            if (!HasRendered)
            {
                Render();
                HasRendered = true;
            }

            OnShowing();

			if(owner == null)
			{
				WindowHolder.AppendChild(Content);
				owner = WindowHolder;				
			}
			else
				owner.AppendChild(Content);				
			Shown();

			Owner = owner;
        }

		public void Shown()
		{
			if(Children == null)
				return;
			for(int i = 0; i < Children.Count; i++)
			{
				if(Children[i] != null &&
					Children[i].OnLoaded != null)
				{
					Children[i].OnLoaded(Children[i]);
				}
			}
			Children.Remove(null);
		}

		public void Show(HTMLElement owner = null)
		{
            if (_IsDialog)
                return;

			if(FormCollections == null || FormCollections.Count == 0)
			{
				ShowStartNewLevel(owner);
				return;
			}

            var activeCollect = GetActiveFormCollection();
            var visbileForms = activeCollect.VisibleForms;
            
			if(!visbileForms.Contains(this))
			{				
                AddFormToParentElement();

                Content.Style.Visibility = Visibility.Visible;
				if(StartPosition != FormStartPosition.Manual && windowState == WindowState.Normal)
				{
                    if (StartPosition == FormStartPosition.Center || (activeCollect == null || visbileForms == null || visbileForms.Count == 0 || visbileForms[visbileForms.Count - 1].windowState != WindowState.Normal || visbileForms[visbileForms.Count - 1].Content == null))
					{
                        CentreForm();

                    }
					else if(StartPosition == FormStartPosition.WindowsDefaultLocation)
					{
						var obj = visbileForms[visbileForms.Count - 1];

						int x = Global.ParseInt(obj.Left);
						int y = Global.ParseInt(obj.Top);

						double pw25 = Owner.ClientWidth * 0.15;
						double ph25 = Owner.ClientHeight * 0.15;

						double pw75 = Owner.ClientWidth * 0.55;
						double ph75 = Owner.ClientHeight * 0.55;						

						if(x < pw25)						
							x = (int)pw25;						
						if(y < ph25)						
							y = (int)ph25;						

						if(x > pw75)
							x = (int)pw25;
						if(y > ph75)
							y = (int)ph25;
						x += 10;
						y += 10;

						Self.Css("left", MinZero(x))
							.Css("top", MinZero(y));
					}
				}				

				Body.Focus();

                visbileForms.Add(this);

				CalculateZOrder();

				OnShowed();
			}

			ActiveForm = this;
		}

		public void BringToFront()
		{
            var activeCollect = GetActiveFormCollection();
            if(activeCollect != null)
            {
                if (activeCollect.FormOwner == this)
                    return;
                var visibleForms = activeCollect.VisibleForms;
				if(visibleForms != null && visibleForms.Count > 1)
				{
					visibleForms.Remove(this);
					visibleForms.Add(this);
				}

				CalculateZOrder();
            }            
		}

        public void SetZIndex(ref int zIndex)
        {
			this.Content.Style.ZIndex = (zIndex++).ToString();

			//Self.Css("zIndex", zIndex++);
        }

		public static void CalculateZOrder()
		{
            GetActiveFormCollection();

			if(FormCollections == null)
				return;
			FormCollections.Remove(null);

            int zIndex = 1;
			
            if(FormCollections.Count == 1)            
                FormOverLay.Style.Opacity = "0";            
            else            
                FormOverLay.Style.Opacity = "0.4";                                

            for (int x = 0; x < FormCollections.Count; x++)
            {                
                if(x == FormCollections.Count - 1)
                {
					FormOverLay.Style.ZIndex = (zIndex++).ToString();
					//jQuery.Select(FormOverLay).Css("zIndex", zIndex++);
                }

                List<Form> TopMostForms = new List<Form>();                

                var VisibleForms = FormCollections[x].VisibleForms;

				if(FormCollections[x].FormOwner != null)
					FormCollections[x].FormOwner.SetZIndex(ref zIndex);

				for (int i = 0; i < VisibleForms.Count; i++)
                {
                    if (VisibleForms[i].Content == null)
                    {
                        ToClean.Add(VisibleForms[i]);
                    }
                    else
                    {
                        if (VisibleForms[i].TopMost)
                            TopMostForms.Add(VisibleForms[i]);
                    }
                }
                for (int i = 0; i < ToClean.Count; i++)
                {
                    if(VisibleForms.Contains(ToClean[i]))
                    {
                        VisibleForms.Remove(ToClean[i]);
                        ToClean[i] = null;
                    }
                    
                }
                ToClean.Remove(null); // Removes all nulls..

                for (int i = 0; i < TopMostForms.Count; i++)
                {
                    var form = TopMostForms[i];
                    VisibleForms.Remove(form);
                    VisibleForms.Add(form);
                }
                for (int i = 0; i < VisibleForms.Count; i++)
                {
                    if (VisibleForms[i] != null &&
                        VisibleForms[i].Content != null)
                    {
                        VisibleForms[i].SetZIndex(ref zIndex);
                    }
                }
            }            
		}

		public static List<Form> ToClean = new List<Form>();

		public void Close()
		{
			if(_IsDialog && InDialogResult)
				return;
			OnClosing();           

			ToClean.Add(this);

            var ownerFormCollection = GetFormCollectionFromForm(this);

            if(ownerFormCollection != null)
            {
                if(ownerFormCollection.FormOwner == this)
                {
                    ownerFormCollection.FormOwner = null;
                    for (int i = 0; i < ownerFormCollection.VisibleForms.Count; i++)
                    {
                        if (ownerFormCollection.VisibleForms[i] == this)
                            continue;
                        ownerFormCollection.VisibleForms[i].Close();
                    }                    
                }
                else
                {
                    ownerFormCollection.VisibleForms.Remove(this);
                }
            }

			if(Content != null)
			{
				jQuery.Select(Content).Empty();
				if(Content != null)
				{
					Content.Delete(); Content = null;
				}
				//jQuery.Select(Content).FadeOut(FadeLength, () => {
				//	jQuery.Select(Content).Empty();
				//	if(Content != null)
				//	{
				//		Content.Delete(); Content = null;
				//	}			
				//});
			}

			CalculateZOrder();

			ActiveForm = _PrevActiveForm;
			if(_IsDialog)
			{
				InDialogResult = true;
				if(DialogResult != DialogResultEnum.None &&
				DialogResults != null && DialogResults.Count > 0)
				{
					for(int i = 0; i < DialogResults.Count; i++)
						DialogResults[i].InvokeIfResult(DialogResult);
				}
			}						

            OnClosed();			
		}

		private bool InDialogResult = false;

		public void FillControlWithParent(HTMLElement element, int widthOffset = 8, int heightOffset = 9)
		{
			element.Style.Position = Position.Absolute;
			element.Style.Width = StyleController.Calc(100, widthOffset);
			element.Style.Height = StyleController.Calc(100, heightOffset);

			element.Style.Top = "1px";
			element.Style.Left = "1px";
		}

		public void FillHorizontalControlWithParent(HTMLElement element, int widthOffset = 8)
		{
			element.Style.Position = Position.Absolute;
			element.Style.Width = StyleController.Calc(100, widthOffset);

			element.Style.Left = "1px";
		}

		public void FillVerticalControlWithParent(HTMLElement element, int heightOffset = 9)
		{
			element.Style.Position = Position.Absolute;
			element.Style.Height = StyleController.Calc(100, heightOffset);

			element.Style.Top = "1px";
		}
	}
}