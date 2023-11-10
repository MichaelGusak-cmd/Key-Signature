using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public float Gravity = 10f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
