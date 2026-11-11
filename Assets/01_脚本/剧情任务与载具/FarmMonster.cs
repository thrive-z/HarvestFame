using UnityEngine;

public sealed class FarmMonster : MonoBehaviour
{
    private enum State { Patrol, Chase, Windup, Recover, Stagger, Defeated }

    [SerializeField] private float maximumHealth = 34f;
    [SerializeField] private float damage = 13f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float noticeDistance = 8f;
    [SerializeField] private float patrolRadius = 2.5f;
    [SerializeField] private Transform body;

    private FarmCampaign campaign;
    private Vector3 home;
    private State state;
    private float health;
    private float nextState;
    private int patrolPoint;
    private float patrolPause;
    private Renderer[] renderers;
    private MaterialPropertyBlock tint;
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public bool IsDefeated => state == State.Defeated;
    public float HealthFraction => health / maximumHealth;
    public string Name => gameObject.name;

    public void Initialize(FarmCampaign owner)
    {
        campaign = owner;
        home = transform.position;
        health = maximumHealth;
        renderers = body.GetComponentsInChildren<Renderer>();
        tint = new MaterialPropertyBlock();
    }

    public void Restore(bool defeated)
    {
        health = defeated ? 0f : maximumHealth;
        state = defeated ? State.Defeated : State.Patrol;
        transform.position = home;
        gameObject.SetActive(!defeated);
    }

    public bool Hurt(float amount)
    {
        if (IsDefeated || campaign == null || !campaign.CombatUnlocked)
            return false;

        var previousHealth = health;
        health = Mathf.Max(0f, health - amount);
        if (health <= 0f)
        {
            state = State.Defeated;
            campaign.Report("kills");
            campaign.ReportCombat($"击败了{Name}");
            gameObject.SetActive(false);
            return true;
        }

        SetState(State.Stagger, 0.30f);
        campaign.PlaySound("MonsterHurt");
        campaign.ReportCombat($"命中{Name} · 伤害 {Mathf.CeilToInt(previousHealth - health)}");
        return true;
    }

    private void Update()
    {
        if (campaign == null || !campaign.IsRunning || campaign.BlocksWorldInput)
            return;

        var player = Player.Instance.transform;
        var delta = player.position - transform.position;
        delta.y = 0f;
        var distance = delta.magnitude;
        var insideOrchard = campaign.CombatUnlocked && player.position.z > 36f
            && player.position.x > 2f && Vector3.Distance(home, player.position) < 13f && !campaign.IsDriving;
        if (!insideOrchard && state != State.Patrol)
            SetState(State.Patrol);

        switch (state)
        {
            case State.Patrol:
                if (insideOrchard && distance < noticeDistance)
                {
                    SetState(State.Chase);
                    break;
                }
                Patrol();
                break;
            case State.Chase:
                if (distance < 1.75f)
                    SetState(State.Windup, 0.58f);
                else
                    Walk(player.position, chaseSpeed);
                break;
            case State.Windup:
                Face(delta);
                if (Time.time < nextState)
                    break;
                if (distance < 2.05f)
                    campaign.HurtPlayer(damage);
                SetState(State.Recover, 0.75f);
                break;
            case State.Recover:
            case State.Stagger:
                if (Time.time >= nextState)
                    SetState(State.Chase);
                break;
        }

        var hop = state == State.Patrol ? 3f : 6f;
        body.localPosition = Vector3.up * (Mathf.Abs(Mathf.Sin(Time.time * hop)) * 0.10f);
    }

    private void Patrol()
    {
        if (Time.time < patrolPause)
            return;
        var angle = patrolPoint * Mathf.PI / 2f;
        var destination = home + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * patrolRadius;
        if ((destination - transform.position).sqrMagnitude < 0.09f)
        {
            patrolPoint = (patrolPoint + 1) % 4;
            patrolPause = Time.time + 0.8f;
        }
        else
            Walk(destination, 1.2f);
    }

    private void Walk(Vector3 destination, float speed)
    {
        destination.y = home.y;
        Face(destination - transform.position);
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
    }

    private void Face(Vector3 delta)
    {
        if (delta.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), 9f * Time.deltaTime);
    }

    private void SetState(State next, float duration = 0f)
    {
        state = next;
        nextState = Time.time + duration;
        if (next == State.Windup || next == State.Stagger)
        {
            tint.SetColor(ColorId, next == State.Windup ? new Color(0.95f, 0.23f, 0.12f) : Color.white);
            if (renderers.Length > 0)
                renderers[0].SetPropertyBlock(tint);
        }
        else if (renderers.Length > 0)
            renderers[0].SetPropertyBlock(null);
    }
}
