using Microsoft.Extensions.Configuration;
using System.Text;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TacitCoreDemo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TacitCoreDemo.Services
{
    public class WebOrderingService : IWebOrderingService
    {
        IConfiguration _configuration;
        #region "Constants"
        const string CacheConnectionString = "CacheConnectionString";
        const string CacheKeyPrefix = "CacheKeyPrefix";
        const string tacitApiUrl = "tacitApiUrl";
        const string Authorization = "Authorization";
        const string SiteName = "Site-Name";
        const string AppKey = "App-Key";
        const string AppLanguage = "App-Language";

        const string Id = "Id";
        const string Name = "Name";
        const string DeliveryTypeCode = "DeliveryTypeCode";
        const string MenuItems = "MenuItems";
        const string MenuItemGroups = "MenuItemGroups";
        const string RestaurantMenus = "RestaurantMenus";
        #endregion

        public WebOrderingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// To get list of menuitems in the Restaurant 
        /// </summary>
        /// <param name="restaurantId"></param>
        /// <param name="searchText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>IEnumerable<RestaurantMenu></returns>
        public async Task<IEnumerable<RestaurantMenu>> GetRestaurantMenusAsync(int restaurantId, string searchText=null, int pageIndex=0, int pageSize=0)
        {
            LoggerManager.InfoLog(string.Format("Executing service to get the menu list of restaurant - {0}", restaurantId));
            IEnumerable<RestaurantMenu> searchList = null;
            
            LoggerManager.DebugLog("Establishing connection with Redis cache");
            
            if (restaurantId>0)
            {
                //Read Redis Cache                
                IRedisList<RestaurantMenu> menuList = getRedisCacheObject(_configuration["CacheKeyPrefix"] + restaurantId);

                //Load Redis Cache
                if (menuList == null || menuList.Count == 0)
                {
                    LoggerManager.DebugLog("Loading items into the Redis cache");
                    menuList = await LoadMenuToRedisCache(restaurantId.ToString());
                }

                //Search cache for menuitem and return search list                
                if (menuList != null)
                {                    
                    if (!String.IsNullOrEmpty(searchText))
                    {
                        LoggerManager.InfoLog(string.Format("Searching for menu items with text - {0}", searchText));
                        searchList = menuList.Where<RestaurantMenu>(menuitem => menuitem.MenuItemName.Contains(searchText.ToUpper()));
                    }
                    else
                    {
                        searchList = menuList.AsEnumerable<RestaurantMenu>();
                    }
                }
            }
            if (pageIndex > 0 && pageSize > 0 && searchList != null && searchList.Count() > 0)
            {
                searchList = searchList.Skip(pageSize*(pageIndex - 1)).Take(pageSize);
                LoggerManager.InfoLog(string.Format("Returning paged menu items of page index - {0} and of size {1}", pageIndex, pageSize));
                if (searchList == null || searchList.Count() <= 0)
                {
                    throw new Exception("End of Page");
                }
            }
            return searchList;
        }

        /// <summary>
        /// to load menuitems to the Redis Cache
        /// </summary>
        /// <param name="restaurantId"></param>
        /// <returns>IRedisList<RestaurantMenu></returns>
        private async Task<IRedisList<RestaurantMenu>> LoadMenuToRedisCache(string restaurantId)
        {
            IRedisList<RestaurantMenu> menuList = null;
            //Get Restaurant Menus 
            Dictionary<string, object> restaurantData = await GetWebResponse(_configuration["tacitApiUrl"] + "restaurants/" + restaurantId );

            if (restaurantData!=null)
            {
                JArray restaurantMenus = (JArray)restaurantData[RestaurantMenus];
                Dictionary<string, object> menuData = null;

                if (restaurantMenus != null)
                {
                    RestaurantMenu menuCache = null;

                    //Create Redis cache object  
                    menuList = getRedisCacheObject(_configuration["CacheKeyPrefix"] + restaurantId);

                    for (int index = 0; index < restaurantMenus.Count(); index++)
                    {
                       JToken menu = (JToken)restaurantMenus[index];
                        
                        String menuId = menu[Id].ToString();
                        String menuName = menu[Name].ToString();
                        String deliveryType = menu[DeliveryTypeCode].ToString();

                        //Get MenuItems from each Menu
                        if (!String.IsNullOrEmpty(menuId))
                        {
                            menuData = await GetWebResponse(_configuration["tacitApiUrl"] + "menus/" + menuId.Trim());
                            JArray menuItemGroups = (JArray)menuData[MenuItemGroups];

                            for (int group = 0; group < menuItemGroups.Count(); group++)
                            {
                                JToken menuItemGroup = (JToken)menuItemGroups[group];
                                JArray menuItems = (JArray)menuItemGroup[MenuItems];
                                                                
                                if (menuItems != null)
                                {
                                    for (int item = 0; item < menuItems.Count(); item++)
                                    {
                                        JToken menuItem = (JToken)menuItems[item];

                                        String menuItemId = menuItem[Id].ToString();
                                        String menuItemName = menuItem[Name].ToString();

                                        //Add menuitems to Redis Cache

                                        menuCache = new RestaurantMenu
                                        {
                                            MenuItemId = menuItemId,
                                            MenuItemName = menuItemName,
                                            MenuId = menuId,
                                            MenuName = menuName,
                                            DeliveryTypeCode = deliveryType
                                        };

                                        menuList.Add(menuCache);

                                    }
                                }
                            }
                        }
                    }
                }
            }
            return menuList;
        }

        private IRedisList<RestaurantMenu> getRedisCacheObject(string cacheKey)
        {
            RedisClient redisClient = RedisRepository.GetInstance(_configuration.GetConnectionString("Redis"));
            IRedisTypedClient<RestaurantMenu> redis = redisClient.As<RestaurantMenu>();

            if(redisClient.ContainsKey(cacheKey))
                redisClient.Remove(cacheKey);

            TimeSpan expiration = new TimeSpan(1, 0, 0);
            redis.ExpireEntryIn(cacheKey, expiration);
            
            return redis.Lists[cacheKey];
        }

        /// <summary>
        /// to get json response from Tacit webapi
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Dictionary<string, object></returns>
        private async Task<Dictionary<string, object>> GetWebResponse(String url)
        {
            Dictionary<string, object> jsonData = null;

            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(url);

            webreq.ContentType = "application/json";
            webreq.Headers.Add(Authorization, _configuration["Authorization"]);
            webreq.Headers.Add(SiteName, _configuration["SiteName"]);
            webreq.Headers.Add(AppKey, _configuration["AppKey"]);
            webreq.Headers.Add(AppLanguage, _configuration["AppLanguage"]);

            LoggerManager.DebugLog("Calling tacit api to get menu lists");
            try
            {
                HttpWebResponse webres = await Task.Run(() => (HttpWebResponse)webreq.GetResponse());

                Stream resStream = webres.GetResponseStream();
                using (StreamReader resReader = new StreamReader(resStream, true))
                {
                    string jsonString = resReader.ReadToEnd();
                    jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);                        
                }
            }
            catch(Exception ex)
            {
                LoggerManager.ErrorLog("Error from Downstream Tacit Api - "+ex.Message);
            }
            return jsonData;
        }

    }
}