using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.UI.Pooling
{
    public class CardButtonPool : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;
        
        private Queue<CardButton> pool = new Queue<CardButton>();
        private List<CardButton> activeButtons = new List<CardButton>();
        
        private void Awake()
        {
            InitializePool();
        }
        
        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewButton();
            }
        }
        
        private void CreateNewButton()
        {
            if (pool.Count >= maxPoolSize) return;
            
            GameObject buttonObj = Instantiate(cardButtonPrefab, transform);
            buttonObj.SetActive(false);
            CardButton button = buttonObj.GetComponent<CardButton>();
            pool.Enqueue(button);
        }
        
        public CardButton GetCardButton(CardData card, Transform parent = null)
        {
            if (pool.Count == 0)
            {
                CreateNewButton();
            }
            
            CardButton button = pool.Dequeue();
            button.transform.SetParent(parent ?? transform);
            button.gameObject.SetActive(true);
            button.Initialize(card);
            activeButtons.Add(button);
            
            return button;
        }
        
        public void ReturnCardButton(CardButton button)
        {
            if (button == null) return;
            
            button.Reset();
            button.transform.SetParent(transform);
            button.gameObject.SetActive(false);
            activeButtons.Remove(button);
            pool.Enqueue(button);
        }
        
        public void ReturnAllButtons()
        {
            foreach (var button in activeButtons.ToArray())
            {
                ReturnCardButton(button);
            }
        }
        
        public void ClearPool()
        {
            ReturnAllButtons();
            
            while (pool.Count > 0)
            {
                CardButton button = pool.Dequeue();
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
        }
        
        private void OnDestroy()
        {
            ClearPool();
        }
    }
} 