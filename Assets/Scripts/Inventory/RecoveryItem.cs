using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Create new recovery items")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHp;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPp;

    [Header("Status Condition")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Monster monster)
    {
        // Revive
        if (revive || maxRevive)
        {
            if (monster.HP > 0)
            {
                return false;
            }
            if (revive)
            {
                monster.IncreaseHP(monster.MaxHp/2);
            }
            else if (maxRevive) 
            {
                monster.IncreaseHP(monster.MaxHp);
            }
            return true;
        }
        // No other items cam be used on fainted monster
        if (monster.HP == 0)
        {
            return false;
        }

        // Restore HP
        if (restoreMaxHp || hpAmount > 0)
        {
            if (monster.HP == monster.MaxHp)
            {
                return false;
            }
            if (restoreMaxHp)
            {
                monster.IncreaseHP(monster.MaxHp);
            }
            else
            {
                monster.IncreaseHP(hpAmount);
            }
        }

        // Recovery Status
        if (recoverAllStatus || status != ConditionID.none)
        {
            if (monster.Status == null && monster.VolatileStatus == null)
            {
                return false;
            }
            if (recoverAllStatus)
            {
                monster.CureStatus();
                monster.CureVolatileStatus();
            }
            else
            {
                if (monster.Status.Id == status)
                {
                    monster.CureStatus();
                }
                else if (monster.VolatileStatus.Id == status) 
                {
                    monster.CureVolatileStatus();
                }
                else
                {
                    return false;
                }
            }
        }

        // Restore PP
        if (restoreMaxPp)
        {
            monster.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount > 0)
        {
            monster.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }
        

        return true;
    }

}




