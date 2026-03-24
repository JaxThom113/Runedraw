using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Linq;

public static class LevelParser
{
    private static List<List<int>> grid = new List<List<int>>();
    private static List<int> bottomEdge = new List<int>();
    private static List<int> topEdge = new List<int>();

    /*
        Main persing function
    */
    public static void GenerateLevelFromCsv(TextAsset csv)
    { 
        if (csv == null)
        {
            Debug.LogError($"Level csv not found.");
            return;
        }
 
        // read all lines and remove empty lines to identify sections
        string[] allLines = csv.text.Split('\n');
 
        // split into sections by blank lines
        // section[0] = topEdge, section[1] = grid, section[2] = bottomEdge
        List<List<string>> sections = new List<List<string>>();
        List<string> currentSection = new List<string>();
 
        foreach (string line in allLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentSection.Count > 0)
                {
                    sections.Add(currentSection);
                    currentSection = new List<string>();
                }
            }
            else
            {
                currentSection.Add(line.Trim());
            }
        }
 
        // add the last section if it didn't end with a blank line
        if (currentSection.Count > 0)
            sections.Add(currentSection);
 
        if (sections.Count < 3)
        {
            Debug.LogError("Expected 3 sections separated by blank lines.");
            return;
        }
 
        // parse first section -> List<int>
        topEdge = sections[0]
            .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(int.Parse)
            .ToList();
 
        // parse second section -> List<List<int>>
        grid = sections[1]
            .Select(line =>
                line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList()
            )
            .ToList();
 
        // parse third section -> List<int>
        bottomEdge = sections[2]
            .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(int.Parse)
            .ToList();
    }

    /*
        Return functions
    */

    public static List<List<int>> GetLevel()
    {
        return grid;
    }

    public static List<int> GetTopEdge()
    {
        return topEdge;
    }

    public static List<int> GetBottomEdge()
    {
        return bottomEdge;
    }
}
