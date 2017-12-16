namespace CodeHub.WebViews
{
    public class UpgradeDetailsModel
    {
        public string Price { get; }

        public bool IsPurchased { get; }

        public UpgradeDetailsModel(string price, bool isPurchased)
        {
            Price = price;
            IsPurchased = isPurchased;
        }
    }
}

