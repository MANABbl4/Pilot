#define DEBUG_SUN

using UnityEngine;
using System.Collections;

public class Sun : SingletonGameObject<Sun>
{
    [System.Diagnostics.Conditional("DEBUG_SUN")]
    private static void Log(string msg)
    {
        Debug.Log("Sun. " + msg);
    }

    protected override void Init()
    {
        base.Init();
        transform.position = m_pos;
        transform.Rotate(m_rot);
    }

    protected override void DeInit()
    {
        base.DeInit();
    }

    [SerializeField]
    private Vector3 m_pos;
    [SerializeField]
    private Vector3 m_rot;
}
