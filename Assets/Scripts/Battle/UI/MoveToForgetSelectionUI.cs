using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MoveToForgetSelectionUI : SelectionUI<TextSlot>
{

    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Text> detailMoveTexts;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].MoveName;
            detailMoveTexts[i].text = "PP: " + currentMoves[i].PP + " Type: " + currentMoves[i].Type;
        }

        moveTexts[currentMoves.Count].text = newMove.MoveName;
        detailMoveTexts[currentMoves.Count].text = "PP: " + newMove.PP + "   Type: " + newMove.Type;

        SetItems(moveTexts.Select(m => m.GetComponent<TextSlot>()).ToList());
    }
}
