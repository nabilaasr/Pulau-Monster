using UnityEngine;

public class SandboxInstaller : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    private void Awake()
    {
        if (player == null) player = Object.FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.Construct(new InputSystemMoveInput());
        }
    }
}
