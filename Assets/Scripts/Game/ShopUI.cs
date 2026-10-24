using System;
using UnityEngine;
using UnityEngine.UI;

// Put on the ShopCanvas object (stays active). R toggles the panel via PlayerController; this spends GameState.Coins.
public class ShopUI : MonoBehaviour
{
    [Serializable]
    public class Row
    {
        public Text title;
        public Text description;
        public Button buyButton;
        public Text buyLabel;
    }

    class Upgrade
    {
        public string name, description;
        public int baseCost, level, maxLevel;
        public Action<PlayerController> apply;
    }

    public GameObject panel;
    public Text coinsText;
    public Button closeButton;
    public Row[] rows;

    PlayerController player;
    Upgrade[] upgrades;

    void Awake()
    {
        upgrades = new[]
        {
            new Upgrade { name = "Collect Range", description = "Pick up trash from further away (+0.6)", baseCost = 5, maxLevel = 5, apply = p => p.collectRadius += 0.6f },
            new Upgrade { name = "Camera Range", description = "Photos reach further targets (+10)", baseCost = 8, maxLevel = 4, apply = p => p.photoRange += 10f },
            new Upgrade { name = "Move Speed", description = "Robot moves faster (+1)", baseCost = 6, maxLevel = 5, apply = p => p.moveSpeed += 1f },
            new Upgrade { name = "Camera Cooldown", description = "Take photos more often (-0.1s)", baseCost = 6, maxLevel = 4, apply = p => p.photoCooldownTime = Mathf.Max(0.2f, p.photoCooldownTime - 0.1f) },
        };

        for (int i = 0; i < rows.Length && i < upgrades.Length; i++)
        {
            int index = i;
            rows[i].description.text = upgrades[i].description;
            rows[i].buyButton.onClick.AddListener(() => Buy(index));
        }
        closeButton.onClick.AddListener(() => { if (player != null) player.SetShopOpen(false); });
        panel.SetActive(false);
    }

    int Cost(Upgrade u) => u.baseCost * (u.level + 1);

    void Buy(int index)
    {
        var u = upgrades[index];
        if (player == null || u.level >= u.maxLevel) return;
        if (!GameState.TrySpend(Cost(u)))
        {
            GameHud.Flash("Not enough coins!");
            return;
        }
        u.level++;
        u.apply(player);
        Refresh();
    }

    void Refresh()
    {
        coinsText.text = $"Coins: {GameState.Coins}";
        for (int i = 0; i < rows.Length && i < upgrades.Length; i++)
        {
            var u = upgrades[i];
            bool maxed = u.level >= u.maxLevel;
            rows[i].title.text = $"{u.name}  (Lv {u.level}/{u.maxLevel})";
            rows[i].buyLabel.text = maxed ? "MAX" : $"Buy - {Cost(u)}";
            rows[i].buyButton.interactable = !maxed && GameState.Coins >= Cost(u);
        }
    }

    void Update()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        bool show = player != null && player.ShopOpen && GameState.Ending == EndingType.None;
        if (panel.activeSelf != show) panel.SetActive(show);
        if (show) Refresh();
    }
}
