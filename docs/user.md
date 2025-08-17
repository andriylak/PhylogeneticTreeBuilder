# Phylogenetic Tree Builder – User Guide

This program builds **phylogenetic trees** from a CSV distance matrix using two common algorithms:

- **UPGMA (Unweighted Pair Group Method with Arithmetic Mean)** – produces rooted, ultrametric trees.
- **Neighbor Joining (NJ)** – produces unrooted trees (but can be represented in Newick format).

The resulting tree is displayed in **Newick format** and can be saved, copied, or visualized with external tools.

---

## Requirements

- **.NET 8 Desktop Runtime** (or newer).  
- CSV file containing a **distance matrix**.  

---

## Input File Format (CSV)

Your input must be a **square, symmetric matrix** with labels.

### Example:

```csv
,A,B,C,D
A,0,5,9,9
B,5,0,10,10
C,9,10,0,8
D,9,10,8,0
```
- The first row contains column labels (`A`, `B`, `C`, ...).
- The first column contains row labels.
- The diagonal must be 0.
- Distances must be symmetric (D(A,B) = D(B,A)).

# How to Use
1. Start the application:
    - Run `PhylogeneticTreeBuilder.App.exe`.
2. Choose the algorithm
    - Use the dropdown to select `UPGMA` or `Neighbor Joining`.
3. Select your input file
    - Either type the path in the textbox, or
    Click Browse and select your `.csv` file.
4. Run the calculation
    - Click `Build Tree`.
    - If the input is valid, the result will appear in the Newick output box.
    - If there is an error (e.g., invalid matrix), you will see a popup window with the error message.
5. Save or copy results
    - Use `Copy` to put the Newick string on your clipboard.
    - Use `Save` to export it as a .txt file.

## Output

The program outputs the Newick tree format, for example:

`((A:2.5,B:2.5):5,(C:4,D:4):3);`

You can paste this into any phylogenetic visualization tool (e.g., iTOL, FigTree) to see the graphical tree.

## Error Messages

The application validates the matrix before building the tree. Common issues:

 - `“Row count does not match column count”` → Not a square matrix.
 - `“Diagonal must be zero”` → A self-distance is not 0.
 - `“Matrix not symmetric”` → Distances differ across diagonal.
 - `“Non-numeric value found”` → A cell contains text instead of a number.

Correct the CSV and try again.
