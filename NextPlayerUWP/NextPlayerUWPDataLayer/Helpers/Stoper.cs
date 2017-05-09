using System;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class Stoper
    {
        public TimeSpan ElapsedTime { get; private set; }
        private DateTime startTime;

        public Stoper()
        {
            startTime = DateTime.UtcNow;
            ElapsedTime = TimeSpan.Zero;
        }

        public void Start()
        {
            startTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            ElapsedTime += DateTime.UtcNow - startTime;
        }

        public void ResetAndStart()
        {
            startTime = DateTime.UtcNow;
            ElapsedTime = TimeSpan.Zero;
        }
    }
}
