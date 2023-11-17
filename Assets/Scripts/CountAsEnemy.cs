using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountAsEnemy : MonoBehaviour
{
    void Awake()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.IncrementFoesLeftMax();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.DecrementFoesLeftMax();
        }
    }
}
