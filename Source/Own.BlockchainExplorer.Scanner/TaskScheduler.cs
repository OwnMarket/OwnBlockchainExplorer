using Own.BlockchainExplorer.Common;
using System;
using System.Timers;

namespace Own.BlockchainExplorer.Scanner
{
    class TaskScheduler
    {
        private readonly Timer _timer;
        private readonly Action _action;

        public TaskScheduler(Action action, int intervalMillis = 1000)
        {
            _action = action;
            _timer = new Timer(intervalMillis)
            {
                AutoReset = false,
                Enabled = true
            };
            _timer.Elapsed += OnTimerElapsed;
        }

        public static void Run(Action action, int intervalSeconds)
        {
            var taskScheduler = new TaskScheduler(action, 1000 * intervalSeconds);
            taskScheduler.Start();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                (sender as Timer).Start();
            }
        }
    }
}
