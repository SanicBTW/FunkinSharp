using System.Reflection;
using FunkinSharp.API.Screens.Navigation;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Layout;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;

namespace FunkinSharp.API.Screens.Presentation;

// class that holds snapshots and ownership of the main screen stack in a buffered container
// TODO: OpenGL and deferred renderers are not working
public partial class FunkinPresentation : CompositeDrawable
{
    // didnt want to get to this but uhhhh alright
    private static readonly FieldInfo shared_data_field = typeof(BufferedContainer<FunkinScreenStack>).GetField("sharedData", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private FunkinScreenStack screenStack = null!;
    public FunkinScreenStack ScreenStack => screenStack;

    private BufferedContainer<FunkinScreenStack> screenStackBuffer = null!;

    private BufferedDrawNodeSharedData sharedDataRef = null!;

    // i didnt want to end up doing this...
    [Resolved] private IRenderer renderer { get; set; } = null!;
    private bool isGl;
    private bool isDeferred;

    private FramebufferCopy? signal;
    public Texture? Snapshot;

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        // lets just fallback to a sane alternative until i figure out a fix...
        isGl = host.ResolvedRenderer == RendererType.OpenGL;
        isDeferred = (int)host.ResolvedRenderer >= 32; // following osu!frameowkr enum def, deferred types begin at 32 with metal

        InternalChildren =
        [
            screenStackBuffer = new BufferedContainer<FunkinScreenStack>()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Child = screenStack = new FunkinScreenStack() { RelativeSizeAxes = Axes.Both, Anchor = Anchor.Centre, Origin = Anchor.Centre }
            }
        ];

        if (!isGl && !isDeferred)
            AddInternal(signal = new FramebufferCopy(screenStackBuffer));

        sharedDataRef = (BufferedDrawNodeSharedData)shared_data_field.GetValueDirect(__makeref(screenStackBuffer))!;

        screenStack.ScreenPushed += ScreenStackOnScreenPushed;
        screenStack.ScreenExited += ScreenStackOnScreenExited;
    }

    private void ScreenStackOnScreenPushed(IScreen lastScreen, IScreen newScreen)
    {
        Logger.Log($"moved to {newScreen} {lastScreen} was suspended");

        if (sharedDataRef.IsInitialised)
            capture();
    }

    private void ScreenStackOnScreenExited(IScreen lastScreen, IScreen newScreen)
    {
        Logger.Log($"exited {lastScreen} moved to {newScreen}");
    }

    public BufferedContainerView<FunkinScreenStack> GetView()
    {
        BufferedContainerView<FunkinScreenStack> bufferView = screenStackBuffer.CreateView();
        bufferView.SynchronisedDrawQuad = true;
        return bufferView;
    }

    // should make async to be able to pause until i get the frame buffer data... oh well
    private void capture()
    {
        if (signal != null)
        {
            signal.Capture(sharedDataRef.MainBuffer);
            Snapshot = signal.FrameBuffer.Texture;
        }
        else
        {
            var img = renderer.ExtractFrameBufferData(sharedDataRef.MainBuffer);
            if (img == null)
            {
                Logger.Log("failed to retrieve the framebuffer data", LoggingTarget.Runtime, LogLevel.Error);
                return;
            }

            var imgUpl = new TextureUpload(img);

            // match the framebuffer object
            Snapshot = renderer.CreateTexture(1280, 720, true);
            Snapshot.SetData(imgUpl);
        }
    }

    private partial class FramebufferCopy(BufferedContainer<FunkinScreenStack> source) : Drawable, ITexturedShaderDrawable
    {
        public IFrameBuffer FrameBuffer = null!;
        private IFrameBuffer sourceFrameBuffer = null!;

        private readonly BufferedContainer<FunkinScreenStack> screenStackBuffer = source;
        private bool didSnapshot;

        public IShader? TextureShader { get; private set; }

        public void Capture(IFrameBuffer source)
        {
            sourceFrameBuffer = source;
            didSnapshot = false;
            Invalidate(Invalidation.DrawNode, InvalidationSource.Parent);
        }

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            FrameBuffer = renderer.CreateFrameBuffer();
            FrameBuffer.Size = new Vector2(1280, 720); // need to get the default size from somewhere static

            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override DrawNode CreateDrawNode() => new FramebufferCopyDrawNode(this);

        private partial class FramebufferCopyDrawNode(FramebufferCopy source) : TexturedShaderDrawNode(source)
        {
            private FramebufferCopy source => (FramebufferCopy)Source;

            private Quad screenSpaceDrawQuad;
            private IFrameBuffer? sourceFrameBuffer;
            private IFrameBuffer? targetFrameBuffer;

            private bool canDraw;

            public override void ApplyState()
            {
                base.ApplyState();

                screenSpaceDrawQuad = source.screenStackBuffer.ScreenSpaceDrawQuad;
                sourceFrameBuffer = source.sourceFrameBuffer;
                targetFrameBuffer = source.FrameBuffer;

                canDraw = sourceFrameBuffer != null && targetFrameBuffer != null;
            }

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (!canDraw || source.didSnapshot)
                    return;

                BindTextureShader(renderer);
                targetFrameBuffer!.Bind();

                renderer.DrawFrameBuffer(
                    sourceFrameBuffer!,
                    screenSpaceDrawQuad,
                    DrawColourInfo.Colour);

                targetFrameBuffer.Unbind();
                UnbindTextureShader(renderer);

                source.didSnapshot = true;
            }
        }
    }
}
