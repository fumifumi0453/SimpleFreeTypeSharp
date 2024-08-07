﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleFreeTypeSharp;
using System.Collections.Generic;

namespace FreeTypeWrapper
{
    public static class TextTextureFactory
    {
        public static GraphicsDevice GraphicsDevice { get; set; }
        
        private static Dictionary<string, ImageFont> _Service = new();

        public static bool LoadFont(string fontKey, string fontPath, float size)
        {
            if (_Service.ContainsKey(fontKey)) return false;

            var font = new ImageFont();
            try
            {
                font.SetFont(fontPath);
                font.SetSize(size);
            }
            catch
            {
                return false;
            }

            _Service.Add(fontKey, font);
            return true;
        }
        public static bool LoadFont(string fontKey, byte[] bytes, float size)
        {
            if (_Service.ContainsKey(fontKey)) return false;

            var font = new ImageFont();
            try
            {
                font.SetFont(bytes);
                font.SetSize(size);
            }
            catch
            {
                return false;
            }

            _Service.Add(fontKey, font);
            return true;
        }

        public static TextTexture Create(string fontKey, string text, Color color)
        {
            return new TextTexture(GraphicsDevice, fontKey, _Service[fontKey], text, color);
        }
    }
}
