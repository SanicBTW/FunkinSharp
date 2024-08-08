using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Cursor;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace FunkinSharp.Game.Tests
{
    public partial class FunkinSharpTestBrowser : FunkinSharpGameBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                new TestBrowser("FunkinSharp"),
                new BasicCursorContainer()
            });
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);
            host.Window.CursorState |= CursorState.Hidden;
        }

        // Don't use a Camera for the game content on tests
        public override Container CreateContent() => new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new osuTK.Vector2(GameConstants.WIDTH, GameConstants.HEIGHT)
        };
    }
}
