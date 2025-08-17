# Phylogenetic Tree Builder

## Specification

Phylogenetic trees are key tools in molecular biology used to understand evolutionary relationships between species or genes. This project aims to implement two foundational algorithms for tree construction: UPGMA (Unweighted Pair Group Method with Arithmetic Mean) and Neighbor Joining (NJ). These methods will be used to construct trees based on distance matrices derived from DNA or protein sequences. The resulting trees will visually represent hypothesized evolutionary paths and similarities among the input organisms or sequences.
The goal is to build a C# application that takes a precomputed distance matrix (CSV format), and constructs a phylogenetic tree (in Newick format) using the chosen algorithm (UPGMA or NJ).  

### Problem Formalization.

#### Input: 
Symmetric distance matrix in CSV format. The user will also specify which algorithm to use (UPGMA or NJ). 

#### Output: 

A phylogenetic tree represented in Newick format.

Testing of all functions and methods will be performed using xUnit.

## Documentation

* [User Guide](docs/user.md)
* [Programming documentation](docs/programmer.md)
