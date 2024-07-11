using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField]
    int damage = 10;

    [SerializeField]
    private LayerMask m_DitectionLayer;

    [SerializeField]
    private Vector3 m_interpolationPosition;
    [SerializeField]
    private float m_Radius;

    void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + m_interpolationPosition, m_Radius, m_DitectionLayer);
        foreach(var hitCollider in hitColliders)
        {
            var damageble = hitCollider.GetComponent<Health>();
            if(damageble != null)
            {
                damageble.Damage(damage);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + m_interpolationPosition, m_Radius);
    }
#endif
}
