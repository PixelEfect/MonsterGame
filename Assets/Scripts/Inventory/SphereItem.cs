using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new sphere")]
public class SphereItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;


    public override bool Use(Monster monster)
    {
        return true;
    }

    public override bool CanUseOutBattle => false;

    public float CatchRateModifier => catchRateModifier;
}
