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

public enum BattleAction { Move, SwitchMonster, UseItem, UseSphere, Run}

public enum BattleTrigger { LongGrass, Water}
public class BattleSystem : MonoBehaviour
{
    //[SerializeField] private int monsterPartyCount = 5;
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
    public ItemBase SelectedItem { get; set; }

    public bool IsBattleOver { get; private set; }

    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty TrainerParty { get; private set; }
    public Monster WildMonster { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    public TrainerController Trainer { get; private set; }
    public int EscapeAttempts { get; set; }

    BattleTrigger battletrigger;

    public void StartBattle(MonsterParty playerParty, Monster wildMonster, 
        BattleTrigger trigger = BattleTrigger.LongGrass)
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
        Trainer = trainerParty.GetComponent<TrainerController>();

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
            trainerImage.sprite = Trainer.Sprite;

            yield return dialogBox.TypeDialog($"{Trainer.Name} wants to battle");

            //Send out first monster of the trainer
            //trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyMonster = TrainerParty.GetHealthyMonster();
            enemyUnit.Setup(enemyMonster);
            yield return dialogBox.TypeDialog($"{Trainer.Name} send out {enemyMonster.Base.Name}");


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

    public void HandleUpdate()
    {
        StateMachine.Execute();
        ///if (Input.GetKeyDown(KeyCode.T))
        ///{
        ///    if (state == BattleState.ActionSelection)
        ///    {
        ///        StartCoroutine(RunTurns(BattleAction.UseSphere));
        ///    }
        ///}
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

    public IEnumerator SendNextTrainerMonster()
    {
        var nextMonster = TrainerParty.GetHealthyMonster();
        enemyUnit.Setup(nextMonster);
        yield return dialogBox.TypeDialog($"{Trainer.Name} send out {nextMonster.Base.Name}!");
    }

    public IEnumerator ThrowSphere(SphereItem sphereItem)
    {
        dialogBox.EnableActionSelector(false);
        if (IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal the trainer monster!");
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
