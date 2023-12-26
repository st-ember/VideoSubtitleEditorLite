using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SubtitleEditor.Web.Infrastructure.Models.Captcha;
using SubtitleEditor.Web.Infrastructure.Services;
using System.Text;

namespace SubtitleEditor.Web.Infrastructure.ServiceImplements;

public class CaptchaService : ICaptchaService
{
    private const string _letters = "23456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly Random _random = new();

    public string GenerateCaptchaCode()
    {
        var maxRand = _letters.Length - 1;

        var sb = new StringBuilder();

        for (var i = 0; i < 4; i++)
        {
            var index = _random.Next(maxRand);
            sb.Append(_letters[index]);
        }

        return sb.ToString();
    }

    public CaptchaResult GenerateCaptchaImage(int width, int height, string captchaCode)
    {
        using var img = new Image<Rgba32>(width, height);
        img.Mutate(context => context.BackgroundColor(_getRandomLightColor()));

        _drawLines(img, width, height);
        _drawText(img, width, height, captchaCode);
        //_drawLines(img, width, height);
        _drawDots(img, width, height);

        using var stream = new MemoryStream();
        img.SaveAsPng(stream);

        return new CaptchaResult
        {
            CaptchaCode = captchaCode,
            CaptchaByteData = stream.ToArray()
        };
    }

    private void _drawText(Image<Rgba32> image, int width, int height, string captchaCode)
    {
        var fontSize = _getFontSize(width, captchaCode.Length);
        var font = SystemFonts.CreateFont(
            Environment.OSVersion.Platform == PlatformID.Win32NT ? "Arial" : "DejaVu Sans", 
            fontSize,
            Environment.OSVersion.Platform == PlatformID.Win32NT ? FontStyle.Bold : FontStyle.Regular);

        for (var i = 0; i < captchaCode.Length; i++)
        {
            using var textImage = new Image<Rgba32>(fontSize, fontSize);

            var shiftPx = fontSize / 16;
            var maxY = height - fontSize - 10;

            var x = i * fontSize + _random.Next(-shiftPx, shiftPx) + _random.Next(-shiftPx, shiftPx);
            var y = _random.Next(0, maxY < 0 ? 0 : maxY);
            var location = new Point(x, y);

            textImage.Mutate(context =>
                context.DrawText(captchaCode[i].ToString(), font, _getRandomDeepColor(), new Point(0, 0)).Rotate(_random.Next(-45, 45))
                );

            image.Mutate(context => context.DrawImage(textImage, location, 1));
        }
    }

    private void _drawLines(Image<Rgba32> image, int width, int height)
    {
        for (var i = 0; i < _random.Next(3, 6); i++)
        {
            var startPoint = new PointF(_random.Next(0, width), _random.Next(0, height));
            var endPoint = new PointF(_random.Next(0, width), _random.Next(0, height));
            image.Mutate(ctx => ctx.DrawLines(_getRandomDeepColor(), 2, new PointF[] { startPoint, endPoint }));
        }
    }

    private void _drawDots(Image<Rgba32> image, int width, int height)
    {
        for (var i = 0; i < _random.Next(30, 60); i++)
        {
            var point = new PointF(_random.Next(0, width), _random.Next(0, height));
            image.Mutate(ctx => ctx.DrawLines(_getRandomDeepColor(), 1, new PointF[] { point, point }));
        }
    }

    private static int _getFontSize(int imageWidth, int captchCodeCount)
    {
        return Convert.ToInt32((float)imageWidth / captchCodeCount);
    }

    private Color _getRandomDeepColor()
    {
        //return Color.FromRgb((byte)_random.Next(160), (byte)_random.Next(100), (byte)_random.Next(160));
        return Color.FromRgb(63, 67, 72);
    }

    private Color _getRandomLightColor()
    {
        //var low = 180;
        //var high = 255;
        //var nRend = _random.Next(high - low) + low;
        //var nGreen = _random.Next(high - low) + low;
        //var nBlue = _random.Next(high - low) + low;
        //return Color.FromRgb((byte)nRend, (byte)nGreen, (byte)nBlue);
        return Color.FromRgb(238, 238, 238);
    }
}