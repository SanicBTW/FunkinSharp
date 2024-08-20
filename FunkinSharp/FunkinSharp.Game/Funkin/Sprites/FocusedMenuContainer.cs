using System;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class SettingsScreen
    {
        private partial class FocusedMenuContainer : FocusedOverlayContainer
        {
            private Container<MenuAtlasText> entries = new();

            public bool ReactsToKeypresses = true;
            public bool KeyPressBlocked = false;
            public Action OnConfirm;

            private int ccselect = -1;
            public int CurSelected
            {
                get => ccselect;
                set
                {
                    ccselect += value;

                    if (ccselect < 0)
                        ccselect = entries.Count - 1;
                    if (ccselect >= entries.Count)
                        ccselect = 0;

                    int tf = 0;
                    foreach (MenuAtlasText item in entries)
                    {
                        item.TargetY = tf - ccselect;
                        tf++;

                        item.Alpha = 0.6f;

                        if (item.TargetY == 0)
                            item.Alpha = 1;
                    }
                }
            }

            public string CurText => entries[CurSelected].Text;

            public FocusedMenuContainer(int initialSelected = -1, string[] initialEntries = null)
            {
                AddInternal(entries);

                ccselect = initialSelected;
                if (initialEntries != null)
                    RegenEntries(initialEntries);
            }

            public void RegenEntries(string[] array)
            {
                entries.Clear();

                int i = 0;

                foreach (string str in array)
                {
                    MenuAtlasText item = new MenuAtlasText(50, 80, str, true)
                    {
                        ChangeX = false,
                        X = 50,
                        TargetY = i - CurSelected,
                        ID = i
                    };

                    entries.Add(item);
                    i++;
                }

                CurSelected = (ccselect != -1) ? 0 : entries.Count + 1;
            }

            protected override void PopIn()
            {
                this.FadeIn(250D, Easing.InQuint);
            }

            protected override void PopOut()
            {
                this.FadeOut(250D, Easing.OutQuint);
            }

            public void OnActionPressed(FunkinAction action)
            {
                if (EnumExtensions.GetString(action).StartsWith("note_") || !ReactsToKeypresses)
                    return;

                switch (action)
                {
                    case FunkinAction.UI_UP:
                        CurSelected = -1;
                        break;

                    case FunkinAction.UI_DOWN:
                        CurSelected = 1;
                        break;

                    case FunkinAction.CONFIRM:
                        OnConfirm?.Invoke();
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
