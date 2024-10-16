using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy}

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image UpArrow;
    [SerializeField] Image DownArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    Action<ItemBase> OnItemUsed;

    int selectedCategory = 0;
    MoveBase moveToLearn;

    InventoryUIState state;

    const int itemsInViewport = 8;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;
    RectTransform itemListRect;
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform> ();
    }

    private void Start()
    {
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        //Clear all the existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        SetItems(slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());

        UpdateSelectionInUI();
    }

    public override void HandleUpdate()
    {
        int prevCategories = selectedCategory;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++selectedCategory;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --selectedCategory;
        }
        if (selectedCategory > Inventory.ItemCategories.Count - 1)
        {
            selectedCategory = 0;
        }
        else if (selectedCategory < 0)
        {
            selectedCategory = Inventory.ItemCategories.Count - 1;
        }

        if (prevCategories != selectedCategory)
        {
            ResetSelection();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
            UpdateItemList();
        }

        base.HandleUpdate();
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if (GameController.Instance.State == GameState.Shop) 
        {
            OnItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }

        if (GameController.Instance.State == GameState.Battle)
        {
            // In Battle
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            // Outside Battle
            if (!item.CanUseOutBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in this moment");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if (selectedCategory == (int)ItemCategory.Sphere)
        {
            StartCoroutine (UseItem());
        }
        else
        {
            OpenPartyScreen();

            if(item is SpItem)
            {
                partyScreen.ShowIfSpIsUsable(item as SpItem);
            }
        }
    }
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleSPItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var monster = partyScreen.SelectedMember;
        // Handle Evolution Items
        if (item is EvolutionItem)
        {
            var evolution = monster.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(monster, evolution);
            }
            else
            {
                yield return DialogManager.Instance.ShowDialogText($"Not used massage - inventoryUI");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
        if (usedItem != null)
        {
            if (usedItem is RecoveryItem)
            {
                yield return DialogManager.Instance.ShowDialogText($"{usedItem.UseMassage}");
            }
            OnItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCategory == (int)ItemCategory.Items)
            {
                yield return DialogManager.Instance.ShowDialogText($"Not used massage - inventoryUI");
            }
        }
        ClosePartyScreen();
    }

    IEnumerator HandleSPItems()
    {
        var spItem = inventory.GetItem(selectedItem, selectedCategory) as SpItem;
        if (spItem == null)
        {
            yield break;
        }
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
            yield return ChooseMoveToForget(monster, spItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
            yield return new WaitForSeconds(2f);
        }
    }
    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move you wan't to forget", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var slots = inventory.GetSlotsByCategory(selectedCategory);
        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport ) return;
        float scrollPos =Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        UpArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        DownArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelection()
    {
        selectedItem = 0;

        UpArrow.gameObject.SetActive(false);
        DownArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.ClearMemberSlotMessage();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var monster = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();
        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == MonsterBase.MaxNumOfMoves)
        {
            // Dont learn the new move
            yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} did not learn {moveToLearn.MoveName}");
        }
        else
        {
            // Forget and learn
            var selectedMove = monster.Moves[moveIndex].Base;
            yield return DialogManager.Instance.ShowDialogText($"{monster.Base.Name} forgot {selectedMove.MoveName} and learned {moveToLearn.MoveName}");
            monster.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
