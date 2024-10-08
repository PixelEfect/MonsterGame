using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new sphere")]
public class SphereItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;


    public override bool Use(Monster monster)
    {
        if (GameController.Instance.State == GameState.Battle)
        {
            return true;
        }
        return false;
    }

    public float CatchRateModifier => catchRateModifier;
}
