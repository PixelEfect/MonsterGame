using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ItemSlotUI : MonoBehaviour
{

    [SerializeField] Text nameText;
    [SerializeField] Text countText;

    RectTransform rectTransform;

    private float initialHeight = 0;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)      //moj wlasny if niweluje blad  - brak wczytania recttransform przy karmieniu potkami
        {
            Debug.LogError("Brak komponentu RectTransform na obiekcie " + gameObject.name);
        }
        else
        {
            initialHeight = rectTransform.rect.height;
        }
    }

    public Text NameText => nameText;
    public Text CountText => countText;


    public float Height => initialHeight;
    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.ItemName;
        countText.text = $"X {itemSlot.Count}";
    }

}
