namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktListItem
    {
        public int Rank { get; set; }
        public long Id { get; set; }
        public string Type { get; set; }
        public TraktMovie Movie { get; set; }
        public TraktShow Show { get; set; }
        public TraktPerson Person { get; set; }
    }

    public class TraktMovie
    {
        public string Title { get; set; }
        public int Year { get; set; }
        public TraktIds Ids { get; set; }
    }

    public class TraktShow
    {
        public string Title { get; set; }
        public int Year { get; set; }
        public TraktIds Ids { get; set; }
    }

    public class TraktPerson
    {
        public string Name { get; set; }
        public TraktIds Ids { get; set; }
    }

    public class TraktIds
    {
        public int Trakt { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public int? Tmdb { get; set; }
    }
}
