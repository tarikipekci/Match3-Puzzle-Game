using UnityEngine;

namespace SpecialItems
{
    [CreateAssetMenu(menuName = "Match3/SpecialItem/Clover")]
    public class Clover : Item, ISpecialItem
    {
        public void ExecuteSpecialItem(Tile tile)
        {
            var levelManager = FindObjectOfType<LevelManager>();
            levelManager.numberOfMoves++;
        }
    }
}