using System.Text.Json;
using System.Text.Json.Serialization;

namespace WikiCommons2Ganjoor
{
    

    public class ImageInfoRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ImageInfoRepository(string filePath)
        {
            _filePath = filePath;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
        }

        // Write the entire collection to file
        public async Task WriteAllAsync(IEnumerable<ImageInfo> images)
        {
            try
            {
                var json = JsonSerializer.Serialize(images, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
                throw;
            }
        }

        // Read the entire collection from file
        public async Task<List<ImageInfo>> ReadAllAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<ImageInfo>();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<ImageInfo>>(json, _jsonOptions) ?? new List<ImageInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from file: {ex.Message}");
                return new List<ImageInfo>();
            }
        }

        // Update specific image by title and save the entire collection
        public async Task UpdateImageAsync(string title, Action<ImageInfo> updateAction)
        {
            var images = await ReadAllAsync();
            var image = images.Find(img => img.Title == title);

            if (image != null)
            {
                updateAction(image);
                await WriteAllAsync(images);
            }
        }

        // Add new image and save the entire collection
        public async Task AddImageAsync(ImageInfo newImage)
        {
            var images = await ReadAllAsync();
            images.Add(newImage);
            await WriteAllAsync(images);
        }

        // Remove image by title and save the entire collection
        public async Task RemoveImageAsync(string title)
        {
            var images = await ReadAllAsync();
            images.RemoveAll(img => img.Title == title);
            await WriteAllAsync(images);
        }
    }
    
}
