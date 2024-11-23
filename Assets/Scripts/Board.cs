using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }
    private AudioManager audioManager;
    public ItemDatabase itemDatabase;
    Item[] currentLevelItems = new Item[] { };
    Item[] specialItems = new Item[] { };
    LevelManager levelManager;
    public LevelStates levelStates;

    public Row[] rows;
    public Tile[,] tiles { get; private set; }

    public int width => tiles.GetLength(0);
    public int height => tiles.GetLength(1);

    public float tweenDuration = 0.2f;
    private const float specialItemPossibility = 0.1f;

    private List<Tile> _selection = new List<Tile> { };
    private void Awake() => Instance = this;

    private void Start()
    {
        itemDatabase = FindObjectOfType<ItemDatabase>();
        tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        foreach (var currentItem in itemDatabase.itemDictionary)
        {
            if (currentItem.key == SceneManager.GetActiveScene().name)
            {
                currentLevelItems = currentItem.value;
            }
            else if (currentItem.key == "SpecialItems")
            {
                specialItems = currentItem.value;
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.Item = currentLevelItems[Random.Range(0, currentLevelItems.Length)];

                tiles[x, y] = tile;
            }
        }

        audioManager = FindObjectOfType<AudioManager>();
        levelManager = FindObjectOfType<LevelManager>();
        levelManager.UpdateMoveCount();
    }

    private async void DropUpperTiles()
    {
        while (true)
        {
            bool moved = false;
            var tasks = new List<Task>();

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile.isEmpty) continue;

                    var lowestEmptyTileBelow = FindLowestEmptyTileBelow(x, y);

                    if (lowestEmptyTileBelow != null && lowestEmptyTileBelow != tile)
                    {
                        lowestEmptyTileBelow.isEmpty = false;
                        tile.isEmpty = true;

                        tasks.Add(Swap(tile, lowestEmptyTileBelow));
                        moved = true;
                    }
                }
            }

            await Task.WhenAll(tasks);

            await SpawnNewTiles();

            if (!moved && !IsThereEmptyTile())
            {
                if (CanPopHorizontal())
                {
                    StartPop(false);
                }
                else if (CanPopVertical())
                {
                    StartPop(true);
                }

                break;
            }
        }
    }

    private Tile FindLowestEmptyTileBelow(int x, int startY)
    {
        bool emptyTileFound = false;
        int emptyTileCount = 0;
        Tile firstEmptyTile = null;
        Tile lowestEmptyTile = null;

        for (int y = startY + 1; y < height; y++)
        {
            if (!tiles[x, y].isEmpty) break;

            if (tiles[x, y].isEmpty)
            {
                if (emptyTileFound == false)
                {
                    emptyTileFound = true;
                    emptyTileCount++;
                    firstEmptyTile = tiles[x, y];
                    lowestEmptyTile = tiles[x, y];
                }
                else
                {
                    emptyTileCount++;
                    lowestEmptyTile = tiles[x, y];
                }
            }
        }

        if (emptyTileCount > 1)
        {
            return lowestEmptyTile;
        }

        return firstEmptyTile;
    }
    
    private async Task SpawnNewTiles(int retryCount = 0)
    {
        List<Tile> targetTiles = new List<Tile>();
        const int maxRetries = 8;
        if (retryCount > maxRetries)
        {
            FillEmptyTiles(targetTiles);
            Debug.Log("Added tiles manually");
            return;
        }

        for (var x = 0; x < height; x++)
        {
            var tile = tiles[x, 0];
            if (tile.isEmpty)
            {
                if (Random.Range(0f, 1f) <= specialItemPossibility)
                {
                    tile.Item = specialItems[Random.Range(0, specialItems.Length)];
                }
                else
                {
                    tile.Item = currentLevelItems[Random.Range(0, currentLevelItems.Length)];
                }

                tile.isEmpty = false;
                targetTiles.Add(tile);
            }
        }

        if (!CheckIsThereAnyPossibleMatch())
        {
            foreach (var targetTile in targetTiles)
            {
                targetTile.isEmpty = true;
            }

            await SpawnNewTiles(retryCount + 1);
        }
        else
        {
            foreach (var targetTile in targetTiles)
            {
                var inflateItem = DOTween.Sequence();
                inflateItem.Join(targetTile.icon.transform.DOScale(Vector3.one, tweenDuration));
                await inflateItem.Play().AsyncWaitForCompletion();
                targetTile.isEmpty = false;
            }
        }
    }

    private async void FillEmptyTiles(List<Tile> targetTiles)
    {
        var randomItem = currentLevelItems[Random.Range(0, currentLevelItems.Length)];
        foreach (var tile in targetTiles)
        {
            tile.Item = randomItem;
            var inflateItem = DOTween.Sequence();
            inflateItem.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration));
            await inflateItem.Play().AsyncWaitForCompletion();
            tile.isEmpty = false;
        }
    }

    private bool CheckIsThereAnyPossibleMatch()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var currentTile = tiles[x, y];

                foreach (var tile in currentTile.Neighbours)
                {
                    if (tile)
                    {
                        var oldItem1 = currentTile.Item;
                        var oldItem2 = tile.Item;
                        var currentTileItem = currentTile.Item;
                        currentTile.Item = tile.Item;
                        tile.Item = currentTileItem;
                        if (CanPopHorizontal() || CanPopVertical())
                        {
                            currentTile.Item = oldItem1;
                            tile.Item = oldItem2;
                            return true;
                        }

                        currentTile.Item = oldItem1;
                        tile.Item = oldItem2;
                    }
                }
            }
        }

        return false;
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

        if (CanPopHorizontal())
        {
            StartPop(false);
            levelManager.numberOfMoves--;
            levelManager.UpdateMoveCount();
        }
        else if (CanPopVertical())
        {
            StartPop(true);
            levelManager.numberOfMoves--;
            levelManager.UpdateMoveCount();
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

    private bool CanPopVertical()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                if (tiles[x, y].GetConnectedTiles(true).Skip(1).Count() >= 2 && levelManager.levelFinished == false)
                    return true;
        }

        return false;
    }

    private bool CanPopHorizontal()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                if (tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2 && levelManager.levelFinished == false)
                    return true;
        }

        return false;
    }

    private async void StartPop(bool isVertical)
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];
                if (await Pop(tile, isVertical)) continue;

                x = 0;
                y = 0;
            }
        }

        DropUpperTiles();
    }

    private async Task<bool> Pop(Tile tile, bool isVertical)
    {
        var connectedTiles = tile.GetConnectedTiles(isVertical);
        if (connectedTiles.Skip(1).Count() < 2) return true;

        var deflateSequence = DOTween.Sequence();

        foreach (var connectedTile in connectedTiles)
        {
            deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, tweenDuration));
            await UpdateGoal(connectedTile);

            connectedTile.isEmpty = true;

            if (connectedTile.Item is ISpecialItem specialItem)
            {
                specialItem.ExecuteSpecialItem(connectedTile);
            }
        }

        await deflateSequence.Play().AsyncWaitForCompletion();

        if (audioManager != null)
        {
            audioManager.soundEffects[0].Play();
        }

        ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;

        foreach (var connectedTile in connectedTiles)
        {
            connectedTile.Item = currentLevelItems[Random.Range(0, currentLevelItems.Length)];
        }

        return false;
    }

    public async Task UpdateGoal(Tile connectedTile)
    {
        if (levelManager.levelTarget.targetItem.Contains(connectedTile.Item))
        {
            var targetIndex = Array.IndexOf(levelManager.levelTarget.targetItem, connectedTile.Item);
            if (targetIndex >= 0)
            {
                if (levelManager.levelTarget.targetAmount[targetIndex] > 0)
                {
                    levelManager.levelTarget.targetAmount[targetIndex]--;
                    var currentAmount = levelManager.levelTarget.targetAmount[targetIndex];
                    levelManager.targetAmounts[targetIndex].text = currentAmount.ToString();
                }

                if (HasTargetReached())
                {
                    levelManager.victoryPanel.SetActive(true);
                    levelStates.levels[PlayerPrefsBehaviour.GetCurrentLevelValue() - 1].isCompleted = true;
                    var inflateSequence = DOTween.Sequence();
                    inflateSequence.Join(levelManager.victoryPanel.transform.DOScale(Vector3.one, tweenDuration));
                    await inflateSequence.Play().AsyncWaitForCompletion();
                }
            }
        }
    }

    private bool HasTargetReached()
    {
        foreach (var amount in levelManager.levelTarget.targetAmount)
        {
            if (amount > 0)
            {
                return false;
            }
        }
        levelManager.levelFinished = true;
        return true;
    }
}