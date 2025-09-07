// EnemyState.cs
using UnityEngine;

public abstract class EnemyState
{
    public abstract void OnEnter(EnemyBase controller);
    public abstract void OnUpdate(EnemyBase controller);
    public abstract void OnExit(EnemyBase controller);
}