using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


public abstract class GameLoop
{
    public int MsecTillExecution
    {
        get 
        {
            if(TargetUpdateFrequency <= 0) return 0;
            if(Delta == 0.0) return (int)(1000.0 / TargetUpdateFrequency);
            return (int)((Delta/ TargetUpdateFrequency) * 1000.0);
        }
    }
    public double TargetUpdateFrequency = 60;
    public double Delta = 0.0;

    protected bool ExecutesEveryUpdate = false;

    protected abstract void OnInit();
    protected abstract void OnDeltaUpdate();

    public abstract void OnEnd();

    public void UpdateTime(double secondsElapsed)
    {
        Delta += secondsElapsed * TargetUpdateFrequency;
        if(ExecutesEveryUpdate)
        {
            OnDeltaUpdate();
        }
        else if(Delta > 1.0)
        {
            Delta -= 1.0;
            OnDeltaUpdate();
        }
    }

    public GameLoop() => OnInit();
}