using System.Collections.Generic;
using System.Net;
using System.Xml;

namespace AmazonPAAPI.Models
{
    public class ItemSearch
    {
        SignedRequestHelper helper = new SignedRequestHelper(Globals.ACCESS_KEY_ID, Globals.SECRET_KEY, Globals.LOCALE);

        public ItemSearch(string searchIndex, string searchKeywords, int pageNr)
        {
            _searchIndex = searchIndex;
            if (_searchIndex == null || _searchIndex == "") { _searchIndex = "All"; }
            _searchKeywords = searchKeywords;
            if (_searchKeywords == null || _searchKeywords == "")
            {
                _errorMessage = "Please, insert a keyword to find items!";
                return;
            }
            _pageNr = pageNr;
            _totalResults = TotalItemResults(_searchIndex, _searchKeywords);
            if (_totalResults == 0 && _errorMessage == null)
            {
                _errorMessage = "No items match with keyword "+'"'+_searchKeywords+'"'+"!";
                return;
            }
            else if (_totalResults == 0 && _errorMessage != null)
            {
                return;
            }
            if (_totalResults > 50 && _searchIndex == "All") { _totalResults = 50; }
            int initialItemsOnPage = 10;
            int newItemsOnPage = Globals.MAX_ITEMS_ON_PAGE;
            int initialPageCount = _totalResults / initialItemsOnPage;
            int newPageCount = _totalResults / newItemsOnPage;
            if ((newPageCount * newItemsOnPage) < _totalResults) { newPageCount = newPageCount + 1; }

            _firstItemIndex = (_pageNr - 1) * newItemsOnPage;
            int lastItemIndex = _firstItemIndex + (newItemsOnPage - 1);

            int pageNrForFirstItem = CalculatePageNr(_firstItemIndex);
            int pageNrForLastItem = CalculatePageNr(lastItemIndex);
            int pageDifference = pageNrForLastItem - pageNrForFirstItem;
            float numberOfRequests = (pageDifference + 1) / 2f;
            int _pageNrForFirstItem = pageNrForFirstItem;

            int firstItemIndexOnPage = _firstItemIndex - ((pageNrForFirstItem - 1) * 10);
            int lastItemIndexOnPage = firstItemIndexOnPage + (newItemsOnPage - 1);
            string pageNrString = pageNrForFirstItem.ToString();
            string pageNrStringNext = pageNrForLastItem.ToString();

            // loop for receiving items
            for (int i = 0; i < numberOfRequests; i++)
            {
                IDictionary<string, string> r1 = new Dictionary<string, string>();
                r1["Service"] = "AWSECommerceService";
                r1["Version"] = "2011-08-01";
                r1["AssociateTag"] = Globals.ASSOCIATE_TAG;
                r1["Operation"] = "ItemSearch";
                r1["ItemSearch.Shared.Availability"] = "Available";
                r1["ItemSearch.Shared.ResponseGroup"] = "Small,Images,Offers";
                r1["ItemSearch.Shared.SearchIndex"] = _searchIndex;
                r1["ItemSearch.Shared.Keywords"] = _searchKeywords;

                pageNrForFirstItem = _pageNrForFirstItem + (2 * i);
                pageNrString = pageNrForFirstItem.ToString();

                // getting correct amount of items is an iffi business
                if (pageNrForFirstItem > initialPageCount)
                {
                    _errorMessage = "No more items!";
                    return;
                } else if (pageNrForLastItem > initialPageCount && pageDifference == 1 || pageNrForFirstItem == initialPageCount && pageNrForFirstItem == pageNrForLastItem)
                {
                    
                    r1["ItemSearch.1.ItemPage"] = pageNrString;
                    if (i != 0)
                    {
                        firstItemIndexOnPage = 0;
                        lastItemIndexOnPage = (lastItemIndex - ((pageNrForLastItem - 1) * 10));
                    }
                }
                else if (pageDifference > 1)
                {
                    
                    if (pageNrForFirstItem < initialPageCount)
                    {
                        if (pageNrForFirstItem == pageNrForLastItem)
                        {
                            r1["ItemSearch.1.ItemPage"] = pageNrString;
                            firstItemIndexOnPage = 0;
                            lastItemIndexOnPage = (lastItemIndex - ((pageNrForLastItem - 1) * 10));
                        }
                        else
                        {
                            int pageNrForMiddleItems = (pageNrForFirstItem + 1);
                            pageNrStringNext = pageNrForMiddleItems.ToString();
                            r1["ItemSearch.1.ItemPage"] = pageNrString;
                            r1["ItemSearch.2.ItemPage"] = pageNrStringNext;

                            if (pageNrForMiddleItems == pageNrForLastItem)
                            {
                                lastItemIndexOnPage = (lastItemIndex - ((pageNrForLastItem - 2) * 10));
                            }
                            else
                            {
                                lastItemIndexOnPage = 20;
                                if (pageNrForMiddleItems >= initialPageCount)
                                {
                                    numberOfRequests = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        r1["ItemSearch.1.ItemPage"] = pageNrString;
                    } 

                }
                else if (pageDifference == 1)
                {
                    r1["ItemSearch.1.ItemPage"] = pageNrString;
                    r1["ItemSearch.2.ItemPage"] = pageNrStringNext;
                }
                else if (pageDifference == 0)
                {
                    r1["ItemSearch.1.ItemPage"] = pageNrString;
                }

                // sign request
                string requestUrl = helper.Sign(r1);

                WebRequest request = HttpWebRequest.Create(requestUrl);
                WebResponse response = request.GetResponse();

                XmlDocument doc = new XmlDocument();
                doc.Load(response.GetResponseStream());

                // Get item values
                int currentIndex = 0;
                foreach (XmlNode node in doc.GetElementsByTagName("Item"))
                {
                    if (currentIndex >= firstItemIndexOnPage && currentIndex <= lastItemIndexOnPage)
                    {
                        var item = new Item();
                        item.ASIN = node["ASIN"].InnerText;
                        item.Title = node["ItemAttributes"]["Title"].InnerText;
                        item.ItemURL = node["DetailPageURL"].InnerText;
                        try
                        {
                            item.LowestPriceInt = (int.Parse(node["OfferSummary"]["LowestNewPrice"]["Amount"].InnerText, null) / 100.0);
                        }
                        catch
                        {
                            item.LowestPriceStr = "N/A";
                        }
                        try
                        {
                            item.ImageURL = node["MediumImage"]["URL"].InnerText;
                        }
                        catch
                        {
                            item.ImageURL = "http://g-ecx.images-amazon.com/images/G/02/misc/no-img-lg-uk._V366109917_.gif";
                        }
                        try
                        {
                            item.CurrencyCode = node["OfferSummary"]["LowestNewPrice"]["CurrencyCode"].InnerText;
                        }
                        catch
                        {
                            item.CurrencyCode = "";
                        }
                        _itemsList.Add(item);
                    }
                    currentIndex++;
                }
            }
        }

        // calculate page nr according to item index
        private int CalculatePageNr(int pageIndex)
        {
            int tenthIndex = 0;
            for (int i = 0; tenthIndex <= pageIndex; i++)
            {
                tenthIndex = tenthIndex + 10;
            }
            return tenthIndex/10;
        }

        // make a request to receive number of results
        public int TotalItemResults(string searchIndex, string searchKeywords){
            IDictionary<string, string> r2 = new Dictionary<string, string>();
            r2["Service"] = "AWSECommerceService";
            r2["Version"] = "2011-08-01";
            r2["AssociateTag"] = Globals.ASSOCIATE_TAG;
            r2["Operation"] = "ItemSearch";
            r2["Availability"] = "Available";
            r2["ResponseGroup"] = "Small";
            r2["SearchIndex"] = searchIndex;
            r2["Keywords"] = searchKeywords;

            string requestUrl = helper.Sign(r2);
            try
            {
                WebRequest request = HttpWebRequest.Create(requestUrl);
                WebResponse response = request.GetResponse();

                XmlDocument doc = new XmlDocument();
                doc.Load(response.GetResponseStream());

                XmlNode node = doc.GetElementsByTagName("Items")[0];
                _totalResults = 0;
                _totalResults = int.Parse(node["TotalResults"].InnerText, null);
                if (_totalResults >= 100)
                {
                    _totalResults = 100;
                }
                return _totalResults;
            }
            catch
            {
                _totalResults = 0;
                _errorMessage = "Are you sure you followed the <b>README.md</b> correctly?";
                return _totalResults;
            }
        }   
        
        public List<Item> Items()
        {
            return _itemsList;
        }
        public int PageCount()
        {
            int pageCount = _totalResults / Globals.MAX_ITEMS_ON_PAGE;
            if ((pageCount * Globals.MAX_ITEMS_ON_PAGE) < _totalResults) { pageCount = pageCount + 1; }
            return pageCount;
        }
        public int PageNr()
        {
            return _pageNr;
        }
        public int ItemIndex()
        {
            return _firstItemIndex;
        }
        public string SearchKeywords()
        {
            return _searchKeywords;
        }
        public string SearchIndex()
        {
            return _searchIndex;
        }
        public string ErrorMessage()
        {
            return _errorMessage;
        }

        // declare private values
        private List<Item> _itemsList = new List<Item>();
        private int _totalResults;
        private int _pageNr;
        private int _firstItemIndex;
        private string _searchKeywords;
        private string _searchIndex;
        private string _errorMessage;
        
    }
}