namespace HackerAPI.Model
{
    public class APIResponseModel<T>
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public List<T> Data { get; set; } = new List<T>();
    }

    public class HackerNews
    {
        public string by { get; set; }

        public int descendants { get; set; }

        public int id { get; set; }

        public int score { get; set; }

        public int time { get; set; }

        public string title { get; set; }

        public string type { get; set; }

        public string url { get; set; }
    }
}
