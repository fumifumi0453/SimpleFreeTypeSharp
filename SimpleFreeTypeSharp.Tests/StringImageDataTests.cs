namespace SimpleFreeTypeSharp.Tests
{
    public class StringImageDataTests : IDisposable
    {
        private const string STR_TwoLines = "日本語でおｋ\r\nそう日本語ならね";

        private readonly ImageFont _Font = TestFont.Create();

        public void Dispose()
        {
            _Font.Dispose();
        }

        [Fact]
        public void Length_改行を含まない文字数()
        {
            var data = _Font.Render(STR_TwoLines);

            Assert.Equal(14, data.Length);
            Assert.Equal(14, ImageFont.CountChars(STR_TwoLines));
        }

        [Fact]
        public void ToImageData_長さ0なら何も描画しない()
        {
            var data = _Font.Render(STR_TwoLines);

            Assert.Equal(0, TestFont.Ink(data.ToImageData(0)));
            Assert.True(TestFont.Ink(data.ToImageData()) > 0);
        }

        [Fact]
        public void ToImageData_改行をまたいでも文字数がずれない()
        {
            var data = _Font.Render(STR_TwoLines);
            var firstLine = _Font.Render("日本語でおｋ").ToImageData();

            // 6文字 = 1行目すべて、7文字 = 2行目の「そ」まで
            Assert.Equal(TestFont.Ink(firstLine), TestFont.Ink(data.ToImageData(6)));
            Assert.True(TestFont.Ink(data.ToImageData(7)) > TestFont.Ink(data.ToImageData(6)));
        }

        [Fact]
        public void ToImageData_幅指定_高さ自動なら折り返した行も描画する()
        {
            var data = _Font.Render(STR_TwoLines);
            var full = data.ToImageData();

            var wrapped = data.ToImageData(100, 0, data.Length);

            Assert.Equal(100, wrapped.Width);
            Assert.True(wrapped.Height > full.Height);
            Assert.Equal(TestFont.Ink(full), TestFont.Ink(wrapped));
        }

        [Theory]
        [InlineData("jfjfj")]
        [InlineData("fffff")]
        [InlineData("Wj")]
        [InlineData("j")]
        [InlineData("abcdefghijklmnopqrstuvwxyz!?")]
        [InlineData("　全角スペース　")]
        [InlineData("\tタブ\t")]
        public void ToImageData_幅自動なら折り返さない(string text)
        {
            var data = _Font.Render(text);

            var image = data.ToImageData();

            Assert.Equal(data.FontHeight + data.LineDistance, image.Height);
            Assert.Equal(TestFont.Ink(data.ToImageData(10000, 0, data.Length)), TestFont.Ink(image));
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(5, 5)]
        [InlineData(1, 1)]
        public void ToImageData_文字より狭い範囲でも例外にならない(int width, int height)
        {
            var data = _Font.Render(STR_TwoLines);

            var image = data.ToImageData(width, height, data.Length);

            Assert.Equal(width, image.Width);
        }

        [Fact]
        public void ToImageData_高さに収まらない行は描画しない()
        {
            var data = _Font.Render(STR_TwoLines);
            var firstLine = _Font.Render("日本語でおｋ").ToImageData();

            var image = data.ToImageData(0, data.FontHeight + data.LineDistance, data.Length);

            Assert.Equal(TestFont.Ink(firstLine), TestFont.Ink(image));
        }

        [Fact]
        public void Render_サロゲートペアは1文字として扱う()
        {
            var data = _Font.Render("𠮷a");

            Assert.Equal(2, data.Length);
            Assert.Equal(2, ImageFont.CountChars("𠮷a"));
        }

        [Fact]
        public void Render_空文字列ならEmpty()
        {
            Assert.Same(StringImageData.Empty, _Font.Render(string.Empty));
            Assert.Same(StringImageData.Empty, _Font.Render(null));
        }
    }
}
