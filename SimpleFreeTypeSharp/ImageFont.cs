using FreeTypeSharp;
using System.Runtime.InteropServices;

namespace SimpleFreeTypeSharp
{
    /// <summary>
    /// FreeTypeFontから文字のイメージデータを作成します
    /// </summary>
    public unsafe class ImageFont 
    {
        private static readonly FreeTypeLibrary FTL_Library;
        private static readonly Dictionary<byte[], FontFileContent> SL_FontDatas;
        private const int INT_FontSize = 12;

        private FreeTypeFaceFacade _FaceFacade;
        private float _FontSize;

        static ImageFont()
        {
            FTL_Library = new();
            SL_FontDatas = new();
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
        public string FontName { get { return _FaceFacade.MarshalFamilyName(); } }
        /// <summary>
        /// FaceFacadeを返します
        /// </summary>
        public FreeTypeFaceFacade FaceFacade { get { return _FaceFacade; } }
        /// <summary>
        /// フォントサイズを返します
        /// </summary>
        public float Size { get { return _FontSize; } }

        /// <summary>
        /// フォントを設定します。
        /// </summary>
        /// <param name="fontpath">フォントファイルのパス</param>
        /// <exception cref="FreeTypeException"></exception>
        public void SetFont(string fontpath)
        {
            FT_FaceRec_* face;

            // フォントファイルの読み込み
            var error = FT.FT_New_Face(FTL_Library.Native, (byte*)Marshal.StringToHGlobalAnsi(fontpath), IntPtr.Zero, &face);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            SetFont(face);
        }
        /// <summary>
        /// フォントを設定します
        /// </summary>
        /// <param name="data">フォントファイルのバイナリデータ</param>
        /// <exception cref="FreeTypeException"></exception>
        public void SetFont(byte[] data)
        {
            FT_FaceRec_* face;

            // フォントバイナリデータのアドレスを固定する（すでに固定済みの場合は流用する）
            if (SL_FontDatas.ContainsKey(data) == false) SL_FontDatas.Add(data, new FontFileContent(data));  

            // フォントバイナリデータの読み込み
            var error = FT.FT_New_Memory_Face(FTL_Library.Native, (byte*)SL_FontDatas[data].FontMemory, (IntPtr)data.Length, IntPtr.Zero, &face);

            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            SetFont(face);
        }
        /// <summary>
        /// フォントを設定します
        /// </summary>
        /// <param name="face"></param>
        public void SetFont(FT_FaceRec_* face)
        {
            _FaceFacade = new FreeTypeFaceFacade(FTL_Library, face);
            SetSize(_FontSize);
        }
        /// <summary>
        /// フォントサイズを設定します
        /// </summary>
        /// <param name="size">フォントサイズ</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FreeTypeException"></exception>
        public void SetSize(float size)
        {
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
        public StringImageData Render(string text)
        {
            if (string.IsNullOrEmpty(text)) return StringImageData.Empty;

            // 改行に対応するため
            var charList = text.ReplaceLineEndings("\n").ToCharArray();
            var cidList = new List<List<CharImageData>>();
            cidList.Add(new List<CharImageData>());

            // 文字列をきれいに並べるための値
            int baseHeight = (int)_FaceFacade.FaceRec->size->metrics.height >> 6;
            int fontHeight = baseHeight;
            int fontWidth = (int)_FaceFacade.FaceRec->size->metrics.max_advance >> 6;
            foreach (var c in charList)
            {
                switch (c)
                {
                    case '\r':
                    case '\n':
                        // 改行
                        cidList.Add(new List<CharImageData>());
                        break;

                    case '\t':
                        // タブ
                        var tab = new CharImageData(fontWidth * 2, 0, 0, 0, 0);
                        cidList.Last().Add(tab);
                        break;

                    default:
                        // 通常の文字
                        var cid = CreateCharImageData(c);
                        // 文字の高さを揃える
                        var tmpHeight = baseHeight - cid.BearingY + cid.Height;
                        if (fontHeight < tmpHeight) fontHeight = tmpHeight;
                        cidList.Last().Add(cid);
                        break;
                }
            }

            return new StringImageData(text, baseHeight, fontHeight, fontWidth, cidList);
        }
        /// <summary>
        /// １文字分の画像データを作成します
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <exception cref="FreeTypeException"></exception>
        private CharImageData CreateCharImageData(char c)
        {
            FT_Error error;

            var griphIndex = _FaceFacade.GetCharIndex(c);
            // グリフ情報を取得
            error = FT.FT_Load_Glyph(_FaceFacade.FaceRec, griphIndex, FT_LOAD.FT_LOAD_DEFAULT);
            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            // 文字をきれいに並べるための情報取得
            var advanceX = (int)_FaceFacade.GlyphSlot->metrics.horiAdvance >> 6;
            var bealingX = (int)_FaceFacade.GlyphSlot->metrics.horiBearingX >> 6;
            var bearingY = (int)_FaceFacade.GlyphSlot->metrics.horiBearingY >> 6;

            // グリフを描画
            error = FT.FT_Render_Glyph(_FaceFacade.GlyphSlot, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
            if (error != FT_Error.FT_Err_Ok) throw new FreeTypeException(error);

            // 描画した文字を画像データに変換
            var bitmap = _FaceFacade.GlyphSlot->bitmap;
            var cid = new CharImageData(advanceX, bealingX, bearingY, (int)bitmap.width, (int)bitmap.rows);
            for (int i = 0; i < bitmap.rows; i++)
            {
                var tmp = i * bitmap.pitch;
                for (int j = 0; j < bitmap.width; j++)
                {
                    cid.SetData(j, i, bitmap.buffer[j + tmp]);
                }
            }

            return cid;
        }
    }
}
