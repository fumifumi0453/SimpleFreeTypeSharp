using SimpleFreeTypeSharp;

// FreeTypeのライブラリ(FT_Library)は全ImageFontで共有しておりスレッドセーフではないため、テストを並列実行しない
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SimpleFreeTypeSharp.Tests
{
    /// <summary>
    /// テストで使うフォントの共通処理
    /// </summary>
    internal static class TestFont
    {
        public static readonly string Path = System.IO.Path.Combine(AppContext.BaseDirectory, "JF-Dot-MPlusH12.ttf");

        /// <summary>
        /// テスト用フォントを読み込んだImageFontを作成します
        /// </summary>
        public static ImageFont Create(float size = 24)
        {
            var font = new ImageFont();
            font.SetFont(File.ReadAllBytes(Path));
            font.SetSize(size);
            return font;
        }

        /// <summary>
        /// 描画されたピクセル数（アルファ値が0以外）を返します
        /// </summary>
        public static int Ink(ImageData data)
        {
            return data.Datas.Count(b => b != 0);
        }
    }
}
