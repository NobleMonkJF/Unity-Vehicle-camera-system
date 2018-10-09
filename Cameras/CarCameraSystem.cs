using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

public class CarCameraSystem : CameraSystem
{
    Rigidbody m_Body;
    float m_DistanceFromCar;

    short m_AtPos = 3;
    short m_TotalPos = 4;
    Transform m_CameraPos1;
    Transform m_CameraPos2;
    Transform m_CameraPos3;
    Transform m_CameraPos4;

    Transform m_LeftView;
    Transform m_RightView;
    Transform m_RearView;

    Transform m_DefaultLookAtPos;
    public Transform m_HoodLookAtPos;

    bool m_LookBack = false;
    bool m_LookRight = false;
    bool m_LookLeft = false;


    Vector3 m_OldNormalizedVelocity;

    [Header("FOV Blast")]
    bool m_FOVBlastActive = false;
    float m_FOVTotalTime;
    float m_FOVTotalTimePass;
    float m_FOVChange = 0;
    float m_FOVBase;

    Camera m_AffectedCamera;

    [Header("Test CameraShake")]
    public Vector2 m_NumberOfShakes;
    public float m_TestDuration;
    public Vector2 m_TestDistance;

    int m_LayerMask;

    public void InIt(Vector3 offSet, Transform player, Transform lookAtPos, Transform wantedPos1, Transform wantedPos2, Transform wantedPos3, Transform wantedPos4, Transform hoodLookAt, Transform LeftPos, Transform RightPos, Transform BackPos)
    {
        //will set all the base stuff
        base.InIt(offSet, player, lookAtPos, new GameObject().transform);
        
        m_CameraPos1 = wantedPos1;
        m_CameraPos2 = wantedPos2;
        m_CameraPos3 = wantedPos3;
        m_CameraPos4 = wantedPos4;

        m_DefaultLookAtPos = lookAtPos;
        m_HoodLookAtPos = hoodLookAt;

        ChangeView();

        m_Body = player.GetComponent<Rigidbody>();
        m_CameraBody = GetComponent<Rigidbody>();
        m_MainCamera.GetComponent<DepthOfField>().focalTransform = player;



        m_LeftView = LeftPos;
        m_RightView = RightPos;
        m_RearView = BackPos;

        //setting layer Mask
        m_LayerMask = 1 << 0 | 1 << 1 | 1 << 3 | 1 << 4 | 1 << 5 |
            1 << 6 | 1 << 7 | 1 << 8 | 1 << 9 | 1 << 10 | 1 << 11 | 1 << 12;
    }

    void Awake()
    {
        m_AffectedCamera = GetComponentInChildren<Camera>();
    }
    protected override void Update()
    {
        base.Update(); 

        //FOVBalst
        if (m_FOVBlastActive && m_Active)
        {
            m_FOVTotalTimePass += Time.deltaTime;

            if (m_FOVTotalTimePass < m_FOVTotalTime / 4f)
            {
                m_MainCamera.fieldOfView +=  (m_FOVChange / (m_FOVTotalTime  / 4f)) *Time.deltaTime;
            }

            else
            {
                m_MainCamera.fieldOfView -= (m_FOVChange / ((m_FOVTotalTime / 4f) * 3)) * Time.deltaTime;

                if (m_MainCamera.fieldOfView <= m_FOVBase)
                {
                    m_MainCamera.fieldOfView = m_FOVBase;
                    m_FOVBlastActive = false;
                }
            }
        }
    }
    protected override void FixedUpdate()
    {
        //if(Input.GetKeyDown(KeyCode.B))
        //{
        //    AddCameraShake(m_NumberOfShakes, m_TestDuration, m_TestDistance);
        //}

        if (m_Inisalized && m_IsActive)
        {
            if (m_LookBack)
            {
                if (m_AtPos == 4)
                {
                    m_AffectedCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    m_AffectedCamera.transform.localPosition = new Vector3(0, 0.15f, -2.0f);
                }

                else
                    m_WantedPosition.position = m_RearView.position;
            }

            else if (m_LookRight)
            {
                if (m_AtPos == 4)
                {
                    m_AffectedCamera.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    m_AffectedCamera.transform.localPosition = new Vector3(0, 0, 0.5f);
                }

                else
                    m_WantedPosition.position = m_RightView.position;
            }

            else if (m_LookLeft)
            {
                if (m_AtPos == 4)
                {
                    m_AffectedCamera.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    m_AffectedCamera.transform.localPosition = new Vector3(0, 0, 0.5f);
                }

                else
                    m_WantedPosition.position = m_LeftView.position;
            }

            else
            {
                if (m_AtPos == 4)
                {
                    m_AffectedCamera.transform.localRotation = Quaternion.identity;
                    m_AffectedCamera.transform.localPosition = Vector3.zero;
                }

                else
                {
                    m_WantedPosition.position = m_Player.position + m_OffSet + Vector3.Slerp(m_Body.transform.forward, m_Body.velocity.normalized, m_Body.velocity.magnitude / 30) * (m_DistanceFromCar);
                    
                }
            }

            if (m_AtPos != 4)
            {
                CameraLookAt();
                ClipCheck(m_LayerMask);
            }
                UpdateCameraVelocity();
                //lerping and looking
                CameraShake();
        }
    }

