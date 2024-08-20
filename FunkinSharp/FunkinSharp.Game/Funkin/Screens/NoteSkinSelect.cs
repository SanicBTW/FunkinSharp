using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin.Notes;
using FunkinSharp.Game.Funkin.Skinnable.Notes;
using FunkinSharp.Game.Funkin.Text;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;

namespace FunkinSharp.Game.Funkin.Screens
{
    // most of the transition and setting ui up code is from song selector and settings screen
    public partial class NoteSkinSelect : FunkinScreen
    {
        // Required stuff for da transition
        private Camera camera = new(false); // World camera
        private AtlasText transObj;

        private BindableFloat worldZoom = new BindableFloat(1);

        // ui stufffss
        private float defaultMargin = 16f;
        private float spaceHeight = 25f; // the space from the center between the tween object and the ui view
        private float spaceTwnObj = 3.25f; // the space between the tween object and the ui view
        private float timesSpace = 12f; // the times the space will get multiplied by
        private float defaultRounding = 15f;
        private Container uiView;

        private Container sideBar;
        private Container contentView;
        private FillFlowContainer content;

        // for da out transition
        private bool backing = false;
        private bool canBack = true;

        // inputs
        private bool canPress = true;
        private bool blocked = false;

        // dacool
        private Bindable<string> currentSkin = new("funkin");

