using System;
using System.Collections.Generic;
using AmazonPAAPI.Models;

namespace AmazonPAAPI
{
    public static class Globals
    {
        public const String OPENEXCHANGERATE_APP_ID = "{YourOER_App_Id}";       // Your openexchangerates.org app id
        public const String ASSOCIATE_TAG = "{YourA_Account_Tag}";              // Your Amazon associate tag
        public const String ACCESS_KEY_ID = "YourAWS_Access_Key";               // Your Amazon Web Services access key
        public const String SECRET_KEY = "YourAWS_Secret_Key";                  // Your Amazon Web Services secret key
        public const String LOCALE = "webservices.amazon.co.uk";
        public const int MAX_ITEMS_ON_PAGE = 13;
        public static List<string> SEARCH_INDEX_VALUES = new IndexValues().SearchIndexValues();
        
    }
}