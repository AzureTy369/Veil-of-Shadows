// EnemyStateMachine.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemyStateMachine : MonoBehaviour
{
    private Dictionary<EnemyStateType, EnemyState> states;
    private EnemyState currentState;
    private EnemyBase controller; // Change to EnemyBase

    public EnemyStateType CurrentStateType { get; private set; }
    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        controller = GetComponent<EnemyBase>();
        InitializeStates();
    }

    private void InitializeStates()
    {
        states = new Dictionary<EnemyStateType, EnemyState>
        {
            { EnemyStateType.Idle, new EnemyIdleState() },
            { EnemyStateType.Patrol, new EnemyPatrolState() },
            { EnemyStateType.Chase, new EnemyChaseState() },
            { EnemyStateType.Attack, new EnemyAttackState() },
            { EnemyStateType.Die, new EnemyDieState() },
            { EnemyStateType.Stunned, new EnemyStunnedState() }
        };
    }

    public void ChangeState(EnemyStateType newStateType)
    {
        // Giữ lại debug quan trọng khi chuyển trạng thái
        if (CurrentStateType == newStateType && currentState != null) return;
        if (currentState != null)
        {
            currentState.OnExit(controller);
        }
        CurrentStateType = newStateType;
        currentState = states[newStateType];
        if (currentState != null)
        {
            currentState.OnEnter(controller);
        }
    }

    private void Update()
    {
        if (controller == null)
        {
            Debug.LogError("[EnemyStateMachine] controller bị null! Hãy chắc chắn GameObject này có component EnemyBase (GroundEnemy hoặc FlyingEnemy).");
            return;
        }
        if (currentState != null && !controller.isDead)
        {
            currentState.OnUpdate(controller);
        }
    }
}