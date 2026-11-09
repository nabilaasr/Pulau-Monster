using System.Collections;
using UnityEngine;

public class WildMonster : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private int minLevel = 2;
    [SerializeField] private int maxLevel = 4;
    [SerializeField] private bool stationary;
    [SerializeField] private bool isBoss;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject alert;

    private StateMachine fsm;
    private WanderState wanderState;
    private ChaseState chaseState;
    private ReturnState returnState;

    private Vector2 homePosition;
    private float cooldown;
    private bool inBattle;
    private bool isDefeated;
    private float respawnTimer;

    private BattleSession session;
    private SceneFlow scenes;
    private PlayerParty party;
    private Transform targetPlayer;
    private DialogueRunner dialogue;
    private InputGate gate;
    private StoryFlags flags;

    public WanderState WanderState => wanderState;
    public ChaseState ChaseState => chaseState;
    public ReturnState ReturnState => returnState;

    public Vector2 Position => transform.position;
    public Vector2 HomePosition => homePosition;
    public Vector2 TargetPosition => targetPlayer != null ? (Vector2)targetPlayer.position : homePosition;
    public Vector2 DesiredVelocity { get; set; }

    public float DistanceToTarget => targetPlayer != null
        ? Vector2.Distance(transform.position, targetPlayer.position)
        : 999f;

    private bool Calm => cooldown > 0f || (session != null && session.IsSafe) || (gate != null && gate.IsLocked);
    public bool CanSeeTarget => party != null && party.Partner != null && !Calm && DistanceToTarget < 3.25f;
    public bool LostTarget => Calm || DistanceToTarget > 6.25f;

    public void Construct(BattleSession battleSession, SceneFlow sceneFlow, PlayerParty playerParty,
                          Transform playerTransform, DialogueRunner dialogueRunner = null,
                          InputGate inputGate = null, StoryFlags storyFlags = null)
    {
        session = battleSession;
        scenes = sceneFlow;
        party = playerParty;
        targetPlayer = playerTransform;
        dialogue = dialogueRunner;
        gate = inputGate;
        flags = storyFlags;

        homePosition = transform.position;
        if (body == null) body = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (alert == null)
        {
            Transform alertTr = transform.Find("Alert");
            if (alertTr != null) alert = alertTr.gameObject;
        }

        fsm = new StateMachine();
        wanderState = new WanderState(this);
        chaseState = new ChaseState(this);
        returnState = new ReturnState(this);
        fsm.ChangeTo(wanderState);

        if (isBoss && flags != null && flags.Has(StoryFlags.BossDefeated))
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeState(IState newState) => fsm?.ChangeTo(newState);

    private void Update()
    {
        if (isDefeated)
        {
            if (!isBoss)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            return;
        }

        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (stationary || inBattle || targetPlayer == null || (gate != null && gate.IsLocked))
        {
            DesiredVelocity = Vector2.zero;
        }
        else
        {
            fsm?.Tick(Time.deltaTime);
        }

        if (alert != null)
        {
            bool showAlert = fsm != null && fsm.Current == chaseState && !inBattle && !Calm;
            alert.SetActive(showAlert);
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = DesiredVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckPlayerCollision(collision.gameObject);
    private void OnCollisionStay2D(Collision2D collision) => CheckPlayerCollision(collision.gameObject);

    private void CheckPlayerCollision(GameObject other)
    {
        if (inBattle || isDefeated || Calm || !other.CompareTag("Player")) return;
        if (party == null || party.Partner == null) return; // cannot fight without partner

        if (isBoss && dialogue != null)
        {
            StartCoroutine(BossIntroAndBattle());
        }
        else
        {
            StartBattle();
        }
    }

    private IEnumerator BossIntroAndBattle()
    {
        inBattle = true;
        DesiredVelocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (dialogue != null)
        {
            yield return dialogue.Say("", "GRRRAAAH!! Raijugar menghalangi jalan, percikan listrik memenuhi gua!");
            dialogue.ForceClose();
        }
        yield return null;
        StartBattle();
    }

    private void StartBattle()
    {
        inBattle = true;
        DesiredVelocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        int lvl = isBoss ? 9 : Random.Range(minLevel, maxLevel + 1);
        var enemyInst = new MonsterInstance(species, lvl);

        session.Begin(enemyInst, isBoss, OnBattleFinished);
        scenes.StartBattle();
    }

    private void OnBattleFinished(BattleResult result)
    {
        inBattle = false;
        cooldown = 2.5f;
        fsm?.ChangeTo(returnState);

        if (result == BattleResult.Win)
        {
            isDefeated = true;
            gameObject.SetActive(false);
            if (isBoss && flags != null)
            {
                flags.Set(StoryFlags.BossDefeated);
            }
            else
            {
                respawnTimer = 20f;
            }
        }
    }

    private void Respawn()
    {
        isDefeated = false;
        transform.position = homePosition;
        gameObject.SetActive(true);
        cooldown = 2.5f;
        fsm?.ChangeTo(wanderState);
    }
}
