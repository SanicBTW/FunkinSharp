using FunkinSharp.Game.Core.Utils;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Input;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class SettingsScreen
    {
        private partial class KeybindOverlay : FocusedOverlayContainer
        {
            private FunkinKeybinds sKeybinds;
            private FunkinAction kAction;
            private Key[] keys;

            private SpriteText leftText;
            private SpriteText rightText;
            private SpriteText leftKey;
            private SpriteText rightKey;
            private int loaded = 0;
            private float defaultSize = 250f;

            private SpriteText selected;
            private double maxTime = 500D;
            private BindableFloat sinealpha = new BindableFloat(1);

            public bool CanApplySine = true;
            public bool ReactsToKeypresses = true;
            public bool KeyPressBlocked = false;

            protected override bool Handle(UIEvent e)
            {
                if (CanApplySine)
                    return base.Handle(e);

                switch (e)
                {
                    case KeyDownEvent key:
                        if (key.Repeat)
                            return true;

                        keys[selected == leftKey ? 0 : 1] = key.Key;
                        selected.Text = key.Key.ToString();
                        repositionText(selected, (selected == leftKey) ? leftText : rightText);
                        OnActionPressed(FunkinAction.BACK); // Dispatching this action makes it go to selecting state
                        return true;

                    default:
                        return base.Handle(e);
                }
            }

            public KeybindOverlay(FunkinKeybinds keybinds, FunkinAction action, Key[] skeys)
            {
                State.Value = Visibility.Visible;
                sKeybinds = keybinds;
                kAction = action;
                keys = skeys;

                Scale = new osuTK.Vector2(0);

                AddRange(new Drawable[]
                {
                    new Container
                    {
                        Masking = true,
                        CornerRadius = 15,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new osuTK.Vector2(defaultSize),
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.DarkSlateGray,
                        }
                    },
                    new Container
                    {
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Text = $"Current Action: {EnumExtensions.GetString(kAction)}",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new osuTK.Vector2(0, -95)
                            },
                            leftText = new SpriteText
                            {
                                Text = "First Key",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new osuTK.Vector2(-65, -55)
                            },
                            rightText = new SpriteText
                            {
                                Text = "Second Key",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new osuTK.Vector2(55, -55)
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new osuTK.Vector2(defaultSize),
                        Children = new Drawable[]
                        {
                            selected = leftKey = new SpriteText
                            {
                                Text = keys[0].ToString(),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,

                            },
                            rightKey = new SpriteText
                            {
                                Text = keys[1].ToString(),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        }
                    }
                });

                leftKey.OnLoadComplete += keyLoad;
                rightKey.OnLoadComplete += keyLoad;
                sinealpha.BindValueChanged((ev) =>
                {
                    if (CanApplySine)
                        selected.Alpha = ev.NewValue;
                });
            }

            protected override void LoadComplete()
            {
                Scheduler.AddDelayed(() =>
                {
                    maxTime /= 2;
                    this.TransformBindableTo(sinealpha, 0.25f, maxTime, Easing.OutQuint)
                        .Then()
                        .TransformBindableTo(sinealpha, 1, maxTime, Easing.InQuint)
                        .Then()
                        .Delay(maxTime)
                        .Loop();
                }, maxTime);
                PopIn();
                base.LoadComplete();
            }

            private void keyLoad(Drawable obj)
            {
                repositionText((SpriteText)obj, loaded == 0 ? leftText : rightText);
                loaded++;
            }

            private void repositionText(SpriteText key, SpriteText text)
            {
                key.Position = text.Position;
                key.Y += text.DrawHeight * 5;
            }

            protected override void PopIn()
            {
                this.ScaleTo(1, 300D, Easing.InQuint);
            }

            // refresh the action map when closing
            protected override void PopOut()
            {
                sKeybinds.Actions[kAction] = keys;
                sKeybinds.Save();
            }

            public void OnActionPressed(FunkinAction action)
            {
                if (EnumExtensions.GetString(action).StartsWith("note_") || !ReactsToKeypresses)
                    return;

                switch (action)
                {
                    case FunkinAction.UI_LEFT:
                        if (selected == leftKey || !CanApplySine)
                            return;

                        selected.Alpha = 1;
                        selected = leftKey;
                        break;

                    case FunkinAction.UI_RIGHT:
                        if (selected == rightKey || !CanApplySine)
                            return;

                        selected.Alpha = 1;
                        selected = rightKey;
                        break;

                    case FunkinAction.CONFIRM:
                        selected.Alpha = 1;
                        CanApplySine = false;
                        selected.ScaleTo(1.25f, 350D, Easing.InQuint);
                        break;

                    case FunkinAction.BACK:
                        selected.Alpha = 1;
                        CanApplySine = true;
                        selected.ScaleTo(1, 150D, Easing.OutQuint);
                        break;

                    case FunkinAction.RESET:
                        int keyIndex = selected == leftKey ? 0 : 1;
                        Key[] defKeys = sKeybinds.DefaultKeys[kAction];
                        keys[keyIndex] = defKeys[keyIndex];
                        selected.Text = defKeys[keyIndex].ToString();
                        repositionText(selected, (selected == leftKey) ? leftText : rightText);

                        selected.ScaleTo(0.75f, 150D, Easing.OutQuint)
                            .Then()
                            .ScaleTo(1, 150D, Easing.InQuint);
                        break;

                    default:
                        break;
                }
            }

            public void OnActionReleased(FunkinAction action)
            {
                if (EnumExtensions.GetString(action).StartsWith("note_") || KeyPressBlocked)
                    return;

                ReactsToKeypresses = true;
            }
        }
    }
}
