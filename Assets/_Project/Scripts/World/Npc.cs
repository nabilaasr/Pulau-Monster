using System.Collections;
using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Warga";
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite faceDown;
    [SerializeField] private Sprite faceLeft;
    [SerializeField] private Sprite faceRight;
    [SerializeField] private Sprite faceUp;
    [SerializeField] private DialogueData dialogue;

    public string NpcName => npcName;
    protected DialogueRunner Dialogue { get; private set; }
    protected StoryFlags Flags { get; private set; }
    protected GameServices Services { get; private set; }

    public void Construct(GameServices s)
    {
        Services = s;
        Dialogue = s.Dialogue;
        Flags = s.Flags;
        if (body == null) body = GetComponent<SpriteRenderer>();
        OnConstructed();
    }

    public void Interact(PlayerInteractor who)
    {
        if (Dialogue != null && Dialogue.IsRunning) return;
        Face(who.transform.position - transform.position);
        if (Dialogue != null)
        {
            Dialogue.Run(Talk());
        }
    }

    protected virtual IEnumerator Talk()
    {
        if (dialogue != null)
        {
            yield return Dialogue.PlayData(dialogue);
        }
        else
        {
            yield return Dialogue.Say(npcName, "Halo!");
        }
    }

    protected virtual void OnConstructed() { }

    protected void Face(Vector2 d)
    {
        if (body == null) return;
        body.sprite = Mathf.Abs(d.x) > Mathf.Abs(d.y)
            ? (d.x > 0 ? faceRight : faceLeft)
            : (d.y > 0 ? faceUp : faceDown);
    }
}
