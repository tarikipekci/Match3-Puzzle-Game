using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int x;
    public int y;
    private Vector3 startPosition;
    [SerializeField] public bool isItObstacle;
    private Item _item;

    public Item Item
    {
        get => _item;

        set
        {
            if (isItObstacle) return;

            if (_item == value) return;

            _item = value;

            icon.sprite = _item.sprite;
        }
    }

    public bool isEmpty;

    public Image icon;

    private Tile Left => x > 0 ? Board.Instance.tiles[x - 1, y] : null;
    private Tile Top => y > 0 ? Board.Instance.tiles[x, y - 1] : null;
    private Tile Right => x < Board.Instance.width - 1 ? Board.Instance.tiles[x + 1, y] : null;
    private Tile Bottom => y < Board.Instance.height - 1 ? Board.Instance.tiles[x, y + 1] : null;

    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom
    };

    bool CanMatch(Tile tile1, Tile tile2, Tile baseTile)
    {
        if (baseTile == null) return false;

        if (tile1.Item is ISpecialItem && tile2.Item is ISpecialItem && baseTile.Item is ISpecialItem)
        {
            if (tile1.Item == tile2.Item && tile2.Item == baseTile.Item)
            {
                return true;
            }
        }

        if ((tile1.Item is ISpecialItem && tile2.Item is ISpecialItem && baseTile.Item is not ISpecialItem) ||
            (tile1.Item is ISpecialItem && baseTile.Item is ISpecialItem && tile2.Item is not ISpecialItem) ||
            (tile2.Item is ISpecialItem && baseTile.Item is ISpecialItem && tile1.Item is not ISpecialItem))
        {
            if ((tile1.Item == tile2.Item) || (tile1.Item == baseTile.Item) || (tile2.Item == baseTile.Item))
            {
                return true;
            }
        }

        if (tile1.Item is not ISpecialItem && tile2.Item is not ISpecialItem && baseTile.Item is not ISpecialItem)
        {
            if (tile1.Item == tile2.Item && tile2.Item == baseTile.Item)
            {
                return true;
            }
        }

        if ((tile1.Item is not ISpecialItem && tile2.Item is not ISpecialItem && baseTile.Item is ISpecialItem) ||
            (tile1.Item is not ISpecialItem && baseTile.Item is not ISpecialItem && tile2.Item is ISpecialItem) ||
            (tile2.Item is not ISpecialItem && baseTile.Item is not ISpecialItem && tile1.Item is ISpecialItem))
        {
            if ((tile1.Item == tile2.Item) || (tile1.Item == baseTile.Item) || (tile2.Item == baseTile.Item))
            {
                return true;
            }
        }

        return false;
    }

    public List<Tile> GetConnectedTiles(bool isVertical = false, List<Tile> exclude = null, Tile baseTile = null)
    {
        var result = new List<Tile> { this };

        if (baseTile == null)
        {
            baseTile = Item is ISpecialItem == false ? this : null;
        }

        if (exclude == null)
        {
            exclude = new List<Tile> { this };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null || exclude.Contains(neighbour) || isEmpty || isItObstacle) continue;

            if (baseTile == null && neighbour.Item is ISpecialItem == false)
            {
                baseTile = neighbour;
            }

            if (baseTile == null || !CanMatch(this, neighbour, baseTile)) continue;

            if (isVertical)
            {
                if (neighbour.x == x)
                {
                    result.AddRange(neighbour.GetConnectedTiles(true, exclude, baseTile));
                }
            }
            else
            {
                if (neighbour.y == y)
                {
                    result.AddRange(neighbour.GetConnectedTiles(false, exclude, baseTile));
                }
            }
        }

        return result;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = eventData.position;
        if (isItObstacle == false)
        {
            if (Board.Instance._selection.Count >= 1)
            {
                Board.Instance._selection.Clear();
            }

            Board.Instance.Select(this);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var draggedObject = eventData.pointerDrag.gameObject;
        Tile draggedTile = draggedObject.GetComponent<Tile>();
        Vector3 endPosition = eventData.position;
        var direction = (endPosition - startPosition).normalized;

        const float threshold = 0.5f;

        if (isItObstacle == false)
        {
            if (direction.y > threshold)
            {
                Board.Instance.Select(draggedTile.Neighbours[1]);
            }
            else if (direction.y < -threshold)
            {
                Board.Instance.Select(draggedTile.Neighbours[3]);
            }
            else if (direction.x > threshold)
            {
                Board.Instance.Select(draggedTile.Neighbours[2]);
            }
            else if (direction.x < -threshold)
            {
                Board.Instance.Select(draggedTile.Neighbours[0]);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
}