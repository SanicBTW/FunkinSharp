using osuTK;

namespace FunkinSharp.Game.Core.Sprites
{
    // Simple Interface to implement custom camera behaviour on containers or anything thats visible
    public interface ICameraComponent
    {
        /// <summary>
        ///     If its 0 on any axis, it won't move alongside the camera position in that axis.
        /// </summary>
        public Vector2 ScrollFactor { get; set; }

        /// <summary>
        ///     If true, it will copy the camera scale to avoid any resize issues.
        /// </summary>
        public bool FollowScale { get; set; }
    }
}
