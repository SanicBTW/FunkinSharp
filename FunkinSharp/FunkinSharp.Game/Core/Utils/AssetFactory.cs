using System;
using System.IO;
using System.Reflection;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sparrow;
using System.Xml;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Graphics.Primitives;
using FunkinSharp.Game.Core.ReAnimationSystem;
using osuTK;

namespace FunkinSharp.Game.Core.Utils
{
    // AssetFactory cuz it holds functions to generate assets (Texture/Tracks/Etc) on runtime (i might be on crack rn chat)
    // TODO: When generating a new asset, automatically add it to the Paths cache & check for it when creating it?
    public static class AssetFactory
    {
        // Texture generation

        // Creates the Texture through the TextureStore
        public static Texture CreateTexture(TextureStore store,
            Stream stream,
            WrapMode hWrap,
            WrapMode vWrap)
        {
            if (stream == null || stream.Length == 0L)
            {
                return null;
            }

            try
            {
                TextureUpload textureUpload = new TextureUpload(stream);
                MethodInfo loadMethod = ReflectionUtils.GetMethodFrom<TextureStore>("loadRaw", BindingFlags.NonPublic | BindingFlags.Instance);
                if (loadMethod == null)
                    return null;

                Texture obj = (Texture)loadMethod.Invoke(store, [textureUpload, hWrap, vWrap]);
                if (obj == null)
                    return null;

                return obj;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        // Copied from Texture but supports passing more arguments for the texture creation

        // Creates the Texture through the renderer
        public static Texture CreateTexture(IRenderer renderer,
            Stream stream,
            TextureFilteringMode fMode,
            WrapMode hWrap,
            WrapMode vWrap,
            bool manualMipmaps = true)
        {
            if (stream == null || stream.Length == 0L)
            {
                return null;
            }

            try
            {
                TextureUpload textureUpload = new TextureUpload(stream);
                Texture obj = renderer.CreateTexture(textureUpload.Width, textureUpload.Height, manualMipmaps, fMode, hWrap, vWrap);
                obj.SetData(textureUpload);
                return obj;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        // Add the texture to the provided TextureAtlas
        public static Texture CreateTexture(TextureAtlas atlas,
            Stream stream,
            WrapMode hWrap,
            WrapMode vWrap)
        {
            if (stream == null || stream.Length == 0L)
            {
                return null;
            }

            try
            {
                TextureUpload textureUpload = new TextureUpload(stream);
                Texture obj = atlas.Add(textureUpload.Width, textureUpload.Height, hWrap, vWrap);
                obj.SetData(textureUpload);
                return obj;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        // Tracks generation, the reflection stuff is used on the SongEventRegistry too

        /// <summary>
        ///     Creates a new <see cref="TrackBass"/> from the provided <see cref="Stream"/>.
        ///     <para/>
        ///     Always fallbacks to a <see cref="TrackVirtual"/> to avoid returning a null value.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> from which this <see cref="TrackBass"/> will be generated.</param>
        /// <param name="name">The name of this <see cref="TrackBass"/> to show up on the Mixer Visualizer.</param>
        /// <param name="quick">If true, the track will not be fully loaded, and should only be used for preview purposes. Defaults to false.</param>
        /// <param name="length">Only used when the generation fails, making it have to pass a new <see cref="TrackVirtual"/> with the provided length.</param>
        /// <returns>Either a functional <see cref="TrackBass"/> or a <see cref="TrackVirtual"/> if the generation failed.</returns>
        public static Track CreateTrack(Stream stream, string name, bool quick = false, double length = double.PositiveInfinity)
        {
            if (stream == null || stream.Length == 0L)
            {
                Logger.Log($"[AssetFactory:CreateTrack] Stream check fail, returning a new TrackVirtual with name - Virtual:{name}", LoggingTarget.Runtime, LogLevel.Debug);
                return new TrackVirtual(length, $"Virtual:{name}");
            }

            // We do this since the class TrackBass is sealed
            ConstructorInfo constructorInfo = ReflectionUtils.GetConstructorFrom<TrackBass>(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(Stream), typeof(string), typeof(bool)]);
            Track newTrack;

            if (constructorInfo != null)
            {
                try
                {
                    newTrack = (TrackBass)constructorInfo.Invoke([stream, name, quick]);
                }
                catch (Exception ex)
                {
                    newTrack = new TrackVirtual(length, $"Virtual:{name}");
                    Logger.Log($"[AssetFactory:CreateTrack] TrackBass invocation failure, returning a new TrackVirtual with name - Virtual:{name}", LoggingTarget.Runtime, LogLevel.Debug);
                    Logger.Log($"[AssetFactory:CreateTrack] Previous Exception Trace\n{ex.Message}", LoggingTarget.Runtime, LogLevel.Debug);
                }
            }
            else
            {
                Logger.Log($"[AssetFactory:CreateTrack] TrackBass constructor failure, returning a new TrackVirtual with name - Virtual:{name}", LoggingTarget.Runtime, LogLevel.Debug);
                newTrack = new TrackVirtual(length, $"Virtual:{name}");
            }

            return newTrack;
        }

        // Atlases generation

        // im keeping the old legacy implementation of sparrow parsing since the new one is made for the new animation system
        public static SparrowAtlas ParseSparrowLegacy(XmlReader xmlReader)
        {
            SparrowAtlas atlas = null;

            bool start = true;
            string lastAnim = "";
            int i = 0, frames = 0;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType is XmlNodeType.XmlDeclaration
                    or XmlNodeType.Comment
                    or XmlNodeType.Whitespace
                    or XmlNodeType.EndElement)
                    continue;

                if (xmlReader.NodeType is XmlNodeType.EndElement)
                    break;

                if (xmlReader.NodeType == XmlNodeType.Element && atlas == null)
                {
                    atlas = new SparrowAtlas(xmlReader.GetAttribute("imagePath"));
                    continue;
                }

                // Required values
                string animName = xmlReader.GetAttribute("name")[..^4].Trim(); // Aint no way I forgot to trim

                // properly parsing da animation name, this is just stupid but needed :sob:

                // it means theres 5 frame numbers, now checks if it has enough length to apply the change
                if (animName.EndsWith("1") && animName[..^1].Trim().Length > 1) 
                    animName = animName[..^1].Trim();

                if (animName.Contains("instance"))
                    animName = animName.Replace("instance", "").Trim();

                int x = int.Parse(xmlReader.GetAttribute("x")!);
                int y = int.Parse(xmlReader.GetAttribute("y")!);
                int width = int.Parse(xmlReader.GetAttribute("width")!);
                int height = int.Parse(xmlReader.GetAttribute("height")!);

                if (start)
                {
                    lastAnim = animName;
                    start = false;
                }

                if (lastAnim != animName)
                {
                    atlas.SetFrame(lastAnim, new AnimationFrame(i - frames, i - 1, frames));
                    lastAnim = animName;
                    frames = 0;
                }

                atlas.FrameNames.Add(xmlReader.GetAttribute("name").Trim());
                atlas.AddRegion(new RectangleF(x, y, width, height));
                i++;
                frames++;
            }

            // Set the frame for the last animation
            // - Theres no way I didn't find this and had to use ChatGPT for this one :sob:
            atlas.SetFrame(lastAnim, new AnimationFrame(i - frames, i - 1, frames));

            return atlas;
        }

        // Had to move to XMLReader since XDocument parsing was giving a lot of exceptions and when an exception is caught, it lags the game
        public static void ParseSparrowNew(in ReAnimatedSprite controller, string directory, XmlReader xmlReader)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType is XmlNodeType.XmlDeclaration
                    or XmlNodeType.Comment
                    or XmlNodeType.Whitespace
                    or XmlNodeType.EndElement)
                    continue;

                if (xmlReader.NodeType is XmlNodeType.EndElement)
                    break;

                // old check was bad for non dll resources assets, so now we checking the current element name to see if its the texture atlas (why didnt we do this already bruh??)
                // and then use funny null check assign operator because some external assets might set the texture before hand since
                // it doesnt want to use the dll resources obviously
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "TextureAtlas")
                {
                    // currently, there shouldnt be any problem setting the texture to the target texture since we are in the middle of parsing frames
                    // but i dont know if it could get to an edge case where the target texture was replaced while parsing frames
                    // also 99% of the time Path.GetDirectoryName should return the correct path, for example NoteTypes/funkin/ or Characters/bf
                    controller.Texture ??= Paths.GetTexture(Paths.SanitizeForResources($"{directory}/{xmlReader.GetAttribute("imagePath")}"), false);
                    continue;
                }

                string name = xmlReader.GetAttribute("name")!;
                string animName = ReAnimation.GetAnimationName(name);

                RectangleF frame = new RectangleF(
                    int.Parse(xmlReader.GetAttribute("x")),
                    int.Parse(xmlReader.GetAttribute("y")),
                    int.Parse(xmlReader.GetAttribute("width")),
                    int.Parse(xmlReader.GetAttribute("height"))
                );

                bool trimmed = xmlReader.GetAttribute("frameX") != null;

                RectangleF size = new RectangleF(
                    trimmed ? int.Parse(xmlReader.GetAttribute("frameX")) : 0,
                    trimmed ? int.Parse(xmlReader.GetAttribute("frameY")) : 0,
                    trimmed ? int.Parse(xmlReader.GetAttribute("frameWidth")) : frame.Width,
                    trimmed ? int.Parse(xmlReader.GetAttribute("frameHeight")) : frame.Height
                );

                bool rotated = bool.Parse(xmlReader.GetAttribute("rotated") ?? "false");
                if (rotated && !trimmed)
                    (size.Height, size.Width) = (size.Width, size.Height);

                /*
                if (trimmed)
                {
                    RectangleF endRect = new RectangleF(
                        frame.Location,
                        size.Size
                    );
                    endRect.Intersect(frame);
                    endRect.Location = frame.Location - (-size.Location);
                    endRect.Size += (-size.Location);
                    frame = endRect;
                }*/

                if (!controller.Animations.ContainsKey(animName))
                    controller.Animations[animName] = new ReAnimation(controller)
                    {
                        Loop = false, // Defaults to false on sparrow atlasses
                    };

                controller.Animations[animName].Frames.Add(new ReAnimationFrame()
                {
                    Name = name,
                    Frame = frame,
                    TextureFrame = new TextureRegion(controller.Texture, frame, WrapMode.Repeat, WrapMode.Repeat),
                    Rect = size,
                    Rotated = rotated
                });
            }
        }
    }
}
