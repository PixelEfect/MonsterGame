using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public static GamePartyState i { get; private set; }
    private void Awake()
    {
        i = this;
    }
    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;

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
        if (gc.StateMachine.GetPrevState() == InventoryState.i)
        {
            // Use item
            Debug.Log("use item");
        }
        else
        {
            //todo
        }
    }

    void OnBack()
    {
        gc.StateMachine.Pop();
    }
}
