using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = SceneFlow.TitleScene;
    [SerializeField] private MonsterDatabase monsterDatabase;
    [SerializeField] private ScreenFader fader;
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private ToastView toastView;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private AudioService audioService;

    private GameServices services;

    private void Awake()
    {
        var moveInput = new InputSystemMoveInput();
        var actionInput = new InputSystemActionInput();
        var gate = new InputGate(moveInput, actionInput);

        services = new GameServices
        {
            Input = gate,
            Database = monsterDatabase,
            Party = new PlayerParty(),
            Battle = new BattleSession(),
            Save = new SaveService(),
            Flags = new StoryFlags(),
            Dialogue = dialogueRunner,
            Toast = toastView,
            Audio = audioService
        };

        services.Scenes = new SceneFlow(this, fader, services);

        if (dialogueRunner != null) dialogueRunner.Construct(gate, dialogueView, audioService);
        if (pauseMenu != null) pauseMenu.Construct(services);
        if (toastView != null) toastView.Construct();
    }

    private void Start()
    {
        services.Scenes.GoTo(firstScene);
    }

    private void Update()
    {
        if (services != null && services.Party != null)
        {
            string curr = services.Scenes.CurrentScene;
            if (curr == SceneFlow.WorldScene || curr == SceneFlow.BattleScene)
            {
                services.Party.Playtime += Time.deltaTime;
            }
        }
    }
}
