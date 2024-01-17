using System.Numerics;
using SixLabors.ImageSharp.Formats.Gif;
using SolidCode.Atlas;
using Color = SixLabors.ImageSharp.Color;

namespace Three_Core;

public static class ImageGen
{
    public static Image<Rgba64> GetImage(ProbabilityMap map, int brightness = 1)
    {
        Image<Rgba64> image = new Image<Rgba64>(map.Size, map.Size);
        
            for (int x = 0; x < map.Size; x++)
            {
                for (int y = 0; y < map.Size; y++)
                {
                    float a =  Adjust(map.MapA[x, y] / (float) map.MaxA, brightness);
                    float b = Adjust(map.MapB[x, y] / (float) map.MaxB, brightness);
                    float c = Adjust(map.MapC[x, y] / (float) map.MaxC, brightness);
                    image[x, y] = new Rgba64(new Vector4(a, b, c, 1f));
                }
            }

            return image;
        
        
    }

    public static (Image<Rgba64>, Image<Rgba64>) GetVelocityImages(VelocityMap map)
    {
        const float multiplier = 1000f;
        Image<Rgba64> abImage = new Image<Rgba64>(map.Size, map.Size);
        
            for (int x = 0; x < map.Size; x++)
            {
                for (int y = 0; y < map.Size; y++)
                {
                    Vector2 aBody = map.MapA[x, y];
                    if (aBody.Length() != 0)
                    {
                        Debug.Log(aBody.ToString());
                    }
                    Vector2 bBody = map.MapB[x, y];
                    float r = 0.5f + aBody.X * multiplier;
                    float g = 0.5f + aBody.Y * multiplier;
                    float b = 0.5f + bBody.X * multiplier;
                    float a = 0.5f + bBody.Y * multiplier;
                    abImage[x, y] = new Rgba64(new Vector4(r, g, b, a));
                }
            }
            
            Image<Rgba64> cImage = new Image<Rgba64>(map.Size, map.Size);
        
            for (int x = 0; x < map.Size; x++)
            {
                for (int y = 0; y < map.Size; y++)
                {
                    Vector2 cBody = map.MapC[x, y];
                    float r = 0.5f + cBody.X * multiplier;
                    float g = 0.5f + cBody.Y * multiplier;
                    cImage[x, y] = new Rgba64(new Vector4(r, g, 0, 1f));
                }
            }

            return (abImage, cImage);
    }

    public static void GenerateImage(ProbabilityMap map, string name, int brightness = 1)
    {
        using (var image = GetImage(map, brightness))
            image.Save(name + ".png");
    }

    public static void GeneratePNGSequence(MultiFrameProbabilityMap map, string name, int brightness = 1, int binFactor = 1)
    {
        int size = map.Maps[0].Size / binFactor;
        if (!Directory.Exists(name))
        {
            Directory.CreateDirectory(name);
        }
        IndexedThreadPool t = new IndexedThreadPool(map.FrameCount, 8, i =>
        {
            ProbabilityMap m = map.Maps[i];
            if (binFactor != 1)
            {
                m = m.BinDown(binFactor);
            }

            using (var image = GetImage(m, brightness))
            {
                image.Save(name + "/" + i + "a.png");
            }

            var (abImage, cImage) = GetVelocityImages(map.VMaps[i]);
            abImage.Save(name + "/" + i + "b.png");
            cImage.Save(name + "/" + i + "c.png");
            abImage.Dispose();
            cImage.Dispose();
        });
        t.RunSync();
    }
    
    public static void GenerateAnimation(MultiFrameProbabilityMap map, string name, int brightness = 1, int binFactor = 1)
    {
        int size = map.Maps[0].Size / binFactor;
        using Image<Rgba64> gif = new(size, size, Color.Black);
        GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
        metadata.FrameDelay = 2;
        gif.Metadata.GetGifMetadata().RepeatCount = 0;
        Image<Rgba64>[] images = new Image<Rgba64>[map.FrameCount];
        IndexedThreadPool t = new IndexedThreadPool(map.FrameCount, 8, i =>
        {
            ProbabilityMap m = map.Maps[i];
            if (binFactor != 1)
            {
                m = m.BinDown(binFactor);
            }

            var image = GetImage(m, brightness);
            metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
            metadata.FrameDelay = 10;
            images[i] = image;
        });
        t.RunSync();
        for (int i = 0; i < images.Length; i++)
        {
            gif.Frames.AddFrame(images[i].Frames.RootFrame);
            images[i].Dispose();
        }
        gif.SaveAsGif(name + ".gif");
    }
    
    private static float Adjust(float value, int brightness)
    {
        return (float)Math.Pow(value, 1 * Math.Clamp(1f - (brightness - 1) / 100f, 0.01f, 1f));
    }
}