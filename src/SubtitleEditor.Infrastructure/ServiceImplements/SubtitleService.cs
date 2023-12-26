using SubtitleEditor.Core.Models;
using SubtitleEditor.Infrastructure.Models.Asr;
using SubtitleEditor.Infrastructure.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEditor.Infrastructure.ServiceImplements;

public class SubtitleService : ISubtitleService
{
    public async Task<string> ReadFileToStringAsync(byte[] file)
    {
        using var stream = new MemoryStream(file);
        using var reader = new StreamReader(stream);

        var lines = new List<string?>();
        while (!reader.EndOfStream)
        {
            lines.Add(await reader.ReadLineAsync());
        }

        return string.Join('\n', lines.Where(line => line != null).Cast<string>());
    }

    public async Task<Subtitle> ReadFileAsync(byte[] file)
    {
        return Parse(await ReadFileToStringAsync(file));
    }

    public async Task<Subtitle> ReadFileAsync(byte[] file, double frameRate)
    {
        var subtitle = await ReadFileAsync(file);
        var msPreFrame = 1000 / (frameRate >= 1 ? frameRate : 1);
        foreach (var line in subtitle.Lines)
        {
            var startArray = line.Start.Split('.');
            var startFrame = startArray.Length == 2 ? int.Parse(startArray.Last()) : 0;
            var startMs = msPreFrame * startFrame / 10;
            line.Start = $"{startArray.First()}.{(int)Math.Round(startMs >= 1000 ? 999 : startMs):D3}";

            var endArray = line.End.Split('.');
            var endFrame = endArray.Length == 2 ? int.Parse(endArray.Last()) : 0;
            var endMs = msPreFrame * endFrame / 10;
            line.End = $"{endArray.First()}.{(int)Math.Round(endMs >= 1000 ? 999 : endMs):D3}";
        }

        return subtitle;
    }

    public Subtitle Parse(string text)
    {
        var adoptedText = text.Trim();

        if (adoptedText.Contains(" --> "))
        {
            return adoptedText.StartsWith("WEBVTT") ? ParseFromVtt(adoptedText) : ParseFromSrt(adoptedText);
        }

        return ParseFromInline(adoptedText);
    }

