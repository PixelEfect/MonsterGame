using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGiver : MonoBehaviour, ISavable
{
    [SerializeField] Monster monsterToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveMonster(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        monsterToGive.Init();
        player.GetComponent<MonsterParty>().AddMonster(monsterToGive);

        used = true;
        
        string dialogText = $"{player.PlayerName} received {monsterToGive.Base.Name}.";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return monsterToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}