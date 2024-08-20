using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Text;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Screens;

namespace FunkinSharp.Game.Funkin.Screens
{
    // TODO: Modular
    // TODO: Custom categories since these are from psych bruh
    // TODO: State Machines
    // TODO: Decouple UI from the screen (Kind of done? Only moved some files tho bruh)
    // TODO: Schedule saving to when the state is closed rather than changing it everytime the bindable gets updated
    // TODO: Move the option creation to another class and make it be able to reset instead of implicitly casting each type in the container
    // TODO: Instead of using custom values for the settings, try to get the bindable from the config class
    public partial class SettingsScreen : FunkinScreen
    {
        private static string[] categories => ["Keybinds", "Framework", "Gameplay", "Input", "Camera", "Visuals and UI", "Audio"];
        private static Dictionary<string, Option[]> options => new()
        {
            {
                "framework",
                [
                    new Option()
                    {
                        Name = "Frame Limiter", Setting = "FrameSync", FromFramework = true,
                        Type = "comboboxframesync", Default = FrameSync.Limit2x
                    },
                    new Option()
                    {
                        Name = "Renderer", Setting = "Renderer", FromFramework = true,
                        Type = "comboboxrendertype", Default = RendererType.Automatic
                    },
                    new Option()
                    {
                        Name = "Threading Execution Mode", Setting = "ExecutionMode", FromFramework = true,
                        Type = "comboboxthreadexecmode", Default = ExecutionMode.MultiThreaded
                    }
                ]
            },
            {
                "gameplay",
                [
                    new Option()
                    {
                        Name = "Down Scroll", Setting = "DownScroll",
                        Type = "checkbox", Default = false
                    },
                    new Option()
                    {
                        Name = "Middle Scroll", Setting = "MiddleScroll",
                        Type = "checkbox", Default = false
                    },
                    new Option()
                    {
                        Name = "Ghost Tapping", Setting = "GhostTapping",
                        Type = "checkbox", Default = true
                    },
                    new Option()
                    {
                        Name = "Scroll Speed", Setting = "ScrollSpeed",
                        Type = "sliderbarfloat", Default = 1, // to use the song speed
                        Values = [1, 10, 0.01f] // Min, Max, Precision
                    }
                ]
            },
            {
                "input",
                [
                    new Option()
                    {
                        Name = "Mouse Sensitivity", Setting = "CursorSensitivity", FromFramework = true,
                        Type = "sliderbardouble", Default = 1,
                        Values = [0.1, 6, 0.01] // https://github.com/ppy/osu-framework/blob/9979a9c790fa08c51794497e89873a2803c555ad/osu.Framework/Configuration/FrameworkConfigManager.cs#L50
                    }
                ]
            },
            {
                "audio",
                [
                    new Option()
                    {
                        Name = "15s preview instead of full song in song selection", Setting = "FullSongInSelection",
                        Type = "checkbox", Default = false
                    }
                ]
            }
        };

        private Sprite bg;
        private Camera camera = new(false); // World camera
        private AtlasText transObj;
        private Container uiView;
        private bool backing = false;
        private bool canBack = true;
        private FillFlowContainer sideBar;
        private Container contentView;
        private FillFlowContainer content;
        private FocusedMenuContainer keybindView;
        private BindableFloat worldZoom = new BindableFloat(1);
        private float keybindSelectZoom = 1.75f;
        private Container zoomedContent = new()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Alpha = 0
        };
        private bool canExit = true;

        [Resolved]
        protected FrameworkConfigManager FrameworkConfig { get; private set; } = null;

        [Resolved]
        protected GameHost GameHost { get; private set; } = null;

