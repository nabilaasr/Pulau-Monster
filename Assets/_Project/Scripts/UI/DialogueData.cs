using UnityEngine;

[CreateAssetMenu(fileName = "Dlg_", menuName = "Pulau Monster/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string speaker;
    [TextArea(2, 5)]
    [SerializeField] private string[] lines;

    public string Speaker => speaker;
    public string[] Lines => lines;
}
