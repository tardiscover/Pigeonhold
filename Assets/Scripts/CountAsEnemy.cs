using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountAsEnemy : MonoBehaviour
{
    void Awake()
    {
        GameManager.Instance.IncrementFoesLeftMax();
    }

    private void OnDestroy()
    {
        GameManager.Instance.DecrementFoesLeftMax();
    }
}
