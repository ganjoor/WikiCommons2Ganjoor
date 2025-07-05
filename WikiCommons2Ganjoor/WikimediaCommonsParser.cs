
using HtmlAgilityPack;
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

                // Try multiple selectors to find image containers
                var imageContainers = FindImageContainers(htmlDocument);

                if (imageContainers.Count > 0)
                {
                    foreach (var container in imageContainers)
                    {
                        var imageInfo = ExtractImageInfo(container);
                        if (imageInfo != null)
                        {
                            imageInfos.Add(imageInfo);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Warning: No image containers found using any selector");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing page: {ex.Message}");
            }

            return imageInfos;
        }

        private List<HtmlNode> FindImageContainers(HtmlDocument htmlDocument)
        {
            var containers = new List<HtmlNode>();

            // Try different selectors for different Wikimedia Commons page formats
            var selectors = new[]
            {
            "//div[contains(@class, 'gallerybox')]", // Standard gallery
            "//li[contains(@class, 'gallerybox')]",  // Alternative gallery format
            "//div[contains(@class, 'thumb')]",     // Thumbnail format
            "//li[contains(@class, 'thumb')]",      // Alternative thumbnail format
            "//div[contains(@class, 'image')]"      // Direct image container
        };

            foreach (var selector in selectors)
            {
                var nodes = htmlDocument.DocumentNode.SelectNodes(selector);
                if (nodes != null)
                {
                    containers.AddRange(nodes);
                    if (containers.Count > 0) break; // Stop at first successful selector
                }
            }

            return containers;
        }

        private ImageInfo ExtractImageInfo(HtmlNode container)
        {
            var imageInfo = new ImageInfo();

            // Try multiple title extraction methods
            imageInfo.Title = ExtractTitle(container) ?? "Untitled";

            // Extract image URLs
            var imgNode = container.SelectSingleNode(".//img");
            if (imgNode != null)
            {
                // Thumbnail URL
                var thumbnailSrc = imgNode.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(thumbnailSrc))
                {
                    imageInfo.ThumbnailUrl = NormalizeUrl(thumbnailSrc);
                }

                // Original file URL - try multiple approaches
                imageInfo.OriginalFileUrl = ExtractOriginalUrl(container) ??
                                          ConstructOriginalFromThumbnail(thumbnailSrc);
            }

            return string.IsNullOrEmpty(imageInfo.OriginalFileUrl) ? null : imageInfo;
        }

        private string ExtractTitle(HtmlNode container)
        {
            // Try multiple title locations
            var titleNodes = new[]
            {
            container.SelectSingleNode(".//div[@class='gallerytext']"),
            container.SelectSingleNode(".//div[@class='thumbcaption']"),
            container.SelectSingleNode(".//figcaption"),
            container.SelectSingleNode(".//a[@title]")
        };

            foreach (var node in titleNodes)
            {
                if (node != null)
                {
                    var title = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(title))
                    {
                        return title;
                    }
                }
            }

            return null;
        }

        private string ExtractOriginalUrl(HtmlNode container)
        {
            // Try multiple link locations
            var linkNodes = new[]
            {
            container.SelectSingleNode(".//a[contains(@class,'image')]"),
            container.SelectSingleNode(".//a[contains(@href,'/wiki/File:')]"),
            container.SelectSingleNode(".//a[img]")
        };

            foreach (var node in linkNodes)
            {
                if (node != null)
                {
                    var href = node.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(href))
                    {
                        var normalized = NormalizeUrl(href);
                        if (normalized.Contains("/wiki/File:"))
                        {
                            return normalized;
                        }
                    }
                }
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
