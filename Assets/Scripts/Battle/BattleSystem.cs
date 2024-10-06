using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver}
public enum BattleAction { Move, SwitchMonster, UseItem, UseSphere, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] private int monsterPartyCount = 5;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject radiusSprite;
    [SerializeField] GameObject sphereSpriteS;
    [SerializeField] GameObject sphereSprite1;
    [SerializeField] GameObject sphereSprite2;
    [SerializeField] GameObject sphereSprite3;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    public int MonsterPartyCount
    {
        get { return monsterPartyCount; }
    }

    BattleState state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    MonsterParty playerParty;
    MonsterParty trainerParty;
    Monster wildMonster;

    bool IsTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    public void StartBattle(MonsterParty playerParty, Monster wildMonster)
    {
        this.playerParty = playerParty;
        this.wildMonster = wildMonster;
        IsTrainerBattle = false;

        player = playerParty.GetComponent<PlayerController>();

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
            //trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyMonster = trainerParty.GetHealthyMonster();
            enemyUnit.Setup(enemyMonster);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyMonster.Base.Name}");


            //Send out first monster of the player
            //playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMonster = playerParty.GetHealthyMonster();
            playerUnit.Setup(playerMonster);
            yield return dialogBox.TypeDialog($"Go {playerMonster.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);

        }


        escapeAttempts = 0;
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
    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }
    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        dialogBox.EnableActionSelector(false);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Monster newMonster)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newMonster.Base.Name}. Do you want to change monster?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }
    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove) 
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you wan't to forget");

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
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
                var selectedMonster = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchMonster(selectedMonster);
            }
            else if (playerAction ==BattleAction.UseSphere)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowSphere();
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // This is handled from item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {

                yield return TryToEscape();
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
            yield return sourceUnit.Hud.WaitForHpUpdate();
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
                yield return targetUnit.Hud.WaitForHpUpdate();
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
                yield return HandleMonsterFainted(targetUnit);
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
        yield return sourceUnit.Hud.WaitForHpUpdate();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return HandleMonsterFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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
   
    // Sprawdzanie czy atak trafia
    bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHists) // Jesli atak zawsze trafia zwraca true
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

    IEnumerator HandleMonsterFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Monster.Base.Name} fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //Exp Gain
            int expYield = faintedUnit.Monster.Base.ExpYield;
            int enemyLevel = faintedUnit.Monster.Level;
            float trainerBonus = (IsTrainerBattle)? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Monster.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            //Check Level Up
            while (playerUnit.Monster.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} grew to level {playerUnit.Monster.Level}");

                //Try to learn a new Move
                var newMove = playerUnit.Monster.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Monster.Moves.Count < MonsterBase.MaxNumOfMoves)
                    {
                        playerUnit.Monster.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Monster.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {MonsterBase.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerUnit.Monster, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
                    StartCoroutine (AboutToUse(nextMonster));
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
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };
            Action onItemUsed = () =>
            {
                state = BattleState.Busy;
                inventoryUI.gameObject.SetActive(false);
                StartCoroutine(RunTurns(BattleAction.UseItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == MonsterBase.MaxNumOfMoves)
                {
                    // Dont learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {                    
                    // Forget and learn
                    var selectedMove = playerUnit.Monster.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    playerUnit.Monster.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (state == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.UseSphere));
            }
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
                OpenBag();
            }
            else if (currentAction == 2)
            {
                //Monster
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
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
    // okno wyboru innego monstera
    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember; 
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

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchMonster(selectedMember, isTrainerAboutToUse));
            }
            partyScreen.CalledFrom = null;
        };


        Action onBack = () =>
        {
            if (playerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);


            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {

                StartCoroutine(SendNextTrainerMonster());
            }
            else
            {
                ActionSelection();
            }
            partyScreen.CalledFrom = null;
        };
        partyScreen.HandleUpdate(onSelected, onBack);
    }
    // Czy chcesz zmienic monstera po pokonaniu wroga - tylko z trenerem
    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                //Yes Option
                OpenPartyScreen();
            }
            else
            {
                //No Option
                StartCoroutine (SendNextTrainerMonster());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerMonster());
        }
    }
    
    IEnumerator SwitchMonster(Monster newMonster, bool isTrainerAboutToUse = false)
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

        if (isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerMonster());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
    }

    IEnumerator SendNextTrainerMonster()
    {
        state = BattleState.Busy;

        var nextMonster = trainerParty.GetHealthyMonster();
        enemyUnit.Setup(nextMonster);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextMonster.Base.Name}!");

        state = BattleState.RunningTurn;

    }

    IEnumerator ThrowSphere()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer monster!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used sphere");

        int shakeCount = TryToCatchMonster(enemyUnit.Monster);
        Vector3 originalScale = enemyUnit.transform.localScale;
        // Animacja PROMIENIA
        float radiusAnimationTime = AnimationHelper.AnimateRadius(enemyUnit.transform.position, radiusSprite);
        yield return new WaitForSeconds(radiusAnimationTime);

        // ANIMACJA SFERY
        float sphereAnimationTime = AnimationHelper.AnimateSphere(shakeCount, enemyUnit.transform.position, sphereSpriteS, sphereSprite1, sphereSprite2, sphereSprite3);

        // ANIMACJA MONSTERA
        enemyUnit.PlayCaptureAnimation();
        yield return new WaitForSeconds(sphereAnimationTime);

        if (shakeCount == 4)
        {
            //Monster is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} was caught");
            playerParty.AddMonster(enemyUnit.Monster);
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} has been added to your party");
            BattleOver(true);
        }
        else
        {
            yield return enemyUnit.PlayBrakeOutAnimation(originalScale);
            //Monster broke out
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} broke free");
            state = BattleState.RunningTurn;
        }
        // Przywrócenie stanu po zakoñczeniu akcji
        //state = BattleState.Waiting;

    }

    // Sprawdzenie czy udalo sie zlapac monstera
    int TryToCatchMonster(Monster monster)
    {
        float a = (3 * monster.MaxHp - 2 * monster.HP) * monster.Base.CatchRate * ConditionsDB.GetStatusBonus(monster.Status) / (3 * monster.MaxHp);

        if (a >= 255)
        {
            Debug.Log("4");
            return 4;
        }
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }
            ++shakeCount;
        }
        Debug.Log($"{shakeCount}");
        return shakeCount;
    }
    // Sprawdzenie czy udalo sie uciec z walki (tylko wild)
    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;
        int playerSpeed = playerUnit.Monster.Speed;
        int enemySpeed = enemyUnit.Monster.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
        f = f % 256;

        if (UnityEngine.Random.Range(0,256) < f) 
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            yield return dialogBox.TypeDialog($"Can't escape!");
            state = BattleState.RunningTurn;
        }
    }
}
