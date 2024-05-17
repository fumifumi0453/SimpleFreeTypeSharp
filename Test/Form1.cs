using SimpleFreeTypeSharp;
using System;
using System.Security.Cryptography;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var data = SimpleFreeTypeSharp.Test.TestString(Path.GetFullPath(@"JF-Dot-MPlusH12.ttf"), textBox1.Text);

            SetCharImageData(CharImageData.Merge(data));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ImageFont.SetFont(Path.GetFullPath(@"JF-Dot-MPlusH12.ttf"));
            ImageFont.SetSize(24f);

            var data = ImageFont.Render(textBox1.Text);

            SetCharImageData(data.ToImageData());
        }

        private void SetCharImageData(ImageData data)
        {
            var baseColor = Color.LimeGreen;
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
