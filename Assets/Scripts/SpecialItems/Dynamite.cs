using DG.Tweening;
using UnityEngine;

namespace SpecialItems
{
    [CreateAssetMenu(menuName = "Match3/SpecialItem/Dynamite")]
    public class Dynamite : Item, ISpecialItem
    {
        public async void ExecuteSpecialItem(Tile tile)
        {
            var deflateSequence = DOTween.Sequence();
            for (int y = 0; y < Board.Instance.height; y++)
            {
                for (var x = 0; x < Board.Instance.width; x++)
                {
                    if (x == tile.x || y == tile.y)
                    {
                        var currentTile = Board.Instance.tiles[x, y];
                        if (currentTile.isItObstacle) continue;
                            
                        deflateSequence.Join(currentTile.icon.transform.DOScale(Vector3.zero, Board.Instance.tweenDuration));

                        currentTile.isEmpty = true;

                        await Board.Instance.UpdateGoal(currentTile);
                    }
                }
            }
            await deflateSequence.Play().AsyncWaitForCompletion();
        }
    }
}
