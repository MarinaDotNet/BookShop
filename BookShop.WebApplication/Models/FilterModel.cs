namespace BookShop.WebApplication.Models
{
    public class FilterModel
    {
        public string? SelectedGenre { get; set; }

        public string ? SearchTearm { get; set; }

        public bool IsAscendingOrder { get; set; } = true;
    }
}