        public SettingsScreen()
        {
            OnActionPressed += goBack;
            OnActionPressed += delegate (FunkinAction action)
            {
                // resets all the options currently available in the page
                if (action == FunkinAction.RESET)
                {
                    if (content.Count > 0 && content[0] != null && content[0] is not Container)
                    {
                        foreach (var cock in content)
                        {
                            if (cock is BasicCheckbox bcb)
                                bcb.Current.SetDefault();

                            if (cock is BasicSliderBar<double> bsdoub)
                                bsdoub.Current.SetDefault();
                            if (cock is BasicSliderBar<float> bsflot)
                                bsflot.Current.SetDefault();

                            if (cock is BasicDropdown<FrameSync> bdfs)
                                bdfs.Current.SetDefault();
                            if (cock is BasicDropdown<RendererType> bdrt)
                                bdrt.Current.SetDefault();
                            if (cock is BasicDropdown<ExecutionMode> bdexm)
                                bdexm.Current.SetDefault();
                        }
                    }
                }
            };
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            OnActionPressed -= goBack;
            return base.OnExiting(e);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore store)
        {
            CursorVisible = true;

            Add(bg = new Sprite()
            {
                Alpha = 0.5f,
                Texture = store.Get("General/BGS/menuDesat"),
                Size = new osuTK.Vector2(GameConstants.WIDTH, GameConstants.HEIGHT),
                Colour = Colour4.LightGreen
            });

            Add(camera);

            worldZoom.BindValueChanged((ev) =>
            {
                camera.Zoom = ev.NewValue;
                zoomedContent.Size = new osuTK.Vector2(1 * ev.NewValue);
            });

            uiView = new Container()
            {
                Masking = true,
                CornerRadius = 15,
                Position = new osuTK.Vector2(0, 25 * 4.5f),
                Size = new osuTK.Vector2(GameConstants.WIDTH - 32, GameConstants.HEIGHT - (25 * 4.5f) - 32),
                Margin = new MarginPadding(16),
                Alpha = 0f
            };

            uiView.Children = new Drawable[]
            {
                new Box
                {
                    Colour = Colour4.Snow,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.75f
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
                            Width = 25 * 10,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Colour4.SlateGray,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new BasicScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = sideBar = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                    }
                                }
                            }
                        },
                        // Content
                        contentView = new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            X = 25 * 10,
                            Width = uiView.Width - (25 * 10),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Colour4.DarkSlateBlue,
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.5f,
                                },
                                new BasicScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = content = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                    }
                                }
                            }
                        }
                    }
                }
                
            };

            camera.Add(uiView);

            foreach (string cat in categories)
            {
                BasicButton openCat = new BasicButton
                {
                    CornerRadius = 15,
                    Masking = true,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.85f,
                    Height = 60,
                    Text = cat,
                    Margin = new MarginPadding(16),
                    Action = () => evaluateAction(cat.ToLower())
                };
                sideBar.Add(openCat);
            }
            
            camera.Add(transObj = new AtlasText("Settings", fontName: AtlasFontType.BOLD)
            {
                Alpha = 0f
            });

            camera.Add(zoomedContent); // on top of everything else
        }

        protected override void LoadComplete()
        {
            transObj.Position = new osuTK.Vector2((camera.DrawWidth / 2) - (transObj.DrawWidth / 2), (camera.DrawHeight / 2) - (transObj.DrawHeight / 2));
            transObj.Alpha = 1f;

            transObj.MoveTo(new osuTK.Vector2(25), 500D, Easing.OutSine).OnComplete((_) =>
            {
                uiView.FadeIn(250D, Easing.InQuint);
            });

            base.LoadComplete();
        }

        private void evaluateAction(string action)
        {
            content.Clear();
            if (keybindView != null)
            {
                OnActionPressed -= keybindView.OnActionPressed;
                OnActionReleased -= keybindView.OnActionReleased;
                keybindView = null;
            }

            switch (action)
            {
                case "keybinds":
                    Container view = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new osuTK.Vector2(contentView.DrawWidth, contentView.DrawHeight),
                        Alpha = 0,
                        Child = keybindView = new FocusedMenuContainer()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new osuTK.Vector2(contentView.DrawWidth, contentView.DrawHeight),
                            OnConfirm = keybindViewConfirm
                        }
                    };
                    content.Add(view);
                    view.FadeIn(250D, Easing.InQuint);

                    OnActionPressed += keybindView.OnActionPressed;
                    OnActionReleased += keybindView.OnActionReleased;

                    List<string> temp = [];
                    foreach (var entry in Game.FunkinKeybinds.Actions)
                    {
                        temp.Add(EnumExtensions.GetString(entry.Key).Replace("_", " "));
                    }

                    keybindView.RegenEntries([.. temp]);
                    keybindView.ToggleVisibility();

                    break;

                default:

                    if (options.TryGetValue(action, out Option[] catOptions))
                    {
                        populateFromOptions(catOptions);
                        return;
                    }

                    CircularProgress prog;
                    Container progMask = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new osuTK.Vector2(contentView.DrawWidth, contentView.DrawHeight),
                        Alpha = 0,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Y = -50,
                                Size = new osuTK.Vector2(120),
                                Child = prog = new CircularProgress
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new osuTK.Vector2(0.75f, 0.75f),
                                }
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Y = 50,
                                Text = "Category not done yet, wait for the next release, maybe?",
                            }
                        }
                    };
                    content.Add(progMask);
                    progMask.FadeIn(250D, Easing.InQuint);
                    prog.ProgressTo(0, 1000, Easing.InOutQuart).Then().ProgressTo(1, 1000, Easing.InOutQuart).Loop();
                    break;
            }
        }

        private void goBack(FunkinAction action)
        {
            if (backing || action != FunkinAction.BACK)
                return;

            if (keybindView != null && !canBack)
            {
                if (zoomedContent.Count > 0 && zoomedContent[0] is KeybindOverlay kbOverlay)
                {
                    if (!kbOverlay.CanApplySine)
                        return;

                    kbOverlay.ToggleVisibility();
                }

                Game.FunkinKeybinds.Load(); // Re load the keybinds
                resetBlocks();
                zoomedContent.FadeOut(150D, Easing.OutQuint);
                keybindView.ToggleVisibility();
                this.TransformBindableTo(worldZoom, 1f, 250D, Easing.OutQuint);
                return;
            }

            if (!canBack)
                return;

            backing = true;
            uiView.FadeOut(250D, Easing.OutQuint).OnComplete((_) =>
            {
                transObj.MoveTo(new osuTK.Vector2((camera.DrawWidth / 2) - (transObj.DrawWidth / 2), (camera.DrawHeight / 2) - (transObj.DrawHeight / 2)), 500D, Easing.InSine).OnComplete((_) =>
                {
                    if (!canExit)
                    {
                        AddInternal(new RestartGameRequest(this, "Renderer")); // the only one for now
                        return;
                    }

                    Schedule(() =>
                    {
                        Game.ScreenStack.Push(new ChartFormatSelect(true));
                    });
                });
            });
        }

        private void keybindViewConfirm()
        {
            keybindView.ReactsToKeypresses = false;
            keybindView.KeyPressBlocked = true;
            canBack = false;
            keybindView.ToggleVisibility();

            if (Enum.TryParse(typeof(FunkinAction), keybindView.CurText.ToUpper().Replace(" ", "_"), out object action))
            {
                if (zoomedContent.Count > 0)
                    zoomedContent.Clear();

                zoomedContent.FadeIn(150D, Easing.InQuint);

                this.TransformBindableTo(worldZoom, keybindSelectZoom, 250D, Easing.InQuint);

                FunkinAction fAction = (FunkinAction)action;
                KeybindOverlay kbOverlay = new KeybindOverlay(Game.FunkinKeybinds, fAction, Game.FunkinKeybinds.Actions[fAction]);
                zoomedContent.Add(kbOverlay);

                OnActionPressed += kbOverlay.OnActionPressed;
                OnActionReleased += kbOverlay.OnActionReleased;
            }
            else
            {
                resetBlocks();
                keybindView.ToggleVisibility();
            }
        }

        private void resetBlocks()
        {
            keybindView.ReactsToKeypresses = true;
            keybindView.KeyPressBlocked = false;
            canBack = true;
        }

        private void populateFromOptions(Option[] catOptions)
        {
            float defaultWidth = 215;
            float defaultMarginLabel = 16;
            MarginPadding optMargin = new MarginPadding(0) { Top = defaultMarginLabel / 4, Bottom = defaultMarginLabel / 4, Left = defaultMarginLabel, Right = defaultMarginLabel };
            foreach (Option opt in catOptions)
            {
                object enumValue = Enum.Parse((opt.FromFramework ? typeof(FrameworkSetting) : typeof(FunkinSetting)), opt.Setting);

                switch (opt.Type)
                {
                    case "checkbox":
                        BindableBool bb = new(opt.Default)
                        {
                            Default = opt.Default,
                            Value = (opt.FromFramework) ? FrameworkConfig.Get<bool>((FrameworkSetting)enumValue) : Game.FunkinConfig.Get<bool>((FunkinSetting)enumValue),
                        };

                        BasicCheckbox cb = new()
                        {
                            Margin = new MarginPadding(defaultMarginLabel),
                            LabelText = opt.Name,
                            Current = bb
                        };

                        cb.Current.BindValueChanged((ev) =>
                        {
                            if (opt.FromFramework)
                                FrameworkConfig.SetValue((FrameworkSetting)enumValue, ev.NewValue);
                            else
                                Game.FunkinConfig.SetValue((FunkinSetting)enumValue, ev.NewValue);
                        });
                        content.Add(cb);
                        break;

                    case "sliderbardouble":
                        BindableDouble bd = new(opt.Default)
                        {
                            Default = opt.Default,
                            Value = (opt.FromFramework) ? FrameworkConfig.Get<double>((FrameworkSetting)enumValue) : Game.FunkinConfig.Get<double>((FunkinSetting)enumValue),
                            MinValue = opt.Values[0],
                            MaxValue = opt.Values[1],
                            Precision = opt.Values[2]
                        };

                        SpriteText sbld = new SpriteText()
                        {
                            Margin = new MarginPadding(defaultMarginLabel)
                        };

                        bd.BindValueChanged((ev) =>
                        {
                            sbld.Text = $"{opt.Name}: {ev.NewValue}";

                            if (opt.FromFramework)
                                FrameworkConfig.SetValue((FrameworkSetting)enumValue, ev.NewValue);
                            else
                                Game.FunkinConfig.SetValue((FunkinSetting)enumValue, ev.NewValue);
                        }, true);
                        content.Add(sbld);

                        BasicSliderBar<double> sbd = new()
                        {
                            Size = new osuTK.Vector2(defaultWidth, 30), // same height as a checkbox
                            Margin = optMargin,
                            Current = bd
                        };
                        content.Add(sbd);
                        break;

                    case "sliderbarfloat":
                        BindableFloat bf = new(opt.Default)
                        {
                            Default = opt.Default,
                            Value = (opt.FromFramework) ? FrameworkConfig.Get<float>((FrameworkSetting)enumValue) : Game.FunkinConfig.Get<float>((FunkinSetting)enumValue),
                            MinValue = opt.Values[0],
                            MaxValue = opt.Values[1],
                            Precision = opt.Values[2]
                        };

                        SpriteText sblf = new SpriteText()
                        {
                            Margin = new MarginPadding(defaultMarginLabel)
                        };

                        bf.BindValueChanged((ev) =>
                        {
                            sblf.Text = $"{opt.Name}: {ev.NewValue}";

                            if (opt.FromFramework)
                                FrameworkConfig.SetValue((FrameworkSetting)enumValue, ev.NewValue);
                            else
                                Game.FunkinConfig.SetValue((FunkinSetting)enumValue, ev.NewValue);
                        }, true);
                        content.Add(sblf);

                        BasicSliderBar<float> sbf = new()
                        {
                            Size = new osuTK.Vector2(defaultWidth, 30), // same height as a checkbox
                            Margin = optMargin,
                            Current = bf
                        };
                        content.Add(sbf);
                        break;

                    // why the fuck am i casting on known types??? .sob:
                    case "comboboxframesync":
                        Bindable<FrameSync> bfs = FrameworkConfig.GetBindable<FrameSync>((FrameworkSetting)enumValue);

                        SpriteText cbfl = new SpriteText()
                        {
                            Margin = new MarginPadding(defaultMarginLabel)
                        };
                        bfs.BindValueChanged((ev) =>
                        {
                            cbfl.Text = $"{opt.Name}: {bfs.Value.GetDescription()}";
                        }, true);
                        content.Add(cbfl);

                        BasicDropdown<FrameSync> cbfs = new()
                        {
                            Width = defaultWidth,
                            Items = [FrameSync.VSync, FrameSync.Limit2x, FrameSync.Limit4x, FrameSync.Limit8x, FrameSync.Unlimited],
                            Margin = optMargin,
                            Current = bfs
                        };
                        content.Add(cbfs);
                        break;

                    case "comboboxrendertype":
                        Bindable<RendererType> brt = FrameworkConfig.GetBindable<RendererType>((FrameworkSetting)enumValue);
                        RendererType savedrt = FrameworkConfig.Get<RendererType>((FrameworkSetting)enumValue);

                        SpriteText cbrl = new SpriteText()
                        {
                            Margin = new MarginPadding(defaultMarginLabel)
                        };
                        brt.BindValueChanged((ev) =>
                        {
                            string concat = "";
                            canExit = (ev.NewValue == savedrt);
                            if (!canExit)
                                concat = "(You must restart the game in order to apply the new renderer!)";

                            cbrl.Text = $"{opt.Name}: {brt.Value} {concat}";
                        }, true);
                        content.Add(cbrl);

                        BasicDropdown<RendererType> cbrt = new()
                        {
                            Width = defaultWidth,
                            Items = GameHost.GetPreferredRenderersForCurrentPlatform(),
                            Margin = optMargin,
                            Current = brt
                        };
                        content.Add(cbrt);
                        break;

                    case "comboboxthreadexecmode":
                        Bindable<ExecutionMode> btem = FrameworkConfig.GetBindable<ExecutionMode>((FrameworkSetting)enumValue);
                        SpriteText cbtel = new SpriteText()
                        {
                            Margin = new MarginPadding(defaultMarginLabel)
                        };
                        btem.BindValueChanged((ev) =>
                        {
                            cbtel.Text = $"{opt.Name}: {btem.Value} ({(btem.Value == ExecutionMode.SingleThread ? "Capped to Frame Limiter" : "Capped to 1000hz")})";
                        }, true);
                        content.Add(cbtel);

                        BasicDropdown<ExecutionMode> cbtem = new()
                        {
                            Width = defaultWidth,
                            Items = [ExecutionMode.SingleThread, ExecutionMode.MultiThreaded],
                            Margin = optMargin,
                            Current = btem
                        };
                        content.Add(cbtem);
                        break;
                }
            }
        }

        private record Option
        {
            public string Name;
            public string Setting;
            public string Type;
            public dynamic Default;
            public dynamic[] Values;
            public bool FromFramework;
        }
    }
}
