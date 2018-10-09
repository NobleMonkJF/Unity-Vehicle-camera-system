using UnityEngine;
using System.Collections;

public class CameraSystem : MonoBehaviour
{
    public Camera m_MainCamera;
    public Camera m_HUDCamera;

    [Header("Base")]
    public Vector3 m_OffSet;
    public Transform m_WantedPosition;
    public Transform m_LookAtPos;
    public Transform m_Player;
    public float m_CameraLerpSpeed;
    public float m_MoveSensitivity = 10;
    public Rigidbody m_CameraBody;

    public bool m_IsActive = false;
    public bool m_Inisalized = false;
    
    [Header("ClipChecking")]
    public float m_NormalMultiplyer = 0.5f;

    
    public virtual void InIt(Vector3 offSet, Transform player, Transform lookAtPos, Transform wantedPos)
    {
        m_OffSet = offSet;
        m_Player = player;
        m_LookAtPos = lookAtPos;
        m_WantedPosition = wantedPos;
        m_CameraBody = GetComponent<Rigidbody>();
        m_Inisalized = true;
        Activate();
    }
    
    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
    }

    protected void ClipCheck(int mask)
    {
        //settng direction dictance and ray start pos
        Vector3 dir = Vector3.Normalize(m_WantedPosition.position - m_Player.position);
        float  distance = Vector3.Distance(m_WantedPosition.position, m_Player.position );
        Vector3 startPos = m_Player.position + dir * 1;
        
        //sphere casting
        RaycastHit[] hitInfos = Physics.RaycastAll(startPos, dir, distance, mask, QueryTriggerInteraction.Ignore);

        //Vector3 point = m_WantedPosition.position;
        float addedDistance = 0;
        Vector3 hitNormal = Vector3.zero;

        //cycling through hits
        foreach (RaycastHit hitInfo in hitInfos)
        {
            if (hitInfo.collider.gameObject.tag != Tags.Speeder &&
                hitInfo.collider.gameObject.tag != Tags.Splicer &&
                distance - hitInfo.distance > addedDistance)
            {
                hitNormal = hitInfo.normal;
                addedDistance = distance - hitInfo.distance;
            }
        }
        
        //adding to the m_wantedpos and using the hitnormal to prevent cliping
        m_WantedPosition.position -= addedDistance * dir - hitNormal * m_NormalMultiplyer;
    }


    protected void UpdateCameraVelocity()
    {
        float moveSpeed = Vector3.Distance(transform.position, m_WantedPosition.position) * m_MoveSensitivity;
        Vector3 moveDir = (m_WantedPosition.position - transform.position).normalized;
        
        m_CameraBody.velocity = moveDir * moveSpeed;
    }

    protected void CameraLookAt()
    {
        transform.LookAt(m_LookAtPos);
    }
    
    public void Deativate()
    {
        m_CameraBody.velocity = Vector3.zero;
        m_CameraBody.isKinematic = true;
        m_CameraBody.interpolation = RigidbodyInterpolation.None;
        m_IsActive = false;
        enabled = false;
    }

    public void Activate()
    {
        m_CameraBody.isKinematic = false;
        m_CameraBody.interpolation = RigidbodyInterpolation.Interpolate;
        m_IsActive = true;
        enabled = true;
    }

    public void FirstPersonMode(Transform parent)
    {
        Deativate();
        transform.position = parent.position;
        transform.rotation = parent.rotation;
        transform.parent = parent;
    }

    public void ThirdPersonMode()
    {
        transform.parent = null;
        Activate();
    }

    public void LerpCameraFOV(float to, float lerpSpeed)
    {
        m_MainCamera.fieldOfView = Mathf.Lerp(m_MainCamera.fieldOfView, to, lerpSpeed);
    }

    public void SetCameraFOV(float FOV)
    {
        m_MainCamera.fieldOfView = FOV;
    }



    [Header("Camera Shake")]
    public bool m_Active = false;
    public Vector2 m_Speed;
    public float m_Duration;
    public Vector2 m_CycleLength;
    public Vector2 m_CycleTime;
    
    public void AddCameraShake(Vector2 numberOfShakes, float duration, Vector2 speed)
    {
        //old
        //m_Intensity = intensity;
        //m_Duration = duration;
        //m_ShakeSpeed = speed;
        if (!m_Active)
        {
            m_Duration = duration;
            m_CycleLength.x = Mathf.Clamp(duration / numberOfShakes.x, 0.00001f, float.MaxValue);
            m_CycleLength.y = Mathf.Clamp(duration / numberOfShakes.y, 0.00001f, float.MaxValue);
            m_Speed = speed;
            m_CycleTime = Vector3.zero;
            m_Active = true;
        }
    }

    float ShakeLinear(float time, float startValue, float changeInValue, float Duration)
    {
        return changeInValue * time / Duration + startValue;
    }

    protected void CameraShake()
    {
        if (m_Duration > 0)
        {
            m_Duration -= Time.deltaTime;
            m_CycleTime.x += Time.deltaTime;
            m_CycleTime.y += Time.deltaTime;


            if (m_CycleTime.x > m_CycleLength.x)
            {
                m_Speed.x /= 1.3f;
                m_CycleTime.x = 0;
            }

            if (m_CycleTime.y > m_CycleLength.y)
            {
                m_Speed.y /= 1.3f;
                m_CycleTime.y = 0;
            }

            Vector3 newVelocity = m_CameraBody.velocity;

            //X veclocity
            newVelocity += transform.right * ShakeLinear(Mathf.Sin(((m_CycleTime.x - m_CycleLength.x / 2) * 2) / m_CycleLength.x), 0, m_Speed.x, m_CycleLength.x);
            //Y velocity
            newVelocity += transform.up * ShakeLinear(Mathf.Sin(((m_CycleTime.y - m_CycleLength.y / 2) * 2) / m_CycleLength.y), 0, m_Speed.y, m_CycleLength.y);

            m_CameraBody.velocity = newVelocity;
            //old
            //Vector3 newVelocity = m_CameraBody.velocity;
            //newVelocity += transform.right * Mathf.Sin(m_Duration * m_Intensity.x) * m_ShakeSpeed.x;
            //newVelocity += transform.up * Mathf.Sin(m_Duration * m_Intensity.y) * m_ShakeSpeed.y;

            //m_CameraBody.velocity = newVelocity;

            //m_Duration -= Time.fixedDeltaTime;
        }

        else
            m_Active = false;
    }
}
