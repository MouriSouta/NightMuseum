using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using State = StateMachine<EnemyBase>.StateBase;

public class EnemyBase :  MonoBehaviour
{
    //タイム関係
    [SerializeField]
    private float AttackingTime;
    public float e_AttackingTime { get => AttackingTime; set => AttackingTime = value; }
    [SerializeField]
    private float DestroyTime;

    //血のエフェクト
    [SerializeField]
    public GameObject m_Blood;

    [SerializeField]
    public GameObject m_BloodPoint;

    //Anime
    private Animator m_Animator;
    public Animator e_Animator => m_Animator;

    //Enemyスピード
    private Transform m_Target;
    public Transform Target { get => m_Target; private set => m_Target = value; }
    NavMeshAgent agent;
    public NavMeshAgent Agent => agent;
    [SerializeField]
    private float moveSpeed;
    public float Movespeed => moveSpeed;

    //Attack関係
    [HideInInspector]
    public GameObject m_AttackTarget;
    public GameObject A_Target => m_AttackTarget;
    [SerializeField]
    public float m_AttackPoint;
    public float A_Point => m_AttackPoint;

    /// <summary>
    /// ステート定義
    /// </summary>
    public enum StateType
    {
        Idle,  //停止状態
        Walk,　//移動状態
        Attack, //攻撃状態
        Knock, //ノックバック状態
        Death,  //死亡状態
    }
    private StateMachine<EnemyBase> _stateMachine;

    // Start is called before the first frame update
    void Start()
    {
        Target = GameObject.FindGameObjectWithTag(StringDate.Player).transform;
        agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();

        //ステートマシン定義
        _stateMachine = new StateMachine<EnemyBase>(this);
        _stateMachine.Add<StateIdle>((int)StateType.Idle);
        _stateMachine.Add<StateIdle>((int)StateType.Knock);
        _stateMachine.Add<StateWalk>((int)StateType.Walk);
        _stateMachine.Add<StateAttack>((int)StateType.Attack);
        //_stateMachine.Add<StateDamage>((int)StateType.Damage);
        
        //ステート開始
        _stateMachine.OnStart((int)StateType.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        //ステート更新
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

    //死亡時のスクリプトに移行
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

    //ゾンビの声再生
    public void ZombieVoice()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieVoice, gameObject, seObjectIndex:2);
    }

    //歩くアニメーション再生
    public void Move1()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieMove1, gameObject, seObjectIndex:2);
    }

    public void Move2()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieMovw2, gameObject);
    }
}

//各ステート処理
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