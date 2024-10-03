using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{

    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Text> detailMoveTexts;
    [SerializeField] Color highlightedColor;

    int currentSelection = 0;
    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
            detailMoveTexts[i].text = "PP: " + currentMoves[i].PP + " Type: " + currentMoves[i].Type;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
        detailMoveTexts[currentMoves.Count].text = "PP: " + newMove.PP + "   Type: " + newMove.Type;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
        currentSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentSelection--;

        currentSelection = Mathf.Clamp(currentSelection, 0, MonsterBase.MaxNumOfMoves);

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onSelected?.Invoke(4);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i =0; i < MonsterBase.MaxNumOfMoves+1; i++)
        {
            if (i == selection)
            {
                moveTexts[i].color = highlightedColor;
                detailMoveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
                detailMoveTexts[i].color = Color.black;
            }
        }
    }

}
