using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DuplicateGraffitiManager : MonoBehaviour
{
    public List<GraffitiWall> graffitiWalls; // Assign your walls here
    public CodeInputter codeInputter;         // Assign your CodeInputter script here

    public int numbersPerWall = 10;  // How many numbers total to spawn on each wall

    void Start()
    {
        SpawnWalls();
    }

    public void SpawnWalls()
    {
        string code = codeInputter.correctCode; // e.g. "123"
        List<int> codeDigits = code.Select(c => int.Parse(c.ToString())).Distinct().ToList();

        List<List<int>> wallNumbers = new List<List<int>>();

        if (graffitiWalls.Count < 2)
        {
            Debug.LogError("Need at least two graffiti walls assigned!");
            return;
        }

        List<int> wall1 = new List<int>(codeDigits);
        List<int> wall2 = new List<int>(codeDigits);

        System.Random rnd = new System.Random();

        List<int> allDigits = Enumerable.Range(0, 10).ToList();

        List<int> nonCodeDigits = allDigits.Except(codeDigits).ToList();

        while (wall1.Count < numbersPerWall)
        {
            int choice;
            if (rnd.NextDouble() < 0.5 && codeDigits.Count > 0)
                choice = codeDigits[rnd.Next(codeDigits.Count)];
            else
                choice = nonCodeDigits[rnd.Next(nonCodeDigits.Count)];

            if (!wall2.Contains(choice) || codeDigits.Contains(choice))
                wall1.Add(choice);
        }

        List<int> wall1NonCode = wall1.Except(codeDigits).Distinct().ToList();

        List<int> wall2AllowedExtras = nonCodeDigits.Except(wall1NonCode).ToList();

        while (wall2.Count < numbersPerWall)
        {
            int choice;
            if (rnd.NextDouble() < 0.5 && codeDigits.Count > 0)
                choice = codeDigits[rnd.Next(codeDigits.Count)];
            else if (wall2AllowedExtras.Count > 0)
                choice = wall2AllowedExtras[rnd.Next(wall2AllowedExtras.Count)];
            else
                choice = codeDigits[rnd.Next(codeDigits.Count)];

            wall2.Add(choice);
        }

        graffitiWalls[0].SpawnNumbers(wall1);
        graffitiWalls[1].SpawnNumbers(wall2);
    }
}
