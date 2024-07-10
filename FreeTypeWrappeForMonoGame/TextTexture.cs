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

        public string FontKey { get; private set; }
        public Color Color { get; set; }
        public string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                _ViewLength = _Text.Length;
            }
        }
        public int ViewLength
        {
            get { return _ViewLength; }
            set { _ViewLength = MathHelper.Clamp(value, 0, _Text.Length); }
        }
        public int FontHeight
        {
            get { return _ImageData.FontHeight; }
        }

        public Texture2D GetTexture()
        {
            if (_Text is null || _Text.Length == 0) return TT2_Empty;

            bool tmp;

            if (tmp = string.Equals(_ImageData.Value, _Text) == false)
            {
                _ImageData = _Font.Render(_Text);
            }

            if (tmp || Color.Equals(_PrevColor, Color) == false || _PrevViewLength != _ViewLength)
            {
                _Texture?.Dispose();
                _Texture = CreateTexture2D(_Device, _ImageData.ToImageData(_ViewLength), Color);
                _PrevColor = Color;
                _PrevViewLength = _ViewLength;
            }

            return _Texture;
        }
        public static Texture2D CreateTexture2D(GraphicsDevice device, ImageData image, Color color)
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
