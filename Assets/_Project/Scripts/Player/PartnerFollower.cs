using UnityEngine;

public class PartnerFollower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;

    private PlayerParty party;
    private PlayerMovement player;

    public void Construct(PlayerParty playerParty, PlayerMovement playerMovement)
    {
        party = playerParty;
        player = playerMovement;
        if (body == null) body = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        MonsterInstance m = party != null ? party.Partner : null;
        if (body != null) body.enabled = m != null;
        if (m == null || player == null || body == null) return;

        Vector2 f = player.Facing;
        Vector2 offset = f.y < 0 ? new Vector2(-12, 2)
                       : f.x < 0 ? new Vector2(10, 4)
                       : f.x > 0 ? new Vector2(-10, 4)
                       : new Vector2(12, -2);

        transform.position = player.transform.position + (Vector3)(offset / 16f);

        if (m.Data != null && m.Data.Frames != null && m.Data.Frames.Length > 0)
        {
            int frame = Mathf.FloorToInt(Time.time / 0.25f) % 2;
            if (frame < m.Data.Frames.Length)
            {
                body.sprite = m.Data.Frames[frame];
            }
            body.flipX = f.x < 0;

            float spriteWidth = m.Data.Frames[0].rect.width;
            float s = spriteWidth <= 16f ? 1f : 0.75f;
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
