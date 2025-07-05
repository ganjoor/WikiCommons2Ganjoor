using HtmlDocument = HtmlAgilityPack.HtmlDocument;

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
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                // Find all gallery boxes
                var galleryBoxes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'gallerybox')]");

                if (galleryBoxes != null)
                {
                    foreach (var box in galleryBoxes)
                    {
                        var imageInfo = new ImageInfo()
                        {
                            PageNumber = 0,
                            Uploaded = false
                        };

                        // Extract title from the gallerytext div
                        var titleNode = box.SelectSingleNode(".//div[@class='gallerytext']");
                        if (titleNode != null)
                        {
                            imageInfo.Title = titleNode.InnerText.Trim();
                        }

                        // Extract image node and get the source
                        var imgNode = box.SelectSingleNode(".//img");
                        if (imgNode != null)
                        {
                            // Get thumbnail URL
                            var thumbnailSrc = imgNode.GetAttributeValue("src", "");
                            if (!string.IsNullOrEmpty(thumbnailSrc))
                            {
                                // Ensure proper URL format
                                if (thumbnailSrc.StartsWith("//"))
                                {
                                    thumbnailSrc = "https:" + thumbnailSrc;
                                }
                                imageInfo.ThumbnailUrl = thumbnailSrc;
                            }

                            // Get original file URL
                            var fileLink = box.SelectSingleNode(".//a[contains(@class,'image')]");
                            if (fileLink != null)
                            {
                                var href = fileLink.GetAttributeValue("href", "");
                                if (!string.IsNullOrEmpty(href))
                                {
                                    if (href.StartsWith("//"))
                                    {
                                        href = "https:" + href;
                                    }
                                    else if (href.StartsWith("/"))
                                    {
                                        href = "https://commons.wikimedia.org" + href;
                                    }

                                    // Follow the link to get the original file URL
                                    imageInfo.OriginalFileUrl = await GetOriginalFileUrl(href);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(imageInfo.Title) &&
                            (!string.IsNullOrEmpty(imageInfo.OriginalFileUrl) ||
                             !string.IsNullOrEmpty(imageInfo.ThumbnailUrl)))
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

        private async Task<string> GetOriginalFileUrl(string filePageUrl)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(filePageUrl);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                // Find the original file link
                var fileLink = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='fullMedia']//a[@href]");
                if (fileLink != null)
                {
                    var href = fileLink.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(href))
                    {
                        if (href.StartsWith("//"))
                        {
                            return "https:" + href;
                        }
                        return href;
                    }
                }

                // Alternative approach if the above doesn't work
                var ogImage = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                if (ogImage != null)
                {
                    var content = ogImage.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(content))
                    {
                        // Remove any size parameters to get the original
                        return content.Split(new[] { "/thumb/" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                }
            }
            catch
            {
                // If we can't fetch the file page, try to construct from thumbnail URL
                return null;
            }

            return null;
        }
    }

    
}
