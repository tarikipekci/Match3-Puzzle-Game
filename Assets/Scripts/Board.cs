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

    [SerializeField] private bool canMakeMove;

    public Row[] rows;
    public Tile[,] tiles { get; private set; }

    public int width => tiles.GetLength(0);
    public int height => tiles.GetLength(1);

    public float tweenDuration = 0.2f;
    private const float specialItemPossibility = 0.1f;
    private bool isThereObstacle;

    public List<Tile> _selection = new List<Tile> { };
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

        audioManager = FindObjectOfType<AudioManager>();
        levelManager = FindObjectOfType<LevelManager>();
        levelManager.UpdateMoveCount();
        InitBoard();
        isThereObstacle = IsThereObstacle();
        canMakeMove = true;
    }

    private void InitBoard(int retryCount = 0)
    {
        const int maxRetries = 10;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tiles[x, y] = tile;

                if (tile.isItObstacle) continue;
                tile.Item = currentLevelItems[Random.Range(0, currentLevelItems.Length)];
            }
        }

        if (!CheckIsThereAnyPossibleMatch() && retryCount < maxRetries)
        {
            InitBoard(retryCount + 1);
        }
    }

    private async void UpdateBoard()
    {
        while (true)
        {
            var moved = DropTile(out var tasks);

            await Task.WhenAll(tasks);

            if (isThereObstacle)
            {
                SpawnNewTileBelowObstacle();
            }

            await SpawnNewTiles();

            if (!moved && !IsThereDropPossibility())
            {
                if (CanPopHorizontal())
                {
                    StartPop(false);
                }
                else if (CanPopVertical())
                {
                    StartPop(true);
                }
                else
                {
                    if (HasTargetReached())
                    {
                        levelManager.resultPanel.SetActive(true);
                        var currentLevel = levelStates.levels[PlayerPrefsBehaviour.GetCurrentLevelValue() - 1];
                        currentLevel.isCompleted = true;
                        var currentScore = ScoreCounter.Instance.Score;
                        if (ScoreCounter.Instance.Score > currentLevel.BestScore)
                        {
                            currentLevel.BestScore = currentScore;
                        }

                        levelManager.OpenResultPanel();
                    }
                    else
                    {
                        canMakeMove = true;
                    }
                }

                break;
            }
        }
    }

    private bool DropTile(out List<Task> tasks)
    {
        bool moved = false;
        tasks = new List<Task>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var tile = tiles[x, y];
                if (tile.isEmpty) continue;
                if (tile.isItObstacle) continue;
                if (IsBelowObstacle(tile)) continue;

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

        return moved;
    }

    private bool IsBelowObstacle(Tile tile)
    {
        var currentTile = tile;

        while (currentTile != null && currentTile.y > 0)
        {
            if (currentTile.isItObstacle)
            {
                return true;
            }

            currentTile = tiles[currentTile.x, currentTile.y - 1];
        }

        return false;
    }

    private List<Tile> FindAllBelowTiles(Tile tile)
    {
        List<Tile> belowTiles = new List<Tile>();

        int x = tile.x;
        int y = tile.y + 1;

        while (y < tiles.GetLength(1))
        {
            var currentTile = tiles[x, y];

            if (currentTile != null && currentTile.isEmpty)
            {
                belowTiles.Add(currentTile);
            }

            y++;
        }

        return belowTiles;
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

    private async void SpawnNewTileBelowObstacle()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var tile = tiles[x, y];
                if (tile.isItObstacle)
                {
                    var belowTiles = FindAllBelowTiles(tile);

                    foreach (var belowTile in belowTiles)
                    {
                        if (belowTile.isEmpty)
                        {
                            var randomItem = currentLevelItems[Random.Range(0, currentLevelItems.Length)];
                            belowTile.Item = randomItem;
                            var inflateItem = DOTween.Sequence();
                            inflateItem.Join(belowTile.icon.transform.DOScale(Vector3.one, tweenDuration));
                            belowTile.isEmpty = false;
                            await inflateItem.Play().AsyncWaitForCompletion();
                        }
                    }
                }
            }
        }
    }

    private async Task SpawnNewTiles(int retryCount = 0)
    {
        const int maxRetries = 10;
        List<Tile> targetTiles = new();

        while (retryCount < maxRetries)
        {
            targetTiles.Clear();

            for (int x = 0; x < height; x++)
            {
                var tile = tiles[x, 0];
                if (!tile.isEmpty) continue;

                tile.Item = Random.Range(0f, 1f) <= specialItemPossibility
                    ? specialItems[Random.Range(0, specialItems.Length)]
                    : currentLevelItems[Random.Range(0, currentLevelItems.Length)];

                targetTiles.Add(tile);
            }

            if (CheckIsThereAnyPossibleMatch())
            {
                break;
            }

            retryCount++;
        }

        if (retryCount >= maxRetries)
        {
            ForceMatchForGrid();
        }

        var animations = new List<Task>();
        foreach (var targetTile in targetTiles)
        {
            animations.Add(AnimateTileInflation(targetTile));
            targetTile.isEmpty = false;
        }

        await Task.WhenAll(animations);
    }

    private void ForceMatchForGrid()
    {
        for (int x = 0; x < height; x++)
        {
            var tile = tiles[x, 0];
            if (tile.isEmpty)
            {
                tile.Item = specialItems[Random.Range(0, specialItems.Length)];
            }
        }
    }

    private async Task AnimateTileInflation(Tile tile)
    {
        var inflateItem = DOTween.Sequence();
        inflateItem.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration));
        await inflateItem.Play().AsyncWaitForCompletion();
    }


    private bool CheckIsThereAnyPossibleMatch()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var currentTile = tiles[x, y];
                if (currentTile.isItObstacle) continue;

                foreach (var tile in currentTile.Neighbours)
                {
                    if (tile == null || tile.isItObstacle) continue;

                    (currentTile.Item, tile.Item) = (tile.Item, currentTile.Item);

                    if (CanPopHorizontal() || CanPopVertical())
                    {
                        (currentTile.Item, tile.Item) = (tile.Item, currentTile.Item);
                        return true;
                    }

                    (currentTile.Item, tile.Item) = (tile.Item, currentTile.Item);
                }
            }
        }

        return false;
    }

    private bool IsThereDropPossibility()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height - 1; y++)
            {
                var tile = tiles[x, y];
                if (tile.isEmpty || tile.isItObstacle) continue;

                var bottomTile = tiles[x, y + 1];
                if (bottomTile != null && bottomTile.isEmpty)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async void Select(Tile tile)
    {
        if (tile == null)
        {
            canMakeMove = true;
            _selection.Clear();
            return;
        }

        if (canMakeMove)
        {
            if (tile.isItObstacle) return;

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
            canMakeMove = false;

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
                canMakeMove = true;
            }

            _selection.Clear();
        }
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
            {
                var tile = tiles[x, y];

                if (tile.isItObstacle) continue;

                if (tiles[x, y].GetConnectedTiles(true).Skip(1).Count() >= 2 && levelManager.levelFinished == false)
                    return true;
            }
        }

        return false;
    }

    private bool CanPopHorizontal()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];

                if (tile.isItObstacle) continue;

                if (tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2 && levelManager.levelFinished == false)
                    return true;
            }
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

        UpdateBoard();
    }

    private async Task<bool> Pop(Tile tile, bool isVertical)
    {
        var connectedTiles = tile.GetConnectedTiles(isVertical);
        if (connectedTiles.Skip(1).Count() < 2) return true;

        var deflateSequence = DOTween.Sequence();

        foreach (var connectedTile in connectedTiles)
        {
            deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, tweenDuration));
            UpdateGoal(connectedTile);

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

        ScoreCounter.Instance.CalculateScoreMultiplication(tile.Item, connectedTiles.Count);

        foreach (var connectedTile in connectedTiles)
        {
            connectedTile.Item = currentLevelItems[Random.Range(0, currentLevelItems.Length)];
        }

        return false;
    }

    public void UpdateGoal(Tile connectedTile)
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

    private bool IsThereObstacle()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = tiles[x, y];
                if (tile.isItObstacle)
                {
                    return true;
                }
            }
        }

        return false;
    }
}