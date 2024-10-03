using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDB
{

    static Dictionary<string, MonsterBase> monsters;
    
    public static void Init()
    {
        monsters = new Dictionary<string, MonsterBase>();

        var monsterArray =  Resources.LoadAll<MonsterBase>("");
        foreach ( var monster in monsterArray )
        {
            if (monsters.ContainsKey(monster.Name))
            {
                Debug.LogError($"There are two monsters with the name {monster.Name}");
                continue;
            }

            monsters[monster.Name] = monster;
        }
    }

    public static MonsterBase GetMonsterByName(string name)
    {
        if( !monsters.ContainsKey(name))
        {
            Debug.LogError($"Monster with name {name} not found in the database");
            return null;
        }
        return monsters[name];
    }
}
