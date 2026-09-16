using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osuTK;

namespace FunkinSharp.API.Animation;

// TODO: finish and do proper frame addition
public partial class SparrowAnimation() : AnimationClockComposite(false)
{
    // you can either map animation datas like: animName => animation with real name / real name => animation with animName, this acts like a quick alias, rather than having an alias dictionary
    // also you can do animName => animName, if you really really need to use the alias rather than the anim name
    public Dictionary<string, SparrowAnimationData> Animations { get; } = [];

    protected readonly List<SparrowFrame> Frames = [];

    public int CurrentFrameIndex { get; private set; }

    public SparrowAnimationData? CurrentAnimation { get; private set; }

    public bool Reversed { get; private set; }

    public bool Finished { get; private set; }

    public bool Paused
    {
        get => !IsPlaying;
        set => IsPlaying = !value;
    }

    public SparrowFrame? CurrentSparrowFrame
    {
        get
        {
            var animation = CurrentAnimation;

            if (animation == null ||
                animation.Frames.Length == 0 ||
                CurrentFrameIndex < 0 ||
                CurrentFrameIndex >= animation.Frames.Length)
            {
                return null;
            }

            return Frames[animation.Frames[CurrentFrameIndex]];
        }
    }

    private readonly Cached currentFrameCache = new();

    protected override void Update()
    {
        base.Update();

        if (CurrentAnimation == null || Finished)
            return;

        updateTimeline();

        if (!currentFrameCache.IsValid)
            updateCurrentFrame();
    }

    public virtual void Play(string name,
        bool force = true,
        bool reversed = false,
        int frame = 0)
    {
        string resolvedName = ResolveAnimationName(name);

        if (!Animations.TryGetValue(resolvedName, out var animation))
            throw new ArgumentException(
                $"Animation '{resolvedName}' does not exist.",
                nameof(resolvedName));

        if (!force &&
            ReferenceEquals(CurrentAnimation, animation) &&
            Reversed == reversed &&
            !Finished)
        {
            return;
        }

        double frameDuration = 1000.0 / animation.FrameRate;

        CurrentAnimation = animation;
        Duration = animation.Frames.Length * frameDuration;
        Reversed = reversed;
        Finished = false;
        Paused = false;
        OnAnimationChanged(CurrentAnimation);

        CurrentFrameIndex = Math.Clamp(
            frame,
            0,
            animation.Frames.Length - 1);

        Seek(CurrentFrameIndex * frameDuration);
        currentFrameCache.Invalidate();
    }

    public void Reset()
    {
        var animation = CurrentAnimation;
        if (animation == null || animation.Frames.Length == 0)
            return;

        Finished = false;
        IsPlaying = true;

        Seek(0);
        currentFrameCache.Invalidate();
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void Resume()
    {
        if (Finished)
            return;

        IsPlaying = true;
    }

    public void Reverse()
    {
        Reversed = !Reversed;

        currentFrameCache.Invalidate();
    }

    public void AddFrame(SparrowFrame frame)
    {
        Frames.Add(frame);
        OnFrameAdded(frame);

        if (Frames.Count == 1)
            currentFrameCache.Invalidate();
    }

    public void AddFrames(IEnumerable<SparrowFrame> newFrames)
    {
        foreach (var f in newFrames)
            AddFrame(f);
    }

    public void ClearFrames()
    {
        Frames.Clear();

        CurrentAnimation = null;
        CurrentFrameIndex = 0;
        Duration = 0;
        Finished = false;
        Paused = false;

        clearDisplay();
        currentFrameCache.Invalidate();
    }

    protected virtual string ResolveAnimationName(string animation) => animation;

    protected virtual void OnAnimationChanged(SparrowAnimationData animation) { }

    protected virtual void OnAnimationFinished(SparrowAnimationData animation) { }

    protected virtual void OnFrameChanged(SparrowFrame frame) { }

    protected virtual void OnFrameAdded(SparrowFrame frame) { }

    private void updateTimeline()
    {
        if (CurrentAnimation == null)
            return;

        SparrowAnimationData animation = CurrentAnimation;

        double frameDuration = 1000.0 / animation.FrameRate;

        double position = PlaybackPosition;

        if (position >= Duration)
        {
            if (!animation.Loop)
            {
                finishAtEnd(animation);
                return;
            }

            double loopStart = animation.LoopPoint * frameDuration;
            double loopDuration = Duration - loopStart;

            if (loopDuration <= 0)
            {
                loopStart = 0;
                loopDuration = Duration;
            }

            position = loopStart + ((position - loopStart) % loopDuration);
        }

        int frame = Math.Min(
            (int)(position / frameDuration),
            animation.Frames.Length - 1);

        if (Reversed)
            frame = animation.Frames.Length - 1 - frame;

        if (frame == CurrentFrameIndex)
            return;

        CurrentFrameIndex = frame;
        currentFrameCache.Invalidate();
    }

    private void finishAtEnd(SparrowAnimationData animation)
    {
        Finished = true;
        IsPlaying = false;

        CurrentFrameIndex = Reversed
            ? 0
            : animation.Frames.Length - 1;
        OnAnimationFinished(animation);
        currentFrameCache.Invalidate();
    }

    private void updateCurrentFrame()
    {
        displayFrame(CurrentSparrowFrame!);
        OnFrameChanged(CurrentSparrowFrame!);
        UpdateSizing();
        currentFrameCache.Validate();
    }

    private SparrowSprite? frameHolder;

    protected override Vector2 GetCurrentDisplaySize()
        => CurrentSparrowFrame == null ? Vector2.Zero : CurrentSparrowFrame.SourceRect.Size;

    protected override float GetFillAspectRatio() => frameHolder?.FillAspectRatio ?? 1;

    public override Drawable CreateContent() => frameHolder = new SparrowSprite()
    {
        RelativeSizeAxes = Axes.Both,
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
    };

    private void displayFrame(SparrowFrame content) => frameHolder?.Apply(CurrentAnimation!, content);

    private void clearDisplay() => frameHolder?.Clean();
}
