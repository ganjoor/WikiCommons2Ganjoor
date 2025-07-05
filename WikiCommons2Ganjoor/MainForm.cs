using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Text;

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
    }
}