        public NoteSkinSelect()
        {
            OnActionPressed += actionPressed;
            OnActionReleased += actionReleased;

            worldZoom.BindValueChanged((ev) =>
            {
                camera.Zoom = ev.NewValue;
            });
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            OnActionPressed -= actionPressed;
            OnActionReleased -= actionReleased;
            return base.OnExiting(e);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore store, FunkinConfig config)
        {
            currentSkin = config.GetBindable<string>(FunkinSetting.CurrentNoteSkin);

            CursorVisible = true;
            Add(camera);

            camera.Add(new Sprite()
            {
                Texture = store.Get("General/BGS/menuBG"),
                Size = new Vector2(GameConstants.WIDTH, GameConstants.HEIGHT),
                //Colour = Colour4.MidnightBlue
                Colour = Colour4.MediumSeaGreen
            });

            uiView = new Container()
            {
                Masking = true,
                CornerRadius = defaultRounding,
                Position = new Vector2(0, spaceHeight * spaceTwnObj),
                Size = new Vector2(GameConstants.WIDTH - defaultMargin * 2, GameConstants.HEIGHT - (spaceHeight * spaceTwnObj) - defaultMargin * 2),
                Margin = new MarginPadding(defaultMargin),
                Alpha = 0f
            };

            uiView.Children = new Drawable[]
            {
                new Box
                {
                    Colour = Colour4.Black,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.45f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        // Side Bar
                        new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = spaceHeight * timesSpace,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Colour4.Black,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.5f
                                },
                                sideBar = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                        // Content
                        contentView = new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            X = spaceHeight * timesSpace,
                            Width = uiView.Width - (spaceHeight * timesSpace),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Colour4.Black,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.15f,
                                },
                                new BasicScrollContainer(Direction.Horizontal)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(defaultMargin),
                                    Child = content = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Direction = FillDirection.Horizontal,
                                    }
                                }
                            }
                        }
                    }
                }
            };

            camera.Add(uiView);
            prepareSkins();

            camera.Add(transObj = new AtlasText("Note Skin Selector", fontName: AtlasFontType.BOLD)
            {
                Alpha = 0f
            });
        }

        protected override void LoadComplete()
        {
            transObj.Position = new Vector2((camera.DrawWidth / 2) - (transObj.DrawWidth / 2), (camera.DrawHeight / 2) - (transObj.DrawHeight / 2));
            transObj.Alpha = 1f;

            transObj.MoveToY((transObj.DrawHeight / 2), 500D, Easing.OutSine).OnComplete((_) =>
            {
                uiView.FadeIn(250D, Easing.InQuint);
            });

            base.LoadComplete();
        }

        private void actionPressed(FunkinAction action)
        {
            if (!canPress)
                return;

            switch (action)
            {
                case FunkinAction.BACK:
                    if (backing || !canBack)
                        return;

                    backing = true;

                    uiView.FadeOut(250D, Easing.OutQuint).OnComplete((_) =>
                    {
                        transObj.MoveTo(new Vector2((camera.DrawWidth / 2) - (transObj.DrawWidth / 2), (camera.DrawHeight / 2) - (transObj.DrawHeight / 2)), 500D, Easing.InSine).OnComplete((_) =>
                        {
                            Schedule(() =>
                            {
                                Game.ScreenStack.Push(new ChartFormatSelect(true));
                            });
                        });
                    });
                    break;

                default:
                    break;
            }
        }

        private void actionReleased(FunkinAction action)
        {
            if (blocked)
                return;

            canPress = true;
        }

        private void prepareSkins()
        {
            List<string> skins = NoteSkinRegistry.Scan();
            if (!skins.Contains(currentSkin.Value)) // in case some skin was deleted, reset to the default one
                currentSkin.Value = "funkin";

            foreach (string skin in skins)
            {
                Vector2 previewSize = new Vector2(210, uiView.DrawHeight - defaultMargin * 4); // because the margin gets applied in the container too

                if (skin == currentSkin.Value)
                {
                    previewSize = sideBar.DrawSize - new Vector2(defaultMargin * 2);
                    SelectedSkin curSkin = new SelectedSkin(skin)
                    {
                        Size = previewSize,
                    };
                    curSkin.Clicked += preview_Clicked;
                    sideBar.Add(curSkin);
                    continue;
                }

                NoteSkinPreview preview = new NoteSkinPreview(skin)
                {
                    Size = previewSize,
                };
                preview.Clicked += preview_Clicked;
                content.Add(preview);
            }
        }

        private void preview_Clicked(NoteSkinPreview obj)
        {
            // do nothing since expanding the preview is double click
            if (sideBar[0] == obj)
                return;

            // TODO: Move the selected skin and the future selected skin to its desired positions, probably need to use some proxy shit but gonna keep it like this
            // should probably preserve the order too, might need to make a function to refresh the skin preview rather than changing objects positions
            SelectedSkin curSkin = (SelectedSkin)sideBar[0];

            // changing the bindable value triggers saving
            currentSkin.Value = obj.Skin;

            NoteSkinPreview last = (NoteSkinPreview)content[^1];
            float nextX = (sideBar.Width + last.Width / 2) + (last.X + last.Width);
            Vector2 selSize = curSkin.Size;
            Vector2 oldSize = last.Size;

            obj.FadeOut(350D, Easing.OutQuint).OnComplete((_) =>
            {
                curSkin.MoveTo(new Vector2(nextX, last.Margin.Top), 750D, Easing.OutQuint);
                curSkin.ResizeTo(oldSize, 750D, Easing.OutQuint);

                Scheduler.AddDelayed(() =>
                {
                    content.Remove(obj, false);
                    sideBar.Remove(curSkin, false);

                    curSkin.Clicked -= preview_Clicked;
                    curSkin.Clicked += preview_Clicked;
                    content.Add(curSkin);

                    SelectedSkin newSelected = new SelectedSkin(obj)
                    {
                        Size = selSize
                    };
                    newSelected.Clicked -= preview_Clicked;
                    newSelected.Clicked += preview_Clicked;
                    sideBar.Add(newSelected);

                    newSelected.FadeIn(350D, Easing.InQuint);
                }, 500D);
            });
        }

        // TODO: Let the user decide if it should load the legacy or the new sustain sheet
        private partial class NoteSkinPreview : Container
        {
            public readonly string Skin;

            private float defaultMargin = 16f;
            private float defaultRounding = 15f;

            public event Action<NoteSkinPreview> Clicked;

            protected Box BG;
            private SkinnableReceptor receptor;
            private SkinnableNote head;
            private SkinnableSustain tail;

            protected override bool OnClick(ClickEvent e)
            {
                Clicked?.Invoke(this);
                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                BG.FadeColour(Colour4.Gray, 150D, Easing.InQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                BG.FadeColour(Colour4.Black, 150D, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            public NoteSkinPreview(string skin)
            {
                Skin = skin;

                Masking = true;
                CornerRadius = defaultRounding;
                Margin = new MarginPadding(defaultMargin);

                AddRange(new Drawable[]
                {
                    BG = new Box
                    {
                        Colour = Colour4.Black,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f
                    },
                    new SpriteText
                    {
                        Text = skin,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = new FontUsage(family: "RedHatDisplay", size: defaultMargin * 3, weight: "Bold"),
                        Margin = new MarginPadding(defaultMargin / 2)
                    },
                    receptor = new SkinnableReceptor(0, skin)
                    {
                        Y = -120
                    },
                    head = new SkinnableNote(0, 0, isPreview: true, skin: skin),
                    tail = new SkinnableSustain(head)
                    {
                        TargetHeight = { Value = Sustain.SustainHeight(210, 2.2f) },
                        Alpha = 1,
                    }
                });

                receptor.OnLoadComplete += delegate (Drawable obj)
                {
                    float receptorWidth = receptor.Texture.DisplayWidth;
                    receptor.SetGraphicSize(receptorWidth * receptor.ReceptorData.Size);
                };
            }
        }

        private partial class SelectedSkin : NoteSkinPreview
        {
            public event Action<SelectedSkin> ExpandRequest;

            protected override bool OnDoubleClick(DoubleClickEvent e)
            {
                ExpandRequest?.Invoke(this);
                return base.OnDoubleClick(e);
            }

            public SelectedSkin(NoteSkinPreview existing) : base(existing.Skin)
            {
                Alpha = 0;
            }

            public SelectedSkin(string skin) : base(skin) { }
        }
    }
}
