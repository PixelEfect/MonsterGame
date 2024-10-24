using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSlot : MonoBehaviour, ISelectableItem
{

    [SerializeField] Text text;

    Color originalColor;
    public void Init()
    {
        originalColor = text.color;
    }

    public void Clear()
    {
        text.color = originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        text.color = (selected) ? GlobalSettings.i.HighlightColor : originalColor;
    }

    public void SetText (string s) 
    {
        text.text = s;
    }

}
