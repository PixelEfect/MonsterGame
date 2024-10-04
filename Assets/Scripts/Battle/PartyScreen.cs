using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messegeText;

    PartyMemberUI[] memberSlots;
    List<Monster> monsters;

    int selection = 0;

    public Monster SelectedMember => monsters[selection];

    /// <summary>
    /// Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    /// </summary>
    public BattleState? CalledFrom { get; set; }
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Monster> monsters)
    {
        this.monsters = monsters;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < monsters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(monsters[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        UpdateMemberSelection(selection);

        messegeText.text = "Choose a monster";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selection < monsters.Count - 1)
            {
                ++selection;
            }
            else
            {
                selection = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selection > 0)
            {
                --selection;
            }
            else
            {
                selection = (monsters.Count - 1);
            }
        }
        if (selection != prevSelection)
        {
            UpdateMemberSelection(selection);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0;i < monsters.Count;i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }
    public void SetMessageText(string message)
    {
        messegeText.text = message;
    }
}