    public Subtitle ParseFromSrt(string srt)
    {
        var rawLines = srt.Split('\n');
        var lines = new List<SubtitleLine>();
        var currentLine = new SubtitleLine();
        var contentMode = false;
        var contentArray = new List<string>();
        for (var i = 0; i < rawLines.Length; i++)
        {
            var line = rawLines[i];
            if (line.Contains(" --> "))
            {
                var periodTexts = line.Replace(',', '.').Split(" --> ");
                currentLine.Start = TimeSpan.Parse(periodTexts[0]).ToString("hh\\:mm\\:ss\\.fff");
                currentLine.End = TimeSpan.Parse(periodTexts[1]).ToString("hh\\:mm\\:ss\\.fff");
                contentMode = true;
            }
            else if (contentMode)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    contentArray.Add(line.Trim());
                }
                else
                {
                    currentLine.Content = string.Join(string.Empty, contentArray);
                    currentLine.OriginalContent = currentLine.Content;
                    contentArray = new List<string>();

                    lines.Add(currentLine);
                    contentMode = false;
                    currentLine = new SubtitleLine();
                }
            }
        }

        if (contentArray.Any() && contentMode)
        {
            currentLine.Content = string.Join(string.Empty, contentArray);
            currentLine.OriginalContent = currentLine.Content;
            lines.Add(currentLine);
        }

        return new Subtitle { Lines = lines.ToArray() };
    }

    public Subtitle ParseFromVtt(string vtt)
    {
        var rawLines = vtt.Split('\n');
        var startLineIndex = 0;
        for (var i = 0; i < rawLines.Length; i++)
        {
            var line = rawLines[i];
            if (line.Contains(" --> "))
            {
                startLineIndex = i;
                break;
            }
        }

        var header = string.Join('\n', rawLines.Take(startLineIndex));
        var lines = string.Join('\n', rawLines.Skip(startLineIndex)).Split("\n\n")
            .Where(textSection => !string.IsNullOrWhiteSpace(textSection))
            .Select(textSection => textSection.Split('\n'))
            .Where(textArray => textArray.Length > 2 && textArray[1].Contains(" --> "))
            .Select(textArray =>
            {
                var periodTexts = textArray[1].Split(' ');
                var content = string.Join('\n', textArray.Skip(2));
                return new SubtitleLine
                {
                    Format = periodTexts.Length >= 4 ? string.Join(' ', periodTexts.Skip(3)) : null,
                    Start = TimeSpan.Parse(periodTexts[0]).ToString("hh\\:mm\\:ss\\.fff"),
                    End = TimeSpan.Parse(periodTexts[2]).ToString("hh\\:mm\\:ss\\.fff"),
                    Content = content,
                    OriginalContent = content,
                };
            });

        return new Subtitle { Header = header, Lines = lines.ToArray() };
    }

    public Subtitle ParseFromInline(string inline)
    {
        var rawLines = inline.Split('\n').Where(textSection => !string.IsNullOrWhiteSpace(textSection)).ToArray();
        if (!rawLines.Any(line => Regex.IsMatch(line, @"(\d{1,2}:){1,2}\d{1,2}.\d{1,3}\s{2,}.+")))
        {
            throw new Exception("輸入的文字不是字幕。");
        }

        var lines = new List<SubtitleLine>();
        var subtitleLine = new SubtitleLine();

        for (var i = 0; i < rawLines.Length; i++)
        {
            var line = rawLines[i];
            if (Regex.IsMatch(line, @"(\d{1,2}:){1,2}\d{1,2}.\d{1,3}\s{2,}.+"))
            {
                if (i > 0)
                {
                    subtitleLine = new SubtitleLine();
                }

                lines.Add(subtitleLine);

                var textArray = line.Split("  ");
                var periodText = textArray.First().Trim().Replace(';', '.').Replace(',', '.');
                var start = TimeSpan.TryParse(periodText, out var s) ? s : TimeSpan.Zero;
                var content = string.Join("  ", textArray.Skip(1));

                subtitleLine.Start = start.ToString("hh\\:mm\\:ss\\.fff");
                subtitleLine.Content = content;
                subtitleLine.OriginalContent = content;
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                subtitleLine.Content = $"{subtitleLine.Content}\n{line.Trim()}";
                subtitleLine.OriginalContent = $"{subtitleLine.OriginalContent}\n{line.Trim()}";
            }
        }

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            line.End = lines.Count > i + 1 ? lines[i + 1].Start : line.Start;
        }

        return new Subtitle { Lines = lines.ToArray() };
    }

    public Subtitle UpdateSubtitleWithWordSegments(Subtitle subtitle, NctuWordSegment[] nctuWordSegments)
    {
        for (var lineIndex = 0; lineIndex < subtitle.Lines.Length; lineIndex++)
        {
            var line = subtitle.Lines[lineIndex];
            var wordSegments = nctuWordSegments.Length > lineIndex ? nctuWordSegments[lineIndex] : null;
            var nextLine = subtitle.Lines.Length > lineIndex + 1 ? subtitle.Lines[lineIndex + 1] : null;
            var nextLineStart = nextLine?.Time;

            if (wordSegments != null)
            {
                var adoptedSegments = new List<SubtitleLineWordSegment>();
                for (var wordSegmentIndex = 0; wordSegmentIndex < wordSegments.Result.Length; wordSegmentIndex++)
                {
                    var wordSegment = wordSegments.Result[wordSegmentIndex];
                    var nextWordSegment = wordSegments.Result.Length > wordSegmentIndex + 1 ? wordSegments.Result[wordSegmentIndex + 1] : null;
                    var adoptedTime = wordSegment.Time;

                    if (nextWordSegment != null && nextWordSegment.Time < adoptedTime)
                    {
                        adoptedTime = nextWordSegment.Time;
                    }

                    if (nextLineStart.HasValue && nextLineStart.Value < adoptedTime)
                    {
                        adoptedTime = nextLineStart.Value;
                    }

                    var isEnglish = Regex.IsMatch(wordSegment.Word, @"^([a-zA-Z])+$");
                    var prevWordSegment = wordSegmentIndex > 0 ? wordSegments.Result[wordSegmentIndex - 1] : null;
                    var prevSpace = prevWordSegment != null && Regex.IsMatch(prevWordSegment.Word, @"^([a-zA-Z]|\d)+$");
                    var nextSpace = nextWordSegment != null && Regex.IsMatch(nextWordSegment.Word, @"^(\d)+$");
                    var adoptedWord = $"{(isEnglish && prevSpace ? " " : string.Empty)}{wordSegment.Word}{(isEnglish && nextSpace ? " " : string.Empty)}";

                    adoptedSegments.Add(new SubtitleLineWordSegment(adoptedTime, adoptedWord));
                }

                line.WordSegments = adoptedSegments.ToArray();
                line.OriginalWordSegments = adoptedSegments.ToArray();
            }
        }

        return subtitle;
    }

    public void ReplaceInSubtitle(Subtitle subtitle, Dictionary<string, string> replaceMap)
    {
        if (!replaceMap.Any())
        {
            return;
        }

        foreach (var line in subtitle.Lines.Where(line => replaceMap.Any(pair => line.Content.Contains(pair.Key))))
        {
            if (line.WordSegments == null)
            {
                foreach (var pair in replaceMap)
                {
                    line.Content = line.Content.Replace(pair.Key, pair.Value);
                }

                continue;
            }

            foreach (var pair in replaceMap)
            {
                var target = pair.Key;
                var text = pair.Value;

                var temp = string.Empty;
                var indexs = new List<int>();
                for (var i = 0; i < line.Content.Length; i++)
                {
                    temp += line.Content[i];

                    if (temp.Contains(target))
                    {
                        temp = string.Empty;
                        indexs.Add(i - (target.Length - 1));
                    }
                }

                var targetIndex = 0;
                var textIndex = 0;
                var isResultMode = false;
                var resultLength = 0;
                var newSegments = new List<SubtitleLineWordSegment>();

                for (var i = 0; i < line.WordSegments.Length; i++)
                {
                    var word = line.WordSegments[i].Word;
                    var builder = new StringBuilder();

                    for (var charIndex = 0; charIndex < word.Length; charIndex++)
                    {
                        if (indexs.Contains(targetIndex))
                        {
                            isResultMode = true;
                        }

                        if (isResultMode)
                        {
                            if (textIndex < text.Length)
                            {
                                builder.Append(text[textIndex]);
                                textIndex++;
                            }

                            if (textIndex == target.Length && text.Length > target.Length)
                            {
                                builder.Append(text.AsSpan(textIndex));
                                textIndex += text.Length - target.Length;
                            }

                            resultLength++;
                        }
                        else
                        {
                            builder.Append(word[charIndex]);
                        }

                        if (resultLength >= target.Length)
                        {
                            isResultMode = false;
                            resultLength = 0;
                            textIndex = 0;
                        }

                        targetIndex++;
                    }

                    var newWord = builder.ToString();
                    if (!string.IsNullOrEmpty(newWord))
                    {
                        newSegments.Add(new SubtitleLineWordSegment
                        {
                            Start = line.WordSegments[i].Start,
                            Word = newWord
                        });
                    }
                }

                line.WordSegments = newSegments.ToArray();
            }
        }
    }

    public string ToSrt(Subtitle subtitle, double? frameRate = null)
    {
        var msPreFrame = frameRate.HasValue && frameRate.Value >= 1 ? 1000 / frameRate.Value : 1;
        var srtLines = new List<string>();
        for (var i = 0; i < subtitle.Lines.Length; i++)
        {
            var line = subtitle.Lines[i];
            var start = _convertMsToFrame(line.Start, frameRate, msPreFrame);
            var end = _convertMsToFrame(line.End, frameRate, msPreFrame);

            var texts = new List<string>
            {
                (i + 1).ToString(),
                $"{start} --> {end}".Replace('.', ','),
                line.Content
            };

            srtLines.Add(string.Join("\n", texts));
        }

        return string.Join("\n\n", srtLines);
    }

    public string ToVtt(Subtitle subtitle, double? frameRate = null)
    {
        var msPreFrame = frameRate.HasValue && frameRate.Value >= 1 ? 1000 / frameRate.Value : 1;
        var srtLines = new List<string>();
        for (var i = 0; i < subtitle.Lines.Length; i++)
        {
            var line = subtitle.Lines[i];
            var start = _convertMsToFrame(line.Start, frameRate, msPreFrame);
            var end = _convertMsToFrame(line.End, frameRate, msPreFrame);

            var texts = new List<string>
            {
                (i + 1).ToString(),
                $"{start} --> {end}{(line.Format != null ? $" {line.Format}" : "")}",
                line.Content
            };

            srtLines.Add(string.Join("\n", texts));
        }

        var mainPart = string.Join("\n\n", srtLines);
        var header = !string.IsNullOrWhiteSpace(subtitle.Header) ? subtitle.Header : "WEBVTT\n\n";
        return $"{header}{mainPart}";
    }

    public string ToInline(Subtitle subtitle, double? frameRate = null)
    {
        var msPreFrame = frameRate.HasValue && frameRate.Value >= 1 ? 1000 / frameRate.Value : 1;
        var srtLines = new List<string>();
        for (var i = 0; i < subtitle.Lines.Length; i++)
        {
            var line = subtitle.Lines[i];
            var start = _convertMsToFrame(line.Start, frameRate, msPreFrame);
            srtLines.Add($"{start.Replace('.', ';')}  {line.Content}");
        }

        return string.Join('\n', srtLines);
    }

    public string ToNoTime(Subtitle subtitle)
    {
        return string.Join('\n', subtitle.Lines.Select(line => line.Content));
    }

    private static string _convertMsToFrame(string timeString, double? frameRate, double msPreFrame)
    {
        if (string.IsNullOrWhiteSpace(timeString) || !frameRate.HasValue)
        {
            return timeString;
        }

        var digitLength = frameRate.Value.ToString().Length;
        var timeArray = timeString.Split('.');
        var ms = timeArray.Length == 2 ? int.Parse(timeArray.Last()) : 0;
        var frame = (int)Math.Round(ms / msPreFrame);
        return $"{timeArray.First()}.{frame.ToString($"D{digitLength}")}";
    }
}
