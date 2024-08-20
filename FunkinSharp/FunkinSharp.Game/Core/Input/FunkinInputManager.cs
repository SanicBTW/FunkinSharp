using osu.Framework.Bindables;
using osu.Framework.Input;
using osuTK.Input;

namespace FunkinSharp.Game.Core.Input
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Input/OsuUserInputManager.cs
    public partial class FunkinInputManager : UserInputManager
    {
        protected override bool AllowRightClickFromLongTouch => !LocalUserPlaying.Value;

        // On osu!lazer, this gets managed by a master class called "Player" but since I'm too lazy to do proper code, I'm gonna get the InputManager on game states and set this to the proper value
        public readonly BindableBool LocalUserPlaying = new();

        public FunkinInputManager() { }

        protected override MouseButtonEventManager CreateButtonEventManagerFor(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Right:
                    return new RightMouseManager(button);
            }

            return base.CreateButtonEventManagerFor(button);
        }

        private class RightMouseManager : MouseButtonEventManager
        {
            public RightMouseManager(MouseButton button)
                : base(button)
            {
            }

            public override bool EnableDrag => true; // allow right-mouse dragging for absolute scroll in scroll containers.
            public override bool EnableClick => false;
            public override bool ChangeFocusOnClick => false;
        }
    }
}
