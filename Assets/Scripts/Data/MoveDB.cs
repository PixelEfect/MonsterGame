using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{

    static Dictionary<string, MoveBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();

        var moveList = Resources.LoadAll<MoveBase>("");
        foreach (var move in moveList)
        {
            if (moves.ContainsKey(move.MoveName))
            {
                Debug.LogError($"There are two moves with the name {move.MoveName}");
                continue;
            }

            moves[move.MoveName] = move;
        }
    }

    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Move with name {name} not found in the database");
            return null;
        }
        return moves[name];
    }
}
