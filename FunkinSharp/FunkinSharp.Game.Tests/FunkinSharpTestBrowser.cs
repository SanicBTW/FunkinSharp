using FunkinSharp.Game.Core.Cursor;
using osu.Framework.Graphics;
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
    }
}
