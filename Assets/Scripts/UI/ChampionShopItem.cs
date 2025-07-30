using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TacticsArena.Champions;
using TacticsArena.Shop;
using TacticsArena.Core;

namespace TacticsArena.UI
{
    public class ChampionShopItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image championIcon;
        public TextMeshProUGUI championName;
        public TextMeshProUGUI championCost;
        public Button buyButton;
        
        private ChampionData championData;
        
        private void Start()
        {
            if (buyButton != null)
                buyButton.onClick.AddListener(OnBuyClicked);
        }
        
        public void SetChampion(ChampionData data)
        {
            championData = data;
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (championData == null) return;
            
            if (championIcon != null && championData.championIcon != null)
                championIcon.sprite = championData.championIcon;
                
            if (championName != null)
                championName.text = championData.championName;
                
            if (championCost != null)
                championCost.text = $"{championData.cost}g";
        }
        
        private void OnBuyClicked()
        {
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            Player player = FindObjectOfType<Player>();
            
            if (shopManager != null && player != null && championData != null)
            {
                bool success = shopManager.BuyChampion(championData, player);
                if (success)
                {
                    // Champion bought successfully
                    gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("Not enough gold!");
                }
            }
        }
    }
}
