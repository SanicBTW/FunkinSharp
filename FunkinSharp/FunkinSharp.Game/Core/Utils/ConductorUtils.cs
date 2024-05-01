namespace FunkinSharp.Game.Core.Utils
{
    public static class ConductorUtils
    {
        public static BPMChangeEvent GetBPMChange(double time)
        {
            BPMChangeEvent ret = new BPMChangeEvent(0, 0, Conductor.BPM, Conductor.StepCrochet);

            foreach (BPMChangeEvent change in Conductor.BPMChanges)
            {
                if (time >= change.SongTime)
                    ret = change;
            }

            return ret;
        }

        public static BPMChangeEvent GetBPMChange(int step)
        {
            BPMChangeEvent ret = new BPMChangeEvent(0, 0, Conductor.BPM, Conductor.StepCrochet);

            foreach (BPMChangeEvent change in Conductor.BPMChanges)
            {
                if (change.StepTime <= step)
                    ret = change;
            }

            return ret;
        }

        public static double GetCrochetAtTime(double time)
        {
            BPMChangeEvent lastChange = GetBPMChange(time);
            return lastChange.StepCrochet * 4;
        }

        public static double BeatToSeconds(int beat)
        {
            int step = beat * 4;
            BPMChangeEvent lastChange = GetBPMChange(step);
            return lastChange.SongTime + ((step - lastChange.StepTime) / (lastChange.BPM / 60) / 4) * 1000;
        }

        public static double GetStep(double time)
        {
            BPMChangeEvent lastChange = GetBPMChange(time);
            return lastChange.StepTime + (time - lastChange.SongTime) / lastChange.StepCrochet;
        }

        public static int GetStepRounded(double time)
        {
            BPMChangeEvent lastChange = GetBPMChange(time);
            return (int)(lastChange.StepTime + (time - lastChange.SongTime) / lastChange.StepCrochet);
        }

        public static double GetBeat(double time) => GetStep(time) / 4;

        public static double GetBeatRounded(double time) => (int)(GetStep(time) / 4);

        public static double CalculateCrochet(double bpm) => (60 / bpm) * 1000;

        public static int CompareBPMChanges(BPMChangeEvent event1, BPMChangeEvent event2) => (int)(event1.SongTime - event2.SongTime);
    }
}
