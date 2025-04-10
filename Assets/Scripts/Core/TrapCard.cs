using UnityEngine;
using YuGiOhTowerDefense.Base;

namespace YuGiOhTowerDefense.Cards
{
    public class TrapCard : MonoBehaviour
    {
        private YuGiOhCard cardData;
        
        public void Initialize(YuGiOhCard card)
        {
            cardData = card;
        }
        
        public YuGiOhCard GetCardData()
        {
            return cardData;
        }
    }
} 