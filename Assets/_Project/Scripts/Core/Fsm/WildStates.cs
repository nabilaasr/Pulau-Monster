using UnityEngine;

public class WanderState : IState
{
    private readonly WildMonster monster;
    private Vector2 wanderTarget;
    private float timer;

    public WanderState(WildMonster wild) => monster = wild;

    public void Enter() => PickNewTarget();

    public void Tick(float deltaTime)
    {
        if (monster.CanSeeTarget)
        {
            monster.ChangeState(monster.ChaseState);
            return;
        }

        timer -= deltaTime;
        if (timer <= 0f)
        {
            PickNewTarget();
        }

        Vector2 current = monster.Position;
        Vector2 diff = wanderTarget - current;
        if (diff.sqrMagnitude > 0.05f)
        {
            monster.DesiredVelocity = diff.normalized * 1.125f;
        }
        else
        {
            monster.DesiredVelocity = Vector2.zero;
        }
    }

    public void Exit() => monster.DesiredVelocity = Vector2.zero;

    private void PickNewTarget()
    {
        timer = Random.Range(2f, 4f);
        if (Random.value < 0.4f)
        {
            // Idle standing
            wanderTarget = monster.Position;
        }
        else
        {
            Vector2 offset = Random.insideUnitCircle * 2.5f;
            wanderTarget = monster.HomePosition + offset;
        }
    }
}

public class ChaseState : IState
{
    private readonly WildMonster monster;

    public ChaseState(WildMonster wild) => monster = wild;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (monster.LostTarget)
        {
            monster.ChangeState(monster.ReturnState);
            return;
        }

        Vector2 diff = monster.TargetPosition - monster.Position;
        if (diff.sqrMagnitude > 0.1f)
        {
            monster.DesiredVelocity = diff.normalized * 2.5f;
        }
        else
        {
            monster.DesiredVelocity = Vector2.zero;
        }
    }

    public void Exit() => monster.DesiredVelocity = Vector2.zero;
}

public class ReturnState : IState
{
    private readonly WildMonster monster;

    public ReturnState(WildMonster wild) => monster = wild;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (monster.CanSeeTarget)
        {
            monster.ChangeState(monster.ChaseState);
            return;
        }

        Vector2 diff = monster.HomePosition - monster.Position;
        if (diff.sqrMagnitude > 0.1f)
        {
            monster.DesiredVelocity = diff.normalized * 1.5f;
        }
        else
        {
            monster.ChangeState(monster.WanderState);
        }
    }

    public void Exit() => monster.DesiredVelocity = Vector2.zero;
}
