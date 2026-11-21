using FreeTypeSharp;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// FreeTypeFontから文字のイメージデータを作成します
    /// 使い終わったら Dispose してください
    /// </summary>
    public unsafe class ImageFont : IDisposable
    {
        private static readonly FreeTypeLibrary FTL_Library;
        private const int INT_FontSize = 12;

        private FreeTypeFaceFacade? _FaceFacade = null;
        private FontFileContent? _FontContent = null;
        private float _FontSize;
        private bool disposedValue;

        static ImageFont()
        {
            FTL_Library = new();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public ImageFont()
        {
            _FontSize = INT_FontSize;
        }

        /// <summary>
        /// フォントのファミリーネームを取得します
        /// </summary>
        public string FontName { get { return GetFace().MarshalFamilyName(); } }
        /// <summary>
        /// FaceFacadeを返します
        /// </summary>
        public FreeTypeFaceFacade? FaceFacade { get { return _FaceFacade; } }
        /// <summary>
        /// フォントサイズを返します
        /// </summary>
        public float Size { get { return _FontSize; } }

        /// <summary>
        /// 文字列のうち、画像データとして描画される文字数を返します
        /// 改行は含まず、サロゲートペアは1文字として数えます
        /// <see cref="StringImageData.ToImageData(int)"/> の length と同じ単位です
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int CountChars(string? text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            int count = 0;
            foreach (var r in text.EnumerateRunes())
            {
                if (r.Value != '\r' && r.Value != '\n') count++;
            }
            return count;
        }

        /// <summary>
        /// フォントを設定します。
        /// </summary>
        /// <param name="fontpath">フォントファイルのパス</param>
        /// <exception cref="FreeTypeException"></exception>
        public void SetFont(string fontpath)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(fontpath);

            FT_FaceRec_* face;
            FT_Error error;

            // FreeTypeは内部でfopenを使うため、Windowsではシステムのコードページ、それ以外ではUTF-8で渡す
            var pathPtr = OperatingSystem.IsWindows()
                ? Marshal.StringToHGlobalAnsi(fontpath)
                : Marshal.StringToCoTaskMemUTF8(fontpath);
            try
            {
                // フォントファイルの読み込み
                error = FT.FT_New_Face(FTL_Library.Native, (byte*)pathPtr, IntPtr.Zero, &face);
            }
            finally
            {
                if (OperatingSystem.IsWindows()) Marshal.FreeHGlobal(pathPtr);
                else Marshal.FreeCoTaskMem(pathPtr);
            }

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            SetFaceInternal(face, null);
        }
        /// <summary>
        /// フォントを設定します
        /// </summary>
        /// <param name="data">フォントファイルのバイナリデータ</param>
        /// <exception cref="FreeTypeException"></exception>
        public void SetFont(byte[] data)
        {
            ThrowIfDisposed();

            // フォントバイナリデータのアドレスを固定する（同じ内容のデータが固定済みの場合は流用する）
            var content = FontFileContent.Acquire(data);

            FT_FaceRec_* face;
            FT_Error error;
            try
            {
                // フォントバイナリデータの読み込み
                error = FT.FT_New_Memory_Face(FTL_Library.Native, (byte*)content.FontMemory, (IntPtr)content.Length, IntPtr.Zero, &face);
            }
            catch
            {
                FontFileContent.Release(content);
                throw;
            }

            if (error != FT_Error.FT_Err_Ok)
            {
                FontFileContent.Release(content);
                throw new FreeTypeException(error);
            }

            SetFaceInternal(face, content);
        }
        /// <summary>
        /// フォントを設定します
        /// 渡したfaceの所有権はこのインスタンスに移り、Dispose時またはフォント再設定時に FT_Done_Face されます
        /// </summary>
        /// <param name="face"></param>
        public void SetFont(FT_FaceRec_* face)
        {
            ThrowIfDisposed();
            SetFaceInternal(face, null);
        }
        /// <summary>
        /// フォントサイズを設定します
        /// </summary>
        /// <param name="size">フォントサイズ</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FreeTypeException"></exception>
        public void SetSize(float size)
        {
            ThrowIfDisposed();
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            _FontSize = size;
            if (_FaceFacade != null)
            {
                // フォントのサイズ情報を登録
                var error = FT.FT_Set_Char_Size(_FaceFacade.FaceRec, IntPtr.Zero, new IntPtr((int)(_FontSize * 64)), 0, 96);

                if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);
            }
        }
        /// <summary>
        /// 指定された文字列を画像データに変換します
        /// 改行した場合、画像データ上でも改行します
        /// </summary>
        /// <param name="text">変換元の文字列</param>
        /// <returns>変換した画像データ</returns>
        public StringImageData Render(string? text)
        {
            if (string.IsNullOrEmpty(text)) return StringImageData.Empty;

            var faceFacade = GetFace();

            var cidList = new List<List<CharImageData>>();
            cidList.Add(new List<CharImageData>());

            // 文字列をきれいに並べるための値
            int baseHeight = (int)faceFacade.FaceRec->size->metrics.height >> 6;
            int fontHeight = baseHeight;
            int fontWidth = (int)faceFacade.FaceRec->size->metrics.max_advance >> 6;

            // 改行コードを統一し、サロゲートペアを1文字として扱うためRune単位で処理する
            foreach (var r in text.ReplaceLineEndings("\n").EnumerateRunes())
            {
                switch (r.Value)
                {
                    case '\n':
                        // 改行
                        cidList.Add(new List<CharImageData>());
                        break;

                    case '\t':
                        // タブ
                        var tab = new CharImageData(fontWidth * 2, 0, 0, 0, 0);
                        cidList[^1].Add(tab);
                        break;

                    default:
                        // 通常の文字
                        var cid = CreateCharImageData(faceFacade, r);
                        // 文字の高さを揃える
                        var tmpHeight = baseHeight - cid.BearingY + cid.Height;
                        if (fontHeight < tmpHeight) fontHeight = tmpHeight;
                        cidList[^1].Add(cid);
                        break;
                }
            }

            return new StringImageData(text, baseHeight, fontHeight, fontWidth, cidList);
        }
        /// <summary>
        /// １文字分の画像データを作成します
        /// </summary>
        /// <param name="faceFacade"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <exception cref="FreeTypeException"></exception>
        private static CharImageData CreateCharImageData(FreeTypeFaceFacade faceFacade, Rune c)
        {
            FT_Error error;

            var griphIndex = faceFacade.GetCharIndex((uint)c.Value);
            // グリフ情報を取得
            error = FT.FT_Load_Glyph(faceFacade.FaceRec, griphIndex, FT_LOAD.FT_LOAD_DEFAULT);
            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            // 文字をきれいに並べるための情報取得
            var advanceX = (int)faceFacade.GlyphSlot->metrics.horiAdvance >> 6;
            var bealingX = (int)faceFacade.GlyphSlot->metrics.horiBearingX >> 6;
            var bearingY = (int)faceFacade.GlyphSlot->metrics.horiBearingY >> 6;

            // グリフを描画（埋め込みビットマップの場合はすでにビットマップになっているので何もしない）
            error = FT.FT_Render_Glyph(faceFacade.GlyphSlot, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            // 描画した文字を画像データに変換
            var bitmap = faceFacade.GlyphSlot->bitmap;
            var cid = new CharImageData(advanceX, bealingX, bearingY, (int)bitmap.width, (int)bitmap.rows);
            CopyBitmap(ref bitmap, cid.Data);

            return cid;
        }
        /// <summary>
        /// FreeTypeのビットマップを8bitのアルファ値(0～255)として画像データにコピーします
        /// 埋め込みビットマップ(1bit/2bit/4bit)やカラービットマップにも対応します
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="dest"></param>
        private static void CopyBitmap(ref FT_Bitmap_ bitmap, ImageData dest)
        {
            int width = (int)bitmap.width;
            int rows = (int)bitmap.rows;
            if (width == 0 || rows == 0 || bitmap.buffer == null) return;

            int absPitch = Math.Abs(bitmap.pitch);

            for (int y = 0; y < rows; y++)
            {
                // pitchが負の場合、メモリ上の先頭は一番下の行になる
                var row = bitmap.buffer + (bitmap.pitch >= 0 ? y : rows - 1 - y) * absPitch;

                switch (bitmap.pixel_mode)
                {
                    case FT_Pixel_Mode_.FT_PIXEL_MODE_GRAY:
                        {
                            int grays = bitmap.num_grays;
                            if (grays == 256 || grays <= 1)
                            {
                                dest.SetData(new ReadOnlySpan<byte>(row, width), dest.GetIndex(0, y));
                            }
                            else
                            {
                                for (int x = 0; x < width; x++)
                                    dest.SetData(x, y, (byte)Math.Min(255, row[x] * 255 / (grays - 1)));
                            }
                        }
                        break;

                    case FT_Pixel_Mode_.FT_PIXEL_MODE_MONO:
                        // 1ピクセル1bit（MSBが左）
                        for (int x = 0; x < width; x++)
                            dest.SetData(x, y, (row[x >> 3] & (0x80 >> (x & 7))) != 0 ? (byte)255 : (byte)0);
                        break;

                    case FT_Pixel_Mode_.FT_PIXEL_MODE_GRAY2:
                        // 1ピクセル2bit
                        for (int x = 0; x < width; x++)
                        {
                            int v = (row[x >> 2] >> (6 - (x & 3) * 2)) & 0x03;
                            dest.SetData(x, y, (byte)(v * 255 / 3));
                        }
                        break;

                    case FT_Pixel_Mode_.FT_PIXEL_MODE_GRAY4:
                        // 1ピクセル4bit
                        for (int x = 0; x < width; x++)
                        {
                            int v = (row[x >> 1] >> ((x & 1) == 0 ? 4 : 0)) & 0x0F;
                            dest.SetData(x, y, (byte)(v * 255 / 15));
                        }
                        break;

                    case FT_Pixel_Mode_.FT_PIXEL_MODE_BGRA:
                        // カラービットマップはアルファ値のみ使用する
                        for (int x = 0; x < width; x++)
                            dest.SetData(x, y, row[x * 4 + 3]);
                        break;

                    default:
                        // LCD等は FT_RENDER_MODE_NORMAL では生成されないため未対応
                        return;
                }
            }
        }

        /// <summary>
        /// faceを差し替えます。古いfaceとフォントデータは解放します
        /// </summary>
        /// <param name="face"></param>
        /// <param name="content"></param>
        private void SetFaceInternal(FT_FaceRec_* face, FontFileContent? content)
        {
            var newFacade = new FreeTypeFaceFacade(FTL_Library, face);

            var oldFacade = _FaceFacade;
            var oldContent = _FontContent;

            _FaceFacade = newFacade;
            _FontContent = content;

            ReleaseFace(oldFacade, oldContent);

            SetSize(_FontSize);
        }
        private static void ReleaseFace(FreeTypeFaceFacade? facade, FontFileContent? content)
        {
            // faceがフォントデータを参照しているので、faceを先に解放する
            if (facade is not null) FT.FT_Done_Face(facade.FaceRec);
            if (content is not null) FontFileContent.Release(content);
        }
        private FreeTypeFaceFacade GetFace()
        {
            ThrowIfDisposed();
            return _FaceFacade ?? throw new InvalidOperationException("フォントが設定されていません。先に SetFont を呼んでください。");
        }
        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // FreeTypeはスレッドセーフではないため、ファイナライザからは解放しない
                if (disposing)
                {
                    ReleaseFace(_FaceFacade, _FontContent);
                    _FaceFacade = null;
                    _FontContent = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
