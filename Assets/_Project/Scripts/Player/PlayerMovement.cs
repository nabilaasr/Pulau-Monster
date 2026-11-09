using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Rigidbody2D rb;

    private IMoveInput input;
    private Vector2 velocity;

    public Vector2 Velocity => velocity;
    public Vector2 Facing { get; private set; } = Vector2.down;

    public void Construct(IMoveInput moveInput)
    {
        input = moveInput;
    }

    public void Teleport(Vector2 position, Vector2 facingDirection)
    {
        transform.position = position;
        Facing = facingDirection.sqrMagnitude > 0.01f ? ToCardinal(facingDirection) : Vector2.down;
        velocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (input == null) return;

        Vector2 move = input.Move;
        float walkSpd = config != null ? config.WalkSpeed : 3.75f;
        float runSpd = config != null ? config.RunSpeed : 6.0f;
        float speed = input.IsRunning ? runSpd : walkSpd;

        velocity = move * speed;
        if (move.sqrMagnitude > 0.01f)
        {
            Facing = ToCardinal(move);
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }

    private static Vector2 ToCardinal(Vector2 v) =>
        Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2(Mathf.Sign(v.x), 0f)
            : new Vector2(0f, Mathf.Sign(v.y));
}
