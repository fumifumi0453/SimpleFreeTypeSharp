
namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// 文字列の画像データ
    /// </summary>
    public class StringImageData
    {
        public static readonly StringImageData Empty = new(string.Empty, 0, 0, 0, null);

        private const int INT_LineDistance = 2;

        private readonly CharImageData[][] _Datas;
        private readonly int _Length;

        public StringImageData(string value, int baseLine, int fontHeight, int fontWidth, List<List<CharImageData>>? datas)
        {
            Value = value;
            BaseLine = baseLine;
            FontHeight = fontHeight;
            FontWidth = fontWidth;

            _Length = 0;

            if (datas is not null)
            {
                _Datas = new CharImageData[datas.Count][];
                for (int i = 0; i < datas.Count; i++)
                {
                    _Datas[i] = datas[i].ToArray();
                    _Length += _Datas[i].Length;
                }
            }
            else
            {
                _Datas = Array.Empty<CharImageData[]>();
            }
        }

        /// <summary>
        /// 文字列
        /// </summary>
        public string Value { get; private set; }
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Value); }
        }
        /// <summary>
        /// 描画される文字数（改行を除く。サロゲートペアは1文字）
        /// </summary>
        public int Length
        {
            get { return _Length; }
        }

        /// <summary>
        /// 標準的な文字の高さ
        /// </summary>
        public int FontHeight { get; private set; }
        /// <summary>
        /// 標準的な文字の幅
        /// </summary>
        public int FontWidth { get; private set; }
        /// <summary>
        /// 行の上端からベースラインまでの高さ
        /// </summary>
        public int BaseLine { get; private set; }
        /// <summary>
        /// 行間のサイズ
        /// </summary>
        public int LineDistance { get; set; } = INT_LineDistance;

        /// <summary>
        /// 1行分の高さ（行間を含む）
        /// </summary>
        private int LineHeight
        {
            get { return FontHeight + LineDistance; }
        }

        /// <summary>
        /// 文字ごとの画像データを連結して１つの画像データを返す
        /// </summary>
        /// <returns></returns>
        public ImageData ToImageData()
        {
            return ToImageData(_Length);
        }
        /// <summary>
        /// 文字ごとの画像データを指定した文字数分連結して１つの画像データを返す
        /// </summary>
        /// <param name="length">指定する文字数（改行を除く。サロゲートペアは1文字）</param>
        /// <returns></returns>
        public ImageData ToImageData(int length)
        {
            return ToImageData(0, 0, length);
        }
        /// <summary>
        /// 文字ごとの画像データを指定した範囲内に指定した文字数分連結して1つの画像データを返す
        /// 幅を超える場合は自動的に折り返し、高さに収まらない行は描画しません
        /// </summary>
        /// <param name="width">画像の幅。0以下の場合は折り返さず、文字列全体が収まる幅になります</param>
        /// <param name="height">画像の高さ。0以下の場合は（折り返しを含めた）全行が収まる高さになります</param>
        /// <param name="length">指定する文字数（改行を除く。サロゲートペアは1文字）</param>
        /// <returns></returns>
        public ImageData ToImageData(int width, int height, int length)
        {
            var layout = Layout(width);

            if (width <= 0) width = Math.Max(1, layout.Width);
            if (height <= 0) height = Math.Max(1, layout.LineCount * LineHeight);
            var image = new ImageData(width, height);

            int count = Math.Min(length, layout.Glyphs.Count);
            for (int k = 0; k < count; k++)
            {
                var g = layout.Glyphs[k];

                // 行が高さに収まらない場合はそれ以降を描画しない
                var lineTop = g.Line * LineHeight;
                if (height < lineTop + FontHeight) break;

                Blit(image, g.Data, g.X, lineTop + BaseLine - g.Data.BearingY);
            }

            return image;
        }

        /// <summary>
        /// 文字の配置位置
        /// </summary>
        private readonly record struct GlyphPlacement(CharImageData Data, int X, int Line);
        /// <summary>
        /// 文字列全体の配置結果
        /// </summary>
        private sealed record LayoutResult(List<GlyphPlacement> Glyphs, int Width, int LineCount);

        /// <summary>
        /// 各文字の配置位置を計算します
        /// </summary>
        /// <param name="wrapWidth">折り返す幅。0以下の場合は折り返さない</param>
        /// <returns></returns>
        private LayoutResult Layout(int wrapWidth)
        {
            var glyphs = new List<GlyphPlacement>(_Length);
            int maxRight = 0;
            int line = 0;

            foreach (var src in _Datas)
            {
                int pen = 0;
                bool lineStart = true;

                foreach (var data in src)
                {
                    // 行の先頭の文字の描画開始点がマイナス位置の場合、その分オフセットする
                    if (lineStart) pen = Math.Max(0, -data.BearingX);
                    int x = pen + data.BearingX;

                    // 幅を超える場合は折り返す（行の先頭では折り返さない）
                    if (wrapWidth > 0 && lineStart == false && wrapWidth < x + data.Width)
                    {
                        line++;
                        pen = Math.Max(0, -data.BearingX);
                        x = pen + data.BearingX;
                    }

                    glyphs.Add(new GlyphPlacement(data, x, line));

                    pen += data.AdvanceX;
                    maxRight = Math.Max(maxRight, Math.Max(x + data.Width, pen));
                    lineStart = false;
                }

                line++;
            }

            return new LayoutResult(glyphs, maxRight, line);
        }

        /// <summary>
        /// 文字の画像を範囲外をクリップしながら描画します
        /// </summary>
        /// <param name="image"></param>
        /// <param name="data"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void Blit(ImageData image, CharImageData data, int x, int y)
        {
            int srcX = Math.Max(0, -x);
            int destX = x + srcX;
            int copyWidth = Math.Min(data.Width - srcX, image.Width - destX);
            if (copyWidth <= 0) return;

            for (int i = 0; i < data.Height; i++)
            {
                int destY = y + i;
                if (destY < 0) continue;
                if (destY >= image.Height) break;

                image.SetData(data.Data.Datas, data.Data.GetIndex(srcX, i), image.GetIndex(destX, destY), copyWidth);
            }
        }
    }
}
