using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/SpecialItem")]
public class SpecialItem : Item
{
    public async void UseSpecialItem(Tile tile)
    {
        var deflateSequence = DOTween.Sequence();
        for (int y = 0; y < Board.Instance.height; y++)
        {
            for (var x = 0; x < Board.Instance.width; x++)
            {
                if (x == tile.x || y == tile.y)
                {
                    var currentTile = Board.Instance.tiles[x, y];
                
                    deflateSequence.Join(currentTile.icon.transform.DOScale(Vector3.zero, Board.Instance.tweenDuration));

                    currentTile.isEmpty = true;
                }
            }
        }
        await deflateSequence.Play().AsyncWaitForCompletion();
    }

}
