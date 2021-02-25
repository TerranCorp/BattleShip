using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    char[] guessGrid;
    List<int> potentialHits;
    List<int> currentHits;
    private int guess;
    public GameObject enemyMissilePrefab;

    private void Start()
    {
        potentialHits = new List<int>();
        currentHits = new List<int>();
        guessGrid = Enumerable.Repeat('o', 100).ToArray();
    }

    public List<int[]> PlaceEnemyShips()
    {
        List<int[]> enemyShips = new List<int[]>
        {
            new int[]{-1, -1, -1, -1, -1},
            new int[]{-1, -1, -1, -1},
            new int[]{-1, -1, -1},
            new int[]{-1, -1, -1},
            new int[]{-1, -1}
        };

        int[] gridNumbers = Enumerable.Range(1, 100).ToArray();
        bool taken = true;

        foreach(int[] tileNumArray in enemyShips)
        {
            taken = true;
            while (taken == true)
            {
                taken = false;
                int shipNose = UnityEngine.Random.Range(0, 99);
                int rotateBool = UnityEngine.Random.Range(0, 2);
                int minusAmount = rotateBool == 0 ? 10 : 1;

                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    //Check that ship end will not go off board and check tile for taken.
                    if ((shipNose - (minusAmount * i)) < 0 || gridNumbers[shipNose - i * minusAmount] < 0)
                    {
                        taken = true;
                        break;
                    }
                    //Ship is horizontal, check ship doesnt go off the sides 0 to 10, 11 to 20
                    else if (minusAmount == 1 && shipNose /10 != ((shipNose - i * minusAmount) - 1) / 10)
                    {
                        taken = true;
                        break;
                    }
                }

                // if tile is not take, loop through tile numbers assign them to the array in the list
                if (taken == false)
                {
                    for (int j = 0; j < tileNumArray.Length; j++)
                    {
                        tileNumArray[j] = gridNumbers[shipNose - j * minusAmount];
                        gridNumbers[shipNose - j * minusAmount] = -1;
                    }
                }
            }
        }

        return enemyShips;
    }

    public void NPCTurn()
    {
        List<int> hitIndex = new List<int>();

        for (int i = 0; i < guessGrid.Length; i++)
        {
            if (guessGrid[i] == 'h')
            {
                hitIndex.Add(i);
            }
        }

        if (hitIndex.Count > 1)
        {
            //comp should guess around the area if they hit a ship
            int diff = hitIndex[1] = hitIndex[0];  //diff between hit index 1 and 0

            //randomize guess
            int posNEG = Random.Range(0, 2) * 2 - 1; //could be a pos or -1

            int nextIndex = hitIndex[0] + diff;

            while (guessGrid[nextIndex] != 'o')
            {
                if (guessGrid[nextIndex] == 'm' || nextIndex > 100 || nextIndex < 0) //make sure we arent off the grid as well 
                {
                    diff *= -1;
                }

                nextIndex += diff;
            }

            guess = nextIndex;
        }
        else if (hitIndex.Count == 1)
        {
            List<int> closeTiles = new List<int>();  //N,S,E,W
            closeTiles.Add(-1);
            closeTiles.Add(1);
            closeTiles.Add(-10);
            closeTiles.Add(10);

            int index = Random.Range(0, closeTiles.Count);
            int possibleGuess = hitIndex[0] + closeTiles[index];
            bool onGrid = possibleGuess > -1 && possibleGuess < 100;

            while((!onGrid || guessGrid[possibleGuess] != 'o') && closeTiles.Count > 0)
            {
                closeTiles.RemoveAt(index);

                index = Random.Range(0, closeTiles.Count);
                possibleGuess = hitIndex[0] + closeTiles[index];
                onGrid = possibleGuess > -1 && possibleGuess < 100;
            }

            guess = possibleGuess;
        }

        else
        {
            int nextIndex = Random.Range(0, 100);

            while (guessGrid[nextIndex] != 'o')
            {
                nextIndex = Random.Range(0, 100);
            }

            guess = nextIndex;
        }

        GameObject tile = GameObject.Find("Tile (" + (guess + 1) + ")");
        guessGrid[guess] = 'm';

        Vector3 vec = tile.transform.position;
        vec.y += 15;
        GameObject missile = Instantiate(enemyMissilePrefab, vec, enemyMissilePrefab.transform.rotation);

        //Set target
        missile.GetComponent <EnemyMissileScript>().SetTarget(guess);
        missile.GetComponent<EnemyMissileScript>().targetTileLocation = tile.transform.position;
    }

    public void MissileHit(int hit)
    {
        guessGrid[hit] = 'h';
    }

    public void SunkPlayer()
    {
        for (int i = 0; i < guessGrid.Length; i++)
        {
            if (guessGrid[i] == 'h')
            {
                guessGrid[i] = 'x';
            }
        }
    }
}
