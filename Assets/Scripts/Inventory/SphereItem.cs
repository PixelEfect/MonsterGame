using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new sphere")]
public class SphereItem : ItemBase
{
    public override bool Use(Monster monster)
    {
        return true;
    }
}
