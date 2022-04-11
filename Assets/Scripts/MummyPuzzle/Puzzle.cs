using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle
{
    const int moveLimit = 50;

    public int[] board = new int[7]; // Index = piece, value = position on board
    public int[] startBoard = new int[7];
    float startDist;
    public int[][] map = new int[7][] // All possible moves from [index] position on board
    {
        new int[6]{ 1, 2, 3, 4, 5, 6 },
        new int[3]{ 2, 0, 6 },
        new int[3]{ 1, 0, 3 },
        new int[3]{ 2, 0, 4 },
        new int[3]{ 3, 0, 5 },
        new int[3]{ 4, 0, 6 },
        new int[3]{ 1, 0, 5 }
    };

    public int moves =  0;
    public bool gameEnded = false;
    public float fitness;

    public Puzzle(params int[] newBoard)
    {
        board = newBoard;
        SaveStart();
    }

    void SaveStart()
    {
        for (int i = 0; i < board.Length; i++)
            startBoard[i] = board[i];

        startDist = CalcDistance();
    }

#region Public Methods
    public void MovePiece(int piece)
    {
        if (CheckPiece(piece))
        {
            int temp = board[0];
            board[0] = board[piece];
            board[piece] = temp;
            moves++;
        }
        else
        {
            Debug.Log("Illegal move!");
        }

        CheckEnd();
    }

    void CheckEnd()
    {
        bool win = false;
        if (CheckWin()) 
        {
            gameEnded = true;
            win = true;
        }
        else gameEnded = (moves >= moveLimit);

        if (!gameEnded) return;

        CalculateFitness(win);
    }

    void CalculateFitness(bool win)
    {
        float v = 0f;
        v = (float)moves;

        // if win, this value will be zero.
        if (!win)
        {
            float distVal = (CalcDistance() / startDist) * 50f;
            v += distVal;
        }

        // Calculated as the inverse of (moves + distance)
        fitness = (100f - v) / 100f;
    }

    // Calculates cumulative distance between each piece and its target space. Used for fitness
    public float CalcDistance()
    {
        float r = 0;
        for (int i = 1; i < board.Length; i++)
        {
            if (System.Array.IndexOf(map[board[i]], i) > -1)
                r += 1;
            else if (board[i] != i)
                r += 2;
        }

        return r;
    }

    public float CalcDistance(int i)
    {
        if (System.Array.IndexOf(map[board[i]], i) > -1)
            return 1; // If target is in its move array, it is one space away
        else if (board[i] != i)
            return 2; // Otherwise, it's 2. This is the size limit of the puzzle.
        else
            return 0; // Or it's in the right place.
    }

    // Check if the empty space is a space the selected piece can move to. Otherwise, it is an illegal move.
    public bool CheckPiece(int piece)
    {
        return System.Array.IndexOf(map[board[piece]], board[0]) > -1;
    }

    public bool CheckWin()
    {
        for (int i = 0; i < board.Length; i++)
            if (board[i] != i) return false;

        return true;
    }

    public void ResetBoard()
    {
        for (int i = 0; i < board.Length; i++)
            board[i] = startBoard[i];

        moves = 0;
        fitness = 0f;
        gameEnded = false;
    }
#endregion
}
