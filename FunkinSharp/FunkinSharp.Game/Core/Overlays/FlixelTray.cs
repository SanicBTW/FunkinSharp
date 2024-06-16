using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace FunkinSharp.Game.Core.Overlays
{
    public partial class VolumeOverlay
    {
        // https://github.com/FunkinCrew/Funkin/blob/0242a6b64f80e45ec8ea6df5cf76a9c26c1b2162/source/funkin/ui/options/FunkinSoundTray.hx
        // TODO: ~Saving~ - apparently the framework does it by itself so big kuros
        // TODO: Rename it to FunkinSoundTray or sum??? idkkk
        // The tray starts at Y 0 because the volume overlay is visible on startup
        private partial class FlixelTray : Container
        {
            // i should make it a sparrow animation but nah id win (the amount of mem allocated is crazy) actually these textures get added to a texture atlas so it aint that bad i guess
            private List<Sprite> bars = [];
            private Vector2 defaultScale = new(1.3f);
            private Vector2 barsPosition = new(19, 12);

            private DrawableSample volUp;
            private DrawableSample volDown;
            private DrawableSample volMax;

            private AudioManager audioman; // cache the provided instance to be able to set the mute adjustment
            private bool muted = false;
            private readonly BindableDouble masterBindable = new() { MinValue = 0, MaxValue = 1, Precision = 0.01 };
            private readonly BindableDouble muteAdjustment = new();

            public FlixelTray()
            {
                AutoSizeAxes = Axes.Both;
                Anchor = Origin = Anchor.TopCentre;
                Margin = new MarginPadding(8);
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore store, ISampleStore sampleStore, AudioManager audio)
            {
                audioman = audio;

                Add(new Sprite
                {
                    Texture = store.Get("SoundTray/volumebox"),
                    Scale = defaultScale
                });

                Add(new Sprite
                {
                    Position = barsPosition,
                    Alpha = 0.4f,
                    Scale = defaultScale,
                    Texture = store.Get("SoundTray/bars_10")
                });

                for (var i = 1; i < 11; i++)
                {
                    Sprite bar = new Sprite
                    {
                        Position = barsPosition,
                        Scale = defaultScale,
                        Texture = store.Get($"SoundTray/bars_{i}"),
                        Alpha = 0
                    };
                    Add(bar);
                    bars.Add(bar);
                }

                AddInternal(volUp = new(sampleStore.Get("SoundTray/Volup")));
                AddInternal(volDown = new(sampleStore.Get("SoundTray/Voldown")));
                AddInternal(volMax = new(sampleStore.Get("SoundTray/VolMAX")));

                masterBindable.BindTo(audio.Volume);
            }

            // TODO: Replicate the slide in easing, tried lerping but always crashed due to being infinite (it was properly set to a finite value idk)
            public void SlideIn()
            {
                this.FadeIn();
                this.MoveToY(0, 300D, Easing.InOutQuad);
            }

            public void SlideOut()
            {
                this.FadeOut(250D, Easing.None);
                this.MoveToY(-(Height + Margin.Top * 2), 300D, Easing.None);
            }

            public void UpdateVol(bool up = false, bool mute = false)
            {
                if (!mute)
                {
                    if (muted)
                        setMute(muted = false);

                    masterBindable.Value += up ? 0.1 : -0.1;
                }
                else
                    setMute(muted = !muted); // setting it to mute would be always true so we just flag it

                int masterVolume = (!muted) ? (int)Math.Round(masterBindable.Value * 10) : 0;

                DrawableSample sound = up ? volUp : volDown;
                if (masterVolume == 10)
                    sound = volMax;

                sound?.Play();

                UpdateBars(masterVolume);
            }

            public void UpdateBars(int max)
            {
                for (int i = 0; i < bars.Count; i++)
                {
                    if (i < max)
                        bars[i].Alpha = 1f;
                    else
                        bars[i].Alpha = 0f;
                }
            }

            private void setMute(bool state)
            {
                if (state)
                    audioman.AddAdjustment(AdjustableProperty.Volume, muteAdjustment);
                else
                    audioman.RemoveAdjustment(AdjustableProperty.Volume, muteAdjustment);
            }
        }
    }
}
