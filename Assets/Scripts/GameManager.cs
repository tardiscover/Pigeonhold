using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Note that this is a SingletonMonoBehavior.
/// </summary>
public class GameManager : SingletonMonoBehavior<GameManager>
{
    [SerializeField] int enemiesCountInitial;
    [SerializeField] int enemiesCountCurrent;

    protected override void Awake()
    {
        base.Awake();   //Remember this!
        //things to do on Awake in the child, if anything...
    }

    public void IncrementEnemyCount()
    {
        enemiesCountInitial++;
        enemiesCountCurrent++;
    }

    public void DecrementCurrentEnemyCount()
    {
        enemiesCountCurrent--;
    }
}
