using FunkinSharp.Game.Core.Sprites;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace FunkinSharp.Game.Core.Containers
{
    // Basic Container that acts as a "Camera", it just clips the content to screen and makes it somewhat easier to manipulate some stuff
    // not a full rewrite since i cannot really change a lot in this, but i think its far better now i guess
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

        // Bind the Camera Position when creating it { CameraPosition = { BindTarget = <vector2bindable> } }
        public Bindable<Vector2> CameraPosition = new(Vector2.Zero);
        private Vector2 prevPosition = Vector2.Zero;

        private Drawable target;
        private bool followTarget;

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
            CameraPosition.BindValueChanged(v =>
            {
                followTarget = false;
            }, true);
        }

        public void Follow(Drawable target)
        {
            this.target = target;
            followTarget = true;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (followTarget && target != null)
                Position = -target.Position * 0.5f;
            else
                Position = -CameraPosition.Value;

            Vector2 movDelta = Position - prevPosition;
            prevPosition = Position;

            foreach (Drawable child in AliveChildren)
            {
                if (child is ICameraComponent cameraComp)
                {
                    Vector2 relativeMovement = movDelta * (Vector2.One - cameraComp.ScrollFactor);
                    child.Position += relativeMovement;
                }
            }
        }
    }
}
