using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip popUpSound;
    [SerializeField] private AudioSource audioSource;

    public Row[] rows;
    public Tile[,] tiles { get; private set; }

    public int width => tiles.GetLength(0);
    public int height => tiles.GetLength(1);

    private const float tweenDuration = 0.15f;

    private readonly List<Tile> _selection = new List<Tile>();
    private void Awake() => Instance = this;

    private void Start()
    {
        tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.Item = ItemDatabase.items[Random.Range(0, ItemDatabase.items.Length)];
                tiles[x, y] = tile;
            }
        }
    }

    private async Task DropUpperTiles()
    {
        while (true)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var tile = tiles[x, y];
                    var bottomTile = tile.Neighbours[3];
                    if (tile.isEmpty) continue;
                    switch (y)
                    {
                        case 4:
                            continue;
                        case 0 when bottomTile.isEmpty:
                            bottomTile.Item = ItemDatabase.items[Random.Range(0, ItemDatabase.items.Length)];
                            var deflateSequence = DOTween.Sequence();
                            deflateSequence.Join(bottomTile.icon.transform.DOScale(Vector3.zero, tweenDuration));
                            await deflateSequence.Play().AsyncWaitForCompletion();
                            bottomTile.isEmpty = false;
                            tile.isEmpty = true;
                            await Swap(tile, bottomTile);
                            break;
                    }

                    if (bottomTile.isEmpty)
                    {
                        bottomTile.isEmpty = false;
                        tile.isEmpty = true;
                        await Swap(tile, bottomTile);
                    }
                }
            }

            await SpawnNewTiles();
            if (IsThereEmptyTile())
            {
                continue;
            }

            if (CanPop())
            {
                Pop();
            }

            break;
        }
    }

    private async Task SpawnNewTiles()
    {
        for (var x = 0; x < height; x++)
        {
            var tile = tiles[x, 0];
            if (tile.isEmpty)
            {
                tile.Item = ItemDatabase.items[Random.Range(0, ItemDatabase.items.Length)];
                var inflateSequence = DOTween.Sequence();
                inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration));
                await inflateSequence.Play().AsyncWaitForCompletion();
                tile.isEmpty = false;
            }
        }
    }

    private bool IsThereEmptyTile()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];
                if (tile.isEmpty) continue;

                var bottomTile = tile.Neighbours[3];
                if (bottomTile == null) continue;

                if (bottomTile.isEmpty)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async void Select(Tile tile)
    {
        if (!_selection.Contains(tile))
        {
            if (_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) != -1)
                {
                    _selection.Add(tile);
                }
                else
                {
                    _selection.Clear();
                }
            }
            else
            {
                _selection.Add(tile);
            }
        }

        if (_selection.Count < 2) return;

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    private async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2.transform.position, tweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1.transform.SetParent(tile2.transform);
        icon2.transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    private bool CanPop()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                if (tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                    return true;
        }

        return false;
    }

    private async void Pop()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];
                var connectedTiles = tile.GetConnectedTiles();
                if (connectedTiles.Skip(1).Count() < 2) continue;

                var deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, tweenDuration));
                    connectedTile.isEmpty = true;
                }

                await deflateSequence.Play().AsyncWaitForCompletion();

                audioSource.PlayOneShot(popUpSound);
                ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDatabase.items[Random.Range(0, ItemDatabase.items.Length)];
                }

                x = 0;
                y = 0;
            }
        }

        await DropUpperTiles();
    }
}