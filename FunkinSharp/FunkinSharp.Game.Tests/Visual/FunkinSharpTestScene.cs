using osu.Framework.Testing;

namespace FunkinSharp.Game.Tests.Visual
{
    public abstract partial class FunkinSharpTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new FunkinSharpTestSceneTestRunner();

        private partial class FunkinSharpTestSceneTestRunner : FunkinSharpGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
