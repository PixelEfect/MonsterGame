using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;

    // Input
    public BattleTrigger trigger { get; set; }
    public TrainerController trainer { get; set; }

    public static BattleState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        battleSystem.gameObject.SetActive(true);
        gc.WorldCamera.gameObject.SetActive(false);

        var playerParty = gc.PlayerController.GetComponent<MonsterParty>();

        if (trainer == null)
        {
            var wildMonster = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildMonster(trigger);
            var wildMonsterCopy = new Monster(wildMonster.Base, wildMonster.Level);
            battleSystem.StartBattle(playerParty, wildMonster, trigger);
        }
        else
        {
            var trainerParty = trainer.GetComponent<MonsterParty>();
            battleSystem.StartTrainerBattle(playerParty, trainerParty, trigger);
        }

        battleSystem.OnBattleOver += EndBattle;

    }
    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        gc.WorldCamera.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        gc.StateMachine.Pop();
    }
}
