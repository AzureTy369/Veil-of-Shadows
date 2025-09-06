using UnityEngine;

public abstract class EnemyState
{
    public abstract void OnEnter(EnemyController controller);
    public abstract void OnUpdate(EnemyController controller);
    public abstract void OnExit(EnemyController controller);
}