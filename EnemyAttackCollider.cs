using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    [SerializeField]
    private int m_Damage;

    [SerializeField]
    private SphereCollider m_Collider;

    [SerializeField]
    private LayerMask m_DitectionLayer;
    
    void EnemyAttack()
    {
        Vector3 center = m_Collider.transform.TransformPoint(m_Collider.center);

        Collider[] hitColliders = Physics.OverlapSphere(center, m_Collider.radius, m_DitectionLayer);
        foreach(var hitCollider in hitColliders)
        {
            var damageble = hitCollider.GetComponent<Health>();
            if(damageble != null)
            {
                damageble.Damage(m_Damage);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 center = m_Collider.transform.TransformPoint(m_Collider.center);
        Gizmos.DrawWireSphere(center, m_Collider.radius);
    }
#endif
}
