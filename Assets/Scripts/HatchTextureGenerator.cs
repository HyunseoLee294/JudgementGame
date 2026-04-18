using UnityEngine;

public class HatchTextureGenerator : MonoBehaviour
{
    public static Texture2D CreateHatchTexture(int size = 16)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color line = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // 대각선 빗금 패턴
                if ((x + y) % 6 < 5)
                    tex.SetPixel(x, y, line);
                else
                    tex.SetPixel(x, y, clear);
            }
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }
}