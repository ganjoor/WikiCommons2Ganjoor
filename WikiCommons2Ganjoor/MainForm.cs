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
            labelStatus.Text = "Reading ...";
            Application.DoEvents();
            WikimediaCommonsParser parser = new WikimediaCommonsParser();
            imageInfos = await parser.ParsePageAsync("https://commons.wikimedia.org/wiki/%D8%B4%D8%A7%D9%87%D9%86%D8%A7%D9%85%D9%87_%D8%AA%D9%87%D9%85%D8%A7%D8%B3%D8%A8%DB%8C");
            dataGridView1.DataSource = imageInfos;
            labelStatus.Text = "Ready";
        }
    }
}
