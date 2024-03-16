using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adobe.Substance
{
    public static class Globals
    {
        public static Texture2D CreateColoredTexture(int pWidth, int pHeight, Color pColor)
        {
            Texture2D t;
            Color[] pix = new Color[pWidth * pHeight];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = pColor;
            }

            t = new Texture2D(pWidth, pHeight);
            t.SetPixels(pix);
            t.Apply();

            return t;
        }
    }
}