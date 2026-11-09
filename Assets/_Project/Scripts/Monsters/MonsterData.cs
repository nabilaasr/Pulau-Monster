using UnityEngine;

[CreateAssetMenu(fileName = "Mon_", menuName = "Pulau Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Element element;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private int baseHP;
    [SerializeField] private int baseATK;
    [SerializeField] private int baseDEF;
    [SerializeField] private int baseSPD;
    [SerializeField] private string skillName = "Semburan Api";
    [SerializeField] private MonsterData evolvesTo;
    [SerializeField] private int evolveLevel;

    public string Id => id;
    public string DisplayName => displayName;
    public Element Element => element;
    public Sprite[] Frames => frames;
    public int BaseHP => baseHP;
    public int BaseATK => baseATK;
    public int BaseDEF => baseDEF;
    public int BaseSPD => baseSPD;
    public string SkillName => skillName;
    public MonsterData EvolvesTo => evolvesTo;
    public int EvolveLevel => evolveLevel;

    public void Setup(string monId, string name, Element elem, int hp, int atk, int def, int spd,
                      string skill, MonsterData evolveTarget, int evLvl, Sprite[] monFrames)
    {
        id = monId;
        displayName = name;
        element = elem;
        baseHP = hp;
        baseATK = atk;
        baseDEF = def;
        baseSPD = spd;
        skillName = skill;
        evolvesTo = evolveTarget;
        evolveLevel = evLvl;
        frames = monFrames;
    }
}
