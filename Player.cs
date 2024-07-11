using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private PlayerAnimation m_Animator;

    private PlayerHealth m_PlayerHealth;

    //�^�C���֌W
    [SerializeField]
    public float AttackTime;

    //InputSystem
    public PlayerAct m_Playeract;

    //�n�ʂɐڒn���Ă��邩
    [SerializeField]
    private bool m_IsGrounded;
    //�O���̕ǂɏՓ˂��Ă��邩�ǂ���
    [SerializeField]
    private bool m_IsCollision;
    //Rigdbody
    private Rigidbody rigid;

    [SerializeField]
    private float m_InputHorizontal;
    [SerializeField]
    private float m_InputVertical;

    //�ړ����x
    [SerializeField]
    private Vector3 inputValue;
    //���͒l
    private Vector3 velocity;
    //��������
    [SerializeField]
    private float m_WalkSpeed = 0.1f;

    //�i�������鏈��
    //�O���ɒi�������邩���ׂ郌�C
    [SerializeField]
    private Vector3 stepRayOffset = new Vector3(0f, 0.05f, 0f);
    //���C���΂�����
    [SerializeField]
    private float stepDistance = 0.5f;
    //�����i��
    [SerializeField]
    private float stepOffset = 0.3f;
    //�����p�x
    [SerializeField]
    private float slopeLimit;
    //�����i���̈ʒu�����΂����C
    [SerializeField]
    private float slopeDistance = 0.6f;

    //�J�����n
    [SerializeField]
    private Quaternion m_TargetRotation;
    [SerializeField]
    private float m_RotationSpeed;

    //��΂����C�̃I�t�Z�b�g
    [SerializeField]
    private Vector3 m_RayOffset;
    //�n�ʂƂ̋���
    [SerializeField]
    private float m_GroundMinDistance = 0.5f;

    //�A�C�e���֌W
    [SerializeField]
    private bool m_Item;
    //�J�M
    private bool m_GetKey;

    private GameObject HoldItem;
    private Item m_ItemScript;

    [SerializeField]
    private GameObject Root;

    [SerializeField]
    private float CGOTime;

    public enum Trigger
    {
        Idle,   //�������Ȃ�
        Attack, //�U��
        Damage, //�_���[�W���
        Death   //���S���
    }

    public Trigger cState;

    private void Awake()
    {
        this.inputValue = Vector3.zero;
    }

    private void OnEnable()
    {
        m_Playeract = new();

        m_Playeract.Player.Walk.started += OnWalk; //�������Ƃ�
        m_Playeract.Player.Walk.performed += OnWalk; //�l���ύX���ꂽ�Ƃ�(���������Ă�Ԃł͂Ȃ�)
        m_Playeract.Player.Walk.canceled += OnWalk; //�l�̕ύX�������Ȃ����Ƃ�

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


    //�{�^������
    private void OnWalk(InputAction.CallbackContext callback)
    {
        inputValue = callback.ReadValue<Vector2>();
        m_Animator.SetFloat(StringDate.Walk, inputValue.magnitude, true);
    }

    // Update is called once per frame
    void Update()
    {
        var stepRayPosition = rigid.position + stepRayOffset;

        //�X�e�b�v�p�̃��C���n�ʂɐڐG���Ă��邩�ǂ���
        if (Physics.Linecast(stepRayPosition, stepRayPosition + rigid.transform.forward
            * stepDistance, out var stepHit))
        {
            //�i�s�����̒n�ʂ̊p�x���w��ȉ��A�܂��͏����i����艺�������ꍇ�̈ړ�����
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
                //�w�肵�������ɓ��Ă͂܂�Ȃ��ꍇ�n���x���O�ɂ���
                velocity = Vector3.zero;
            }
        }
        else //�O���̕ǂɐڐG���Ă��Ȃ����
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

    //Idle���
    private void executeIdle()
    {
        InputWalk();
        GroundCheck();

        //���͒l
        velocity = new Vector3(inputValue.x, 0, inputValue.y);

        if (m_IsGrounded)
        {
            if (velocity.magnitude > 0)
            {
                // cState = Trigger.Walk;
            }

        }

        //�ڐG���Ă�����ړ������̒l��0�ɂ���
        if (!m_IsGrounded && m_IsCollision)
        {
            inputValue = new Vector3(0f, inputValue.y, 0f);
        }
    }

    #endregion

    #region Move

    //Walk���
    private void InputWalk()
    {
        if (m_PlayerHealth.GetHealth() <= 0) return;

        if (inputValue.magnitude == 0) return;
        if (GameManeger.instance.GetPlayerStop()) return;
        //�O�i
        MoveForward();

        //���͒l
        velocity = new Vector3(inputValue.x, 0f, inputValue.y);

        //�J�����̕������琳�ʂ��v�Z����
        Quaternion horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);

        LookTargetRotation(horizontalRotation, velocity);

        //�ړ����x�v�Z
        var clampedVelocity = Vector3.ClampMagnitude(velocity, 1f);
    }

    //�O�i�����鏈��
    public void MoveForward()
    {
        //�v���C���[��O�ɂ����ړ������鏈��
        rigid.MovePosition(transform.position + (m_WalkSpeed * Time.fixedDeltaTime * transform.forward));
    }

    //�J�����̉�]����
    private void LookTargetRotation(Quaternion horizontalRotation, Vector3 velocity)
    {
        Vector3 rotationVelocity = horizontalRotation * velocity.normalized;
        float rotationSpeed = m_RotationSpeed * Time.fixedDeltaTime;
        if (rotationVelocity.magnitude > 0.3f)
            m_TargetRotation = Quaternion.LookRotation(rotationVelocity, Vector3.up);

        //���X�ɉ�]������
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
    //Item�֌W
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
