using System;
using System.Collections.Generic;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Core
{
    // https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/network/MultiCallback.hx
    public class MultiCallback
    {
        public Action Callback;
        public string LogId = null;

        public int Length { get; private set; } = 0;
        public int NumRemaining { get; private set; } = 0;

        private Dictionary<string, Action> unfired = [];
        private List<string> fired = [];

        public MultiCallback(Action callback, string logId = null)
        {
            Callback = callback;
            LogId = logId;
        }

        public Action Add(string id = "untitled")
        {
            id = $"{Length}:{id}";
            Length++;
            NumRemaining++;

            void func()
            {
                if (unfired.ContainsKey(id))
                {
                    unfired.Remove(id);
                    fired.Add(id);
                    NumRemaining--;

                    log($"Fired {id}, {NumRemaining} remaining");

                    if (NumRemaining == 0)
                    {
                        log("All callbacks fired");
                        Callback();
                    }
                }
                else
                    log($"Already fired {id}");
            }

            unfired[id] = func;
            return func;
        }

        private void log(string msg)
        {
            if (LogId != null)
                Logger.Log(msg, LoggingTarget.Performance, LogLevel.Debug);
        }

        public string[] GetFired() => [.. fired];

        public string[] GetUnfired()
        {
            string[] copy = new string[unfired.Count];
            unfired.Keys.CopyTo(copy, 0);
            return copy;
        }
    }
}
