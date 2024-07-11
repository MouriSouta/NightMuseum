using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    //ˆÚ“®ˆ—
    [SerializeField]
    private Transform Target;
    NavMeshAgent agent;
    [SerializeField]
    private float moveSpeed;

    public enum Trigger 
    {
        Walk@@//ˆÚ“®
    }

    public Trigger cState;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //moveSpeed = agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, Target.position) < 10.0f)
        {
            agent.destination = Target.position;
            agent.speed = moveSpeed;
        }
    }
}
