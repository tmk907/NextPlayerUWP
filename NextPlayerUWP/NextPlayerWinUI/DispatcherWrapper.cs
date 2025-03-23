using System;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Windows.UI.Core;

namespace NextPlayerWinUI
{
    public interface IDispatcherWrapper
    {
        bool HasThreadAccess();
        void Dispatch(Action action, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
        Task DispatchAsync(Func<Task> func, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
        Task DispatchAsync(Action action, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
        Task<T> DispatchAsync<T>(Func<T> func, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
    }

    public class DispatcherWrapper : IDispatcherWrapper
    {
        private readonly DispatcherQueue dispatcher;

        public DispatcherWrapper(DispatcherQueue dispatcherQueue) 
        {
            dispatcher = dispatcherQueue;
        }

        public bool HasThreadAccess() => dispatcher.HasThreadAccess;
        public void Dispatch(Action function, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    function();
                }
                catch (Exception e)
                {
                    
                }
            }

            dispatcher.TryEnqueue(priority, () => function());
        }

        public Task DispatchAsync(Action function, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    function();

                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            }

            static Task TryEnqueueAsync(DispatcherQueue dispatcher, Action function, DispatcherQueuePriority priority)
            {
                var taskCompletionSource = new TaskCompletionSource<object?>();

                if (!dispatcher.TryEnqueue(priority, () =>
                {
                    try
                    {
                        function();

                        taskCompletionSource.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetException(e);
                    }
                }))
                {
                    taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
                }
                return taskCompletionSource.Task;
            }

            return TryEnqueueAsync(dispatcher, function, priority);
        }

        public Task DispatchAsync(Func<Task> function, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    if (function() is Task awaitableResult)
                    {
                        return awaitableResult;
                    }

                    return Task.FromException(GetEnqueueException("The Task returned by function cannot be null."));
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            }

            static Task TryEnqueueAsync(DispatcherQueue dispatcher, Func<Task> function, DispatcherQueuePriority priority)
            {
                var taskCompletionSource = new TaskCompletionSource<object?>();

                if (!dispatcher.TryEnqueue(priority, async () =>
                {
                    try
                    {
                        if (function() is Task awaitableResult)
                        {
                            await awaitableResult.ConfigureAwait(false);

                            taskCompletionSource.SetResult(null);
                        }
                        else
                        {
                            taskCompletionSource.SetException(GetEnqueueException("The Task returned by function cannot be null."));
                        }
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetException(e);
                    }
                }))
                {
                    taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
                }

                return taskCompletionSource.Task;
            }

            return TryEnqueueAsync(dispatcher, function, priority);
        }

        public Task<T> DispatchAsync<T>(Func<T> function, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            if (dispatcher.HasThreadAccess)
            {
                try
                {
                    return Task.FromResult(function());
                }
                catch (Exception e)
                {
                    return Task.FromException<T>(e);
                }
            }

            static Task<T> TryEnqueueAsync(DispatcherQueue dispatcher, Func<T> function, DispatcherQueuePriority priority)
            {
                var taskCompletionSource = new TaskCompletionSource<T>();

                if (!dispatcher.TryEnqueue(priority, () =>
                {
                    try
                    {
                        taskCompletionSource.SetResult(function());
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetException(e);
                    }
                }))
                {
                    taskCompletionSource.SetException(GetEnqueueException("Failed to enqueue the operation"));
                }

                return taskCompletionSource.Task;
            }

            return TryEnqueueAsync(dispatcher, function, priority);
        }

        /// <summary>
        /// Creates an <see cref="InvalidOperationException"/> to return when an enqueue operation fails.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <returns>An <see cref="InvalidOperationException"/> with a specified message.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetEnqueueException(string message)
        {
            return new InvalidOperationException(message);
        }
    }
}
