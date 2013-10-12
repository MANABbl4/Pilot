using UnityEngine;
using System.Collections;

[System.Serializable]
public class HUDRect
{
    public void Init()
    {
        m_screenRect.Set(m_rect.x * Screen.width, m_rect.y * Screen.height,
            m_rect.width * Screen.width, m_rect.height * Screen.height);
    }

    public void DeInit()
    {
    }

    public Rect GetRect()
    {
        return m_screenRect;
    }

    [SerializeField]
    private Rect m_rect;

    private Rect m_screenRect;
}
