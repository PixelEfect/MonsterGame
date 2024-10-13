//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MonsterGiver : MonoBehaviour, ISavable
//{
//    [SerializeField] Monster monsterToGive;
//    [SerializeField] Dialog dialog;

//    bool used = false;

//    public IEnumerator GiveMonster(PlayerController player)
//    {
//        yield return DialogManager.Instance.ShowDialog(dialog);

//        monsterToGive.Init();
//        player.GetComponent<MonsterParty>().AddMonster(monsterToGive);

//        used = true;
        
//        string dialogText = $"{player.PlayerName} received {monsterToGive.Base.Name}.";

//        yield return DialogManager.Instance.ShowDialogText(dialogText);
//    }

//    public bool CanBeGiven()
//    {
//        return monsterToGive != null && !used;
//    }

//    public object CaptureState()
//    {
//        return used;
//    }

//    public void RestoreState(object state)
//    {
//        used = (bool)state;
//    }
//}

//Dodana opcja wyboru czy chcemy tego poka z tym ze nie wspolpracuje w pelni z npccontroller
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGiver : MonoBehaviour, ISavable
{
    [SerializeField] Monster monsterToGive;
    [SerializeField] Dialog dialog;
    [SerializeField] bool monsterGiverChoice;

    bool used = false;

    public IEnumerator GiveMonster(PlayerController player, Dialog dialog, QuestBase questToComplete, Transform initiator )
    {
        int selectedChoice = 0;
        if (monsterGiverChoice)
        {
            yield return DialogManager.Instance.ShowDialog(dialog, new List<string>() { "Yes", "No" },
//yield return DialogManager.Instance.ShowDialog(dialog);
(choiceIndex) => selectedChoice = choiceIndex);
        }


        if (selectedChoice == 0)
        {
            // Yes
            monsterToGive.Init();
            player.GetComponent<MonsterParty>().AddMonster(monsterToGive);
            used = true;

            string dialogText = $"{player.PlayerName} received {monsterToGive.Base.Name}.";

            yield return DialogManager.Instance.ShowDialogText(dialogText);
            if (questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(initiator);
            }
        }
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogManager.Instance.ShowDialogText($"Okay! Come back if you change your mind");
        }
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
