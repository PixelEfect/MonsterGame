using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{

    [SerializeField] string itemName;

    [SerializeField] string description;

    [SerializeField] string useMassage;

    [SerializeField] Sprite icon;


    public virtual string ItemName => itemName;

    public string Description => description;

    public string UseMassage => useMassage;

    public Sprite Icon => icon;

    public virtual bool Use(Monster monster)
    {
        return false;
    }


    public virtual bool IsReusable => false;

    public virtual bool CanUseInBattle => true;

    public virtual bool CanUseOutBattle => true;
}
