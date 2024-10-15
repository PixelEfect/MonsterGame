using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<MonsterEncounterRecord> wildMonsters;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    private void OnValidate()
    {
        totalChance = 0;
        foreach (var record in wildMonsters)
        {
            record.chanceLower = totalChance;
            record.chanceUpper = totalChance + record.chancePercentage;

            totalChance = totalChance + record.chancePercentage;
        }
    }
    private void Start()
    {

    }

    public Monster GetRandomWildMonster()
    {
        int randVal = Random.Range(1, 101);
        var monsterRecord = wildMonsters.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

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