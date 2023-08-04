using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerMgr : MonoBehaviour {

    public static readonly int DEFINE_TIMER_ID_INVALID = int.MinValue;

    private List<Timer>            m_delTimerList = new List<Timer>();
    private Dictionary<int, Timer> m_timerDict    = new Dictionary<int, Timer>();
    private int                    m_RTTI         = DEFINE_TIMER_ID_INVALID;
    
    public int AddTimer(float interval, bool isLoop, UnityAction<int> action) {
        int   timerID = ++m_RTTI;
        Timer timer   = new Timer(timerID, interval, isLoop, action, OnTimerFinishedCallback);
        if (m_timerDict.ContainsKey(timerID)) {
            m_timerDict[timerID].UnInit();
        }

        m_timerDict[timerID] = timer;
        timer.Init();
        return timerID;
    }

    public void CancelTimer(int timerID) {
        if (m_timerDict.ContainsKey(timerID)) {
            m_delTimerList.Add(m_timerDict[timerID]);
        }
    }

    private void OnTimerFinishedCallback(int timerID) {
        CancelTimer(timerID);
    }
    
    void Awake() {
        m_timerDict.Clear();
        m_delTimerList.Clear();
    }

    // Update is called once per frame
    void Update() {
        foreach (KeyValuePair<int, Timer> kv in m_timerDict) {
            kv.Value.Update(Time.deltaTime);
        }
        
        foreach (Timer timer in m_delTimerList) {
            if (m_timerDict.ContainsKey(timer.ID)) {
                m_timerDict.Remove(timer.ID);
            }
        }
        
        m_delTimerList.Clear();
    }
}
