using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHandler : MonoBehaviour
{
    public int[] boardFlat;
    public int boardSize;
    public List<MoveData> presetMoves; // List of preset moves

    public void BoardData (int[,] board,int size)
    {
        boardSize = size;
        boardFlat = new int[boardSize * boardSize];
        presetMoves = new List<MoveData>();
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                boardFlat[y * boardSize + x] = board[x,y];
            }
        }
    }

    public int[,] ToArray ()
    {
        int[,] array = new int[boardSize,boardSize];
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                array[x,y] = boardFlat[y * boardSize + x];
            }
        }
        return array;
    }
}

[System.Serializable]
public class MoveData
{
    public int x;
    public int y;
    public int player; // 1 for black, 2 for white

    public MoveData (int x,int y,int player)
    {
        this.x = x;
        this.y = y;
        this.player = player;
    }
}

