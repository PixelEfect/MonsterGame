using GDEUtils.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    //Output
    public bool ItemUsed { get; private set; }

    public static UseItemState i { get; private set; }
    Inventory inventory;
    private void Awake()
    {
        i = this;
        inventory = Inventory.GetInventory();
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        ItemUsed = false;
        StartCoroutine(UseItem());
    }

    IEnumerator UseItem()
    {
        var item = inventoryUI.SelectedItem;
        var monster = partyScreen.SelectedMember;
        // Scroll and Rune
        if (item is SpItem)
        {
            yield return HandleSPItems();
        }
        // Other
        else
        {
            if (item is EvolutionItem)
            {
                var evolution = monster.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionState.i.Evolve(monster, evolution);
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialogText($"Not used massage - inventoryUI");
                    gc.StateMachine.Pop();
                    yield break;
                }
            }

            var usedItem = inventory.UseItem(item, partyScreen.SelectedMember);
            if (usedItem != null)
            {
                ItemUsed = true;

                if (usedItem is RecoveryItem)
                {
                    yield return DialogManager.Instance.ShowDialogText($"{usedItem.UseMassage}");
                }
            }
            else
            {
                if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                {
                    yield return DialogManager.Instance.ShowDialogText($"Not used massage - inventoryUI");
                }
            }
        }
        // Handle Evolution Items
        gc.StateMachine.Pop();
    }

    IEnumerator HandleSPItems()
    {
        var spItem = inventoryUI.SelectedItem as SpItem;
        if (spItem == null)
            yield break;

        var monster = partyScreen.SelectedMember;

        if (monster.HasMove(spItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} already know {spItem.Move.MoveName}");
            yield break;
        }

        if (!spItem.CanBeTaught(monster))
        {
            yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} can't learn {spItem.Move.MoveName}");
            yield break;
        }

        if (monster.Moves.Count < MonsterBase.MaxNumOfMoves)
        {
            monster.LearnMove(spItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} learned {spItem.Move.MoveName}");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} trying to learn {spItem.Move.MoveName}");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {MonsterBase.MaxNumOfMoves} moves");
            
            yield return DialogManager.Instance.ShowDialogText($"Choose a move you wan't to forget", true, false);

            MoveToForgetState.i.CurrentMoves = monster.Moves.Select(m => m.Base).ToList();
            MoveToForgetState.i.NewMove = spItem.Move;
            yield return gc.StateMachine.PushAndWait(MoveToForgetState.i);

            int moveIndex = MoveToForgetState.i.Selection;
            if (moveIndex == MonsterBase.MaxNumOfMoves || moveIndex == -1)
            {
                // Dont learn the new move
                yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} did not learn {spItem.Move.MoveName}");
            }
            else
            {
                // Forget and learn
                var selectedMove = monster.Moves[moveIndex].Base;
                yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} forgot {selectedMove.MoveName} and learned {spItem.Move.MoveName}");
                monster.Moves[moveIndex] = new Move(spItem.Move);
            }
            //yield return new WaitForSeconds(2f);
        }
    }

    //public override void Execute()
    //{
    //    inventoryUI.HandleUpdate();
    //}
    //public override void Exit()
    //{
    //    inventoryUI.gameObject.SetActive(false);
    //    inventoryUI.OnSelected -= OnItemSelected;
    //    inventoryUI.OnBack -= OnBack;
    //}

    //void OnItemSelected(int selection)
    //{
    //    gc.StateMachine.Push(GamePartyState.i);
    //}

    //void OnBack()
    //{
    //    gc.StateMachine.Pop();
    //}
}
