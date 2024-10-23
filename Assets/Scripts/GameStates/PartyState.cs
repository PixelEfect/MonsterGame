using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public Monster SelectedMonster { get; private set; }

    public static PartyState i { get; private set; }
    private void Awake()
    {
        i = this;
    }
    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;
        SelectedMonster = null;
        partyScreen.ClearSelection();
        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnMonsterSelected;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }
    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnMonsterSelected;
        partyScreen.OnBack -= OnBack;
    }

    void OnMonsterSelected(int selection)
    {
        SelectedMonster = partyScreen.SelectedMember;
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == InventoryState.i)
        {
            // Use item
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            if (SelectedMonster.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a faited monster");
                return;
            }
            if (SelectedMonster == battleState.BattleSystem.PlayerUnit.Monster)
            {
                partyScreen.SetMessageText("You can't switch with the same monster");
                return;
            }
            gc.StateMachine.Pop();
        }
        else
        {
            //todo
        }
    }
    IEnumerator GoToUseItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedMonster = null;

        var prevState = gc.StateMachine.GetPrevState();
        if(prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster to continue");
                return;
            }
        }

        gc.StateMachine.Pop();
    }
}
