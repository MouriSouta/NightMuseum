using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : MonoBehaviour
{
    [SerializeField]
    private int damage;

    private Player m_Player;

    [SerializeField]
    private SphereCollider m_Collider;

    [SerializeField]
    private LayerMask m_DitectionLayer;
    private void Awake()
    {
        m_Player = GetComponent<Player>();
    }

    void Attack()
    {
        Vector3 center = m_Collider.transform.TransformPoint(m_Collider.center);

        Collider[] hitColliders = Physics.OverlapSphere( center, m_Collider.radius, m_DitectionLayer);
        foreach (var hitCollider in hitColliders)
        {
            var damageble = hitCollider.GetComponent<Health>();
            if(damageble != null)
            {
                //HitStop
                AudioSystem.instance.PlaySEFromObjectPosition(StringDate.PAttackSE, gameObject);
                m_Player.GetItem().HitStop();
                damageble.Damage(damage);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //AttackCollider‚Ì”ÍˆÍ•\Ž¦
        Gizmos.color = Color.blue;
        Vector3 center = m_Collider.transform.TransformPoint(m_Collider.center);
        Gizmos.DrawWireSphere(center, m_Collider.radius);
    }
#endif
}
