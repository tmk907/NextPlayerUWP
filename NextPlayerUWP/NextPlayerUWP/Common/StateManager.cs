using System.Collections.Generic;

namespace NextPlayerUWP.Common
{
    public class StateManager
    {
        private static readonly StateManager current = new StateManager();
        public static StateManager Current
        {
            get { return current; }
        }
        static StateManager() { }
        private StateManager()
        {

        }

        private Dictionary<string, Dictionary<string, object>> savedStates = new Dictionary<string, Dictionary<string, object>>();

        public  Dictionary<string,object> Read(string pageName)
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            savedStates.TryGetValue(pageName, out state);
            return state;
        }

        public void Add(string pageName, string key, string value)
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            savedStates.TryGetValue(pageName, out state);
            state[key] = value;
            savedStates[pageName] = state;
        }

        public void Save(string pageName, Dictionary<string,object> state)
        {
            savedStates[pageName] = state;
        }

        public  void Clear(string pageName)
        {
            savedStates.Remove(pageName);
        }
    }
}
