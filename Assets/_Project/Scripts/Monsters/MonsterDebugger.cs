using UnityEngine;

public class MonsterDebugger : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private int debugLevel = 4;

    private MonsterInstance currentMonster;

    [ContextMenu("1. Buat monster")]
    private void CreateMonster()
    {
        if (species == null) return;
        currentMonster = new MonsterInstance(species, debugLevel);
    }

    [ContextMenu("3. Beri 100 EXP")]
    private void Give100Exp()
    {
        if (currentMonster == null) CreateMonster();
        foreach (GrowthEvent ge in currentMonster.GainExp(100))
        {
            Debug.Log(ge.Message);
        }
    }
}
