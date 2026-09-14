using System.Collections.Generic;
using System.Drawing;
using FunkinSharp.API.Configuration;
using FunkinSharp.API.Input;
using FunkinSharp.Resources;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;

namespace FunkinSharp.Game
{
    // https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.Game/LivinOnSweetsGameBase.cs

    public partial class FunkinSharpGameBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content => content;
        private Container content;

        protected FunkinActionContainer ActionContainer;
        protected DependencyContainer GameDependencies;

        protected SafeAreaContainer SafeAreaContainer { get; private set; }

        protected virtual Edges SafeAreaOverrideEdges => Edges.None;

        protected Storage Storage { get; set; }

        // config here too lolz

        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new DllResourceStore(FunkinSharpResources.ResourceAssembly));

            GameDependencies.Cache(Storage);

            IResourceStore<TextureUpload> resxUpload = Host.CreateTextureLoaderStore(Resources);
            Textures.AddTextureSource(resxUpload);

            LargeTextureStore largeTs = new(Host.Renderer, resxUpload);
            GameDependencies.Cache(largeTs);

            GameDependencies.Cache(new SessionConfig());

            base.Content.Add(SafeAreaContainer = new SafeAreaContainer
            {
                SafeAreaOverrideEdges = SafeAreaOverrideEdges,
                RelativeSizeAxes = Axes.Both,
                Child = CreateScalingContainer().WithChild(ActionContainer = new FunkinActionContainer().WithChild(content = new Container { RelativeSizeAxes = Axes.Both })) // cursor its supposed to be this container but we have none so far
            });

            GameDependencies.Cache(ActionContainer);
            base.Content.Add(new TouchInputInterceptor());
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            Storage ??= host.Storage;

            // init config here
        }

        protected virtual Container CreateScalingContainer() => new DrawSizePreservingFillContainer() { TargetDrawSize = new Vector2(1280, 720) };

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            GameDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
        {
            // Setting the default locale to en to populate the object on first run instead of having an emtpy string and falling back on localisable strings
            { FrameworkSetting.Locale, "en" },
            { FrameworkSetting.WindowedSize, new Size(1280, 720) },
            { FrameworkSetting.AudioUseExperimentalWasapi, true },
        };
    }
}
