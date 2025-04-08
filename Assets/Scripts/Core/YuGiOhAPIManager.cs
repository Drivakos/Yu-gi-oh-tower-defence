using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace YuGiOhTowerDefense.Core
{
    public class YuGiOhAPIManager : MonoBehaviour
    {
        private const string API_BASE_URL = "https://db.ygoprodeck.com/api/v7/";
        private const string CACHE_FOLDER = "YuGiOhCache";
        private const string CARD_DATA_FILE = "card_data.json";
        private const string CARD_IMAGES_FOLDER = "card_images";
        
        [Header("API Settings")]
        [SerializeField] private bool useCache = true;
        [SerializeField] private float cacheExpirationHours = 24f;
        [SerializeField] private int maxConcurrentRequests = 5;
        
        [Header("Card Type Filters")]
        [SerializeField] private bool includeNormalMonsters = true;
        [SerializeField] private bool includeEffectMonsters = true;
        [SerializeField] private bool includeRitualMonsters = true;
        [SerializeField] private bool includeFusionMonsters = true;
        [SerializeField] private bool includeSynchroMonsters = true;
        [SerializeField] private bool includeXyzMonsters = true;
        [SerializeField] private bool includeLinkMonsters = true;
        [SerializeField] private bool includeSpellCards = true;
        [SerializeField] private bool includeTrapCards = true;
        
        private Dictionary<string, YuGiOhCard> cardDatabase = new Dictionary<string, YuGiOhCard>();
        private Dictionary<string, Sprite> cardSprites = new Dictionary<string, Sprite>();
        private Queue<CardImageRequest> imageRequestQueue = new Queue<CardImageRequest>();
        private int activeRequests = 0;
        private bool isLoading = false;
        private DateTime lastCacheUpdate;
        
        private class CardImageRequest
        {
            public string cardId;
            public Action<Sprite> callback;
            
            public CardImageRequest(string id, Action<Sprite> cb)
            {
                cardId = id;
                callback = cb;
            }
        }
        
        private void Awake()
        {
            // Create cache directory if it doesn't exist
            string cachePath = Path.Combine(Application.persistentDataPath, CACHE_FOLDER);
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            
            string imagesPath = Path.Combine(cachePath, CARD_IMAGES_FOLDER);
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }
            
            // Load cached data if available
            if (useCache)
            {
                LoadCachedData();
            }
        }
        
        private void LoadCachedData()
        {
            string cachePath = Path.Combine(Application.persistentDataPath, CACHE_FOLDER);
            string dataPath = Path.Combine(cachePath, CARD_DATA_FILE);
            
            if (File.Exists(dataPath))
            {
                try
                {
                    string json = File.ReadAllText(dataPath);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    
                    if (data.ContainsKey("lastUpdate"))
                    {
                        lastCacheUpdate = DateTime.Parse(data["lastUpdate"].ToString());
                        
                        // Check if cache is expired
                        if ((DateTime.Now - lastCacheUpdate).TotalHours > cacheExpirationHours)
                        {
                            Debug.Log("Cache expired, fetching fresh data...");
                            StartCoroutine(FetchAllCardData());
                            return;
                        }
                    }
                    
                    if (data.ContainsKey("cards"))
                    {
                        var cards = JsonConvert.DeserializeObject<Dictionary<string, YuGiOhCard>>(data["cards"].ToString());
                        cardDatabase = cards;
                        Debug.Log($"Loaded {cardDatabase.Count} cards from cache");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading cached data: {e.Message}");
                    StartCoroutine(FetchAllCardData());
                }
            }
            else
            {
                Debug.Log("No cache found, fetching card data...");
                StartCoroutine(FetchAllCardData());
            }
        }
        
        private void SaveCachedData()
        {
            string cachePath = Path.Combine(Application.persistentDataPath, CACHE_FOLDER);
            string dataPath = Path.Combine(cachePath, CARD_DATA_FILE);
            
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "lastUpdate", DateTime.Now.ToString() },
                    { "cards", cardDatabase }
                };
                
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(dataPath, json);
                Debug.Log($"Saved {cardDatabase.Count} cards to cache");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving cache: {e.Message}");
            }
        }
        
        private IEnumerator FetchAllCardData()
        {
            isLoading = true;
            
            using (UnityWebRequest www = UnityWebRequest.Get(API_BASE_URL + "cardinfo.php"))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
                        
                        if (response.ContainsKey("data"))
                        {
                            var cards = JsonConvert.DeserializeObject<List<YuGiOhCard>>(response["data"].ToString());
                            
                            cardDatabase.Clear();
                            foreach (var card in cards)
                            {
                                if (ShouldIncludeCard(card))
                                {
                                    cardDatabase[card.id] = card;
                                }
                            }
                            
                            Debug.Log($"Fetched {cardDatabase.Count} cards from API");
                            
                            if (useCache)
                            {
                                SaveCachedData();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error parsing card data: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"Error fetching card data: {www.error}");
                }
            }
            
            isLoading = false;
        }
        
        private bool ShouldIncludeCard(YuGiOhCard card)
        {
            switch (card.type.ToLower())
            {
                case "normal monster":
                    return includeNormalMonsters;
                case "effect monster":
                    return includeEffectMonsters;
                case "ritual monster":
                    return includeRitualMonsters;
                case "fusion monster":
                    return includeFusionMonsters;
                case "synchro monster":
                    return includeSynchroMonsters;
                case "xyz monster":
                    return includeXyzMonsters;
                case "link monster":
                    return includeLinkMonsters;
                case "spell card":
                    return includeSpellCards;
                case "trap card":
                    return includeTrapCards;
                default:
                    return false;
            }
        }
        
        public void GetCardImage(string cardId, Action<Sprite> callback)
        {
            if (cardSprites.ContainsKey(cardId))
            {
                callback?.Invoke(cardSprites[cardId]);
                return;
            }
            
            string cachePath = Path.Combine(Application.persistentDataPath, CACHE_FOLDER, CARD_IMAGES_FOLDER, $"{cardId}.png");
            
            if (File.Exists(cachePath))
            {
                StartCoroutine(LoadCachedImage(cachePath, cardId, callback));
            }
            else
            {
                imageRequestQueue.Enqueue(new CardImageRequest(cardId, callback));
                ProcessImageQueue();
            }
        }
        
        private IEnumerator LoadCachedImage(string path, string cardId, Action<Sprite> callback)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            cardSprites[cardId] = sprite;
            
            callback?.Invoke(sprite);
            yield return null;
        }
        
        private void ProcessImageQueue()
        {
            if (activeRequests >= maxConcurrentRequests || imageRequestQueue.Count == 0)
            {
                return;
            }
            
            CardImageRequest request = imageRequestQueue.Dequeue();
            StartCoroutine(FetchCardImage(request.cardId, request.callback));
        }
        
        private IEnumerator FetchCardImage(string cardId, Action<Sprite> callback)
        {
            activeRequests++;
            
            string imageUrl = $"https://images.ygoprodeck.com/images/cards/{cardId}.jpg";
            
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    
                    cardSprites[cardId] = sprite;
                    
                    // Cache the image
                    string cachePath = Path.Combine(Application.persistentDataPath, CACHE_FOLDER, CARD_IMAGES_FOLDER, $"{cardId}.png");
                    File.WriteAllBytes(cachePath, www.downloadHandler.data);
                    
                    callback?.Invoke(sprite);
                }
                else
                {
                    Debug.LogError($"Error fetching card image: {www.error}");
                    callback?.Invoke(null);
                }
            }
            
            activeRequests--;
            ProcessImageQueue();
        }
        
        public YuGiOhCard GetCardData(string cardId)
        {
            if (cardDatabase.TryGetValue(cardId, out YuGiOhCard card))
            {
                return card;
            }
            return null;
        }
        
        public List<YuGiOhCard> GetAllCards()
        {
            return new List<YuGiOhCard>(cardDatabase.Values);
        }
        
        public List<YuGiOhCard> GetCardsByType(string type)
        {
            return cardDatabase.Values.FindAll(card => card.type.ToLower() == type.ToLower());
        }
        
        public List<YuGiOhCard> SearchCards(string query)
        {
            query = query.ToLower();
            return cardDatabase.Values.FindAll(card => 
                card.name.ToLower().Contains(query) || 
                card.desc.ToLower().Contains(query));
        }
        
        public bool IsLoading()
        {
            return isLoading;
        }
    }
    
    [Serializable]
    public class YuGiOhCard
    {
        public string id;
        public string name;
        public string type;
        public string desc;
        public string race;
        public string archetype;
        public List<YuGiOhCardSet> card_sets;
        public List<YuGiOhCardImage> card_images;
        public List<YuGiOhCardPrice> card_prices;
        
        [Serializable]
        public class YuGiOhCardSet
        {
            public string set_name;
            public string set_code;
            public string set_rarity;
            public string set_price;
        }
        
        [Serializable]
        public class YuGiOhCardImage
        {
            public string id;
            public string image_url;
            public string image_url_small;
        }
        
        [Serializable]
        public class YuGiOhCardPrice
        {
            public string cardmarket_price;
            public string tcgplayer_price;
            public string ebay_price;
            public string amazon_price;
            public string coolstuffinc_price;
        }
    }
} 