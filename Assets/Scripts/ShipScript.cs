using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipScript : MonoBehaviour
{
    public float xOffset = 0;
    public float zOffset = 0;
    private float nextZRotation = 90f;
    private GameObject clickedTile;
    int hitCount = 0;
    public int shipSize;

    List<GameObject> touchTiles = new List<GameObject>();
    private Material[] allMaterials;
    List<Color> allColors = new List<Color>();

    void Start()
    {
        allMaterials = GetComponent<Renderer>().materials;

        for (int i = 0; i < allMaterials.Length; i++)
        {
            allColors.Add(allMaterials[i].color);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Tile"))
        {
            touchTiles.Add(collision.gameObject);
        }
        
    }

    public void ClearTileList()
    {
        touchTiles.Clear();
    }

    public Vector3 GetOffsetVec(Vector3 tilePosition) 
    {
        return new Vector3(tilePosition.x + xOffset, 2, tilePosition.z + zOffset);
    }

    public void RotateShip()
    {
        if (clickedTile == null)
        {
            return;
        }
        ClearTileList();

        transform.localEulerAngles += new Vector3(0, 0, nextZRotation);
        nextZRotation *= -1;

        float temp = xOffset;
        xOffset = zOffset;
        zOffset = temp;

        SetPosition(clickedTile.transform.position);
    }

    public void SetPosition(Vector3 newVec)
    {
        ClearTileList();
        transform.localPosition = new Vector3(newVec.x + xOffset, 2, newVec.z + zOffset);
    }

    public void SetClickedTile(GameObject tile)
    {
        clickedTile = tile;
    }

    public bool OnGameBoard()
    {
        return touchTiles.Count == shipSize;
    }

    public bool HitCheckSank()
    {
        hitCount++;
        return shipSize <= hitCount;
    }

    public void FlashColor(Color tempcolor)
    {
        foreach (Material material in allMaterials)
        {
            material.color = tempcolor;
        }
        Invoke("ResetColor", 0.5f);
    }

    public void ResetColor()
    {
        int i = 0;

        foreach (Material material in allMaterials)
        {
            material.color = allColors[i++];
        }
    }
}
