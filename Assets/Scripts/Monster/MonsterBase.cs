using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]
public class MonsterBase : ScriptableObject
{
    [SerializeField] string name;
    
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;

    [SerializeField] Sprite backSprite;

    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;
    //public string GetName() 
    //{ 
    //    return name; 
    //}

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public MonsterType Type1
    {
        get { return type1; }
    }
    public MonsterType Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<LearnableMove> LernableMoves
    {
        get { return learnableMoves; }
    }
}
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}

public enum MonsterType
{
    None,
    Crystal,
    Light,
    Electric,
    Fire,
    Psionic,
    Fear,
    Earth,
    Metal,
    Phantom,
    Shadow,
    Water,
    Ice,
    Sound,
    Poison,
    Wood,
    Wind,
    Normal
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    
    //These 2 are not actual stats, they're used to boost the moveAccuracy
    Accuracu,
    Evasion
}


public class TypeChart
{
    static float[][] chart =
    {
        //                     CRY   LIG   ELE   FIR   PSI   FEA   EAR   MET   PHA   SHA   WAT   ICE   SOU   POI   WOO   WIN   NOR
        /*CRY*/ new float[] {  1f,   1f,   1f, 0.5f,   1f,   1f, 0.5f,   2f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f},
        /*LIG*/ new float[] {0.5f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f, 0.5f,   2f,   1f,   2f,   1f,   2f,   1f, 0.5f,   1f},
        /*ELE*/ new float[] {0.5f,   1f,   1f,   1f,   2f,   1f, 0.5f,   1f,   1f,   1f,   2f,   2f,   1f,   1f, 0.5f,   1f,   1f},
        /*FIR*/ new float[] {0.5f,   1f, 0.5f,   1f,   1f,   2f, 0.5f,   2f,   1f,   2f, 0.5f,   2f,   1f,   1f,   2f, 0.5f,   1f},
        /*PSI*/ new float[] {  1f,   1f, 0.5f,   1f,   1f, 0.5f, 0.5f,   2f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*FEA*/ new float[] {  1f, 0.5f,   1f, 0.5f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f,   1f,   1f,   1f},
        /*EAR*/ new float[] {  1f,   1f,   2f,   2f,   2f,   1f,   1f,   2f,   1f,   1f, 0.5f, 0.5f,   1f,   2f, 0.5f,   0f,   1f},
        /*MET*/ new float[] {  1f,   1f,   1f, 0.5f, 0.5f,   1f, 0.5f,   1f,   1f,   1f,   1f,   2f,   1f,   2f,   2f,   1f,   1f},
        /*PHA*/ new float[] {  1f,   2f,   1f,   1f, 0.5f,   2f,   1f,   1f,   1f, 0.5f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*SHA*/ new float[] {  2f, 0.5f,   1f,   1f, 0.5f, 0.5f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f},
        /*WAT*/ new float[] {  1f,   1f, 0.5f,   2f,   1f,   1f,   2f,   1f,   1f,   1f,   1f,   1f,   1f,   1f, 0.5f,   1f,   1f},
        /*ICE*/ new float[] {  1f,   1f,   1f, 0.5f,   1f,   1f,   2f, 0.5f,   1f,   1f,   1f,   1f, 0.5f,   1f,   2f,   2f,   1f},
        /*SOU*/ new float[] {  2f,   1f,   1f,   1f, 0.5f, 0.5f,   1f,   1f,   1f, 0.5f,   1f,   2f,   1f,   1f,   1f,   2f,   1f},
        /*POI*/ new float[] {0.5f,   1f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f},
        /*WOO*/ new float[] {  2f,   1f,   2f, 0.5f,   1f,   1f,   2f, 0.5f,   1f,   1f,   2f, 0.5f,   1f, 0.5f,   1f,   1f,   1f},
        /*WIN*/ new float[] {  1f,   2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f, 0.5f,   1f, 0.5f, 0.5f,   1f,   1f,   1f,   1f},
        /*NOR*/ new float[] {  1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f}
    };

    public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType)
    {
        if (attackType == MonsterType.None || defenseType == MonsterType.None)
        {
            return 1;
        }
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;
        return chart[row][col];
    }
}