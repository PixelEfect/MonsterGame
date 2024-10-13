using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{//-77
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;

        yield return DialogManager.Instance.ShowDialogText("You look tired!", 
            choices: new List<string>() {"Yes", "No" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex );

        if (selectedChoice == 0)
        {
            // Yes
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<MonsterParty>();
            playerParty.Monsters.ForEach(m => m.Heal());
            playerParty.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialogText($"Your monster should be fully healed now");
        }
        else if (selectedChoice == 1)
        {
            // No
            yield return DialogManager.Instance.ShowDialogText($"Okay! Come back if you change your mind");
        }



    }
}