    public Vector3 threshHold;
    public float lerpSpeed1;
    public float lerpSpeed2;


    public void LookLeft()
    {
        m_LookLeft = true;
        m_LookAtPos = m_Player.transform;
    }

    public void LookRight()
    {
        m_LookRight = true;
        m_LookAtPos = m_Player.transform;
    }

    public void LookBack()
    {
        m_LookBack = true;
        m_LookAtPos = m_Player.transform;
    }

    public void LookForward()
    {
        m_LookBack = false;
        m_LookLeft = false;
        m_LookRight = false;
        m_LookAtPos = m_DefaultLookAtPos;
    }

    public void ChangeView()
    {
        m_AtPos--;

        if (m_AtPos > m_TotalPos)
        {
            m_AtPos = 1;
        }

        if (m_AtPos < 1)
            m_AtPos = m_TotalPos;

        switch (m_AtPos)
        {
            case 1:
                m_DistanceFromCar = m_CameraPos1.localPosition.z;
                m_OffSet = m_CameraPos1.localPosition;
                m_LookAtPos = m_DefaultLookAtPos;
                m_AffectedCamera.transform.parent = this.transform;
                m_AffectedCamera.transform.localPosition = Vector3.zero;
                m_AffectedCamera.transform.localRotation = Quaternion.identity;
                break;

            case 2:
                m_DistanceFromCar = m_CameraPos2.localPosition.z;
                m_OffSet = m_CameraPos2.localPosition;
                m_LookAtPos = m_DefaultLookAtPos;
                m_AffectedCamera.transform.parent = this.transform;
                m_AffectedCamera.transform.localPosition = Vector3.zero;
                m_AffectedCamera.transform.localRotation = Quaternion.identity;
                break;

            case 3:
                m_DistanceFromCar = m_CameraPos3.localPosition.z;
                m_OffSet = m_CameraPos3.localPosition;
                m_LookAtPos = m_DefaultLookAtPos;
                m_AffectedCamera.transform.parent = this.transform;
                m_AffectedCamera.transform.localPosition = Vector3.zero;
                m_AffectedCamera.transform.localRotation = Quaternion.identity;
                break;

            case 4:
                //m_DistanceFromCar = m_CameraPos4.localPosition.z;
                ////transform.position = m_CameraPos4.position;
                //m_OffSet = m_CameraPos4.localPosition;
                //m_LookAtPos = m_HoodLookAtPos;
                //m_MoveSensitivity *= 10;
                m_AffectedCamera.transform.parent = m_CameraPos4;
                m_AffectedCamera.transform.localPosition = Vector3.zero;
                m_AffectedCamera.transform.rotation = m_CameraPos4.rotation;
                break;

            default:
                print("Camera Out Of range");
                break;
        }

        m_OffSet.z = 0;

    }
    
    public void AddCameraFOVBlast(float FOVChange, float TotalTime)
    {
        m_FOVBase = m_MainCamera.fieldOfView;
        m_FOVChange = FOVChange;
        m_FOVBlastActive = true;
        m_FOVTotalTime = TotalTime;
        m_FOVTotalTimePass = 0;
    }

}
