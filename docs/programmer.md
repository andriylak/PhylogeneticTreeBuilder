# Phylogenetic Tree Builder – Developer Documentation

This document describes the design and API of the **Phylogenetic Tree Builder** project.  
It covers all major classes, their methods, exceptions, and usage notes.

---

## Core Library

### Class: `DistanceMatrix`

**Purpose:**  
Load, validate, and maintain a dynamic pairwise distance structure plus the active set of clusters during agglomerative algorithms (UPGMA/NJ).

#### Properties

- `Dictionary<(int, int), double> Distances`  
  Stores upper-triangle distances with normalized keys `(min, max)`.

- `Dictionary<int, Cluster> Clusters`  
  Active clusters by **1-based** id.

- `PriorityQueue<(int, int), double> Queue`  
  Min-heap of candidate pairs with priority = distance.

---

#### `DistanceMatrix(string csvPath)`

**Purpose:** Parse CSV, validate it as a distance matrix, build initial clusters.

**Steps:**
1. Read lines from file.  
2. Validate that matrix is square and labels match.  
3. Validate row width, label uniqueness, and symmetry.  
4. Parse numeric values with `InvariantCulture`.  
5. Create leaf `Cluster` objects and fill `Distances` + `Queue`.

**Exceptions:**
- `InvalidDataException` with descriptive messages (non-square, missing value, symmetry error, etc.).  
- File I/O exceptions (`IOException`, etc.).

---

#### `double GetDistance(int i, int j)`

Returns the symmetric distance between clusters `i` and `j`.

- **Parameters:** `i`, `j` (1-based).  
- **Exceptions:** `KeyNotFoundException` if key doesn’t exist.  

---

#### `(int, int) GetMinDistance()`

Dequeues the closest valid pair.

- **Returns:** `(i, j)` where both are still in `Clusters`.  
- **Exceptions:** `InvalidOperationException` if no valid pairs remain.  

---

#### `void RemoveClusters(int i, int j)`

Removes merged clusters and their distances.

- Deletes `Clusters[i]`, `Clusters[j]`.  
- Deletes all entries from `Distances` containing `i` or `j`.  

---

#### `void AddCluster(int id, Cluster cluster)`

Registers a new merged cluster.  
- Usually `id = min(oldA, oldB)` for determinism.  

---

#### `void UpdateDistances(int newId, Cluster a, Cluster b)`

Computes new distances to all remaining clusters.

- Formula (UPGMA):  
D(AB,k) = (|A| * D(A,k) + |B| * D(B,k)) / (|A| + |B|)

- Inserts results into `Distances` and pushes to `Queue`.

**Call order:**  
`UpdateDistances → RemoveClusters → AddCluster`

---

---

### Class: `Cluster`

**Purpose:** Represent an active cluster, its size, and tree representation.

#### Properties
- `int Id` – cluster id.  
- `TreeNode Node` – root node of its subtree.  
- `int Size` – number of original taxa.  
- `double Height` – convenience (subtree height).

---

#### `Cluster(int id, TreeNode node)`  

Creates a singleton cluster with `Size = 1`.

---

#### `static Cluster Merge(Cluster a, Cluster b, int newId, TreeNode node)`

Creates a merged cluster.

- `Size = a.Size + b.Size`  
- `Node = node`  

Does not mutate `a` or `b`.

---

---

### Class: `TreeNode`

**Purpose:** Represents nodes in the phylogenetic tree.

#### Properties
- `string Name` – taxon name or generated label.  
- `List<TreeNode> Children` – child nodes.  
- `List<double> Heights` – branch lengths to each child.

---

#### `TreeNode(string name)`

Creates a leaf node.

---

#### `TreeNode()`

Default constructor, initializes empty `Children` and `Heights`.

---

#### `void AddChild(TreeNode child, double length)`

Adds a child with its branch length.  
Ensures `Children.Count == Heights.Count`.

---

#### `double GetMaxHeight()`

Computes longest root-to-leaf path in this subtree.  

---

#### `string ToNewick(double? lengthFromParent = null, bool first = false)`

Serializes the tree to **Newick** format.

- **Leaf:** `"{Name}:{len}"`  
- **Internal:** `"({child1},{child2},...):{len}"`  
- Root omits the final branch length if `lengthFromParent == null`.

---

---

### Class: `UPGMAEngine`

**Purpose:** Builds an ultrametric (rooted, binary) tree via UPGMA.

#### `TreeNode BuildTree(DistanceMatrix matrix)`

Algorithm:

1. Get `(i, j) = matrix.GetMinDistance()`.  
2. Fetch clusters `A`, `B`.  
3. Compute distance `d = D(i, j)`.  
4. Branch lengths:  
 - `left = d/2 − A.Node.GetMaxHeight()`  
 - `right = d/2 − B.Node.GetMaxHeight()`  
5. Create parent node, attach children.  
6. `newId = min(i, j)` → merged cluster.  
7. Update distances, remove old, add new.  

**Returns:** root node of the final tree.

---

---

### Class: `NeighborJoining`

**Purpose:** Builds unrooted phylogenetic trees.

#### `TreeNode BuildTree(DistanceMatrix matrix)`

Algorithm:
- Compute totals `r_i = Σ_j D(i,j)`.  
- Build Q-matrix: `Q(i,j) = (n−2)D(i,j) − r_i − r_j`.  
- Pick `(i, j)` minimizing Q.  
- Compute branch lengths:  
`L(i,u) = 0.5 * D(i,j) + (r_i - r_j) / (2(n-2))` 
`L(j,u) = D(i,j) - L(i,u)`
- New cluster `u`, update distances:  
`D(u,k) = (D(i,k) + D(j,k) - D(i,j)) / 2`

---

---

## Windows Application

### MainForm

UI wrapper for selecting algorithm, loading CSV, and displaying Newick.

#### Controls
- `ComboBox cmbAlgorithm` – choose “UPGMA” or “Neighbor Joining”.  
- `TextBox txtCsvPath` – path to CSV.  
- `Button btnBrowse` – select CSV.  
- `Button btnRun` – run algorithm.  
- `TextBox txtNewick` – output Newick format.  
- `Button btnCopy` – copy Newick.  
- `Button btnSave` – save Newick.  

---

#### Event Handlers

- `BtnBrowse_Click` → choose file with `OpenFileDialog`.  
- `BtnRun_Click` → run selected engine, handle errors, display Newick.  
- `BtnCopy_Click` → copy output.  
- `BtnSave_Click` → save to file.  

---

## Error Handling

- `InvalidDataException` – thrown on invalid CSV matrices (format, symmetry, diagonal).  

---

## Conventions & Notes

- **Cluster IDs:** 1-based indexing. 
- **ID of new merged Cluster** is `(n + 1)` where n is the maximum ID before.   
- **Distances:** Always symmetric; only `(i<j)` stored.  
- **Newick:** Always formatted with `InvariantCulture` and `.` decimal.  
- **PriorityQueue:** Accepts staleness; filtered out in `GetMinDistance()`.

---

## Complexity

- Construction: **O(n²)**.  
- UPGMA loop: **O(n² log n)**.
- Neighbor Joining loop: **O(n³)**
- Memory: ~ **n(n−1)/2** doubles.  
