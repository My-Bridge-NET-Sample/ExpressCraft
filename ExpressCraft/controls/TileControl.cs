﻿using Bridge;
using static Retyped.dom;
using System.Collections.Generic;

namespace ExpressCraft
{
    public class TileControl : Control, IPages
    {
        public List<RibbonPage> RibbonPages { get; set; } = new List<RibbonPage>();
        private TileViewState _viewState = TileViewState.Hidden;
        private string prevOverFlow = null;
        private int ClearTimeOut = -1;

        public TileViewState ViewState
        {
            get { return _viewState; }
            set
            {
                if(ClearTimeOut != -1)
                {
                    clearTimeout(ClearTimeOut);
                    ClearTimeOut = -1;
                }
                if(value != _viewState)
                {
                    _viewState = value;
                    if(_viewState == TileViewState.Hidden)
                    {
                        ClearTimeOut = (int)setTimeout((obj) =>
                        {
                            Content.style.visibility = "hidden";
                        }, 1000);
                        Location = new Vector2("(100% * -1)", 0);

                        if(Content.parentElement != null && prevOverFlow != null)
                        {
                            Content.parentElement.style.overflow = prevOverFlow;
                        }
                    }
                    else
                    {
                        Location = new Vector2(0, 0);
                        Content.style.visibility = "inherit";
                        RenderTiles();
                        if(Content.parentElement != null)
                        {
                            prevOverFlow = Content.parentElement.style.overflow;
                            Content.parentElement.style.overflow = "hidden";
                        }
                    }
                }
            }
        }

        public void RenderTiles()
        {
            int x = 0;
            int y = 6;

            var doc = document.createDocumentFragment();
            var div = new Control();
            div.Width = "100%";
            div.SetLocation(0, 0);
            div.Style.zIndex = "10";

            var div2 = new Control("primary");
            div2.Width = "100%";
            div2.Style.minHeight = "100%";
            div2.SetLocation(0, 0);
            div2.Style.filter = "brightness(50%)";

            div2.Style.opacity = "0.9";
            div2.Style.zIndex = "9";

            foreach(var page in RibbonPages)
            {
                if(!string.IsNullOrWhiteSpace(page.Caption))
                {
                    var llb = Control.Label(page.Caption, 6, y);
                    llb.style.fontSize = "14px";
                    llb.style.color = "white";
                    doc.appendChild(llb);
                    y += 26;
                }

                foreach(var group in page.RibbonGroups)
                {
                    foreach(var item in group.Items)
                    {
                        if(item is RibbonButton)
                        {
                            var button = (RibbonButton)item;

                            item.ExchangeClass("ribbonbuttonsmall", "ribbonbutton");
                            item.Style.borderRadius = "4px";

                            button.AfterItemClick = (ev) =>
                            {
                                ViewState = TileViewState.Hidden;
                            };
                            if(item.Enabled)
                            {
                                item.ExchangeClass("primary", "primary");
                                item.Style.border = "0";
                                item.Style.filter = "brightness(110%)";
                            }
                            else
                            {
                                item.ExchangeClass("primary", "");
                                item.Style.border = "1px";
                                item.Style.filter = "";
                            }
                            item.Style.opacity = "1";
                            item.Style.boxShadow = "0px 0px 10px -2px rgba(0,0,0,0.25)";
                            button.IsSmallItem = false;
                            item.ProcessCaption();
                            button.ProcessImage();
                            item.Size = new Vector2("((100% - 24px) * 0.33)", 100);
                            item.Location = new Vector2("(((100% - 24px) * (" + (x * 0.33m) + ")) + (" + (6 * (x)) + "px) + 6px)", y);

                            if(item.captionDiv != null)
                            {
                                if(item.Enabled)
                                {
                                    item.captionDiv.style.color = "white";
                                }
                                else
                                {
                                    item.captionDiv.style.color = "grey";
                                }

                                item.captionDiv.style.fontSize = "14px";
                                item.captionDiv.style.top = "65px";
                            }

                            if(button.imageDiv != null)
                            {
                                button.imageDiv.ExchangeClass("ribbonbuttonsmallicon", "ribbonbuttonicon");
                                button.IconURL = (button.IconURL + "").Replace("x16x16", "x32x32");
                                button.imageDiv.style.top = "27px";
                                if(item.Enabled)
                                {
                                    //  button.imageDiv.Style.filter = "brightness(90%) grayscale(100%) contrast(60%) brightness(180%)";
                                }
                                else
                                {
                                    //  button.imageDiv.Style.filter = "";
                                }
                            }

                            if(x == 2)
                            {
                                x = 0;
                                y += 106;
                            }
                            else
                            {
                                x++;
                            }
                            item.Style.opacity = "1";
                            doc.appendChild((Node)item);
                        }
                        
                    }
                }

                if(x != 0)
                {
                    x = 0;
                    y += 106;
                }
            }

            div.Height = y;
            div2.Height = y;

            Content.Empty();
            div.Content.appendChild(doc);

            div2.Content.onmousedown = (ev) =>
            {
                ev.stopPropagation();
            };

            Content.appendChild((Node)div2);
            Content.appendChild((Node)div);

            div.Content.click();
        }

        public override void Render()
        {
            base.Render();
            RenderTiles();
        }

        public TileControl() : base()
        {
            Style.overflowY = "auto";
            Location = new Vector2("(100% * -1)", 0);
            Size = new Vector2("100%", "100%");

            Content.style.backgroundColor = "transparent";
            Content.style.visibility = "hidden";
            Content.style.transition = "left 1s ease";
        }

        public void AddRibbonPages(params RibbonPage[] pages)
        {
            if(pages != null)
            {
                foreach(var item in pages)
                {
                    if(item != null)
                    {
                        RibbonPages.Add(item);
                    }
                }                
            }
                
        }
    }

    public enum TileViewState
    {
        Hidden,
        Visible
    }
}