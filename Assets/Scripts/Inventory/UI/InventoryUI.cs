using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image UpArrow;
    [SerializeField] Image DownArrow;

    //[SerializeField] PartyScreen partyScreen;
    //[SerializeField] MoveSelectionUI moveSelectionUI;

    int selectedCategory = 0;
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

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var slots = inventory.GetSlotsByCategory(selectedCategory);
        if (slots.Count > 0)
        {
            selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

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

        UpArrow.gameObject.SetActive(false);
        DownArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }


    public ItemBase SelectedItem => inventory.GetItem(selectedItem, selectedCategory);

    public int SelectedCategory => selectedCategory;
}
