using System;
using System.Threading;

namespace FunkinSharp.Game.Core
{
    // https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Textures/TextureWithRefCount.cs#L60
    public class ReferenceCount
    {
        private readonly object lockObject;
        private readonly Action onAllReferencesLost;

        private int referenceCount;

        /// <summary>
        /// Creates a new <see cref="ReferenceCount"/>.
        /// </summary>
        /// <param name="lockObject">The <see cref="object"/> which locks will be taken out on.</param>
        /// <param name="onAllReferencesLost">A delegate to invoke after all references have been lost.</param>
        public ReferenceCount(object lockObject, Action onAllReferencesLost)
        {
            this.lockObject = lockObject;
            this.onAllReferencesLost = onAllReferencesLost;
        }

        /// <summary>
        /// Increments the reference count.
        /// </summary>
        public void Increment()
        {
            lock (lockObject)
                Interlocked.Increment(ref referenceCount);
        }

        /// <summary>
        /// Decrements the reference count, invoking <see cref="onAllReferencesLost"/> if there are no remaining references.
        /// The delegate is invoked while a lock on the provided <see cref="lockObject"/> is held.
        /// </summary>
        public void Decrement()
        {
            lock (lockObject)
            {
                if (Interlocked.Decrement(ref referenceCount) == 0)
                    onAllReferencesLost?.Invoke();
            }
        }
    }
}
