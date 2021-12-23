using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameLock : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void ExcuteFrameLock()
    {
        Application.targetFrameRate = 60;
    }
}
