using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;
    [TextArea]
    [SerializeField] string description;
    [Header("Statistic and Type")]
    [SerializeField] MonsterType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;
    [Header("SecondaryEffects")]
    [SerializeField] List<SecondaryEffects> secondaries;
    [Header("Boosts")]
    [SerializeField] List<BoostEffects> boostAdded;
    [SerializeField] float enhancementChance;     // Szansa na aktywacj� efektu wzmocnienia
    [TextArea]
    [SerializeField] string enhancementEffectDescription; // Opis efektu wzmocnienia
    //moje linijki
    [Header("MoveAdded")]
    [SerializeField] MoveBase enhancementMove;    // Atak, kt�ry wzmacnia ten atak
    [Header("Audio")]
    [SerializeField] AudioClip sound;
    public string MoveName
    {
        get { return moveName; }
    }
    public string Description
    {
        get { return description; }
    }
    public MonsterType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public bool AlwaysHists 
    {
        get { return alwaysHits; }
    }
    public int PP
    {
        get { return pp; }
    }
    public int Priority
    {
        get { return priority; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }
    public MoveEffects Effects
    {
        get { return effects; }
    }
    public MoveTarget Target
    {
        get { return target; }
    }
    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }
    public List<BoostEffects> BoostAdded
    {
        get { return boostAdded; }
    }
    public AudioClip Sound => sound;
    // moje pola - dostepy
    public MoveBase EnhancementMove => enhancementMove;
    public float EnhancementChance => enhancementChance;
    public string EnhancementEffectDescription => enhancementEffectDescription;
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get { return status; }
    }
    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }
}
[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance
    {
        get { return chance;}
    }
    public MoveTarget Target
    {
        get { return target; }
    }
}
[System.Serializable]
public class BoostEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;
    public int Chance
    {
        get { return chance; }
    }
    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical, Spirit, Status
}

public enum MoveTarget
{
    Foe, Self
}

