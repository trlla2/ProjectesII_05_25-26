public class ShopOffer
{
    public enum OfferType { Item, Heal, StatUpgrade }
    public OfferType Type { get; private set; }
    public Item Item { get; private set; }
    public int Cost { get; private set; }
    public float HealPercent { get; private set; }
    public StatUpgradeType StatType   { get; private set; }
    public float StatAmount { get; private set; }

    public bool IsPurchased { get; private set; }

    public void MarkAsPurchased() => IsPurchased = true;

    public static ShopOffer CreateItemOffer(Item item, int cost)
    {
        return new ShopOffer { Type = OfferType.Item, Item = item, Cost = cost };
    }

    public static ShopOffer CreateHealOffer(float healPercent, int cost)
    {
        return new ShopOffer { Type = OfferType.Heal, HealPercent = healPercent, Cost = cost };
    }

    public static ShopOffer CreateStatUpgradeOffer(StatUpgradeType stat, float amount, int cost)
    {
        return new ShopOffer { Type = OfferType.StatUpgrade, StatType = stat, StatAmount = amount, Cost = cost };
    }
}