using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;
using System.Threading.Tasks;

namespace YuGiOhTowerDefense.Core.UI
{
    public class CardImageManager : MonoBehaviour
    {
        private static CardImageManager instance;
        public static CardImageManager Instance => instance;
        
        [Header("Settings")]
        [SerializeField] private string cardImagesPath = "CardImages";
        [SerializeField] private Sprite defaultCardImage;
        [SerializeField] private int maxCacheSize = 100;
        
        private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
        private Queue<string> cacheQueue = new Queue<string>();
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public async Task<Sprite> LoadCardImage(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning("Card ID is null or empty");
                return defaultCardImage;
            }
            
            // Check cache first
            if (imageCache.TryGetValue(cardId, out Sprite cachedSprite))
            {
                return cachedSprite;
            }
            
            // Load from resources
            string imagePath = $"{cardImagesPath}/{cardId}";
            ResourceRequest request = Resources.LoadAsync<Sprite>(imagePath);
            
            while (!request.isDone)
            {
                await Task.Yield();
            }
            
            Sprite sprite = request.asset as Sprite;
            
            if (sprite == null)
            {
                Debug.LogWarning($"Card image not found for ID: {cardId}");
                return defaultCardImage;
            }
            
            // Add to cache
            AddToCache(cardId, sprite);
            
            return sprite;
        }
        
        private void AddToCache(string cardId, Sprite sprite)
        {
            // Remove oldest item if cache is full
            if (imageCache.Count >= maxCacheSize)
            {
                string oldestId = cacheQueue.Dequeue();
                imageCache.Remove(oldestId);
            }
            
            // Add new item
            imageCache[cardId] = sprite;
            cacheQueue.Enqueue(cardId);
        }
        
        public void ClearCache()
        {
            imageCache.Clear();
            cacheQueue.Clear();
        }
        
        public void PreloadCardImages(List<string> cardIds)
        {
            foreach (string cardId in cardIds)
            {
                _ = LoadCardImage(cardId);
            }
        }
    }
} 