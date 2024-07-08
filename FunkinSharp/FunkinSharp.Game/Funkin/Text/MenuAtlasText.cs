using System;
using osuTK;

namespace FunkinSharp.Game.Funkin.Text
{
    public partial class MenuAtlasText : AtlasText
    {
        public int TargetX = 1; // not used yet
        public int TargetY = 0;
        public bool ChangeX = true;
        public bool ChangeY = true;
        public int ID = 0;

        public Vector2 DistancePerItem = new Vector2(20, 120);
        public Vector2 StartPosition = new Vector2(0);

        public MenuAtlasText(float x, float y, string text = "", bool bold = true) : base(text, 0, 0, bold ? AtlasFontType.BOLD : AtlasFontType.DEFAULT)
        {
            StartPosition = new Vector2(x, y);
        }

        protected override void Update()
        {
            float lerpVal = boundTo(((float)Clock.ElapsedFrameTime / 1000) * 9.6f, 0, 1);
            // TargetX - TargetY to make it go from right to left
            if (ChangeX)
                X = float.Lerp(X, GetXPos(), lerpVal);
            if (ChangeY)
                Y = float.Lerp(Y, GetYPos(), lerpVal);
            base.Update();
        }

        public float GetXPos() => (TargetY * DistancePerItem.X) + StartPosition.X;
        public float GetYPos() => (TargetY * 1.3f * DistancePerItem.Y) + StartPosition.Y;

        // Should move this to a MathUtil class or sum bruh
        private float boundTo(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
