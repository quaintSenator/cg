using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer {

    private int              m_id               = TimerMgr.DEFINE_TIMER_ID_INVALID;
    private bool             m_enabled          = false;
    private bool             m_isLoop           = false;
    private float            m_interval         = 0f;
    private float            m_timeCal          = 0f;
    private UnityAction<int> m_callback         = null;
    private UnityAction<int> m_finishedCallback = null;
    
    public int ID { get => m_id;}
    
    public Timer(int id, float interval, bool isLoop, UnityAction<int> action, UnityAction<int> finishedCallback) {
        m_id               = id;
        m_interval         = interval;
        m_isLoop           = isLoop;
        m_callback         = action;
        m_finishedCallback = finishedCallback;
        if (m_finishedCallback == null) {
            Debug.LogError("Timer FinishedCallback is null!");
        }
    }
    
    public void Init() {
        m_enabled = true;
        m_timeCal = 0;
    }

    public void UnInit() {
        m_enabled = false;
    }

    public void Update(float dt) {
        if (m_enabled == false) return;
        m_timeCal += dt;
        if (m_timeCal >= m_interval) {
            m_timeCal -= m_interval;
            m_callback?.Invoke(m_id);
            if (!m_isLoop) {
                m_enabled = false;
                m_finishedCallback?.Invoke(m_id);
            }
        }
    }
}
