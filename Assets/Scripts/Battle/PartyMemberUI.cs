using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image memberImage;

    Monster _monster;
    public void Init(Monster monster)
    {
        _monster = monster;
        UpdateData();

        _monster.OnHPChanged +=UpdateData;

    }

    void UpdateData()
    {
        nameText.text = _monster.Base.Name;
        levelText.text = "Lvl " + _monster.Level;
        hpBar.SetHP((float)_monster.HP / _monster.MaxHp);
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
