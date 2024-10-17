using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<MonsterEncounterRecord> wildMonsters;
    [SerializeField] List<MonsterEncounterRecord> wildMonstersInWater;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    [HideInInspector]
    [SerializeField] int totalChanceWater = 0;


    private void Start()
    {
        InitializeChanceBounds();
    }
    private void Awake()
    {
        InitializeChanceBounds();
    }
    private void OnValidate()
    {
        InitializeChanceBounds();
    }
    private void InitializeChanceBounds()
    {
        totalChance = 1;
        totalChanceWater = -1;

        if (wildMonsters.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildMonsters)
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.chancePercentage;

                totalChance += record.chancePercentage;
            }
        }
        if (wildMonstersInWater.Count > 0)
        {
            totalChanceWater = 0;
            foreach (var record in wildMonstersInWater)
            {
                record.chanceLower = totalChanceWater;
                record.chanceUpper = totalChanceWater + record.chancePercentage;

                totalChanceWater += record.chancePercentage;
            }
        }
    }
    public Monster GetRandomWildMonster(BattleTrigger trigger)
    {
        var monsterList = (trigger == BattleTrigger.LongGrass) ? wildMonsters : wildMonstersInWater;

        int randVal = Random.Range(1, 101);
        var monsterRecord = monsterList.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

        var levelRange = monsterRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);

        var wildMonster = new Monster(monsterRecord.monster, level);
        wildMonster.Init();
        return wildMonster;
    }
}

[System.Serializable]
public class MonsterEncounterRecord
{
    public MonsterBase monster;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower {  get; set; }
    public int chanceUpper { get; set;}
}