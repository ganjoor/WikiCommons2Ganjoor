using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;

namespace WikiCommons2Ganjoor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        List<ImageInfo> imageInfos = new List<ImageInfo>();

        private async void readWikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            labelStatus.Text = "Reading ...";
            Application.DoEvents();
            WikimediaCommonsParser parser = new WikimediaCommonsParser();
            imageInfos = await parser.ParsePageAsync("https://commons.wikimedia.org/wiki/%D8%B4%D8%A7%D9%87%D9%86%D8%A7%D9%85%D9%87_%D8%AA%D9%87%D9%85%D8%A7%D8%B3%D8%A8%DB%8C");
            dataGridView1.DataSource = imageInfos;
            labelStatus.Text = "Saving";

            ImageInfoRepository infoRepository = new ImageInfoRepository(@"C:\g\commons.json");
            await infoRepository.WriteAllAsync(imageInfos);
            labelStatus.Text = "Ready";
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Process p = new Process();

            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = imageInfos[e.RowIndex].OriginalFileUrl;
            p.Start();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(@"C:\g\commons.json"))
            {
                ImageInfoRepository infoRepository = new ImageInfoRepository(@"C:\g\commons.json");
                imageInfos = await infoRepository.ReadAllAsync();
                dataGridView1.DataSource = imageInfos;
            }
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelStatus.Text = "Saving ...";
            ImageInfoRepository infoRepository = new ImageInfoRepository(@"C:\g\commons.json");
            await infoRepository.WriteAllAsync(imageInfos);
            labelStatus.Text = "Ready";
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            Application.DoEvents();

            DialogResult = DialogResult.None;
            LoginViewModel model = new LoginViewModel()
            {
                Username = txtEmail.Text,
                Password = txtPassword.Text,
                ClientAppName = "Desktop Ganjoor",
                Language = "fa-IR"
            };

            using (HttpClient httpClient = new HttpClient())
            {
                var stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var loginUrl = "https://api.ganjoor.net/api/users/login";
                var response = await httpClient.PostAsync(loginUrl, stringContent);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Cursor = Cursors.Default;
                    Enabled = true;
                    MessageBox.Show(response.ToString());
                    return;
                }
                response.EnsureSuccessStatusCode();

                var result = JObject.Parse(await response.Content.ReadAsStringAsync());
                Properties.Settings.Default.Email = txtEmail.Text;
                Properties.Settings.Default.Password = txtPassword.Text;
                Properties.Settings.Default.MuseumToken = result["token"].ToString();
                Properties.Settings.Default.SessionId = Guid.Parse(result["sessionId"].ToString());
                Properties.Settings.Default.UserId = Guid.Parse(result["user"]["id"].ToString());
                Properties.Settings.Default.Save();
            }

            Enabled = true;
            Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
        }

        public async Task<string> ReplaceImageAsync(Guid imageId, Stream fileStream, string fileName)
        {
            using (HttpClient client = new HttpClient())
            {
                // Set the base address if not already set
                client.BaseAddress = new Uri("https://api.ganjoor.net/");
                client.Timeout = TimeSpan.FromSeconds(3600);

                // Add authorization header
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Properties.Settings.Default.MuseumToken);

                // Create multipart form data content
                using var formContent = new MultipartFormDataContent();

                // Add the file stream as content
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                formContent.Add(fileContent, "file", fileName);

                // Make the PUT request
                var response = await client.PutAsync($"api/images/replace/{imageId}", formContent);

                // Ensure success status code
                response.EnsureSuccessStatusCode();

                // Read and return the response content
                return await response.Content.ReadAsStringAsync();
            }
        }

        class ArtifactImage
        {
            public int Order { get; set; }
            public Guid FirstImageId { get; set; }
            public string ExternalOriginalSizeImageUrl { get; set; }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            
            if (imageInfos.Any(i => i.PageNumber != 0 && i.Uploaded == false))
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Ganjoor Wikimedia Commons downloader (https://ganjoor.net; ganjoor@ganjoor.net)");
                    labelStatus.Text = "Reaing artifact info ...";
                    Application.DoEvents();
                    HttpResponseMessage responseArtifactInfo = await client.GetAsync("https://api.ganjoor.net/api/artifacts/shahname-tahmasb");
                    responseArtifactInfo.EnsureSuccessStatusCode();


                    JObject jsonResponse = JObject.Parse(await responseArtifactInfo.Content.ReadAsStringAsync());

                    // Extract the items array
                    JArray items = (JArray)jsonResponse["items"];

                    // Create a list to store the extracted data
                    var artifactImageIds = new List<ArtifactImage>();

                    foreach (var item in items)
                    {
                        // Get the order value
                        int order = item["order"].Value<int>();

                        // Get the first image id (check if images array exists and has at least one item)
                        Guid? imageId = null;
                        string externalNormalSizeImageUrl = "";
                        if (item["images"] is JArray images && images.Count > 0)
                        {
                            imageId = Guid.Parse(images[0]["id"].ToString());
                            externalNormalSizeImageUrl = images[0]["externalNormalSizeImageUrl"].ToString();
                        }

                        // Add to our extracted data
                        artifactImageIds.Add(new ArtifactImage()
                        {
                            Order = order,
                            FirstImageId = (Guid)imageId,
                            ExternalOriginalSizeImageUrl = externalNormalSizeImageUrl.Replace("norm", "orig")
                        });
                    }

                    foreach (var item in imageInfos.Where(i => i.PageNumber != 0 && i.Uploaded == false))
                    {
                        labelStatus.Text = $"Updating page number {item.PageNumber}";
                        Application.DoEvents();


                        var imageResult = await client.GetAsync(item.OriginalFileUrl);
                        imageResult.EnsureSuccessStatusCode();
                        string commonMD5;
                        using (Stream imageStream = await imageResult.Content.ReadAsStreamAsync())
                        {
                            imageStream.Seek(0, SeekOrigin.Begin);
                            using (var md5 = MD5.Create())
                            {
                                commonMD5 = string.Join("", md5.ComputeHash(imageStream).Select(x => x.ToString("X2")));
                            }
                            var artifactItem = artifactImageIds.Where(i => i.Order == item.PageNumber).Single();
                            imageStream.Seek(0, SeekOrigin.Begin);
                            await ReplaceImageAsync(artifactItem.FirstImageId, imageStream, item.PageNumber.ToString() + ".jpg");


                            var uploadedImageResult = await client.GetAsync(artifactItem.ExternalOriginalSizeImageUrl);
                            uploadedImageResult.EnsureSuccessStatusCode();
                            using (Stream uploadedImageStream = await uploadedImageResult.Content.ReadAsStreamAsync())
                            {
                                using (var md5 = MD5.Create())
                                {
                                    string uploadedMD5 = string.Join("", md5.ComputeHash(uploadedImageStream).Select(x => x.ToString("X2")));
                                    if (commonMD5 == uploadedMD5)
                                    {
                                        item.Uploaded = true;
                                        labelStatus.Text = "Saving ...";
                                        ImageInfoRepository infoRepository = new ImageInfoRepository(@"C:\g\commons.json");
                                        await infoRepository.WriteAllAsync(imageInfos);
                                        labelStatus.Text = "Ready";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            labelStatus.Text = "Ready";

        }
    }
}
