using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver}
public enum BattleAction { Move, SwitchMonster, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    MonsterParty playerParty;
    MonsterParty trainerParty;
    Monster wildMonster;

    bool IsTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;


    public void StartBattle(MonsterParty playerParty, Monster wildMonster)
    {
        this.playerParty = playerParty;
        this.wildMonster = wildMonster;
        StartCoroutine (SetupBattle());
    }
    public void StartTrainerBattle(MonsterParty playerParty, MonsterParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        IsTrainerBattle=true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }
    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!IsTrainerBattle)
        {
            //Wild Monster Battle
            playerUnit.Setup(playerParty.GetHealthyMonster());
            enemyUnit.Setup(wildMonster);
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Monster.Base.Name} appeared.");
        }
        else
        {
            //Trainer Battle

            //Show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            //Send out first monster of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyMonster = trainerParty.GetHealthyMonster();
            enemyUnit.Setup(enemyMonster);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyMonster.Base.Name}");


            //Send out first monster of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMonster = playerParty.GetHealthyMonster();
            playerUnit.Setup(playerMonster);
            yield return dialogBox.TypeDialog($"Go {playerMonster.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        }



        partyScreen.Init();
        ActionSelection();
    }


    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Monsters.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);

    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Monsters);
        partyScreen.gameObject.SetActive(true);
    }


    void MoveSelection()
    {
        Debug.Log("playermove");
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Monster.CurrentMove = playerUnit.Monster.Moves[currentMove];
            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove();
            int playerMovePriority = playerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Monster.CurrentMove.Base.Priority;
            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Monster.Speed >= enemyUnit.Monster.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secoundMonster = secondUnit.Monster;

            //First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
            if(secoundMonster.HP > 0)
            {
                //Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }

        }
        else
        {
            if (playerAction ==BattleAction.SwitchMonster)
            {
                var selectedMonster = playerParty.Monsters[currentMember];
                state = BattleState.Busy;
                yield return SwitchMonster(selectedMonster);
            }
            //Enemy Turn
            var enemyMove = enemyUnit.Monster.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }
        if (state !=BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Monster.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Monster);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);


            }
            else
            {
                var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count> 0 && targetUnit.Monster.HP > 0)
            
            {
                foreach ( var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Monster, targetUnit.Monster, secondary.Target);
                    }
                }
            }

            if (targetUnit.Monster.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Monster.Base.Name} fainted");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}'s attack missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, MoveTarget moveTarget)
    {
        //Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }
        //Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        //Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {   
        if (state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        // Statuses like burn or psn will hurt the monster after the turn
        sourceUnit.Monster.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }




    IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0 )
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
   
    bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHists)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracu];
        int evasion = source.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];              //dzielenie przez 0?
        }
        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];              //dzielenie przez 0?
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }


    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!IsTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextMonster = trainerParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    //Send Out next monster
                    StartCoroutine (SendNextTrainerMonster(nextMonster));
                }
                else
                {
                    BattleOver(true);
                }

            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective!");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction +=2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -=2;
        }
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
            }
            else if (currentAction == 2)
            {
                //Monster
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Monster.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Monster.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Monster.Moves[currentMove];
            if ( move.PP == 0) 
            {
                return;
            }
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }

    }
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMember < playerParty.Monsters.Count - 1)
            {
                ++currentMember;
            }
            else
            {
                currentMember = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMember > 0)
            {
                --currentMember;
            }
            else
            {
                currentMember = (playerParty.Monsters.Count - 1);
            }
        }

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Monsters[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a faited monster");
                return;
            }
            if (selectedMember == playerUnit.Monster)
            {
                partyScreen.SetMessageText("You can't switch with the same monster");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            
            if ( prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchMonster(selectedMember));
            }
 
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    
    IEnumerator SwitchMonster(Monster newMonster)
    {
        if (playerUnit.Monster.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Monster.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);
        yield return dialogBox.TypeDialog($"Go {newMonster.Base.Name}!");
        state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerMonster(Monster nextMonster)
    {
        state = BattleState.Busy;

        enemyUnit.Setup(nextMonster);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextMonster.Base.Name}!");

        state = BattleState.RunningTurn;

    }

}
