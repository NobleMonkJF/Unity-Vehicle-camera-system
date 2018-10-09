using UnityEngine;
using System.Collections;

public class JetCameraSystem : CameraSystem
{
    JetController m_JetController;

    public override void InIt(Vector3 offSet, Transform player, Transform lookAtPos, Transform wantedPos)
    {
        base.InIt(offSet, player, lookAtPos, wantedPos);

        m_JetController = player.GetComponentInParent<JetController>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        if (m_Inisalized && m_IsActive)
        {
            //lerping and looking
            UpdateCameraVelocity();
            CameraShake();
            CameraLookAt();

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,  transform.rotation.eulerAngles.y, m_JetController.m_Rotation.z);
        }
    }
    
}
