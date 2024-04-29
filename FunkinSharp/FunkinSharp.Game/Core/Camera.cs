
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

        public Vector2 CameraPosition
        {
            get => camPosition;
            set
            {
                if (camPosition != value)
                {
                    camPosition = value;
                    cameraPositionChanged = true;
                }
            }
        }

        public Camera() { }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (cameraPositionChanged)
            {
                foreach (Drawable drawable in AliveChildren)
                {
                    if (PrevPos == Vector2.Zero)
                        drawable.Position -= CameraPosition;
                    else
                        drawable.Position += PrevPos - CameraPosition;
                }

                Vector2 copycat = CameraPosition;
                if (PrevPos == Vector2.Zero)
                {
                    CameraPosition = Vector2.Zero;
                    PrevPos = copycat;
                }
                else
                    PrevPos = CameraPosition;

                cameraPositionChanged = false;
            }
        }
    }
}
