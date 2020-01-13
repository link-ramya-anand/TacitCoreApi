using System.Collections.Generic;
using System.Threading.Tasks;
using TacitCoreDemo.Models;

namespace TacitCoreDemo.Services
{
    public interface IWebOrderingService
    {
        Task<IEnumerable<RestaurantMenu>> GetRestaurantMenusAsync(int restaurantId, string searchText = null, int pageIndex = 0, int pageSize = 0);

    }
}
