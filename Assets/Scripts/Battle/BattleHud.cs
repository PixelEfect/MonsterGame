using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Monster _monster;
    Dictionary<ConditionID, Color> statusColor;
    public void SetData(Monster monster)
    {
        _monster = monster;
        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        hpBar.SetHP((float) monster.HP / monster.MaxHp);

        statusColor = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor}
        };

        SetStatusText();
        _monster.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_monster.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _monster.Status.Id.ToString().ToUpper();
            statusText.color = statusColor[_monster.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_monster.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_monster.HP / _monster.MaxHp);
            _monster.HpChanged = false;
        }

    }
}
