using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trying a new method of implementing a Singleton as an inheritable abstract class based on a MonoBehavior.
/// https://stackoverflow.com/questions/28447839/how-to-improvise-singleton-for-inherited-classes#28449039
/// 
/// To inherit class "MyClass" from it, use:
///     public class MyClass : MySingleton<MyClass>
///     {
///         protected override void Awake()
///         {
///             base.Awake();   //Remember this!
///             //things to do on Awake in the child, if anything...
///         }
///         
///     }
/// 
/// Also see https://dcastares.github.io/2022/07/28/Properly-inheriting-Update-Awake-and-Start-methods-in-monobehaviours.html
/// </summary>
public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : SingletonMonoBehavior<T>
{
    static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null) _instance = (T)FindObjectOfType(typeof(T));

            //DON'T call the following, or you may get a false error when quitting the application if the Singleton is destroyed before 
            //something tries to access it in OnDestroy().
            //Instead, when accessing the Singleton in an OnDestroy(), check "if ([singletontype].Instance)" first.
            //if (_instance == null) Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null) _instance = this as T;
        else if (_instance != this) Destroy(this);
    }
}
