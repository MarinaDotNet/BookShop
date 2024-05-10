namespace BookShop.WebApplication.Models.ViewsModels
{
    public class ViewModel
    {
        public string[] Genres { get; set; } = ["unspecified"];
        public ProductViewModel ProductViewModel { get; set; } = new();
        public PageViewModel PageViewModel { get; set; } = new();

        public FilterModel FilterViewModel { get; set; } = new();

        public ErrorViewModel ErrorViewModel { get; set; }
    }
}
