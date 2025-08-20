using UnityEngine;

public abstract class State
{
    protected float time { get; set; }
    protected float fixedtime { get; set; }
    protected float latetime { get; set; }

    public StateMachine stateMachine;

    public virtual void OnEnter(StateMachine _stateMachine)
    {
        stateMachine = _stateMachine;
    }


    public virtual void OnUpdate()
    {
        time += Time.deltaTime;
    }

    public virtual void OnFixedUpdate()
    {
        fixedtime += Time.deltaTime;
    }
    public virtual void OnLateUpdate()
    {
        latetime += Time.deltaTime;
    }

    public virtual void OnExit()
    {

    }
}