This repo contains tools to normalise [17land's public datasets](https://www.17lands.com/public_datasets).

Under the Releases tab, there are normalised JSON versions of the 17lands public datasets for various formats.

# Motivation

While a sparse dataframe might be more familiar/convenient for heavy data analysis using something like pandas in the hands of a data science professional, I personally found a hierarchical object-based model more intuitive. The transformed data also serialises much more efficiently, reducing file sizes 5-10x.

# What's in this repo?

### [Analysis.ipynb](/MtgDraftAnalyser/Analysis.ipynb) 
demonstrates how one might perform analysis using the JSON data, via C# and LINQ queries.

### [Process17landsDraftData.ps1](Process17landsDraftData.ps1) 
automates the process of
- pulling down 17lands data
- parsing and transforming
- serialising to JSON
- zipping

It assumes 7Zip is installed (on Windows).

### [MtgDraftAnalyser.sln](MtgDraftAnalyser.sln)

A C# program to parse unzipped 17lands CSV data and transform it into hierarchical JSON data.