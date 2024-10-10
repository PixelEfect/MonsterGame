using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Items/Create new Scroll of Power")]
public class SpItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isRune;

    public override string ItemName => base.ItemName + $": {move.MoveName}";

    public override bool Use(Monster monster)
    {
        // Learning move is handled from Inventory UI, If it was learned then return true
        return monster.HasMove(move) ;
    }

    public bool CanBeTaught(Monster monster)
    {
        return monster.Base.LearnableByItems.Contains(move);
    }


    public override bool IsReusable => isRune;
    public override bool CanUseInBattle => false;
    public MoveBase Move => move;
    public bool IsRune => isRune;
}

//bugsbugs   przy uczeniu sp mozna spamowac text i wtedy wywala 
//bugsbugs   przy braku sp wywala przy probie nauki
