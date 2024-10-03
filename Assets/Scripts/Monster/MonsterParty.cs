using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;
    public BattleSystem battleSystem;
    public List<Monster> Monsters
    {
        get { return monsters; } set { monsters = value; }
    }


    private void Start()
    {
        foreach (var monster in monsters)
        {
            monster.Init();
        }
    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddMonster(Monster newMonster)
    {
        if (battleSystem == null)
        {
            battleSystem = FindObjectOfType<BattleSystem>();
        }
        if ( monsters.Count < battleSystem.MonsterPartyCount)
        {
            monsters.Add(newMonster);
        }
        else
        {
            //TODO add to the PC once that's implemented
        }
    }
}
