using System;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace NextPlayerUWP.Common
{
    public class PlaybackTimer
    {
        ThreadPoolTimer timer = null;
        bool isTimerSet = false;
        private Func<Task> taskToDo;
        private Action actionToDo;

        public void SetTimerWithAction(TimeSpan delay, Action actionToDo)
        {
            this.actionToDo = actionToDo;
            if (delay > TimeSpan.Zero)
            {
                if (isTimerSet)
                {
                    TimerCancel();
                }
                timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), delay);
                isTimerSet = true;
            }
        }

        public void SetTimerWithTask(TimeSpan delay, Func<Task> taskToDo)
        {
            this.taskToDo = taskToDo;

            if (isTimerSet)
            {
                TimerCancel();
            }
            timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(TimerCallback), delay);
            isTimerSet = true;
        }

        private async void TimerCallback(ThreadPoolTimer timer)
        {
            TimerCancel();
            if (taskToDo != null)
            {
                await Task.Run(taskToDo);
            }
            else
            {
                actionToDo?.Invoke();
            }
        }

        public void TimerCancel()
        {
            isTimerSet = false;
            if (timer != null)
            {
                timer.Cancel();
            }
        }
    }
}
