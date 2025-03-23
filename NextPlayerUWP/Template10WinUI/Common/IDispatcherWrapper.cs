using System;
using System.Threading.Tasks;

namespace Template10.Common
{
    using Microsoft.UI.Dispatching;

    public interface IDispatcherWrapper
    {
        bool HasThreadAccess();
        void Dispatch(Action action, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
        Task DispatchAsync(Func<Task> func, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
        Task DispatchAsync(Action action, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
        Task<T> DispatchAsync<T>(Func<T> func, int delayms = 0, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal);
    }
}