using DG.Tweening;
using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public enum BattleStates { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver}
public enum BattleAction { Move, SwitchMonster, UseItem, UseSphere, Run}

public enum BattleTrigger { LongGrass, Water}
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
    [SerializeField] MoveToForgetSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;
    [Header("Music")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVicoryMusic;
    [Header("Background")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;

    public StateMachine<BattleSystem> StateMachine {get; private set;}

    public event Action<bool> OnBattleOver;

    public int SelectedMove { get; set;}

    public BattleAction SelectedAction { get; set; }

    public Monster SelectedMonster { get; set; }

    public bool IsBattleOver { get; private set; }


    BattleStates state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty TrainerParty { get; private set; }
    public Monster WildMonster { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    TrainerController trainer;

    public int EscapeAttempts { get; set; }
    MoveBase moveToLearn;

    BattleTrigger battletrigger;

    public void StartBattle(MonsterParty playerParty, Monster wildMonster, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.PlayerParty = playerParty;
        this.WildMonster = wildMonster;
        IsTrainerBattle = false;

        battletrigger = trigger;

        player = playerParty.GetComponent<PlayerController>();
        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine (SetupBattle());
    }
    public void StartTrainerBattle(MonsterParty playerParty, MonsterParty trainerParty,
        BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.PlayerParty = playerParty;
        this.TrainerParty = trainerParty;

        IsTrainerBattle=true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        battletrigger = trigger;
        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }
    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);

        playerUnit.Clear();
        enemyUnit.Clear();

        backgroundImage.sprite = (battletrigger == BattleTrigger.LongGrass)? grassBackground : waterBackground;

        if (!IsTrainerBattle)
        {
            //Wild Monster Battle
            Debug.Log("rozpoczecie starcia");
            playerUnit.Setup(PlayerParty.GetHealthyMonster());
            Debug.Log("pobranie zdrowego okazu");
            enemyUnit.Setup(WildMonster);
            Debug.Log("pobranie przeciwnika");
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
            var enemyMonster = TrainerParty.GetHealthyMonster();
            enemyUnit.Setup(enemyMonster);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyMonster.Base.Name}");


            //Send out first monster of the player
            //playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMonster = PlayerParty.GetHealthyMonster();
            playerUnit.Setup(playerMonster);
            yield return dialogBox.TypeDialog($"Go {playerMonster.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Monster.Moves);
        }

        IsBattleOver = false;
        EscapeAttempts = 0;
        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.i);
    }


    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        PlayerParty.Monsters.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleStates.ActionSelection;
        dialogBox.EnableActionSelector(true);
    }
    void OpenBag()
    {
        state = BattleStates.Bag;
        inventoryUI.gameObject.SetActive(true);
    }
    void OpenPartyScreen()
    {
        //partyScreen.CalledFrom = state;
        state = BattleStates.PartyScreen;
        dialogBox.EnableActionSelector(false);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleStates.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Monster newMonster)
    {
        state = BattleStates.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newMonster.Base.Name}. Do you want to change monster?");
        state = BattleStates.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }
    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove) 
    {
        state = BattleStates.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you wan't to forget");

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleStates.MoveToForget;
    }

    

    public void HandleUpdate()
    {
        StateMachine.Execute();

        if (state == BattleStates.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleStates.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleStates.ActionSelection;
            };
            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            //inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleStates.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleStates.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == MonsterBase.MaxNumOfMoves)
                {
                    // Dont learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} did not learn {moveToLearn.MoveName}"));
                }
                else
                {                    
                    // Forget and learn
                    var selectedMove = playerUnit.Monster.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Monster.Base.Name} forgot {selectedMove.MoveName} and learned {moveToLearn.MoveName}"));
                    playerUnit.Monster.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleStates.RunningTurn;
            };

            //moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    if (state == BattleState.ActionSelection)
        //    {
        //        StartCoroutine(RunTurns(BattleAction.UseSphere));
        //    }
        //}
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
                //StartCoroutine(RunTurns(BattleAction.Run));
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
            //StartCoroutine(RunTurns(BattleAction.Move));
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

            //if (partyScreen.CalledFrom == BattleState.ActionSelection)
            //{
            //    StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            //}
            //else
            //{
            //    state = BattleState.Busy;
            //    bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
            //    StartCoroutine(SwitchMonster(selectedMember, isTrainerAboutToUse));
            //}
            //partyScreen.CalledFrom = null;
        };


        Action onBack = () =>
        {
            if (playerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);


        //    if (partyScreen.CalledFrom == BattleState.AboutToUse)
        //    {

        //        StartCoroutine(SendNextTrainerMonster());
        //    }
        //    else
        //    {
        //        ActionSelection();
        //    }
        //    partyScreen.CalledFrom = null;
        };
        //partyScreen.HandleUpdate(onSelected, onBack);
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
    
    public IEnumerator SwitchMonster(Monster newMonster)
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
    }

    IEnumerator SendNextTrainerMonster()
    {
        state = BattleStates.Busy;

        var nextMonster = TrainerParty.GetHealthyMonster();
        enemyUnit.Setup(nextMonster);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextMonster.Base.Name}!");

        state = BattleStates.RunningTurn;

    }
    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleStates.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is SphereItem)
        {
            yield return ThrowSphere((SphereItem)usedItem);
        }

        //StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowSphere(SphereItem sphereItem)
    {
        state = BattleStates.Busy;
        dialogBox.EnableActionSelector(false);
        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer monster!");
            state = BattleStates.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.PlayerName} used {sphereItem.ItemName.ToUpper()}!");

        //TU GDZIES PONIZEJ TRZEBA DODAC LOGIKE ANIMACJI DLA KAZDEJ ZE SFER

        int shakeCount = TryToCatchMonster(enemyUnit.Monster, sphereItem);
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
            PlayerParty.AddMonster(enemyUnit.Monster);
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} has been added to your party");
            BattleOver(true);
        }
        else
        {
            yield return enemyUnit.PlayBrakeOutAnimation(originalScale);
            //Monster broke out
            yield return dialogBox.TypeDialog($"{enemyUnit.Monster.Base.Name} broke free");
            state = BattleStates.RunningTurn;
        }
        // Przywrócenie stanu po zakoñczeniu akcji
        //state = BattleState.Waiting;

    }

    // Sprawdzenie czy udalo sie zlapac monstera
    int TryToCatchMonster(Monster monster, SphereItem sphereItem)
    {
        float a = (3 * monster.MaxHp - 2 * monster.HP) * monster.Base.CatchRate * sphereItem.CatchRateModifier * ConditionsDB.GetStatusBonus(monster.Status) / (3 * monster.MaxHp);

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
    

    public BattleDialogBox DialogBox => dialogBox;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public PartyScreen PartyScreen => partyScreen;

    public AudioClip BattleVictoryMusic => battleVicoryMusic;
}
