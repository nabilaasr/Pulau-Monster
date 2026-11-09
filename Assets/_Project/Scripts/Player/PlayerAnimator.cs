using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    [SerializeField] private PlayerMovement movement;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (animator == null || movement == null) return;

        bool isMoving = movement.Velocity.sqrMagnitude > 0.01f;
        animator.SetBool(IsMovingHash, isMoving);
        animator.SetFloat(MoveXHash, movement.Facing.x);
        animator.SetFloat(MoveYHash, movement.Facing.y);
    }
}
