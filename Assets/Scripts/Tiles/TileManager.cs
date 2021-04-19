using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private List<GameObject> activeTiles;
    public static List<int> tileTypes = new List<int>();
    public static List<string> letters = new List<string>();
    public GameObject[] tilePrefabs;

    public static float tileLength = 30;
    public int numberOfTiles = 3;
    public static int totalNumOfTiles = 8;

    public float zSpawn = 0;

    private Transform playerTransform;

    private int previousIndex;
    private bool a = true;
    private string previous = "FIRST";
    private string[] alphabet = new string[4] { "O", "U", "V", "C" };



    void Start()
    {
        activeTiles = new List<GameObject>();
        for (int i = 0; i < numberOfTiles; i++)
        {
            if(i==0)
            {
                SpawnTile();
            }
                
            else
            {
                SpawnTile(Random.Range(0, totalNumOfTiles));
            }
                
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }
    void Update()
    {
        if(playerTransform.position.z - 35 >= zSpawn - (numberOfTiles * tileLength))
        {
            int index = Random.Range(0, totalNumOfTiles);
            while(index == previousIndex)
                index = Random.Range(0, totalNumOfTiles);

            DeleteTile();
            SpawnTile(index);
        }
            
    }

    public void SpawnTile(int index = 0)
    {
        GameObject tile = tilePrefabs[index];
        if (tile.activeInHierarchy)
            tile = tilePrefabs[index + 8];

        if(tile.activeInHierarchy)
            tile = tilePrefabs[index + 16];

        tile.transform.position = Vector3.forward * zSpawn;
        tile.transform.rotation = Quaternion.identity;
        tile.SetActive(true);

        activeTiles.Add(tile);
        zSpawn += tileLength;
        previousIndex = index;

        tileTypes.Add(index);
        if (tileTypes.Count % 2 == 0)
        {
            if (previous.Equals("FIRST"))
            {
                previous = alphabet[Random.Range(0, alphabet.Length)];
                //previous = "C";
                letters.Add(previous);
            }
            else if (!previous.Equals(""))
            {
                letters.Add(previous);
                previous = "";
            }
            else
            {
                //previous = "C";
                letters.Add(alphabet[Random.Range(0, alphabet.Length)]);
            }
            a = !a;
        }
        else {
            letters.Add("");
        }
    }

    private void DeleteTile()
    {
        activeTiles[0].SetActive(false);
        activeTiles.RemoveAt(0);
        PlayerManager.score += 3;
    }
}
