using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject[] ships;

    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;

    public Text topText;
    public Text playerShipText;
    public Text enemyShipText;

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject woodDock;
    public GameObject firePrefab;

    private bool setupComplete = false;
    private bool playerTurn = true;
    private int shipIndex = 0;
    private ShipScript shipScript;
    public EnemyScript enemyScript;
    private List<int[]> enemyShips;
    private List<GameObject> playerFires;
    private List<GameObject> enemyFires;
    private List<TileScript> allTileScripts;



    int enemyShipCount = 5;
    int playerShipCount = 5;

    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        nextBtn.onClick.AddListener(() => NextShipClicked());
        rotateBtn.onClick.AddListener(() => RotateShipClicked());
        enemyShips = enemyScript.PlaceEnemyShips();
    }

    private void RotateShipClicked()
    {
        shipScript.RotateShip();
    }


    private void NextShipClicked()
    {
        if (!shipScript.OnGameBoard())
        {
            
            shipScript.FlashColor(Color.red);
        }

        else
        {
            if (shipIndex <= ships.Length - 2)
            {
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<ShipScript>();
                shipScript.FlashColor(Color.yellow);

            }
            else
            {
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDock.gameObject.SetActive(false);
                topText.text = "Awaiting coordinates, Admiral..";
                setupComplete = true;

                //hideships
                for (int i = 0; i < ships.Length; i++)
                {
                    ships[i].SetActive(false);
                }

            }
            
        }

        
    }


    public void TileClicked(GameObject tile)
    {
        if (playerTurn && setupComplete)
        {
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 15;  //add 15 to y axis

            playerTurn = false;
            Instantiate(missilePrefab, tilePos, missilePrefab.transform.rotation);
            //drop missile script goes here -> KerPlunk ;)
        }
        else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);

        }
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIndex].GetComponent<ShipScript>();
        shipScript.ClearTileList();

        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;
    }

    public void CheckHit(GameObject tile)
    {
        int tileNum = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;

        foreach (int[] tileNumArray in enemyShips)
        {
            if (tileNumArray.Contains(tileNum))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNum)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }

                if (hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    topText.text = "SUNK!!!!!!";

                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));

                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 155, 155, 255));  
                    tile.GetComponent<TileScript>().SwitchColors(1);

                }
                else
                {
                    topText.text = "HIT!!!";
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 0, 0, 255));  //red
                    tile.GetComponent<TileScript>().SwitchColors(1);

                }

                break;
            }

            
        }

        if (hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, new Color32(40, 60, 75, 255));
            tile.GetComponent<TileScript>().SwitchColors(1);

            topText.text = "Missed!";
        }
        Invoke("EndPlayerTurn", 1.0f);
    }

    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    {
        enemyScript.MissileHit(tileNum);
        tile.y += 0.2f; //use this position to set up fire's location.
        playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));

        if (hitObj.GetComponent<ShipScript>().HitCheckSank())
        {
            playerShipCount--;
            playerShipText.text = playerShipCount.ToString();
            enemyScript.SunkPlayer();
        }

        Invoke("EndEnemyTurn", 2.0f);
    }

    private void EndPlayerTurn()
    {
        //show player ships when enemy drops missile

        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].SetActive(true);
        }

        foreach (GameObject fire in playerFires)
        {
            fire.SetActive(true);

        }
        foreach (GameObject fire in enemyFires)
        {
            fire.SetActive(false);
        }

        enemyShipText.text = enemyShipCount.ToString();
        topText.text = "The Enemy is attacking!!!";

        enemyScript.NPCTurn();
        ColorAllTiles(0);

        if(playerShipCount < 1)
        {
            GameOver("Your fleet has been decimated.");
        }
        
        //if (enemyShipCount < 1)
        //{
        //    GameOver("Victory! Excellent tactics, Admirable");
        //}
    }

    private void EndEnemyTurn()
    {
        //show player ships when enemy drops missile

        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].SetActive(false);
        }

        foreach (GameObject fire in playerFires)
        {
            fire.SetActive(false);

        }
        foreach (GameObject fire in enemyFires)
        {
            fire.SetActive(true);
        }

        playerShipText.text = enemyShipCount.ToString();
        topText.text = "What are your orders, sir?";

        enemyScript.NPCTurn();
        ColorAllTiles(1);

        if (enemyShipCount < 1)
        {
            GameOver("Victory! Excellent tactics, Admiral.");
        }

        //if (enemyShipCount < 1)
        //{
        //    GameOver("Victory! Excellent tactics, Admirable");
        //}
    }

    private void ColorAllTiles(int colorIndex)
    {
        foreach (TileScript tileScript in allTileScripts)
        {
            tileScript.SwitchColors(colorIndex);
        }
    }

    private void GameOver(string message)
    {
        topText.text = $"{message} {winner}";

        replayBtn.gameObject.SetActive(true);
    }

    private void ReplayClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
