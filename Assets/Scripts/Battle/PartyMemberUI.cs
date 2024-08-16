using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image memberImage;


    [SerializeField] Color highlightedColor;

    Monster _monster;
    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        hpBar.SetHP((float)monster.HP / monster.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        memberImage.DOKill();

        if (selected)
        {
            memberImage.DOColor(Color.blue, 0f);
            //memberImage.DOColor(Color.red, 0.5f);
        }
        else
        {
            memberImage.DOColor(Color.white, 0.5f);
        }
    }
}
