using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Items/Create new Scroll of Power")]
public class SpItem : ItemBase
{
    [SerializeField] MoveBase move;

    public override bool Use(Monster monster)
    {
        // Learning move is handled from Inventory UI, If it was learned then return true
        return monster.HasMove(move) ;
    }
    public MoveBase Move => move;
}

//bugsbugs   przy uczeniu sp mozna spamowac text i wtedy wywala 
//bugsbugs   przy braku sp wywala przy probie nauki
