using UnityEngine;
using System.Collections.Generic;

public class EnemyStateMachine : MonoBehaviour
{
    private Dictionary<EnemyStateType, EnemyState> states;
    private EnemyState currentState;
    private EnemyController controller;

    public EnemyStateType CurrentStateType { get; private set; }
    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
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
        Debug.Log($"[EnemyStateMachine] ChangeState: {CurrentStateType} -> {newStateType}");
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
        Debug.Log($"[EnemyStateMachine] Update called for: {controller.name} | currentState: {(currentState != null ? currentState.GetType().Name : "null")}");
        if (currentState != null && controller && !controller.isDead)
        {
            currentState.OnUpdate(controller);
        }
    }
}