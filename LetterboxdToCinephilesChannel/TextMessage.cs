namespace LetterboxdToCinephilesChannel
{
    internal class TextMessage
    {
        //Title cant be empty
        public string FilmTitle { get; set; } = string.Empty;
        public string FilmYear { get; set; } = string.Empty;
        //Rating need not be posted
        public string MemberRating { get; set; } = string.Empty;
        //if rating is not posted then no need for total rating
        public string TotalRating { get; set; } = string.Empty;
        public string ImgSrc { get; set; } = string.Empty;
        //review is not required
        public string Review { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
    }
}
