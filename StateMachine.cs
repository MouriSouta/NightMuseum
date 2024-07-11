using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///�X�e�[�g�}�V���N���X
///class��`�̊��N���X�؂�o����
///</summary>
public class StateMachine<TOwner> : MonoBehaviour
{
    ///<summary>
    ///�X�e�[�g���N���X
    ///�X�e�[�g�N���X�͂��̃N���X���p��
    ///</summary>
    public abstract class StateBase
    {
        public StateMachine<TOwner> StateMachine;
        protected TOwner Owner => StateMachine.Owner;

        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnEnd() { }
    }
    protected TOwner Owner { get; }
    protected StateBase _currentState; //���݂̃X�e�[�g
    protected StateBase _prevState;    //�O�̃X�e�[�g
    private readonly Dictionary<int, StateBase> _states = new Dictionary<int, StateBase>();//�S�ẴX�e�[�g��`

    /// <summary>
    /// �R���X�g���N�^
    /// </summary>
    /// <param name="owner">StateMachine���g�p����Owner</param>
    public StateMachine(TOwner owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// �X�e�[�g��`�o�^
    /// �X�e�[�g�}�V����������ɂ��̃��\�b�h���Ă�
    /// </summary>
    /// <param name="stateId">�X�e�[�gID</param>
    /// <typeparam name="T">�X�e�[�g�^</typeparam>
    public void Add<T>(int stateId) where T : StateBase, new()
    {
        if (_states.ContainsKey(stateId))
        {
            Debug.LogError("already register stateId!! :" + stateId);
            return;
        }
        //�X�e�[�g��`��o�^
        var newState = new T
        {
            StateMachine = this
        };
        _states.Add(stateId, newState);
    }

    /// <summary>
    /// �X�e�[�g�J�n
    /// </summary>
    /// <param name="stateId">�X�e�[�gID</param>
    public void OnStart(int stateId)
    {
        if(!_states.TryGetValue(stateId, out var nextState))
        {
            Debug.LogError("not set stateId!! :" + stateId);
            return;
        }
        //���݂̃X�e�[�g�ɐݒ肵�ďC�����J�n
        _currentState = nextState;
        _currentState.OnStart();
    }

    /// <summary>
    /// �X�e�[�g�X�V����
    /// </summary>
    public void Onupdate()
    {
        _currentState.OnUpdate();
    }

    /// <summary>
    /// ���̃X�e�[�g�ɐ؂�ւ���
    /// </summary>
    /// <param name="stateId">�؂�ւ���X�e�[�gID</param>
    public void ChangeState(int stateId)
    {
        if(!_states.TryGetValue(stateId, out var nextState))
        {
            Debug.LogError("not set stateId!! : " + stateId);
            return;
        }
        //�O�̃X�e�[�g��ێ�
        _prevState = _currentState;
        //�X�e�[�g��؂�ւ���
        _currentState.OnEnd();
        _currentState = nextState;
        _currentState.OnStart();
    }

    public StateBase GetState(int StateID)
    {
        return _states[StateID];
    }

    public bool IsState(int StateID)
    {
        bool result = false;

        var a = GetState(StateID);

        if(_currentState == a)
        {
            result = true;
        }
        return result;
    }
}
