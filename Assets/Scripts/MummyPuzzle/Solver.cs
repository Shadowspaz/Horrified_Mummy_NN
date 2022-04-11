using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver
{
    public int id;

    public NeuralNetwork nn;
    Matrix outputs;
    Matrix moveMask;
    int prevMove = 0;

    public bool stopped = false;

    public List<Puzzle> puzzles = new List<Puzzle>();
    int puzzleIndex = 0;
    public float runningFitness = 0f;
    int completePuzzles = 0;

    public Solver(int myID, int[] seed)
    {
        id = myID;
        nn = new NeuralNetwork(13, 6, 13, 13, 12, 11);
        outputs = new Matrix(1, 6);
        moveMask = new Matrix(1, 6);

        for (int i = 0; i < seed.Length; i += 7)
        {
            puzzles.Add(new Puzzle(seed[i], seed[i + 1], seed[i + 2], seed[i + 3], seed[i + 4], seed[i + 5], seed[i + 6]));
        }
    }

    public void Solve()
    {
        if (stopped) return;

        if (puzzles[puzzleIndex].gameEnded) EndPuzzle();

        // Add boardstate and distances to the NN as inputs
        List<float> nextInput = new List<float>(System.Array.ConvertAll(puzzles[puzzleIndex].board, x => (float)x));
        for (int i = 1; i < 7; i++)
            nextInput.Add(puzzles[puzzleIndex].CalcDistance(i));

        // Run the neural network
        outputs = nn.Calculate(nextInput.ToArray());
        UpdateMask();

        int nextMove = 0;
        float max = int.MinValue;

        // Find the best legal move
        for (int i = 0; i < 6; i++)
        {
            if (moveMask[0, i] <= 0f) continue;
            
            if (outputs[0, i] > max)
            {
                nextMove = i + 1;
                max = outputs[0, i];
            }
        }
        puzzles[puzzleIndex].MovePiece(nextMove);
        prevMove = nextMove;
        //Debug.Log(nextMove + ", " + max);
    }

    // Used to mask out illegal moves from the neural network
    void UpdateMask()
    {
        for (int i = 1; i < 7; i++)
            moveMask[0, i - 1] = (puzzles[puzzleIndex].CheckPiece(i) && i != prevMove) ? 1 : 0;
    }

    void EndPuzzle()
    {
        // Initially meant to support solving multiple puzzles with the same network. This is hard to do.
        runningFitness *= (float)completePuzzles;
        runningFitness += puzzles[puzzleIndex].fitness;
        //Debug.Log("Solved puzzle " + completePuzzles + ", Fitness = " + puzzles[puzzleIndex].fitness);
        completePuzzles++;
        runningFitness /= (float)completePuzzles;

        puzzles[puzzleIndex].ResetBoard();

        // if (id == PuzzleContainer.Instance.debugID)
        // {
        //     string s = "";
        //     for (int i = 0; i < 7; i++)
        //         s += puzzles[puzzleIndex].board[i];

        //     Debug.Log(runningFitness + ", " + completePuzzles + ", " + s);
        // }

        if (puzzleIndex >= puzzles.Count - 1) 
        {
            stopped = true;
            PuzzleContainer.Instance.CheckFinish();
        }
        else puzzleIndex++;
    }

    public void Reset()
    {
        puzzleIndex = 0;
        runningFitness = 0;
        completePuzzles = 0;
        stopped = false;
        prevMove = 0;
    }

    [ContextMenu("Mutate")]
    public void Mutate()
    {
        nn.MutateWeights();
    }

    [ContextMenu("Regenerate")]
    public void Regenerate()
    {
        nn.Randomize();
    }
}