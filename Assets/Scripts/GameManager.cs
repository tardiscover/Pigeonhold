using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Note that this is a SingletonMonoBehavior.
/// </summary>
public class GameManager : SingletonMonoBehavior<GameManager>
{
    [SerializeField] int foesLeftMax;
    [SerializeField] int foesLeftCurrent;
    public FoesLeftBar foesLeftBar;

    protected override void Awake()
    {
        base.Awake();   //Remember this!
        //things to do on Awake in the child, if anything...
    }

    public void IncrementFoesLeftMax()
    {
        foesLeftMax++;
        foesLeftCurrent++;
        foesLeftBar.SetFoesLeft(foesLeftCurrent, foesLeftMax);
    }

    public void DecrementFoesLeftMax()
    {
        foesLeftCurrent--;
        foesLeftBar.SetFoesLeft(foesLeftCurrent, foesLeftMax);
    }
}
