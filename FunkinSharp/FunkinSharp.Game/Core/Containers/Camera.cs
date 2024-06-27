using FunkinSharp.Game.Core.Sprites;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace FunkinSharp.Game.Core.Containers
{
    // Basic Container that acts as a "Camera", it just clips the content to screen and makes it somewhat easier to manipulate some stuff
    // This is a BASIC camera and probably needs a lot of work to be put into, I do not expect THIS to work as intended in future stages of the development
    // It took me like uhhh idk 6 hours to get done so hate as much as you want but I ain't touching this ever again :sob:
    // REWRITE SOON :smiling_imp:
    public partial class Camera : ClippedContainer
    {
        private float zoom = 1f;

        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = value;
                Scale = new Vector2(zoom);
            }
        }

        // This took me a lot of fucking time
        public Vector2 PrevPos { get; private set; } = Vector2.Zero;
        private Vector2 camPosition = Vector2.Zero;
        private bool cameraPositionChanged = false;

        // Bind the Camera Position on a new Camera Creation like new() { CameraPosition = { BindTarget = <vector2bindable> } }
        public Bindable<Vector2> CameraPosition = new Bindable<Vector2>(Vector2.Zero); // idk if i should call the movement scrolling or just movement

        /// <param name="shouldClipContent">
        ///     If this camera should clip the content visible on screen.
        ///     <para/>
        ///     If true, this camera will clip everything inside its bounding box
        ///     <para/>
        ///     but we aware that when changing the zoom to anything lower than 1 might lead to smaller container size and thus clipping incorrectly
        ///     <para/>
        ///     If false, no clipping will be done and the zoom won't affect the container size nor clipping region
        /// </param>
        public Camera(bool shouldClipContent = true)
        {
            Masking = shouldClipContent;
            // Use v.OldValue with PrevPos???
            CameraPosition.BindValueChanged(v =>
            {
                if (camPosition != v.NewValue)
                {
                    camPosition = v.NewValue;
                    cameraPositionChanged = true;
                }
            }, true);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            foreach (Drawable drawable in AliveChildren)
            {
                if (cameraPositionChanged)
                {
                    Vector2 endPosition = (PrevPos == Vector2.Zero) ? -camPosition : PrevPos - camPosition;
                    if (drawable is ICameraComponent dScroll)
                    {
                        endPosition *= dScroll.ScrollFactor;
                    }

                    drawable.Position += endPosition;
                }

                /* gotta look into this
                if (drawable is ICameraComponent dScale)
                {
                    if (dScale.FollowScale)
                        drawable.Scale = Scale;
                }
                else
                    drawable.Scale = Scale;*/
            }

            if (cameraPositionChanged)
            {
                Vector2 copycat = camPosition;
                if (PrevPos == Vector2.Zero)
                {
                    CameraPosition.Value = Vector2.Zero;
                    PrevPos = copycat;
                }
                else
                    PrevPos = camPosition;

                cameraPositionChanged = false;
            }
        }
    }
}
