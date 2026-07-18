using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgricultureSystem : MonoBehaviour
{
    public static AgricultureSystem Instance;

    private void Awake()
    {
        Instance = this;
    }
}
