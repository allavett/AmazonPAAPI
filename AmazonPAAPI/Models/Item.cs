using System;

namespace AmazonPAAPI.Models
{
    public class Item
    {
        public String ASIN { get; set; }
        public String Title { get; set; }
        public String ItemURL { get; set; }
        public String ImageURL { get; set; }
        public Nullable<double> LowestPriceInt { get; set; }
        public String LowestPriceStr { get; set; }
        public String CurrencyCode { get; set; }
    }
}