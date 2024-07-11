using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using State = StateMachine<EnemyBase>.StateBase;

public class EnemyBase :  MonoBehaviour
{
    //�^�C���֌W
    [SerializeField]
    private float AttackingTime;
    public float e_AttackingTime { get => AttackingTime; set => AttackingTime = value; }
    [SerializeField]
    private float DestroyTime;

    //���̃G�t�F�N�g
    [SerializeField]
    public GameObject m_Blood;

    [SerializeField]
    public GameObject m_BloodPoint;

    //Anime
    private Animator m_Animator;
    public Animator e_Animator => m_Animator;

    //Enemy�X�s�[�h
    private Transform m_Target;
    public Transform Target { get => m_Target; private set => m_Target = value; }
    NavMeshAgent agent;
    public NavMeshAgent Agent => agent;
    [SerializeField]
    private float moveSpeed;
    public float Movespeed => moveSpeed;

    //Attack�֌W
    [HideInInspector]
    public GameObject m_AttackTarget;
    public GameObject A_Target => m_AttackTarget;
    [SerializeField]
    public float m_AttackPoint;
    public float A_Point => m_AttackPoint;

    /// <summary>
    /// �X�e�[�g��`
    /// </summary>
    public enum StateType
    {
        Idle,  //��~���
        Walk,�@//�ړ����
        Attack, //�U�����
        Knock, //�m�b�N�o�b�N���
        Death,  //���S���
    }
    private StateMachine<EnemyBase> _stateMachine;

    // Start is called before the first frame update
    void Start()
    {
        Target = GameObject.FindGameObjectWithTag(StringDate.Player).transform;
        agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        //�X�e�[�g�}�V����`
        _stateMachine = new StateMachine<EnemyBase>(this);
        _stateMachine.Add<StateIdle>((int)StateType.Idle);
        _stateMachine.Add<StateIdle>((int)StateType.Knock);
        _stateMachine.Add<StateWalk>((int)StateType.Walk);
        _stateMachine.Add<StateAttack>((int)StateType.Attack);
        //_stateMachine.Add<StateDamage>((int)StateType.Damage);
        
        //�X�e�[�g�J�n
        _stateMachine.OnStart((int)StateType.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        //�X�e�[�g�X�V
        _stateMachine.Onupdate();
    }

    public void DamageAnim()
    {
        if (!_stateMachine.IsState((int)EnemyBase.StateType.Knock))
        {
            e_Animator.SetTrigger(StringDate.Knock);
            AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieDamageVoice, gameObject);
            BloodOut(m_Blood);
        }
        e_AttackingTime = 0;
    }

    //���S���̃X�N���v�g�Ɉڍs
    public void Death()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieDeathSE, gameObject);
        agent.isStopped = true;
        GameManeger.instance.EnemyDethCount--;
        e_Animator.SetTrigger(StringDate.Death);
        BloodOut(m_Blood);
        Destroy(gameObject, DestroyTime);
    }

    private void BloodOut(GameObject blood)
    {
        Instantiate(blood, m_BloodPoint.transform.position, Quaternion.identity);
    }

    //�]���r�̐��Đ�
    public void ZombieVoice()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieVoice, gameObject, seObjectIndex:2);
    }

    //�����A�j���[�V�����Đ�
    public void Move1()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieMove1, gameObject, seObjectIndex:2);
    }

    public void Move2()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieMovw2, gameObject);
    }
}

//�e�X�e�[�g����
//----Idle----
public class StateIdle : State
{
    public override void OnStart()
    {
        Owner.e_Animator.SetInteger(StringDate.State, (int)EnemyBase.StateType.Idle);
    }

    public override void OnUpdate()
    {
        if(Mathf.Abs(Owner.transform.position.y - Owner.Target.position.y) > 2.0f)
        {
            return;
        }
        if (Vector3.Distance(Owner.transform.position, Owner.Target.position) < 5.0f)
        {
            StateMachine.ChangeState((int)EnemyBase.StateType.Walk);
        }
    }
}

//----Walk----
public class StateWalk : State
{
    public override void OnStart()
    {
        Owner.e_Animator.SetInteger(StringDate.State, (int)EnemyBase.StateType.Walk);
        Owner.Agent.SetDestination(Owner.Target.position);
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(Owner.transform.position, Owner.Target.position) < 5.0f)
        {
            Owner.Agent.destination = Owner.Target.position;
            Owner.Agent.speed = Owner.Movespeed;
        }
        else
        {
            StateMachine.ChangeState((int)EnemyBase.StateType.Idle);
        }

        if (Vector3.Distance(Owner.transform.position, Owner.Target.position) < 1.0f)
        {
            StateMachine.ChangeState((int)EnemyBase.StateType.Attack);
            Owner.Agent.speed = 0;
        }
    }

    public override void OnEnd()
    {
        base.OnEnd();
    }
}

//----Attack----
public class StateAttack : State
{
    public override void OnStart()
    {
        Owner.e_Animator.SetInteger(StringDate.State, (int)EnemyBase.StateType.Attack);
    }

    public override void OnUpdate()
    {
        Owner.e_AttackingTime += Time.deltaTime;
        if(Owner.e_AttackingTime >= 3.0f)
        {
            StateMachine.ChangeState((int)EnemyBase.StateType.Idle);
            Owner.e_AttackingTime = 0f;
        }
    }
}