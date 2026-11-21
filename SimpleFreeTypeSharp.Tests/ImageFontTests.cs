namespace SimpleFreeTypeSharp.Tests
{
    public class ImageFontTests
    {
        [Fact]
        public void SetFont_ファイルパスから読み込める()
        {
            using var font = new ImageFont();
            font.SetFont(TestFont.Path);
            font.SetSize(16);

            Assert.True(TestFont.Ink(font.Render("あ").ToImageData()) > 0);
        }

        [Fact]
        public void SetFont_再設定できる()
        {
            using var font = new ImageFont();
            font.SetFont(TestFont.Path);
            font.SetFont(TestFont.Path);
            font.SetFont(File.ReadAllBytes(TestFont.Path));
            font.SetSize(16);

            Assert.True(TestFont.Ink(font.Render("あ").ToImageData()) > 0);
        }

        [Fact]
        public void Render_フォント未設定なら例外()
        {
            using var font = new ImageFont();

            Assert.Throws<InvalidOperationException>(() => font.Render("a"));
        }

        [Fact]
        public void Render_Dispose後は例外()
        {
            var font = TestFont.Create();
            font.Dispose();

            Assert.Throws<ObjectDisposedException>(() => font.Render("a"));
        }

        [Fact]
        public void SetSize_0以下なら例外()
        {
            using var font = new ImageFont();

            Assert.Throws<ArgumentOutOfRangeException>(() => font.SetSize(0));
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData("abc", 3)]
        [InlineData("a\r\nb\nc", 3)]
        [InlineData("a\tb", 3)]
        [InlineData("𠮷野家", 3)]
        public void CountChars_改行を除きサロゲートペアを1文字で数える(string? text, int expected)
        {
            Assert.Equal(expected, ImageFont.CountChars(text));
        }
    }
}
