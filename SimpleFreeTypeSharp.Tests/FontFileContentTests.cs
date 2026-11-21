namespace SimpleFreeTypeSharp.Tests
{
    public class FontFileContentTests
    {
        // 他のテストとキャッシュを共有しないよう、テストごとに異なる内容のデータを使う

        [Fact]
        public void Acquire_同じ内容なら別の配列でも共有する()
        {
            var c1 = FontFileContent.Acquire(new byte[] { 1, 2, 3, 4 });
            var c2 = FontFileContent.Acquire(new byte[] { 1, 2, 3, 4 });
            try
            {
                Assert.Same(c1, c2);
            }
            finally
            {
                FontFileContent.Release(c1);
                FontFileContent.Release(c2);
            }
        }

        [Fact]
        public void Release_すべて解放するとメモリを解放する()
        {
            var c1 = FontFileContent.Acquire(new byte[] { 5, 6, 7, 8 });
            var c2 = FontFileContent.Acquire(new byte[] { 5, 6, 7, 8 });

            FontFileContent.Release(c1);
            Assert.NotEqual(IntPtr.Zero, c2.FontMemory);

            FontFileContent.Release(c2);
            Assert.Equal(IntPtr.Zero, c1.FontMemory);

            var c3 = FontFileContent.Acquire(new byte[] { 5, 6, 7, 8 });
            try
            {
                Assert.NotSame(c1, c3);
            }
            finally
            {
                FontFileContent.Release(c3);
            }
        }

        [Fact]
        public void Acquire_元の配列を書き換えても影響しない()
        {
            var data = new byte[] { 9, 10, 11, 12 };
            var content = FontFileContent.Acquire(data);
            try
            {
                data[0] = 0;

                Assert.Equal(9, content.Content[0]);
            }
            finally
            {
                FontFileContent.Release(content);
            }
        }

        [Fact]
        public void Acquire_空データなら例外()
        {
            Assert.Throws<ArgumentException>(() => FontFileContent.Acquire(Array.Empty<byte>()));
        }
    }
}
