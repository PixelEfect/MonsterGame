using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTurnState : State<BattleSystem>
{
    public static RunTurnState i {  get; private set; }

    private void Awake()
    {
        i = this; 
    }
    BattleSystem bs;

    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        StartCoroutine(RunTurns(bs.SelectedAction));
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {

        if (playerAction == BattleAction.Move)
        {
            bs.PlayerUnit.Monster.CurrentMove = bs.PlayerUnit.Monster.Moves[bs.SelectedMove];
            bs.EnemyUnit.Monster.CurrentMove = bs.EnemyUnit.Monster.GetRandomMove();
            int playerMovePriority = bs.PlayerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = bs.EnemyUnit.Monster.CurrentMove.Base.Priority;
            //Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = bs.PlayerUnit.Monster.Speed >= bs.EnemyUnit.Monster.Speed;
            }

            var firstUnit = (playerGoesFirst) ? bs.PlayerUnit : bs.EnemyUnit;
            var secondUnit = (playerGoesFirst) ? bs.EnemyUnit : bs.PlayerUnit;
            var secoundMonster = secondUnit.Monster;

            //First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (bs.IsBattleOver)
                yield break;

            if (secoundMonster.HP > 0)
            {
                //Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (bs.IsBattleOver)
                {
                    yield break;
                }
            }

        }
        else
        {
            if (playerAction == BattleAction.SwitchMonster)
            {
                yield return bs.SwitchMonster(bs.SelectedMonster);
            }
            //else if (playerAction == BattleAction.UseSphere)
            //{
            //    bs.DialogBox.EnableActionSelector(false);
            //    yield return ThrowSphere();
            //}
            else if (playerAction == BattleAction.UseItem)
            {
                // This is handled from item screen, so do nothing and skip to enemy move
                bs.DialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {

                yield return TryToEscape();
            }

            //Enemy Turn
            var enemyMove = bs.EnemyUnit.Monster.GetRandomMove();
            yield return RunMove(bs.EnemyUnit, bs.PlayerUnit, enemyMove);
            yield return RunAfterTurn(bs.EnemyUnit);
            if (bs.IsBattleOver)
            {
                yield break;
            }
        }
        if (!bs.IsBattleOver)
        {
            bs.StateMachine.ChangeState(ActionSelectionState.i);
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
        yield return bs.DialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} used {move.Base.MoveName}");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1);
            targetUnit.PlayHitAnimation();

            AudioManager.i.PlaySfx(AudioId.Hit);

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

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Monster.HP > 0)

            {
                foreach (var secondary in move.Base.Secondaries)
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
                targetUnit.Monster.CureStatus();    // moja implementacja
                yield return HandleMonsterFainted(targetUnit);
            }
        }
        else
        {
            yield return bs.DialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}'s attack missed");
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
        if (bs.IsBattleOver)
            yield break;

        // Statuses like burn or psn will hurt the monster after the turn
        sourceUnit.Monster.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.WaitForHpUpdate();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return HandleMonsterFainted(sourceUnit);
        }
    }

    IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return bs.DialogBox.TypeDialog(message);
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
        yield return bs.DialogBox.TypeDialog($"{faintedUnit.Monster.Base.Name} fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (bs.IsTrainerBattle)
            {
                battleWon = bs.TrainerParty.GetHealthyMonster() == null;
            }
            if (battleWon)
            {
                AudioManager.i.PlayMusic(bs.BattleVictoryMusic);
            }
            //Exp Gain
            int expYield = faintedUnit.Monster.Base.ExpYield;
            int enemyLevel = faintedUnit.Monster.Level;
            float trainerBonus = (bs.IsTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            bs.PlayerUnit.Monster.Exp += expGain;
            yield return bs.DialogBox.TypeDialog($"{bs.PlayerUnit.Monster.Base.Name} gained {expGain} exp");
            yield return bs.PlayerUnit.Hud.SetExpSmooth();

            //Check Level Up
            while (bs.PlayerUnit.Monster.CheckForLevelUp())
            {
                bs.PlayerUnit.Hud.SetLevel();
                yield return bs.DialogBox.TypeDialog($"{bs.PlayerUnit.Monster.Base.Name} grew to level {bs.PlayerUnit.Monster.Level}");

                //Try to learn a new Move
                var newMove = bs.PlayerUnit.Monster.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (bs.PlayerUnit.Monster.Moves.Count < MonsterBase.MaxNumOfMoves)
                    {
                        bs.PlayerUnit.Monster.LearnMove(newMove.Base);
                        yield return bs.DialogBox.TypeDialog($"{bs.PlayerUnit.Monster.Base.Name} learned {newMove.Base.MoveName}");
                        bs.DialogBox.SetMoveNames(bs.PlayerUnit.Monster.Moves);
                    }
                    else
                    {
                        //yield return bs.DialogBox.TypeDialog($"{bs.PlayerUnit.Monster.Base.Name} trying to learn {newMove.Base.MoveName}");
                        //yield return bs.DialogBox.TypeDialog($"But it cannot learn more than {MonsterBase.MaxNumOfMoves} moves");
                        //yield return ChooseMoveToForget(bs.PlayerUnit.Monster, newMove.Base);
                        //yield return new WaitUntil(() => state != BattleStates.MoveToForget);
                        //yield return new WaitForSeconds(2f);
                    }
                }

                yield return bs.PlayerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        yield return CheckForBattleOver(faintedUnit);
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextMonster = bs.PlayerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                yield return GameController.Instance.StateMachine.PushAndWait(PartyState.i);
                yield return bs.SwitchMonster(PartyState.i.SelectedMonster);
            }
            else
            {
                bs.BattleOver(false);
            }
        }
        else
        {
            if (!bs.IsTrainerBattle)
            {
                bs.BattleOver(true);
            }
            else
            {
                var nextMonster = bs.TrainerParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    //Send Out next monster
                    yield break;//StartCoroutine(AboutToUse(nextMonster));
                }
                else
                {
                    bs.BattleOver(true);
                }

            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return bs.DialogBox.TypeDialog("A critical hit!");
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return bs.DialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return bs.DialogBox.TypeDialog("It's not very effective!");
        }
    }

    IEnumerator TryToEscape()
    {
        bs.DialogBox.EnableActionSelector(false);
        if (bs.IsTrainerBattle)
        {
            yield return bs.DialogBox.TypeDialog($"You can't run from trainer battles!");
            yield break;
        }

        ++bs.EscapeAttempts;
        int playerSpeed = bs.PlayerUnit.Monster.Speed;
        int enemySpeed = bs.EnemyUnit.Monster.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return bs.DialogBox.TypeDialog($"Ran away safely!");
            bs.BattleOver(true);
        }
        float f = (playerSpeed * 128) / enemySpeed + 30 * bs.EscapeAttempts;
        f = f % 256;

        if (UnityEngine.Random.Range(0, 256) < f)
        {
            yield return bs.DialogBox.TypeDialog($"Ran away safely!");
            bs.BattleOver(true);
        }
        else
        {
            yield return bs.DialogBox.TypeDialog($"Can't escape!");
        }
    }
}
