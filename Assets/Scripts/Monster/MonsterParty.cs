using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterParty : MonoBehaviour
{
    [SerializeField] List<Monster> monsters;

    public event Action OnUpdated;
    public List<Monster> Monsters
    {
        get 
        { 
            return monsters; 
        } 
        set 
        { 
            monsters = value; 
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var monster in monsters)
        {
            monster.Init();
        }
    }

    private void Start()
    {

    }

    public Monster GetHealthyMonster()
    {
        return monsters.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddMonster(Monster newMonster)  
    {
        if ( monsters.Count < 5)
        {
            monsters.Add(newMonster);
            OnUpdated?.Invoke();
        }
        else
        {
            //TODO add to the PC once that's implemented
        }
    }

    public static MonsterParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonsterParty>();
    }
}
