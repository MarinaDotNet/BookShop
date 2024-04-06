/*
 * From user request
 */
using System.ComponentModel;

namespace BookShop.API.Models
{
    public class PageModel
    {
        public int RequestedPage { get; set; } = Query.MinPossiblePage;
        public int QuantityPerPage { get; set; } = Query.MinPossibleQuantityAtPage;
        public bool InAscendingOrder { get; set; } = false;

        [DefaultValue("Price")]
        public string OrderBy { get; set; } = "Price";
    }
}
