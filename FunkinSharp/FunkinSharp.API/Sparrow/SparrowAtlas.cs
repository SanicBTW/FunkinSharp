using System.Text.RegularExpressions;
using FunkinSharp.API.Animation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using TurboXml;

namespace FunkinSharp.API.Sparrow;

// this code comes from Livin' on Sweets custom playtest code, not published by the time im writing this
// https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/Sparrow/SparrowAtlas.cs and https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/Utils/AssetFactory.cs#L154
public partial class SparrowAtlas
{
    private static readonly Dictionary<string, SparrowAtlas?> frames_cache = [];

    // if parse as sparrow framesanimations will be empty and adv frames will be available instead
    public readonly Dictionary<string, List<FrameData<Texture>>> Animations = [];
    public readonly Dictionary<string, List<SparrowFrame>> AdvFrames = []; // advanced frames, if not needed wont be parsed

    public static SparrowAtlas? Parse(string cacheKey, Texture sheet, Stream xmlStream, double animDuration = 24, bool parseSparrowFrames = true)
    {
        if (frames_cache.TryGetValue(cacheKey, out SparrowAtlas? atlas))
            return atlas;

        atlas = new SparrowAtlas();

        double frameDuration = 1000 / animDuration;
        var handler = new SparrowParser(ref atlas, sheet, frameDuration, parseSparrowFrames);
        XmlParser.Parse(xmlStream, ref handler);

        frames_cache[cacheKey] = atlas;
        return atlas;
    }

    [GeneratedRegex(@"\d+$")]
    private static partial Regex noNumbersRegex();

    public struct SparrowParser(ref SparrowAtlas target, Texture sheet, double frameDuration, bool parseSparrowFrames) : IXmlReadHandler
    {
        private readonly SparrowAtlas atlas = target;

        private bool isOnFrame;
        private string currentFrame;
        private string currentFrameAnim;
        private RectangleF currentFrameRect = RectangleF.Empty;

        private RectangleF currentFrameSize = RectangleF.Empty;
        private bool rotated;

        // checks if were on a frame
        public void OnBeginTag(ReadOnlySpan<char> name, int line, int column)
        {
            if (!name.StartsWith("subtexture", StringComparison.CurrentCultureIgnoreCase))
            {
                isOnFrame = false;
                return;
            }

            isOnFrame = true;
        }

        // sets the corresponding rect properties, consumes the attribute
        public void OnAttribute(ReadOnlySpan<char> name, ReadOnlySpan<char> value, int nameLine, int nameColumn,
            int valueLine, int valueColumn)
        {
            if (!isOnFrame)
                return;

            var attrName = name.ToString();
            var attrValue = value.ToString();

            switch (attrName)
            {
                case "name":
                    currentFrame = attrValue;
                    currentFrameAnim = noNumbersRegex().Replace(currentFrame, string.Empty);
                    if (currentFrameAnim.Length == 0) // alphabet related shenanigans
                        currentFrameAnim = name[0].ToString();
                    break;

                case "x":
                    currentFrameRect.X = int.Parse(attrValue);
                    break;

                case "y":
                    currentFrameRect.Y = int.Parse(attrValue);
                    break;

                case "width":
                    currentFrameRect.Width = int.Parse(attrValue);
                    break;

                case "height":
                    currentFrameRect.Height = int.Parse(attrValue);
                    break;

                case "frameX":
                    currentFrameSize.X = int.Parse(attrValue);
                    break;

                case "frameY":
                    currentFrameSize.Y = int.Parse(attrValue);
                    break;

                case "frameWidth":
                    currentFrameSize.Width = int.Parse(attrValue);
                    break;

                case "frameHeight":
                    currentFrameSize.Height = int.Parse(attrValue);
                    break;

                case "rotated":
                    rotated = bool.Parse(attrValue);
                    break;
            }
        }

        // commits the rect
        public void OnEndTagEmpty()
        {
            if (!isOnFrame)
                return;

            isOnFrame = false;

            if (parseSparrowFrames)
            {
                if (rotated)
                {
                    (currentFrameSize.Height, currentFrameSize.Width) = (currentFrameSize.Height, currentFrameSize.Width);
                    rotated = false;
                }

                if (currentFrameSize == RectangleF.Empty)
                {
                    currentFrameSize.Width = currentFrameRect.Width;
                    currentFrameSize.Height = currentFrameRect.Height;
                }

                var frameData = new SparrowFrame()
                {
                    Atlas = sheet.Crop(currentFrameRect),
                    Name = currentFrame,
                    Region = currentFrameRect,
                    SourceRect = currentFrameSize,
                };

                if (atlas.AdvFrames.TryGetValue(currentFrameAnim, out var frames))
                    frames.Add(frameData);
                else
                    atlas.AdvFrames[currentFrameAnim] = [frameData];

                currentFrameSize = RectangleF.Empty;
            }
            else
            {
                var frameData = new FrameData<Texture>()
                {
                    Content = sheet.Crop(currentFrameRect),
                    Duration = frameDuration
                };

                if (atlas.Animations.TryGetValue(currentFrameAnim, out var frames))
                    frames.Add(frameData);
                else
                    atlas.Animations[currentFrameAnim] = [frameData];
            }
        }
    }
}
