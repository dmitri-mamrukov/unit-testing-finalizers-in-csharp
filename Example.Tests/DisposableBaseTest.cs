using Example;
using System;
using Xunit;

namespace Example.Tests
{
    class Disposable : DisposableBase
    {
        private Action _onDisposeManagedResources;
        private Action _onDisposeUnmanagedResources;

        public Disposable(Action onDisposeManagedResources, Action onDisposeUnmanagedResources)
        {
            _onDisposeManagedResources = onDisposeManagedResources;
            _onDisposeUnmanagedResources = onDisposeUnmanagedResources;
        }

        protected override void DisposeManagedResources() => _onDisposeManagedResources?.DynamicInvoke();

        protected override void DisposeUnmanagedResources() => _onDisposeUnmanagedResources?.DynamicInvoke();
    }

    public class DisposableBaseTest
    {
        [Fact]
        public void DisposeCallsDisposalOfManagedAndUnmanagedResources()
        {
            var disposedManagedResources = false;
            var disposedUnmanagedResources = false;
            var disposable = new Disposable(
                onDisposeManagedResources: () => disposedManagedResources = true,
                onDisposeUnmanagedResources: () => disposedUnmanagedResources = true);

            disposable.Dispose();

            Assert.True(disposedManagedResources);
            Assert.True(disposedUnmanagedResources);
        }

        [Fact]
        public void UsingStatementCallsDisposalOfManagedAndUnmanagedResources()
        {
            var disposedManagedResources = false;
            var disposedUnmanagedResources = false;

            using (var disposable = new Disposable(
                onDisposeManagedResources: () => disposedManagedResources = true,
                onDisposeUnmanagedResources: () => disposedUnmanagedResources = true))
            {
            }

            Assert.True(disposedManagedResources);
            Assert.True(disposedUnmanagedResources);
        }

        // The GC will not finalize objects if the reference can be obtained by
        // walking other reference graphs, or the object itself is rooted. A
        // reference is considered a root in the following conditions:
        //
        // - A local variable in the currently running method, or
        // - Static references
        //
        // So given the above restrictions, how do we make a finalizer
        // deterministic? The secret is a combination of WeakReference<T>,
        // delegated execution of your test action, and blocked execution until
        // the finalizers have executed.
        [Fact]
        public void FinalizationCallsDisposalOfManagedAndUnmanagedResources()
        {
            var disposedManagedResources = false;
            var disposedUnmanagedResources = false;
            // We can't initialize the weak reference until we've created our
            // disposable, which we don't want to do in the current scope.
            WeakReference<Disposable> weakReference = null;
            // We create a delegate we can execute later in the test.
            Action dispose = () =>
            {
                // This will go out of scope after dispose() is executed.
                var disposable = new Disposable(
                    onDisposeManagedResources: () => disposedManagedResources = true,
                    onDisposeUnmanagedResources: () => disposedUnmanagedResources = true);
                // Create our weak reference, rooted in the outer scope, and
                // passing true to the second argument will allow it to continue
                // tracking the object after finalization.
                weakReference = new WeakReference<Disposable>(disposable, true);
            };

            // Execute our test action.
            dispose();
            // Force the GC to collect at this point. Given we haven't GC'd at
            // this point before and our references are heap allocated, this
            // means they exist in generation 0.
            GC.Collect(0, GCCollectionMode.Forced);
            // Block until the GC has finished processing the finalizer queue
            // for collected objects.
            GC.WaitForPendingFinalizers();

            Assert.False(disposedManagedResources);
            Assert.True(disposedUnmanagedResources);
        }
    }
}
