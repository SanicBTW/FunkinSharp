using osuTK;

namespace FunkinSharp.Game.Core.Sprites
{
    // Simple Interface to implement Scroll Factor into sprites, only gets used for the Camera position when moving
    public interface ICameraScrollable
    {
        /// <summary>
        ///     If its 0 on any axis, it won't move alongside the camera position
        /// </summary>
        public Vector2 ScrollFactor { get; set; }
    }
}
