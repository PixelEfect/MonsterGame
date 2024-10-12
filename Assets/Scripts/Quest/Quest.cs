using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{//-68
    // odniesienie do bazy zadan i zabezpieczenie zeby nie mozna jej bylo zmieniac poza ta klasa
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.name);
        Status = saveData.status;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.QuestName,  // albo Base.name
            status = Status
        };
        return saveData;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialogue);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }
    // transform uzyte aby mozna bylo uzyc transformacji do pobrania pleyer component
    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;

        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialogue);
        
        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

            string playername = player.GetComponent<PlayerController>().PlayerName;
            yield return DialogManager.Instance.ShowDialogText($"{playername} received {Base.RewardItem.ItemName}");
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
            {
                return false;
            }
        }
        return true;
    }

}

[Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}

public enum QuestStatus { None, Started, Completed }