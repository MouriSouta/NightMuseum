using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private PlayerAnimation m_Animator;

    private PlayerHealth m_PlayerHealth;

    //タイム関係
    [SerializeField]
    public float AttackTime;

    //InputSystem
    public PlayerAct m_Playeract;

    //地面に接地しているか
    [SerializeField]
    private bool m_IsGrounded;
    //前方の壁に衝突しているかどうか
    [SerializeField]
    private bool m_IsCollision;
    //Rigdbody
    private Rigidbody rigid;

    [SerializeField]
    private float m_InputHorizontal;
    [SerializeField]
    private float m_InputVertical;

    //移動速度
    [SerializeField]
    private Vector3 inputValue;
    //入力値
    private Vector3 velocity;
    //歩く速さ
    [SerializeField]
    private float m_WalkSpeed = 0.1f;

    //段差を昇る処理
    //前方に段差があるか調べるレイ
    [SerializeField]
    private Vector3 stepRayOffset = new Vector3(0f, 0.05f, 0f);
    //レイを飛ばす距離
    [SerializeField]
    private float stepDistance = 0.5f;
    //昇れる段差
    [SerializeField]
    private float stepOffset = 0.3f;
    //昇れる角度
    [SerializeField]
    private float slopeLimit;
    //昇れる段差の位置から飛ばすレイ
    [SerializeField]
    private float slopeDistance = 0.6f;

    //カメラ系
    [SerializeField]
    private Quaternion m_TargetRotation;
    [SerializeField]
    private float m_RotationSpeed;

    //飛ばすレイのオフセット
    [SerializeField]
    private Vector3 m_RayOffset;
    //地面との距離
    [SerializeField]
    private float m_GroundMinDistance = 0.5f;

    //アイテム関係
    [SerializeField]
    private bool m_Item;
    //カギ
    private bool m_GetKey;

    private GameObject HoldItem;
    private Item m_ItemScript;

    [SerializeField]
    private GameObject Root;

    [SerializeField]
    private float CGOTime;

    public enum Trigger
    {
        Idle,   //何もしない
        Attack, //攻撃
        Damage, //ダメージ状態
        Death   //死亡状態
    }

    public Trigger cState;

    private void Awake()
    {
        this.inputValue = Vector3.zero;
    }

    private void OnEnable()
    {
        m_Playeract = new();

        m_Playeract.Player.Walk.started += OnWalk; //押したとき
        m_Playeract.Player.Walk.performed += OnWalk; //値が変更されたとき(押し続けてる間ではない)
        m_Playeract.Player.Walk.canceled += OnWalk; //値の変更が無くなったとき

        m_Playeract.Player.Attack.started += OnAttack;

        m_Playeract.Player.Walk.canceled += OnIdleStarted;

        m_Playeract?.Enable();
    }

    private void OnDisable()
    {
        m_Playeract?.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_IsGrounded = true;
        m_Animator = GetComponent<PlayerAnimation>();
        m_PlayerHealth = GetComponent<PlayerHealth>();
        rigid = GetComponent<Rigidbody>();
    }

    private void ChangeIdle()
    {
        m_Animator.AnimExit();
        if (m_Item)
        {
            m_Animator.SetInt(StringDate.State, 1, true);
        }
        else
        {
            m_Animator.SetInt(StringDate.State, 0, true);          
        }
        cState = Trigger.Idle;

    }

    public void OnIdleStarted(InputAction.CallbackContext callback)
    {
        if (cState == Trigger.Death) return;
    }

    private void OnAttack(InputAction.CallbackContext callback)
    {
        if (m_Item && cState != Trigger.Attack)
        {
            m_Animator.SetTrigger(StringDate.Attack);
            cState = Trigger.Attack;
        }
    }


    //ボタン所得
    private void OnWalk(InputAction.CallbackContext callback)
    {
        inputValue = callback.ReadValue<Vector2>();
        m_Animator.SetFloat(StringDate.Walk, inputValue.magnitude, true);
    }

    // Update is called once per frame
    void Update()
    {
        var stepRayPosition = rigid.position + stepRayOffset;

        //ステップ用のレイが地面に接触しているかどうか
        if (Physics.Linecast(stepRayPosition, stepRayPosition + rigid.transform.forward
            * stepDistance, out var stepHit))
        {
            //進行方向の地面の角度が指定以下、または昇れる段差より下だった場合の移動処理
            if (Vector3.Angle(rigid.transform.up, stepHit.normal) <= slopeLimit
                || (Vector3.Angle(rigid.transform.up, stepHit.normal) > slopeLimit
                && !Physics.Linecast(rigid.position + new Vector3(0f, stepOffset, 0f),
                rigid.position + rigid.transform.forward * slopeDistance)))
            {
                velocity = new Vector3(0f, (Quaternion.FromToRotation(Vector3.up, stepHit.normal)
                    * rigid.transform.forward * m_WalkSpeed).y, 0f)
                    + rigid.transform.forward * m_WalkSpeed;
            }
            else
            {
                //指定した条件に当てはまらない場合ハ速度を０にする
                velocity = Vector3.zero;
            }
        }
        else //前方の壁に接触していなければ
        {
            velocity = transform.forward * m_WalkSpeed;
        }

    }

    private void FixedUpdate()
    {
        switch (cState)
        {
            case Trigger.Idle:
                executeIdle();
                break;
            case Trigger.Attack:
                executeAttack();
                break;

            default: break;
        }
    }

    #region Idle

    //Idle状態
    private void executeIdle()
    {
        InputWalk();
        GroundCheck();

        //入力値
        velocity = new Vector3(inputValue.x, 0, inputValue.y);

        if (m_IsGrounded)
        {
            if (velocity.magnitude > 0)
            {
                // cState = Trigger.Walk;
            }

        }

        //接触していたら移動方向の値は0にする
        if (!m_IsGrounded && m_IsCollision)
        {
            inputValue = new Vector3(0f, inputValue.y, 0f);
        }
    }

    #endregion

    #region Move

    //Walk状態
    private void InputWalk()
    {
        if (m_PlayerHealth.GetHealth() <= 0) return;

        if (inputValue.magnitude == 0) return;
        if (GameManeger.instance.GetPlayerStop()) return;
        //前進
        MoveForward();

        //入力値
        velocity = new Vector3(inputValue.x, 0f, inputValue.y);

        //カメラの方向から正面を計算する
        Quaternion horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);

        LookTargetRotation(horizontalRotation, velocity);

        //移動速度計算
        var clampedVelocity = Vector3.ClampMagnitude(velocity, 1f);
    }

    //前進させる処理
    public void MoveForward()
    {
        //プレイヤーを前にだけ移動させる処理
        rigid.MovePosition(transform.position + (m_WalkSpeed * Time.fixedDeltaTime * transform.forward));
    }

    //カメラの回転処理
    private void LookTargetRotation(Quaternion horizontalRotation, Vector3 velocity)
    {
        Vector3 rotationVelocity = horizontalRotation * velocity.normalized;
        float rotationSpeed = m_RotationSpeed * Time.fixedDeltaTime;
        if (rotationVelocity.magnitude > 0.3f)
            m_TargetRotation = Quaternion.LookRotation(rotationVelocity, Vector3.up);

        //徐々に回転させる
        transform.rotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, rotationSpeed);
    }

    #endregion

    #region Attack

    private void executeAttack()
    {
    }

    #endregion

    private void GroundCheck()
    {
        Ray ray = new(transform.position + m_RayOffset, Vector3.down);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, 1f);
        if (raycastHits.Length > 0)
        {
            foreach (RaycastHit hit in raycastHits)
            {
                float dis = Vector3.Distance(hit.point, transform.position);
                if (dis < m_GroundMinDistance)
                {
                    m_IsGrounded = true;
                    return;
                }
                else continue;
            }
        }
    }

    #region Item
    //Item関係
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.tag == StringDate.Item)
        {
            if (m_Item)
            {
                ItemManeger.instance.NewWepoan();
                Destroy(HoldItem);
            }
            m_ItemScript = other.transform.root.GetComponent<Item>();
            HoldItem = m_ItemScript.passItem();
            HoldItem = Instantiate(HoldItem, Root.transform);
            HoldItem.transform.localPosition = Vector3.zero;
            m_Item = true;
            return;
        }

        if(other.CompareTag(StringDate.Key))
        {
            AudioSystem.instance.PlaySEFromObjectPosition(StringDate.GetKeySE, gameObject);
            m_GetKey = true;
        }
    }

    public bool ClearFrag()
        => m_GetKey;
    public Item GetItem() => m_ItemScript;

    #endregion

    public void Damage()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.ZombieAttackSE, gameObject);
        if(m_PlayerHealth.GetHealth() == 20)
        {
           AudioSystem.instance.PlaySEFromObjectPosition(StringDate.PlayerDamage, gameObject);
        }
        else if(m_PlayerHealth.GetHealth() == 10)
        {
           AudioSystem.instance.PlaySEFromObjectPosition(StringDate.PlayerDamage2, gameObject);
        }
        m_Animator.SetTrigger(StringDate.Damage);
    }

    #region AttackVoice
    private void AttackVoice()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.PAttackVoice, gameObject);
    }

    private void AttackVoice2()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.PAttackVoice2, gameObject);
    }
    #endregion

    #region AnimetionEnd
    private void AttackEnd()
    {
        ChangeIdle();
    }

    private void DamageEnd()
    {
        ChangeIdle();
    }

    public void WalkEnd()
    {
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.WalkSE,gameObject);
    }
    #endregion

    public void Death()
    {
        StartCoroutine(ChangeGameOver());
    }

    private IEnumerator ChangeGameOver()
    {
        cState = Trigger.Death;

        inputValue = new Vector3(0f, inputValue.y, 0f);
        AudioSystem.instance.PlaySEFromObjectPosition(StringDate.PlayerDeath, gameObject);
        m_Animator.SetTrigger(StringDate.Death,true);

        yield return new WaitForSeconds(CGOTime);
        SceneManagment.instance.GameOverScene();
    }
}
