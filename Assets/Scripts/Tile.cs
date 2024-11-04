using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;

    public Item Item
    {
        get => _item;

        set
        {
            if (_item == value) return;

            _item = value;

            icon.sprite = _item.sprite;
        }
    }
    
    public Image icon;
    public Button button;
    
    public Tile Left => x > 0 ? Board.Instance.tiles[x - 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.tiles[x , y - 1] : null;
    public Tile Right => x < Board.Instance.width - 1 ? Board.Instance.tiles[x + 1, y] : null;
    public Tile Bottom => y < Board.Instance.height - 1 ? Board.Instance.tiles[x, y + 1] : null;

    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom
    };

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> { this, };

        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || neighbour.Item != Item) continue;
            
            result.AddRange(neighbour.GetConnectedTiles(exclude));
        }
        
        return result;
    }
}
