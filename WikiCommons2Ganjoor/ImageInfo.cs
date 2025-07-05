namespace WikiCommons2Ganjoor
{
    public class ImageInfo
    {
        public int PageNumber { get; set; }
        public bool Uploaded { get; set; }
        public string Title { get; set; }
        public string OriginalFileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public override string ToString()
        {
            return $"{Title}\nOriginal: {OriginalFileUrl}\nThumbnail: {ThumbnailUrl}\n";
        }
    }
}
