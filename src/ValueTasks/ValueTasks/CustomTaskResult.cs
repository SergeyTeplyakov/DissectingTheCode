using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices.Ex;
using System.Threading.Tasks;
using System.Threading.Tasks.Ex;
using ValueTasks.CustomAwaiter;

namespace ValueTasks.CustomTasks
{
    //[AsyncMethodBuilder(typeof (CustomTaskMethodBuilder<>))]
    public struct CustomTask<TResult>
    {
        private readonly TResult _value;

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a completed operation.</summary>
        public bool IsCompleted => true;

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a successfully completed operation.</summary>
        public bool IsCompletedSuccessfully => true;

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a failed operation.</summary>
        public bool IsFaulted => false;

        /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a canceled operation.</summary>
        public bool IsCanceled => false;

        /// <summary>Gets the result.</summary>
        public TResult Result => _value;

        /// <summary>Gets an awaiter for this value.</summary>
        public ValueTaskAwaiter<TResult> GetAwaiter() => default(ValueTaskAwaiter<TResult>);

        ///// <summary>Configures an awaiter for this value.</summary>
        ///// <param name="continueOnCapturedContext">
        ///// true to attempt to marshal the continuation back to the captured context; otherwise, false.
        ///// </param>
        //public ConfiguredValueTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
        //    new ConfiguredValueTaskAwaitable<TResult>();

        /// <summary>Creates a method builder for use with an async method.</summary>
        /// <returns>The created builder.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)] // intended only for compiler consumption
        public static CustomTaskMethodBuilder<TResult> CreateAsyncMethodBuilder() => CustomTaskMethodBuilder<TResult>.Create();

        public Task<TResult> AsTask() => null;
    }

    public struct CustomTaskMethodBuilder<TResult>
    {
        private AsyncTaskMethodBuilder<TResult> _methodBuilder;
        private TResult _result;
        private bool _haveResult;
        private bool _useBuilder;

        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CustomTaskMethodBuilder<TResult> Create()
        {
            return new CustomTaskMethodBuilder<TResult>()
            {
                _methodBuilder = AsyncTaskMethodBuilder<TResult>.Create()
            };
        }

        /// <param name="stateMachine"></param>
        /// <typeparam name="TStateMachine"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            this._methodBuilder.Start<TStateMachine>(ref stateMachine);
        }

        /// <param name="stateMachine"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            this._methodBuilder.SetStateMachine(stateMachine);
        }

        /// <param name="result"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(TResult result)
        {
            if (this._useBuilder)
            {
                this._methodBuilder.SetResult(result);
            }
            else
            {
                this._result = result;
                this._haveResult = true;
            }
        }

        /// <param name="exception"></param>
        public void SetException(Exception exception)
        {
            this._methodBuilder.SetException(exception);
        }

        /// <returns></returns>
        public CustomTask<TResult> Task
        {
            get
            {
                return  new CustomTask<TResult>();
            }
        }

        /// <param name="awaiter"></param>
        /// <param name="stateMachine"></param>
        /// <typeparam name="TAwaiter"></typeparam>
        /// <typeparam name="TStateMachine"></typeparam>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this._useBuilder = true;
            this._methodBuilder.AwaitOnCompleted<TAwaiter, TStateMachine>(ref awaiter, ref stateMachine);
        }

        /// <param name="awaiter"></param>
        /// <param name="stateMachine"></param>
        /// <typeparam name="TAwaiter"></typeparam>
        /// <typeparam name="TStateMachine"></typeparam>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this._useBuilder = true;
            this._methodBuilder.AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref awaiter, ref stateMachine);
        }
    }

    public class CustomTaskResult
    {
        //public async CustomTask<int> FooAsync()
        //{
        //    await new CustomTask<int>();
        //    return 42;
        //}

        private static Lazy<int> s_lazy = new Lazy<int>(() => 42);

        //public ValueTask<int> AwaitLazyIfNotCompleted()
        //{
        //    var awaiter = s_lazy.GetAwaiter();
        //    if (awaiter.IsCompleted)
        //    {
        //        return new ValueTask<int>(awaiter.GetResult());
        //    }

        //    return Await();

        //    async ValueTask<int> Await()
        //    {
        //        return await awaiter;
        //    }
        //}
    }
}