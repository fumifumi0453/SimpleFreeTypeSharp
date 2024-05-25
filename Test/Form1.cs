using SimpleFreeTypeSharp;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using var fnt = new ImageFont();

            //fnt.SetFont(Path.GetFullPath(@"JF-Dot-MPlusH12.ttf"));
            fnt.SetFont(File.ReadAllBytes(Path.Combine(@"JF-Dot-MPlusH12.ttf")));
            fnt.SetSize(24f);

            var data = fnt.Render(textBox1.Text);

            if (data.IsEmpty == false)
            {
                SetCharImageData(data.ToImageData());
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private void SetCharImageData(ImageData data)
        {
            var baseColor = Color.DarkGreen;
            var image = new Bitmap(data.Width, data.Height);

            for (int i = 0; i < data.Width; i++)
            {
                for (int j = 0; j < data.Height; j++)
                {
                    var tmp = data.GetData(i, j);
                    if (tmp == 0) continue;
                    image.SetPixel(i, j, Color.FromArgb(tmp, baseColor));
                }
            }

            pictureBox1.Image = image;

        }

    }
}
