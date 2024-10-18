using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutToUseState : State<BattleSystem>
{
    // Input
    public Monster NewMonster { get; set; }

    bool aboutToUseChoice;
    public static AboutToUseState i {  get; private set; }

    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;

    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        StartCoroutine(StartState());
    }
    public override void Execute()
    {
        if(!bs.DialogBox.IsChoiceBoxEnabled)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        bs.DialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            bs.DialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //Yes Option
                StartCoroutine(SwitchAndContinueBattle());
            }
            else
            {
                //No Option
                StartCoroutine(ContinueBattle());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            bs.DialogBox.EnableChoiceBox(false);
            StartCoroutine(ContinueBattle());
        }
    }
    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog($"{bs.Trainer.Name} is about to use {NewMonster.Base.Name}. Do you want to change monster?");
        bs.DialogBox.EnableChoiceBox(true);
    }
    IEnumerator SwitchAndContinueBattle()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
        var selectedMonster = PartyState.i.SelectedMonster;
        if (selectedMonster != null)
        {
            yield return bs.SwitchMonster(selectedMonster);
        }

        yield return ContinueBattle();
    }
    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextTrainerMonster();
        bs.StateMachine.Pop();
    }
}
