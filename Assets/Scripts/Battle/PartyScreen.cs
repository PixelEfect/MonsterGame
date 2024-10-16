using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] Text messegeText;

    PartyMemberUI[] memberSlots;
    List<Monster> monsters;
    MonsterParty party;

    public Monster SelectedMember => monsters[selectedItem];

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        //zmiana dla menu wyboru dla grid
        //SetSelectionSettings(SelectionType.Grid, 2);

        party = MonsterParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        monsters = party.Monsters;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < monsters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(monsters[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        var textSlots =  memberSlots.Select(m => m.GetComponent<TextSlot>());
        SetItems(textSlots.Take(monsters.Count).ToList());

        messegeText.text = "Choose a monster";
    }


    public void ShowIfSpIsUsable (SpItem spItem)
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            string message =  spItem.CanBeTaught(monsters[i]) ? "Able" : "Not Able";
            memberSlots[i].SetMessage(message);
        }
    }
    public void ClearMemberSlotMessage()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }


    public void SetMessageText(string message)
    {
        messegeText.text = message;
    }
}
