using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Game of life / Pattern")]
[System.Serializable]
public class Pattern : ScriptableObject
{
    public Vector2Int[] cells;
    
    public Pattern()
    {

    }

    public Pattern(Vector2Int[] cells)
    {
        this.cells = cells;
    }

    public Pattern(List<Vector2Int> cells)
    {
        this.cells = new Vector2Int[cells.Count];
        for(int i = 0; i < cells.Count; i++) 
        {
            this.cells[i] = cells[i];
        }
    }
    public Vector2Int GetCenter()
    {
        if(cells == null || cells.Length == 0)
        {
            return Vector2Int.zero;
        }

        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for(int i = 0; i < cells.Length; i++) 
        {
            Vector2Int cell = cells[i];
            min.x = Mathf.Min(cell.x, min.x);
            min.y = Mathf.Min(cell.y, min.y);
            max.x = Mathf.Max(cell.x, max.x);
            max.y = Mathf.Max(cell.y, max.y);
        }

        return (min + max) / 2;
    }
}
