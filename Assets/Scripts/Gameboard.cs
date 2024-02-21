using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using System.Text;

public class Gameboard : MonoBehaviour
{
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;
    [SerializeField] private Tile aliveTile;
    [SerializeField] private Tile deadTile;
    [SerializeField] private Tile limitTile;
    [SerializeField] private Pattern pattern;
    [SerializeField , Range(0.01f,2f)] public float updateInterval = 0.05f;
    [SerializeField] private Camera cam;
    [HideInInspector]public bool useLimit;
    [SerializeField] private int maxSizeX;
    [SerializeField] private int maxSizeY;

    [HideInInspector] public bool simulationActive;

    private HashSet<Vector3Int> aliveCells = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> cellsToCheck = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> limitCells = new HashSet<Vector3Int>();

    public int population { get; private set; }
    public int iterations { get; private set; }
    public float time { get; private set; }


    private void Awake()
    {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();
        limitCells = new HashSet<Vector3Int>();

        if (useLimit)
        {
            for (int x = -1; x < maxSizeX + 1; x++)
            {
                limitCells.Add(new Vector3Int(x, -1));
                limitCells.Add(new Vector3Int(x, maxSizeY));
            }

            for (int y = -1; y < maxSizeY + 1; y++)
            {
                limitCells.Add(new Vector3Int(-1, y));
                limitCells.Add(new Vector3Int(maxSizeX, y));
            }

            cam.transform.position = new Vector3(maxSizeX / 2, maxSizeY / 2, cam.transform.position.z);
        }


    }
    private void Start()
    {
        simulationActive = false;
        if (pattern != null)
        {
            SetPattern(pattern);
        }
        if(useLimit) 
        {
            DrawLimit();
        }
    }

