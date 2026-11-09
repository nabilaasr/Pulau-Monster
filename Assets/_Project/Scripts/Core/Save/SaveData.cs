using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int version = SaveService.CurrentVersion;
    public string savedAt;
    public string scene = "Scn_World";
    public Vector2 playerPosition;
    public string partnerId;
    public int partnerLevel;
    public int partnerExp;
    public int partnerHp;
    public int potions;
    public float playtime;
    public List<string> flags = new List<string>();
}
