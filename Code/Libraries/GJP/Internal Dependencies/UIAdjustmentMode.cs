using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum UIAdjustmentMode
{
    ///<summary>
    ///Scales the UI against the smallest axis of the screen border.
    ///</summary>
    Compact = 0,
    ///<summary>
    ///Scales the UI against both axis of the screen border.
    ///</summary>
    Extended = 1
}

public static class UIGetter
{
    static Func<Vector2Fi> UISizeGetter = null;

    public static Vector2Fi GetUISize()
    {
        if(UISizeGetter == null) return new Vector2Fi(-1, 0);

        return UISizeGetter.Invoke();
    }

    public static void SetUISizeGetter(Func<Vector2Fi> func)
    {
        UISizeGetter = func;
    }
}