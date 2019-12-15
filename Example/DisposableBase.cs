using System;

namespace Example
{
    /// <summary>
    /// Finalizers are generally non-deterministic. If you leave the GC to its
    /// job, it will finalize eligible objects at *some* point. This doesn't
    /// work very well for us if we need to test that our disposable types are
    /// behaving.
    ///
    /// Let's look at a base type provided as part of a framework; the
    /// DisposableBase. This type provides a base implementation of disposable
    /// operations. Any time we need to implement something as disposable, we
    /// inherit from this type.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        #region Destructor

        /// <summary>
        /// A destructor.
        /// </summary>
        ~DisposableBase()
        {
            Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the flag that tells whether the object is disposed.
        /// </summary>
        /// <value>A boolean value - true if the object is disposed.</value>
        public bool Disposed { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Dispose(true);

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes in two distinct scenarios.
        ///
        /// If disposing equals true, the method has been called directly
        /// or indirectly by the user's code. Managed and unmanaged resources
        /// can be disposed.
        ///
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">A boolean value - true if the call is done
        /// directly or indirectly by the user's code.</param>
        protected void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Disposed = true;

                if (disposing)
                {
                    DisposeManagedResources();
                }

                DisposeUnmanagedResources();

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Disposes managed resources. Invoked by .Dispose() - either directly
        /// or through a using statement:
        ///
        /// <code>
        /// using (new SomethingThatInheritsDisposableBase())
        /// {
        /// }
        /// </code>
        /// </summary>
        protected virtual void DisposeManagedResources() { }

        /// <summary>
        /// Disposes unmanaged resources. Invoked either through
        /// an explicit disposal, or through an object finalization.
        /// </summary>
        protected virtual void DisposeUnmanagedResources() { }

        #endregion
    }
}
