using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleFreeTypeSharp;
using System;
using System.Collections.Generic;

namespace FreeTypeWrapper
{
    public class TextTexture : IDisposable
    {
        private const string STR_SampleText = "abcdefghijklmnopqrstuvwxyz!?";
        private static Texture2D TT2_Empty;

        private GraphicsDevice _Device;
        private ImageFont _Font;
        private Color _PrevColor = Color.Transparent;
        private string _Text;
        private int _ViewLength;
        private int _PrevViewLength;

        private StringImageData _ImageData = StringImageData.Empty;
        private Texture2D _Texture;
        private bool disposedValue;

        protected internal TextTexture(GraphicsDevice device, string fontKey, ImageFont font, string text, Color color)
        {
            _Device = device;
            FontKey = fontKey;
            _Font = font;
            Text = text;
            Color = color;

            _ImageData = _Font.Render(STR_SampleText);

            if (TT2_Empty is null)
            {
                TT2_Empty = new Texture2D(_Device, 1, 1);
                TT2_Empty.SetData(new Color[] { Color.Transparent });
            }
        }

        /// <summary>
        /// 使用しているフォントデータのフォントキーを返します
        /// </summary>
        public string FontKey { get; private set; }
        /// <summary>
        /// 描画するColorを指定します
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// 描画する文字列を指定します
        /// </summary>
        public string Text
        {
            get { return _Text; }
            set
            {
                if (string.IsNullOrEmpty(value)) value = string.Empty;
                _Text = value;
                _ViewLength = _Text.Length;
            }
        }
        /// <summary>
        /// 描画する文字列の範囲を指定します
        /// 文字列を変更すると、自動的に最大範囲になります
        /// </summary>
        public int ViewLength
        {
            get { return _ViewLength; }
            set { _ViewLength = MathHelper.Clamp(value, 0, _Text.Length); }
        }
        /// <summary>
        /// フォントの通常高さを返します
        /// </summary>
        public int FontHeight
        {
            get { return _ImageData.FontHeight; }
        }

        /// <summary>
        /// Texture2Dを作成します
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture()
        {
            if (string.IsNullOrEmpty(_Text)) return TT2_Empty;

            bool tmp;

            if (tmp = string.Equals(_ImageData.Value, _Text) == false)
            {
                _ImageData = _Font.Render(_Text);
            }

            if (tmp || _Texture is null || Color.Equals(_PrevColor, Color) == false || _PrevViewLength != _ViewLength)
            {
                _Texture?.Dispose();
                _Texture = CreateTexture2D(_Device, _ImageData.ToImageData(_ViewLength), Color);
                _PrevColor = Color;
                _PrevViewLength = _ViewLength;
            }

            return _Texture;
        }
        /// <summary>
        /// 与えられた情報からTexture2Dを作成します
        /// </summary>
        /// <param name="device"></param>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        protected static Texture2D CreateTexture2D(GraphicsDevice device, ImageData image, Color color)
        {
            var texture = new Texture2D(device, image.Width, image.Height);
            var array = new Color[image.Width * image.Height];

            var clrTable = new Dictionary<int, Color>(256);
            for (int i = 0; i < array.Length; i++)
            {
                var alpha = image.Datas[i];
                if (alpha == 0) continue;

                if (clrTable.TryGetValue(alpha, out Color clr) == false)
                {
                    //clr = new Color(color, color.A * alpha / 255);
                    clr = Color.Multiply(color, alpha / 255f);
                    clrTable[alpha] = clr;
                }

                array[i] = clr;
            }

            texture.SetData(array);
            return texture;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    _Texture?.Dispose();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                _Font = null;

                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~TextTexture()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
