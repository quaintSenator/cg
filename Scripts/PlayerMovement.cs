using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    
    public TimerMgr m_timerMgr;
    public Light    m_shineLight;
    
    public float    turnSpeed = 20f;

    Animator m_Animator;
    AudioSource m_AudioSource;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    private int  m_timerID = TimerMgr.DEFINE_TIMER_ID_INVALID;

    void Start () {
        
        Player.Instance.IsLogin = false;
        
        if (m_timerMgr == null) 
            Debug.LogError("Timer Mgr is null!");
        
        m_Animator = GetComponent<Animator> ();
        m_AudioSource = GetComponent<AudioSource> ();
        if (m_timerMgr != null) {
            m_timerID = m_timerMgr.AddTimer(0.5f, true, delegate(int id) {
                m_shineLight.enabled = !m_shineLight.enabled;
            });
        }
    }

    private void OnDestroy() {
        if (m_timerID != TimerMgr.DEFINE_TIMER_ID_INVALID) {
            m_timerMgr.CancelTimer(m_timerID);
            m_timerID = TimerMgr.DEFINE_TIMER_ID_INVALID;
        }
    }

    void FixedUpdate () {
        if (!Player.Instance.IsLogin) return;
        
        float horizontal = Input.GetAxis ("Horizontal");
        float vertical = Input.GetAxis ("Vertical");
        
        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize ();

        bool hasHorizontalInput = !Mathf.Approximately (horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately (vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool ("IsWalking", isWalking);
        
        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }
        else
        {
            m_AudioSource.Stop ();
        }

        Vector3 desiredForward = Vector3.RotateTowards (transform.forward, m_Movement, turnSpeed * Time.fixedDeltaTime, 0f);
        m_Rotation = Quaternion.LookRotation (desiredForward);
    }

    void OnAnimatorMove ()
    {
        transform.SetPositionAndRotation(transform.position + m_Movement * m_Animator.deltaPosition.magnitude,m_Rotation);
    }
}