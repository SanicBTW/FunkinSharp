using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Text;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;

namespace FunkinSharp.Game.Funkin.Screens
{
    // Similar to Settings Screen
    // TODO: Blur
    // TODO: Show the user scroll speed alongside the song speed
    // TODO: Make it that the constructor can accept a color the bg will apply
    // TODO: OSU and Quaver charts diffs might be lil bit hard
    public partial class SongSelector : FunkinScreen
    {
        // Required stuff for da transition
        private Camera camera = new(false); // World camera
        private AtlasText transObj;
        private string transContent;
        private bool fromGameplay;

        private BindableFloat worldZoom = new BindableFloat(1);

        // ui stuff
        // WHAR IS THIS NAMING BRUHH
        private static Colour4 cbColor => Colour4.FromHex("3A3C41");
        private static Dictionary<string, Colour4> diffColors => new()
        {
            { "Easy", Colour4.Green },
            { "Normal", Colour4.Yellow },
            { "Hard", Colour4.Red },
            { "Erect", Colour4.LightPink },
            { "Nightmare", Colour4.Purple }
        };

        private float defaultMargin = 16f;
        private float spaceHeight = 25f; // the space from the center between the tween object and the ui view
        private float spaceTwnObj = 4.5f; // the space between the tween object and the ui view
        private float timesSpace = 8f; // the times the space will get multiplied by
        private float defaultRounding = 15f;
        private Container uiView;

        private Container topBar;
        private Container toolBar;
        private AlbumSprite albumSpr;
        private SpriteText songTitle;
        private SpriteText songAuthor;
        private SpriteText songBPM;
        private SpriteText songCharter;
        private SpriteText songSpeed;
        private PlayButton playBtn;
        private DiffComboBox diffSelector;
        private SongItem curSong;

        private Container contentView;
        private FillFlowContainer content;

        // for da out transition
        private bool backing = false;
        private bool canBack = true;

        // inputs
        private bool canPress = true;
        private bool blocked = false;

        // for listing
        private string targetFormat = "FNF VSlice";

        // song preview
        private Track preview;
        private double volFade = 350D;
        private double targetVol = 0.7D;
        private double previewTime = 15000D; // 15 secs
        private bool noPreviewTime = false;

