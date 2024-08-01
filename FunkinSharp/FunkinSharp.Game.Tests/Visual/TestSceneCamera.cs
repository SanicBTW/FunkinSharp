using System;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Sprites;
using NUnit.Framework;
using osu.Framework.Bindables;
using osuTK;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneCamera : FunkinSharpTestScene
    {
        private float maxZoom = 0.7f;

        private Camera camera;
        private Character bf;
        private Character dad;
        // It holds now more variables for some easy camera movement but trust me, this makes stuff way better to work with (?)
        private Vector2 lerpPos = Vector2.Zero;
        private Bindable<Vector2> targetPos = new(Vector2.Zero);

        public TestSceneCamera()
        {
            // Please, create the cameras like this now
            camera = new Camera(false) // Set false for world camera
            {
                CameraPosition = { BindTarget =  targetPos }
            };
            Add(camera);

            camera.Add(bf = new Character("bf", true)
            {
                X = 250,
                Y = -200
            });

            camera.Add(dad = new Character("dad", false)
            {
                X = -350
            });

            Scheduler.AddDelayed(() =>
            {
                camera.Zoom += 0.05f;
                bf.Play("idle");
                dad.Play("idle");
            }, 2000, true);

            // I literally DO not know how I made these just to properly move the camera :sob:
            // Yeah im gonna keep it like this for now since I don't really need anything else in this test scene
            // stfu bro, now its way simpler but offsets and positioning exists (i fucked up centering)

            AddStep("Focus dad", () =>
            {
                Vector2 center = dad.OriginPosition;
                center.Y -= dad.DrawHeight / 2;
                lerpPos = -center;
                //camera.Follow(dad);
            });

            AddStep("Focus bf", () =>
            {
                Vector2 center = bf.OriginPosition;
                center.Y -= bf.DrawHeight / 4;
                lerpPos = center;
                //camera.Follow(bf);
            });
        }

        public float Lerp(float a, float b, float ratio)
        {
            return a + ratio * (b - a);
        }

        public float BoundTo(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        protected override void Update()
        {
            base.Update();

            float elapsed = (float)(Clock.ElapsedFrameTime / 1000);
            camera.Zoom = Lerp(maxZoom, camera.Zoom, BoundTo(1 - (float)(elapsed * 3.125), 0, 1));
            
            float lerpVal = BoundTo(elapsed * 2.4f, 0, 1);
            Vector2 curPos = camera.CameraPosition.Value;
            Vector2 newPos = Vector2.Lerp(curPos, lerpPos, lerpVal);
            targetPos.Value = newPos;
        }
    }
}
