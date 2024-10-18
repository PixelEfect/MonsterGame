using GDE.GenericSelectionUI;
using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI selectionUI;

    public static ActionSelectionState i {  get; set; }

    private void Awake()
    {
        i = this; 
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnActionSelected;

        bs.DialogBox.SetDialog("Choose an action");
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnActionSelected;
    }

    void OnActionSelected(int selection)
    {
        if (selection == 0)
        {
            // Fight
            bs.SelectedAction = BattleAction.Move;
            MoveSelectionState.i.Moves = bs.PlayerUnit.Monster.Moves;
            bs.StateMachine.ChangeState(MoveSelectionState.i);
        }
        else if (selection == 1)
        {
            // Bag
            StartCoroutine(GoToInventoryState());
        }
        else if(selection == 2)
        {
            // Monster
            StartCoroutine(GoToPartyState());
        }
        else if( selection == 3)
        {
            // Run
            bs.SelectedAction = BattleAction.Run;
            bs.StateMachine.ChangeState(RunTurnState.i);
        }
    }
    IEnumerator GoToPartyState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
        var selectedMonster = PartyState.i.SelectedMonster;
        if (selectedMonster != null)
        {
            bs.SelectedAction = BattleAction.SwitchMonster;
            bs.SelectedMonster = selectedMonster;
            bs.StateMachine.ChangeState(RunTurnState.i);
        }
    }
    IEnumerator GoToInventoryState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(InventoryState.i);
        var selectedItem = InventoryState.i.SelectedItem;
        if (selectedItem != null)
        {
            bs.SelectedAction = BattleAction.UseItem;
            bs.SelectedItem = selectedItem;
            bs.StateMachine.ChangeState(RunTurnState.i);
        }
    }

}
