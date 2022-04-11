using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleContainer : MonoBehaviour
{
    const string STRING1 = "ID: ";
    const string STRING2 = " --- Fitness: ";
    const string STRING3 = "\n";

    public static PuzzleContainer Instance;
    
    public int solverCount = 1;
    public int puzzleCount = 100;

    List<Solver> solvers = new List<Solver>();

    public int generation = 0;
    [SerializeField] Text dataBox;
    [SerializeField] Text dataBox2;
    [SerializeField] Text generationBox;
    [SerializeField] Text switchingBox;

    [SerializeField] GameObject generationScreen;
    [SerializeField] GameObject demoScreen;

    [SerializeField] Text[] spaces;
    [SerializeField] Text idBox;

    bool readyForNext = true;
    bool solving = false;
    bool switching = false;

    bool demonstrate = false;
    float tickTime = 0.25f;
    float switchTime = 2f;
    float demoTicker;

    void Start()
    {
        Instance = this;

        // Initially intended to solve multiple puzzles at onces. This doesn't work well with how I am handling the NNs, but the feature is still supported.
        int[] seeds = new int[puzzleCount * 7];
        for (int i = 0; i < puzzleCount; i++)
        {
            int[] nextBoard = RandomizeBoard();
            for (int j = 0; j < 7; j++)
                seeds[i * 7 + j] = nextBoard[j];
        }

        for (int i = 0; i < solverCount; i++)
        {
            solvers.Add(new Solver(i, seeds));
        }

        // Matrix m = new Matrix(3, 2) [1, 2, 3, 4, 5, 6];
        // Matrix n = new Matrix(3, 3) [1, 0, 0, 0, 1, 0, 0, 0, 1];
        // Debug.Log(m + "\n" + n + "\n" + m*n + "\n" + n*m);

        // NeuralNetwork nn = new NeuralNetwork(2, 1, 2);
        // Debug.Log(nn.Calculate(2f, 2f));
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            switching = true;
            switchingBox.text = "Switching...";
        }

        if (demonstrate)
        {
            demoTicker += Time.deltaTime;
            DrawBoard();

            if (demoTicker >= tickTime)
            {
                if (solvers[0].puzzles[0].gameEnded)
                {
                    if (demoTicker >= switchTime)
                    {
                        solvers[0].Reset();
                        demonstrate = false;
                        readyForNext = false;
                        switchingBox.text = "";
                        generationScreen.SetActive(true);
                        demoScreen.SetActive(false);
                    }
                }
                else
                {
                    demoTicker = 0f;
                    solvers[0].Solve();
                }
            }

        // For manual puzzle-solving. Used to make sure the demo works properly
        // if (!puzzle.gameEnded)
        // {
        //     if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) puzzle.MovePiece(1);
        //     if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) puzzle.MovePiece(2);
        //     if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) puzzle.MovePiece(3);
        //     if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) puzzle.MovePiece(4);
        //     if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) puzzle.MovePiece(5);
        //     if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) puzzle.MovePiece(6);
        // }

        // if (Input.GetKeyDown(KeyCode.Space)) puzzle.ResetBoard();


        }
        else
        {
            if (!solving) StartGeneration();

            if (solving)
            {
                for (int i = 0; i < solverCount; i++)
                    solvers[i].Solve();
            }
        }


    }

    void StartGeneration()
    {
        solvers.Sort((a, b) => b.runningFitness.CompareTo(a.runningFitness) );
        DrawFitness();

        // Copy top 25 NNs to replace the weights of the bottom 25.
        for (int i = 0; i < 25; i++)
        {
            solvers[75 + i].nn = new NeuralNetwork(solvers[i].nn);
            solvers[75 + i].nn.MutateWeights();
        }

        // Randomize the middle 50 for the next run.
        for (int i = 25; i < 75; i++)
        {
            solvers[i].nn.Randomize();
        }

        // Reset for the next generation
        for (int i = 0; i < solvers.Count; i++)
            solvers[i].Reset();

        generationBox.text = "Generation: " + generation.ToString();

        if (switching)
        {
            demonstrate = true;
            switching = false;
            generationScreen.SetActive(false);
            demoScreen.SetActive(true);
            demoTicker = -switchTime;
            solvers[0].Reset();
            return;
        }

        generation++;
        readyForNext = false;
        solving = true;
    }

    int[] RandomizeBoard()
    {
        int[] r = new int[7];

        int[] lows = MixArray(1, 2, 3);
        int[] highs = MixArray(4, 5, 6);
        
        r[0] = 0;
        r[1] = highs[0];
        r[2] = highs[1];
        r[3] = highs[2];
        r[4] = lows[0];
        r[5] = lows[1];
        r[6] = lows[2];

        return r;
    }

    int[] MixArray(params int[] ints)
    {
        int[] r = new int[ints.Length];
        List<int> iList = new List<int>(ints);

        for (int i = 0; i < ints.Length; i++)
        {
            int rand = Random.Range(0, iList.Count);
            r[i] = iList[rand];
            iList.RemoveAt(rand);
        }

        return r;
    }

    public void CheckFinish()
    {
        if (AllSolversDone()) 
            solving = false;
    }

    bool AllSolversDone()
    {
        for (int i = 0; i < solvers.Count; i++)
            if (!solvers[i].stopped)
                return false;

        return true;
    }

    void DrawFitness()
    {
        string d = "";
        string d2 = "";

        // 47 lines in the first column. Fits well in the default editor window.
        for (int i = 0; i < 47; i++)
        {
            d += STRING1 + solvers[i].id.ToString("00") + STRING2 + solvers[i].runningFitness.ToString("F3") + STRING3;
        }

        for (int i = 47; i < solvers.Count; i++)
        {
            d2 += STRING1 + solvers[i].id.ToString("00") + STRING2 + solvers[i].runningFitness.ToString("F3") + STRING3;
        }

        dataBox.text = d;
        dataBox2.text = d2;
    }

    void DrawBoard()
    {
        idBox.text = solvers[0].id.ToString("00");
        
        List<int> b = new List<int>(solvers[0].puzzles[0].board);

        for (int i = 1; i < b.Count; i++)
            spaces[b[i]].text = i.ToString();

        spaces[b[0]].text = "";

        spaces[7].text = "Moves: " + solvers[0].puzzles[0].moves.ToString();

        if (solvers[0].puzzles[0].gameEnded)
            spaces[8].text = "Fitness: " + solvers[0].puzzles[0].fitness.ToString("F3");
        else
            spaces[8].text = "";
    }
}
