using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneScenarioToRuleThemAll
{
    /// <summary>
    /// Allows to detach a current <see cref="SynchronizationContext"/> from the async method chain.
    /// </summary>
    public struct DetachSynchronizationContextAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Returns true if a current synchronization context is null.
        /// It means that the continuation is called only when a current context
        /// is presented.
        /// </summary>
        public bool IsCompleted => SynchronizationContext.Current == null;

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem(state => continuation());
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            ThreadPool.UnsafeQueueUserWorkItem(state => continuation(), null);
        }

        public void GetResult() { }

        public DetachSynchronizationContextAwaiter GetAwaiter() => this;
    }

    public static class Awaiters
    {
        public static DetachSynchronizationContextAwaiter DetachCurrentSyncContext()
        {
            return new DetachSynchronizationContextAwaiter();
        }
    }
}
