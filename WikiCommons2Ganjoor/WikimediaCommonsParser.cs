
using HtmlAgilityPack;

namespace WikiCommons2Ganjoor
{



    public class WikimediaCommonsParser
    {
        private readonly HttpClient _httpClient;

        public WikimediaCommonsParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; WikimediaCommonsParser/1.0)");
        }

        public async Task<List<ImageInfo>> ParsePageAsync(string url)
        {
            var imageInfos = new List<ImageInfo>();

            try
            {
                // Download the page content
                var html = await _httpClient.GetStringAsync(url);
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(html);

                // Find all gallery boxes using multiple possible selectors
                var galleryBoxes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'gallerybox')]") ??
                                 htmlDocument.DocumentNode.SelectNodes("//li[contains(@class, 'gallerybox')]") ??
                                 htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'thumb')]");

                if (galleryBoxes != null)
                {
                    foreach (var box in galleryBoxes)
                    {
                        var imageInfo = new ImageInfo();

                        // Extract title from the gallerytext div
                        var titleNode = box.SelectSingleNode(".//div[@class='gallerytext']") ??
                                       box.SelectSingleNode(".//div[@class='thumbcaption']");
                        if (titleNode != null)
                        {
                            imageInfo.Title = titleNode.InnerText.Trim();
                        }

                        // Extract thumbnail URL
                        var imgNode = box.SelectSingleNode(".//img");
                        if (imgNode != null)
                        {
                            var thumbnailSrc = imgNode.GetAttributeValue("src", "");
                            if (!string.IsNullOrEmpty(thumbnailSrc))
                            {
                                imageInfo.ThumbnailUrl = NormalizeUrl(thumbnailSrc);
                            }
                        }

                        // Extract file page link
                        var fileLink = box.SelectSingleNode(".//a[contains(@class,'image')]") ??
                                     box.SelectSingleNode(".//a[contains(@href,'/wiki/File:')]");
                        if (fileLink != null)
                        {
                            var filePageUrl = NormalizeUrl(fileLink.GetAttributeValue("href", ""));
                            if (!string.IsNullOrEmpty(filePageUrl))
                            {
                                // Get the direct image URL from the file page
                                imageInfo.OriginalFileUrl = await GetDirectImageUrl(filePageUrl);
                            }
                        }

                        // Fallback: Try to construct from thumbnail if we couldn't get the original
                        if (string.IsNullOrEmpty(imageInfo.OriginalFileUrl) && !string.IsNullOrEmpty(imageInfo.ThumbnailUrl))
                        {
                            imageInfo.OriginalFileUrl = ConstructOriginalFromThumbnail(imageInfo.ThumbnailUrl);
                        }

                        if (!string.IsNullOrEmpty(imageInfo.Title) && !string.IsNullOrEmpty(imageInfo.OriginalFileUrl))
                        {
                            imageInfos.Add(imageInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing page: {ex.Message}");
            }

            return imageInfos;
        }

        private async Task<string> GetDirectImageUrl(string filePageUrl)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(filePageUrl);
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(html);

                // First try: Look for the "original file" link
                var directLink = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='fullMedia']//a[@href]");
                if (directLink != null)
                {
                    var href = directLink.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(href))
                    {
                        return NormalizeUrl(href);
                    }
                }

                // Second try: Look for the download link
                var downloadLink = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='mw-download-button']/a[@href]");
                if (downloadLink != null)
                {
                    var href = downloadLink.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(href))
                    {
                        return NormalizeUrl(href);
                    }
                }

                // Third try: Look for the og:image meta tag
                var ogImage = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                if (ogImage != null)
                {
                    var content = ogImage.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(content))
                    {
                        return NormalizeUrl(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting direct image URL from {filePageUrl}: {ex.Message}");
            }

            return null;
        }

        private string ConstructOriginalFromThumbnail(string thumbnailUrl)
        {
            if (string.IsNullOrEmpty(thumbnailUrl)) return null;

            // Example conversion:
            // From: https://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/Mirek_Firdousi_Gazna.jpg/300px-Mirek_Firdousi_Gazna.jpg
            // To:   https://upload.wikimedia.org/wikipedia/commons/b/bb/Mirek_Firdousi_Gazna.jpg
            if (thumbnailUrl.Contains("/thumb/"))
            {
                var parts = thumbnailUrl.Split(new[] { "/thumb/" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var filePart = parts[1].Split('/')[1];
                    return $"{parts[0]}/{filePart}";
                }
            }

            return null;
        }

        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            if (url.StartsWith("//"))
            {
                return "https:" + url;
            }
            else if (url.StartsWith("/"))
            {
                return "https://commons.wikimedia.org" + url;
            }

            return url;
        }
    }
}