    private void Update()
    {
        if (!simulationActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cell = currentState.WorldToCell(clickPosition);
                if (currentState.GetTile(cell) == aliveTile)
                {
                    currentState.SetTile(cell, deadTile);
                    aliveCells.Remove(cell);
                    Debug.Log(cell);
                }
                else
                {
                    currentState.SetTile(cell, aliveTile);
                    aliveCells.Add(cell);
                }
            }
        }
    }

    private void SetPatternCenter(Pattern pattern)
    {
        Clear();
        Vector2Int center = pattern.GetCenter() - new Vector2Int((int)cam.transform.position.x, (int)cam.transform.position.y);

        for (int i = 0; i < pattern.cells.Length; i++)
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i] - center);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }

        population = aliveCells.Count;

        DrawLimit();
    }

    private void SetPattern(Pattern pattern)
    {
        Clear();
        for (int i = 0; i < pattern.cells.Length; i++)
        {
            Vector3Int cell = (Vector3Int)(pattern.cells[i]);
            currentState.SetTile(cell, aliveTile);
            aliveCells.Add(cell);
        }

        population = aliveCells.Count;
        ReDrawLimit();
    }

    public void Clear()
    {
        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        aliveCells.Clear();
        limitCells.Clear();
        population = 0;
        iterations = 0;
        time = 0f;
    }

    public void DrawLimit()
    {       
        foreach (Vector3Int cell in limitCells)
        {
            nextState.SetTile(new Vector3Int(cell.x,cell.y,0), limitTile);
        }
    }

    public void DeleteLimit() 
    {
        foreach (Vector3Int cell in limitCells)
        {
            nextState.SetTile(new Vector3Int(cell.x, cell.y, 0), deadTile);
        }
    }

    public void ReDrawLimit()
    {
        if (limitCells.Count > 0)
        {
            DeleteLimit();
        }
        limitCells.Clear();
        for (int x = -1; x < maxSizeX + 1; x++)
        {
            limitCells.Add(new Vector3Int(x, -1));
            limitCells.Add(new Vector3Int(x, maxSizeY));
        }

        for (int y = -1; y < maxSizeY + 1; y++)
        {
            limitCells.Add(new Vector3Int(-1, y));
            limitCells.Add(new Vector3Int(maxSizeX, y));
        }

        cam.transform.position = new Vector3(maxSizeX / 2, maxSizeY / 2, cam.transform.position.z);
        DrawLimit();
    }
    public IEnumerator Simulate()
    {
        //var interval = new WaitForSeconds(updateInterval);
        // Pour voir le pattern initial au debut
        yield return new WaitForSeconds(updateInterval);

        while (simulationActive)
        {
            UpdateState();
            population = aliveCells.Count;
            iterations++;
            time += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateState()
    {
        cellsToCheck.Clear();
        // Trouver uniquement les cellules a verifier pour l'optimisation
        foreach (Vector3Int cell in aliveCells)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (useLimit)
                    {
                        Vector3Int nVector = cell + new Vector3Int(x, y, 0);
                        cellsToCheck.Add(new Vector3Int(nVector.x % maxSizeX, nVector.y % maxSizeY, 0));
                    }
                    else
                    {
                        cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                    }

                }
            }
        }

        // Faire la logique 
        foreach (Vector3Int cell in cellsToCheck)
        {
            int neighbors = CountNeighbors(cell);
            bool alive = IsAlive(cell);

            if (!alive && neighbors == 3)
            {
                if (!useLimit)
                {
                    nextState.SetTile(cell, aliveTile);
                    aliveCells.Add(cell);
                }
                else if (cell.x < maxSizeX && cell.y < maxSizeY && cell.x >= 0 && cell.y >= 0)
                {
                    nextState.SetTile(cell, aliveTile);
                    aliveCells.Add(cell);
                }

            }
            else if (alive && (neighbors < 2 || neighbors > 3))
            {
                nextState.SetTile(cell, deadTile);
                aliveCells.Remove(cell);
            }
            else
            {
                nextState.SetTile(cell, currentState.GetTile(cell));
            }
        }

        if (useLimit)
        {
            foreach (Vector3Int cell in limitCells)
            {
                nextState.SetTile(cell, limitTile);
            }
        }


        Tilemap temp = currentState;
        currentState = nextState;
        nextState = temp;
        nextState.ClearAllTiles();
    }

    private int CountNeighbors(Vector3Int cell)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighbor = new Vector3Int(0, 0, 0);
                if (x == 0 && y == 0)
                {
                    continue;
                }
                if (useLimit)
                {
                    // Calculer les coordonnées du voisin en tenant compte de la topologie périodique
                    int newX = (cell.x + x + maxSizeX) % maxSizeX;
                    int newY = (cell.y + y + maxSizeY) % maxSizeY;
                    neighbor = new Vector3Int(newX, newY, 0);
                }
                else
                {
                    neighbor = cell + new Vector3Int(x, y, 0);
                }

                if (IsAlive(neighbor))
                {
                    count++;
                }

            }
        }
        return count;
    }
    private bool IsAlive(Vector3Int cell)
    {
        return currentState.GetTile(cell) == aliveTile;
    }

    public void loadPatternFromJson(string filePath)
    {
        // Charger le fichier JSON en tant que chaîne de caractères
        string jsonString = File.ReadAllText(filePath);
        Pattern newPattern = (Pattern)ScriptableObject.CreateInstance("Pattern");
        // Désérialiser la chaîne JSON en un objet C#
        try
        {
            JsonUtility.FromJsonOverwrite(jsonString, newPattern);
            SetPattern(newPattern);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void saveCurrentPatternJson(string filename)
    {
        int compteur = 0;
        Vector2Int[] cells = new Vector2Int[aliveCells.Count];
        foreach (Vector3Int cell in aliveCells)
        {
            cells[compteur] = new Vector2Int(cell.x, cell.y);
            compteur++;
        }
        Pattern pattern = new Pattern(cells);
        File.WriteAllText(filename, JsonUtility.ToJson(pattern));

    }

    public void loadPatternFromPng(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(3, 3, TextureFormat.RGBA32, false);
        texture.LoadImage(fileData);

        Color[] pixels = texture.GetPixels();
        List<Vector2Int> whiteSquareCoordinates = new List<Vector2Int>();
        Color centerWhite = new Color(0.5f, 1, 1, 1);

        Vector2Int center = new Vector2Int(0, 0);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                int index = y * texture.width + x;
                Color pixelColor = pixels[index];

                // Vérifier si le pixel est blanc
                if (pixelColor == Color.green || pixelColor == Color.red)
                {
                    // Ajouter les coordonnées du pixel à la liste
                    center = new Vector2Int(x, y);
                }
            }
        }

        
        // Parcourir les pixels de l'image
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                int index = y * texture.width + x;
                Color pixelColor = pixels[index];

                // Vérifier si le pixel est blanc
                if (pixelColor == Color.white || pixelColor == Color.green)
                {
                    // Ajouter les coordonnées du pixel à la liste
                    whiteSquareCoordinates.Add(new Vector2Int(x - center.x, y - center.y));
                }
            }
        }
        SetPattern(new Pattern(whiteSquareCoordinates));
    }

    public void saveCurrentPatternPng(string filename)
    {
        int compteur = 0;
        Vector2Int[] cells = new Vector2Int[aliveCells.Count];

        foreach (Vector3Int cell in aliveCells)
        {
            cells[compteur] = new Vector2Int(cell.x, cell.y);
            compteur++;
        }
        Pattern pattern = new Pattern(cells);

        int sizePngX = 0;
        int sizePngY = 0;

        int minPX = 0;
        int minPY = 0;
        int maxPX = 0;
        int maxPY = 0;

        foreach (Vector3Int cell in pattern.cells)
        {
            minPX = Mathf.Min(cell.x, minPX);
            minPY = Mathf.Min(cell.y, minPY);
            maxPX = Mathf.Max(cell.x, maxPX);
            maxPY = Mathf.Max(cell.y, maxPY);
        }

        sizePngX = Math.Abs(minPX) + Math.Abs(maxPX) + 1;
        sizePngY = Math.Abs(minPY) + Math.Abs(maxPY) + 1;

        Texture2D texture = new Texture2D(sizePngX, sizePngY, TextureFormat.RGBA32, false);
        texture.SetPixels(new Color[sizePngX * sizePngY]);
        for (int x = 0; x < sizePngX; x++)
        {
            for (int y = 0; y < sizePngY; y++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();


        // Ajout des carrés blancs
        foreach (Vector3Int cell in pattern.cells)
        {
           texture.SetPixel(cell.x - minPX, cell.y - minPY, Color.white);           
        }

        if(texture.GetPixel(0 - minPX, 0 - minPY) == Color.white)
        {
            //texture.SetPixel(0 - minPX, 0 - minPY, new Color(0.5f,1,1,1));
            texture.SetPixel(0 - minPX, 0 - minPY, Color.green);
        }
        else
        {
            texture.SetPixel(0 - minPX, 0 - minPY, Color.red);
        }
        
        texture.Apply();
        File.WriteAllBytes(filename, texture.EncodeToPNG());
    }

    private void OnValidate()
    {
        if(useLimit) 
        {
            ReDrawLimit();
        }
        
    }

}


