using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class CuttableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("This tree looks like it can be cut");

        var monsterWithCut = initiator.GetComponent<MonsterParty>().Monsters.FirstOrDefault(p => p.Moves.Any(m => m.Base.name == "Cut"));

        if (monsterWithCut != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Should {monsterWithCut.Base.Name} use cut?",
                choices: new List<string>() { "Yes", "No"},
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                // Yes
                yield return DialogManager.Instance.ShowDialogText($"{monsterWithCut.Base.Name} used cut!");
                
                //player.GetComponent<Inventory>().AddItem(item, count);
                gameObject.SetActive(false);
            }
        }
    }
}
