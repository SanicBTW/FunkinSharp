
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace FunkinSharp.Game.Core
{
    // Basic Container that acts as a "Camera", it just clips the content to screen and makes it somewhat easier to manipulate some stuff
    // This is a BASIC camera and probably needs a lot of work to be put into, I do not expect THIS to work as intended in future stages of the development
    // It took me like uhhh idk 6 hours to get done so hate as much as you want but I ain't touching this ever again :sob:
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
                Size = new Vector2(1 / zoom); // Sets the size of the container to probably fit the whole relative size (Clipping Container)
            }
        }

        // This took me a lot of fucking time
        public Vector2 PrevPos { get; private set; } = Vector2.Zero;
        private Vector2 camPosition = Vector2.Zero;
        private bool cameraPositionChanged = false;

        // Bind the Camera Position on a new Camera Creation like new() { CameraPosition = { BindTarget = <vector2bindable> } }
        public Bindable<Vector2> CameraPosition = new Bindable<Vector2>(Vector2.Zero);

        public Camera()
        {
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

            if (cameraPositionChanged)
            {
                foreach (Drawable drawable in AliveChildren)
                {
                    if (PrevPos == Vector2.Zero)
                        drawable.Position -= camPosition;
                    else
                        drawable.Position += PrevPos - camPosition;
                }

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
