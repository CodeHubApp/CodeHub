namespace CodeHub.iOS.WebViews
{
    public class UpgradeDetailsModel
    {
        public string Price { get; private set; }

        public bool IsPurchased { get; private set; }

        public UpgradeDetailsModel(string price, bool isPurchased)
        {
            Price = price;
            IsPurchased = isPurchased;
        }
    }
}

