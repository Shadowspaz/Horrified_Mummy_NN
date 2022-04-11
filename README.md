# Horrified_Mummy_NN

This was my first neural network project, built to solve a simple sliding-piece puzzle from the board game "Horrified."

![Puzzle Example](https://user-images.githubusercontent.com/6518580/162818854-8a637bfc-b08d-4901-84b3-36c2d5423890.png)

The goal is to slide the green-numbered pieces across the lines, shuffling them to get the tiles next to the appropriately-numbered spaces.

---

### Neural Network

The network iteself consists of 13 inputs:
- Each position on the board, including the blank space (7 inputs)
- The distance each numbered tile is away from its target space (6 inputs)

And 6 outputs, which determines which piece to move: 1-6. No direction is needed, because any piece can only be moved to the empty space.

The fitness algorithm is a combination of two values: How many moves it took, and how "solved" the puzzle was.
- With an upper limit of 50 moves, the first part of the fitness score is simply the inverse of moves taken. 10/50 moves results in a 40/50 score, or 0.8.
- The second part is the change in cumulative distance from the start of the puzzle to the  end state. For a win, the cumulative distance would be zero, resulting in a score of 1. If it stayed just as scrambled as it did at the start, it would be a score of zero, and if it somehow became _more_ scrambled, this score could be negative.
- The average of these two scores returns the overall fitness value, and as the puzzles are consistently solved, the only difference from one neural network to the next will be how efficiently it can be solved.

---

### Generations and Mutations

The solving process starts with 100 different networks all working on solving the same puzzle. At the beginning, each of these have randomized weights.

After all networks have solved the puzzle (Or hit the 50 move limit), they are sorted by fitness. To start the next generation, the top 25 networks have their weights mutated to replace the bottom 25, and the second generation begins.

Mutation is a random process applied to each weight in a neural network, and it can have one of three outcomes:
 - Invert the weight (multiply by -1)
 - Adjust by a small amount (80% - 120%)
 - Adjust by a large amount (50% - 150%)

These are purely arbitrary adjustments that could likely be optimized in some way. But for this project, I just wanted a way to randomly tweak values without just fully regenerating them. Otherwise, they lose whatever properties made a given network perform well in the first place.

---

### Execution and Simulation

When the project starts, a puzzle is randomly generated and the 100 neural networks will begin solving over generations. A live output of each fitness is shown:
![Fitness calculations](https://user-images.githubusercontent.com/6518580/162825214-55e598cf-a53f-4267-9f8b-23c54234b600.png)

At any point, the user can press the space bar to watch the highest-rated network solve the puzzle.

---

### Conclusions

In my testing, it appears that every puzzle can be solved in 19 moves or less, and it will take anywhere between 200 and 500 generation to reach an optimal state. This is not an efficient neural network by any means, but as a proof of concept and a first draft working with neural networks, I'm really happy with the results.

---

#### Known Bugs and Issues

- Occasionally, the top rated network will not stay at the top. They can flicker between two different solves, results in different fitness values over multiple generations, until they are surpassed entirely or become mutated and the issue fixes itself.
- After previewing a solve, the generation may be reset. All fitnesses will become 0, and the process will essentially start over. I suspect this is an issue with how the generation-handling is paused and resumed for the preview.
- When previewing a solve, it will start with the first move already made.
