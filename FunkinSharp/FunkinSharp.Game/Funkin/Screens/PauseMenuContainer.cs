using System;
using FunkinSharp.Game.Funkin.Text;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class GameplayScreen
    {
        // TODO: I shouldnt really add this as an internal but rather to a whole other camera??? most likely since i dont have the stuff as zoom n shit for a cool transition yknow
        private partial class PauseMenuContainer : VisibilityContainer
        {
            private Container<MenuAtlasText> grpOptions = new();
            private int ccselect = 0;
            private int curSelected
            {
                get => ccselect;
                set
                {
                    ccselect += value;

                    if (ccselect < 0)
                        ccselect = grpOptions.Count - 1;
                    if (ccselect >= grpOptions.Count)
                        ccselect = 0;

                    int tf = 0;
                    foreach (MenuAtlasText item in grpOptions)
                    {
                        item.TargetY = tf - ccselect;
                        tf++;

                        item.Alpha = 0.6f;

                        if (item.TargetY == 0)
                            item.Alpha = 1;
                    }
                }
            }

            private string[] options = ["Resume", "Reset Song", "Enable BotPlay", "Exit"];
            private double sneakInTime = 450D;
            private double gameCamTime = 250D;

            private GameplayScreen invoker;
            private bool canPress = true;

            public PauseMenuContainer(GameplayScreen caller)
            {
                invoker = caller;
                invoker.OnActionPressed += OnActionPress;

                if (caller.plyLine.BotPlay.Value)
                    options[2] = "Disable BotPlay";

                Add(grpOptions);
                regenMenu(options);

                ToggleVisibility();
            }

            protected override void PopIn()
            {
                invoker.conductor.Instrumental.Stop();
                foreach (Track voice in invoker.conductor.Voices)
                {
                    voice.Stop();
                }

                invoker.worldCamera.FadeTo(0.35f, gameCamTime, Easing.OutQuint);
                invoker.uiCamera.FadeTo(0.35f, gameCamTime + 150D, Easing.OutQuint);
                invoker.TargetActions = Actors.UI;

                foreach (MenuAtlasText item in grpOptions)
                {
                    // we want no 0s!!!!
                    item.MoveToX(item.GetXPos(), sneakInTime * (Math.Abs(item.TargetY) + 1) / 2f, Easing.InSine).OnComplete((_) =>
                    {
                        item.ChangeX = true;
                    });
                }
            }

            protected override void PopOut()
            {
                canPress = false;

                foreach (MenuAtlasText item in grpOptions)
                {
                    item.ChangeX = false;
                    // we want no 0s!!!!
                    item.MoveToX(-item.DrawWidth, sneakInTime * (Math.Abs(item.TargetY) + 1) / 1.5f, Easing.OutSine);
                }

                Scheduler.AddDelayed(() =>
                {
                    invoker.worldCamera.FadeTo(1f, gameCamTime - 150D, Easing.InQuint);
                    invoker.uiCamera.FadeTo(1f, gameCamTime, Easing.InQuint).OnComplete((_) =>
                    {
                        invoker.RemoveInternal(invoker.pauseMenu, true);
                        invoker.pauseMenu = null;
                        invoker.canPause = true;
                        invoker.TargetActions = Actors.NOTE;
                        invoker.conductor.Resync();
                    });
                }, sneakInTime * (grpOptions.Count / 1.5f));
            }

            private void regenMenu(string[] array)
            {
                grpOptions.Clear();

                int i = 0;

                foreach (string str in array)
                {
                    MenuAtlasText item = new MenuAtlasText(90, 320, str, true)
                    {
                        ChangeX = false,
                        X = 0,
                        TargetY = i - curSelected,
                        ID = i,
                        Alpha = 0
                    };

                    grpOptions.Add(item);
                    i++;
                }

                curSelected = grpOptions.Count + 1;
            }

            public void OnActionPress(FunkinAction action)
            {
                if (!canPress)
                    return;

                switch (action)
                {
                    case FunkinAction.UI_UP:
                        curSelected = -1;
                        break;

                    case FunkinAction.UI_DOWN:
                        curSelected = 1;
                        break;

                    case FunkinAction.CONFIRM:
                        canPress = false;
                        switch (options[curSelected])
                        {
                            case "Resume":
                                ToggleVisibility();
                                break;

                            // we set the alpha to 0.01f so it doesnt get marked as hidden and stops updating this container
                            case "Reset Song":
                                this.FadeTo(0.01f, gameCamTime / 2f, Easing.OutQuint).OnComplete((_) =>
                                {
                                    invoker.uiCamera.FadeTo(1f, gameCamTime, Easing.InQuint).OnComplete((_) =>
                                    {
                                        invoker.uiZoom.Value = 1;
                                        invoker.canLerp = false;
                                        Schedule(() =>
                                        {
                                            invoker.FadeOut(1800D, Easing.OutQuint);
                                            invoker.TransformBindableTo(invoker.uiZoom, 3.15f, 1250D, Easing.InQuint).OnComplete((_) =>
                                            {
                                                invoker.Game.ScreenStack.Push(new SongLoading(invoker.format, invoker.metaData.SongName, invoker.diff));
                                            });
                                        });
                                    });
                                });
                                break;

                            case "Enable BotPlay":
                            case "Disable BotPlay":
                                bool state = invoker.plyLine.BotPlay.Value = !invoker.plyLine.BotPlay.Value;
                                if (state)
                                    options[2] = "Disable BotPlay";
                                else
                                    options[2] = "Enable BotPlay";
                                grpOptions[2].Text = options[2];
                                canPress = true;
                                break;

                            case "Exit":
                                this.FadeTo(0.01f, gameCamTime, Easing.OutQuint).OnComplete((_) =>
                                {
                                    invoker.uiCamera.FadeTo(1f, gameCamTime, Easing.InQuint).OnComplete((_) =>
                                    {
                                        invoker.endSong();
                                    });
                                });
                                break;
                        }

                        break;
                }
            }
        }
    }
}