        public SongSelector(string inTransContent, string format = null, bool fromGameplay = false)
        {
            transContent = inTransContent;
            targetFormat = format ?? targetFormat;
            this.fromGameplay = fromGameplay;

            OnActionPressed += actionPressed;
            OnActionReleased += actionReleased;
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
            CursorVisible = true;
            noPreviewTime = !config.Get<bool>(FunkinSetting.FullSongInSelection);

            Add(camera);

            camera.Add(new Sprite()
            {
                Texture = store.Get("General/BGS/menuBG"),
                Size = new Vector2(GameConstants.WIDTH, GameConstants.HEIGHT),
            });

            worldZoom.BindValueChanged((ev) =>
            {
                camera.Zoom = ev.NewValue;
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
                        // NOTE: Top Bar must be after the view content, so that everything works as intended
                        // View Content
                        contentView = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = spaceHeight * timesSpace,
                            Height = uiView.Height - (spaceHeight * timesSpace),
                            Child = new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                // To make the fill flow really scroll
                                // You have to set these params to scroll vertically
                                Child = content = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical
                                }
                            }
                        },
                        // Top Bar
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = spaceHeight * timesSpace,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Colour4.Black,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.55f
                                },
                                topBar = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        }
                    }
                }
            };

            camera.Add(uiView);
            setupUI(); // once the layout is done, set the visuals

            camera.Add(transObj = new AtlasText(transContent, fontName: AtlasFontType.BOLD)
            {
                Alpha = 0f
            });
        }

        protected override void LoadComplete()
        {
            void load()
            {
                uiView.FadeIn(250D, Easing.InQuint);
                if (!ChartRegistry.CACHED_METADATA.ContainsKey(targetFormat) || ChartRegistry.CACHED_METADATA[targetFormat].Count < 0)
                {
                    content.Add(new Container
                    {
                        Masking = true,
                        CornerRadius = 15,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(660f, 265f),
                        Margin = new MarginPadding(defaultMargin / 2),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.DarkSlateGray,
                            },
                            new FillFlowContainer
                            {
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Margin = new MarginPadding(defaultMargin),
                                        Text = "Missing playable songs for this format",
                                        Font = FontUsage.Default.With(size: 32)
                                    },
                                    new SpriteText
                                    {
                                        Margin = new MarginPadding(defaultMargin),
                                        Text = "if you recently added songs to this format, restart the game",
                                        Font = FontUsage.Default.With(size: 32)
                                    },
                                    new SpriteText
                                    {
                                        Margin = new MarginPadding(defaultMargin),
                                        Text = "but if you didn't maybe this chart format isn't supported yet",
                                        Font = FontUsage.Default.With(size: 32)
                                    },
                                    new SpriteText
                                    {
                                        Margin = new MarginPadding(defaultMargin),
                                        Text = "so check future updates!",
                                        Font = FontUsage.Default.With(size: 32)
                                    }
                                }
                            }
                            
                        }
                    });
                    return;
                }

                BasicTextBox searchBox = new BasicTextBox()
                {
                    PlaceholderText = "Search song",
                    Margin = new MarginPadding(16),
                    Size = new Vector2(1216, 52), // default width for song item
                };
                searchBox.Current.BindValueChanged((ev) =>
                {
                    foreach (Drawable obj in content)
                    {
                        if (obj is SongItem item)
                        {
                            if (item.Metadata.SongName.Contains(ev.NewValue) ||
                                item.Metadata.SongName.ToLower().Contains(ev.NewValue) ||
                                ev.NewValue.Length <= 0)
                                item.FadeIn(250D, Easing.InQuint);
                            else
                                item.FadeOut(250D, Easing.OutQuint);
                        }
                    }
                });
                content.Add(searchBox);

                foreach (BasicMetadata cached in ChartRegistry.CACHED_METADATA[targetFormat])
                {
                    SongItem item = new SongItem(contentView, cached)
                    {
                        OnItemClick = onItemClick
                    };
                    content.Add(item);
                }
            }

            if (!fromGameplay)
            {
                transObj.Position = new Vector2((camera.DrawWidth / 2) - (transObj.DrawWidth / 2), (camera.DrawHeight / 2) - (transObj.DrawHeight / 2));
                transObj.Alpha = 1f;

                transObj.MoveTo(new Vector2(25), 500D, Easing.OutSine).OnComplete((_) =>
                {
                    load();
                });
            }
            else
            {
                transObj.Position = new Vector2(25);
                transObj.Alpha = 1f;
                worldZoom.Value = 3.15f;

                this.FadeIn(1800D, Easing.InQuint);
                this.TransformBindableTo(worldZoom, 1f, 1250D, Easing.OutQuint).OnComplete((_) =>
                {
                    load();
                });
            }

            base.LoadComplete();
        }

        protected override void Update()
        {
            if (preview != null)
            {
                if (preview.CurrentTime >= previewTime && !noPreviewTime || preview.CurrentTime >= preview.Length - 500D && noPreviewTime)
                {
                    this.TransformBindableTo(preview.Volume, 0D, volFade);

                    Scheduler.AddDelayed(() =>
                    {
                        preview.Stop();
                        preview.Seek(0D);
                    }, volFade + 100D);

                    Scheduler.AddDelayed(() =>
                    {
                        preview.Start();
                    }, volFade + 150D);

                    this.Delay(volFade * 2).TransformBindableTo(preview.Volume, targetVol, volFade * 2);
                }
            }

            base.Update();
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

                    if (preview != null)
                        this.TransformBindableTo(preview.Volume, 0D, volFade + 150D);

                    uiView.FadeOut(250D, Easing.OutQuint).OnComplete((_) =>
                    {
                        transObj.MoveTo(new Vector2((camera.DrawWidth / 2) - (transObj.DrawWidth / 2), (camera.DrawHeight / 2) - (transObj.DrawHeight / 2)), 500D, Easing.InSine).OnComplete((_) =>
                        {
                            if (preview != null)
                                Paths.RemoveTrack(preview.Name);

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

        private void setupUI()
        {
            // to make stuff roundy
            Container albumWrap = new Container()
            {
                Masking = true,
                CornerRadius = defaultRounding / 2,
                Margin = new MarginPadding(defaultMargin - defaultMargin / 2),
                AutoSizeAxes = Axes.Both,
                Scale = new Vector2(1.35f),
                Child = albumSpr = new AlbumSprite()
            };
            topBar.Add(albumWrap);

            if (targetFormat == "OSU!")
                albumSpr.AddVolume("volumeOSU");

            /*
            if (targetFormat == "Quaver")
                albumSpr.AddVolume("volumeQuaver");*/

            float defaultX = (defaultMargin * 2) + defaultMargin * (timesSpace + spaceTwnObj - 2.0f);
            songTitle = new SpriteText()
            {
                Text = "Test",
                Font = new FontUsage(family: "RedHatDisplay", size: defaultMargin * 3, weight: "Bold"),
                Position = new Vector2(defaultX, defaultMargin),
            };
            topBar.Add(songTitle);

            songAuthor = new SpriteText()
            {
                Text = "by Funkin' Crew",
                Font = new FontUsage(family: "RedHatDisplay", size: defaultMargin * 2),
                Position = new Vector2(defaultX, defaultMargin * 4)
            };
            topBar.Add(songAuthor);

            // its created here since we have sum stuff ready (not really, its only cuz i worked on this before thinkin of a toolbar bru
            FillFlowContainer binfo;
            FillFlowContainer btns;
            toolBar = new Container()
            {
                Position = new Vector2(defaultX, defaultMargin * 8),
                Size = new Vector2(GameConstants.WIDTH - defaultMargin * defaultMargin, 50),
                Children = new Drawable[]
                {
                    /*
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Blue
                    }*/
                    binfo = new FillFlowContainer()
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal
                    },
                    btns = new FillFlowContainer()
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    }
                }
            };
            topBar.Add(toolBar);

            setupIcons(binfo);
            setupButtons(btns);
        }

        private void setupIcons(FillFlowContainer infoContainer)
        {
            Vector2 centerPosition = new Vector2((infoContainer.X + infoContainer.DrawHeight) + defaultMargin / 2, ((infoContainer.DrawHeight - defaultMargin) / 2) - (defaultMargin / 2));

            Container tempo = new Container()
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Size = new Vector2(infoContainer.DrawHeight),
                        Icon = FontAwesome.Solid.Heartbeat // BPM - Beats-per-minute, heart beats per minute, haha (i wanna kms)
                    },
                    songBPM = new SpriteText()
                    {
                        Text = "100 bpm",
                        Font = new FontUsage(family: "RedHatDisplay", size: defaultMargin * 2),
                        Position = centerPosition
                    }
                }
            };

            infoContainer.Add(tempo);

            /*
            Container charted = new Container()
            {
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding() { Left = defaultMargin },
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Size = new Vector2(infoContainer.DrawHeight),
                        Icon = FontAwesome.Solid.User
                    },
                    songCharter = new SpriteText()
                    {
                        Text = "Unknown",
                        Font = new FontUsage(family: "RedHatDisplay", size: defaultMargin * 2),
                        Position = centerPosition
                    }
                }
            };

            infoContainer.Add(charted);*/

            Container speedy = new Container()
            {
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding() { Left = defaultMargin },
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Size = new Vector2(infoContainer.DrawHeight),
                        Icon = FontAwesome.Solid.Forward
                    },
                    songSpeed = new SpriteText()
                    {
                        Text = "1.0",
                        Font = new FontUsage(family: "RedHatDisplay", size: defaultMargin * 2),
                        Position = centerPosition
                    }
                }
            };

            infoContainer.Add(speedy);
        }

        private void setupButtons(FillFlowContainer btnContainer)
        {
            Box round;
            Container diffMask = new Container()
            {
                Masking = true,
                CornerRadius = defaultRounding / 2,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding()
                {
                    Left = defaultMargin,
                    Right = defaultMargin,
                },
                Children = new Drawable[]
                {
                    round = new Box
                    {
                        Colour = cbColor,
                        Size = new Vector2(defaultMargin * defaultMargin / 1.25f, btnContainer.DrawHeight)
                    },
                    diffSelector = new DiffComboBox(round)
                    {
                        Position = new Vector2(defaultMargin / 4.0f),
                        //Items = ["Easy", "Normal", "Hard", "Erect", "Nightmare"],
                    }
                }
            };
            diffSelector.Current.BindValueChanged((ev) =>
            {
                if (curSong.Metadata.ScrollSpeeds.TryGetValue(ev.NewValue.ToLower(), out double speed))
                    songSpeed.Text = $"{System.Math.Round(speed, 2)}";
                else
                    songSpeed.Text = "?";
            });
            btnContainer.Add(diffMask);
            diffSelector.Width = round.Width - (defaultMargin / 2);

            Container btnMask = new Container()
            {
                Masking = true,
                CornerRadius = defaultRounding / 2,
                AutoSizeAxes = Axes.X,
                Height = btnContainer.DrawHeight,
                Child = playBtn = new PlayButton()
                {
                    Width = defaultMargin * defaultMargin / 2f,
                    Action = initPlay
                }
            };
            playBtn.Enabled.Value = false; // starts disabled
            btnContainer.Add(btnMask);
        }

        private void initPlay()
        {
            canBack = false;
            canPress = false;
            blocked = true;

            CursorVisible = false;
            if (preview != null)
                this.TransformBindableTo(preview.Volume, 0D, volFade + 150D);
            this.FadeOut(1800D, Easing.OutQuint);
            this.TransformBindableTo(worldZoom, 3.15f, 1250D, Easing.InQuint).OnComplete((_) => 
            {
                Schedule(() =>
                {
                    Game.ScreenStack.Push(new SongLoading(targetFormat, curSong.Metadata.SongName, diffSelector.Current.Value.ToLower()));
                });
            });
        }

        // i cant do this no more bruh
        private void onItemClick(SongItem item)
        {
            foreach (Drawable drawable in content)
            {
                if (drawable is not SongItem)
                    continue;

                SongItem child = (SongItem)drawable;
                if (child == item)
                {
                    if (curSong == child)
                        return;

                    curSong = child;

                    child.BG.FadeColour(child.SelectedColor, 150D);
                    child.BG.FlashColour(child.SelectedColor.Lighten(1.05f), 150D);
                    BasicMetadata meta = child.Metadata;
                    if (targetFormat == "FNF VSlice" || targetFormat == "FNF Legacy")
                    {
                        if (meta.Album != "Unknown")
                            albumSpr.Play(meta.Album);
                        else
                            albumSpr.Play("volume1");
                    }

                    if (targetFormat == "OSU!")
                        albumSpr.Play("volumeOSU");

                    if (targetFormat == "Quaver")
                        albumSpr.Play("volumeQuaver");

                    songTitle.Text = meta.SongName;
                    songAuthor.Text = meta.Artist;
                    songBPM.Text = $"{meta.BPM} bpm";

                    diffSelector.ClearItems();
                    diffSelector.Items = meta.Difficulties;

                    playBtn.Enabled.Value = true;

                    if (preview != null)
                    {
                        this.TransformBindableTo(preview.Volume, 0D, volFade).OnComplete((_) =>
                        {
                            Paths.RemoveTrack(preview.Name);
                            preview = null;
                            startPreview();
                        });
                    }
                    else
                        startPreview();
                }
                else
                    child.BG.FadeColour(child.DefaultColor, 150D);
            }
        }

        private void startPreview()
        {
            List<string> diffs = [.. curSong.Metadata.Difficulties];
            preview = ChartRegistry.GetInstPreview(targetFormat, SongLoading.FormatSong(curSong.Metadata.SongName), diffs.Contains("Erect") ? "erect" : null);
            this.TransformBindableTo(preview.Volume, targetVol, volFade);
            preview.Start();
        }
    }
}
