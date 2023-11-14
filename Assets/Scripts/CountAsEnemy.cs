using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountAsEnemy : MonoBehaviour
{
    void Awake()
    {
        GameManager.Instance.IncrementEnemyCount();
    }

    private void OnDestroy()
    {
        GameManager.Instance.DecrementCurrentEnemyCount();
    }
}
