using System.Diagnostics;
using Raylib_cs;


public static class Program
{
    //dotnet publish -c release -r win-x64 --self-contained true -o "bin\Debug\net10.0\release"


    static bool ShouldLive = true;

    static GameLoop[] Loops;

    #region Functions that initialize the program in the order they are executed.
    static void InitLoops()
    {
        List<GameLoop> init = new List<GameLoop>();
        init.Add(new ScreenSystem());

        Loops = init.ToArray();
    }
    
    static void Run()
    {
        double frequency = (double)Stopwatch.Frequency;
        long past = Stopwatch.GetTimestamp();
        long now;
        double deltaDeficit;

        while(ShouldLive)
        {
            now = Stopwatch.GetTimestamp();
            if(now == past) continue;

            deltaDeficit = (now - past) / frequency;

            past = now;

            int wait = 0;

            for(int i = 0; i < Loops.Length; ++i)
            {
                GameLoop loop = Loops[i];
                loop.UpdateTime(deltaDeficit);

                int next_execution = loop.MsecTillExecution;

                if(wait < next_execution) wait = next_execution;
            }

            if(wait > 1) Thread.Sleep(wait);
        }
    }

    static void CloseProgram()
    {
        for(int i = 0; i < Loops.Length; ++i)
        {
            GameLoop curr = Loops[i];

            curr.OnEnd();
        }
    }

    #endregion

    public static void RequestEndProgram()
    {
        ShouldLive = false;
    }

    public static void Main(string[] args)
    {
        InitLoops();
        Run();
    }
}