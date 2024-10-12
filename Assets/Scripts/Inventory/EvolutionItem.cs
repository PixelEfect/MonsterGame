using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new evolution items")]
public class EvolutionItem : ItemBase
{//-75
    public override bool Use(Monster monster)
    {
        return true;
    }
}
