using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;

namespace FunkinSharp.Game.Core.Cursor
{
    public interface ICursorProvider : IDrawable
    {
        CursorContainer Cursor { get; }

        bool ProvidingUserCursor { get; }
    }
}
