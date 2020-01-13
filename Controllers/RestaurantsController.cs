using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TacitCoreDemo.Services;
using TacitCoreDemo.Models;

namespace TacitCoreDemo.Controllers
{
    [ApiVersion("1")]    

    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
   
    public class RestaurantsController : ControllerBase
    {
        private readonly IWebOrderingService _webOrderingService;

        public RestaurantsController(IWebOrderingService webOrderingService)
        {
            _webOrderingService = webOrderingService;
        }

        /// <summary>
        /// To get list of all menu items in the restaurant
        /// </summary>
        /// <param name="restaurantId">Restaurant Id</param>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns>RestaurantMenu</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{restaurantId:int:min(1)}")]
        
        public async Task<ActionResult<IEnumerable<RestaurantMenu>>> GetAllAsync(int restaurantId, [FromQuery] int pageIndex=0, [FromQuery] int pageSize = 0)
        {            
            var restaurantMenu = await _webOrderingService.GetRestaurantMenusAsync(restaurantId,null, pageIndex, pageSize);
            if (restaurantMenu == null || restaurantMenu.Count() == 0)
            { 
                throw new Exception(string.Format("No Menu items found for restaurant id = {0}", restaurantId));
            }
            return Ok(restaurantMenu);
        }

        /// <summary>
        /// To lookup for menu items in the restaurant
        /// </summary>
        /// <param name="restaurantId">Restaurant Id</param>
        /// <param name="searchText">Search Text</param>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns>RestaurantMenu</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{restaurantId:int:min(1)}/search/{searchText:alpha}")]
        [ProducesResponseType(typeof(IEnumerable<RestaurantMenu>), 200)]
        public async Task<ActionResult<IEnumerable<RestaurantMenu>>> GetAsync(int restaurantId, string searchText, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 0)
        {            
            var restaurantMenu = await _webOrderingService.GetRestaurantMenusAsync(restaurantId,searchText, pageIndex, pageSize);
            if (restaurantMenu == null || restaurantMenu.Count()==0)
            {
                throw new Exception(string.Format("No Menu items found for restaurant id = {0} for search value '{1}'", restaurantId, searchText));
            }
            return Ok(restaurantMenu);
        }

    }
}
