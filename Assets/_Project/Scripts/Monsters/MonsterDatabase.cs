using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Pulau Monster/Monster Database")]
public class MonsterDatabase : ScriptableObject
{
    [SerializeField] private List<MonsterData> monsters = new List<MonsterData>();

    public List<MonsterData> Monsters => monsters;

    public void Setup(List<MonsterData> list)
    {
        monsters = list;
    }

    public MonsterData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] != null && monsters[i].Id == id)
                return monsters[i];
        }
        return null;
    }
}
