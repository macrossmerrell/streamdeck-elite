using System.Drawing;

namespace Elite
{
    // Shared auto-fit text drawing used by the info buttons. Fonts and string formats are
    // disposed on every path (the per-button copies this replaced leaked them).
    public static class TextFit
    {
        private static RectangleF Measure(Graphics graphics, string text, Font font)
        {
            using (var sf = new StringFormat(StringFormat.GenericTypographic))
            {
                sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, text.Length) });
                var regions = graphics.MeasureCharacterRanges(text, font, new RectangleF(0, 0, 1000, 1000), sf);
                try { return regions[0].GetBounds(graphics); }
                finally { foreach (var r in regions) r.Dispose(); }
            }
        }

        // Centred, auto-sized (possibly multi-line) text; each line is capped to 40% of the width
        // in height and 85% of the width in length.
        public static void DrawFittedText(Graphics graphics, string text, Color color, double verticalPosition, bool bold, int width)
        {
            if (string.IsNullOrEmpty(text)) return;

            var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;
            var lines = text.Replace("\r\n", "\n").Replace("\n", "\n").Split('\n');

            var maxFontSize = (int)(48 * (width / 256.0));
            if (maxFontSize < 10) maxFontSize = 10;

            var maxLineHeight = width * 0.40f;

            using (var brush = new SolidBrush(color))
            using (var drawFmt = new StringFormat(StringFormat.GenericTypographic))
            {
                for (int size = maxFontSize; size >= 10; size--)
                {
                    using (var font = new Font("Arial", size, fontStyle))
                    {
                        bool fits = true;

                        foreach (var line in lines)
                        {
                            if (string.IsNullOrEmpty(line)) continue;

                            var bounds = Measure(graphics, line, font);
                            if (bounds.Width > width * 0.85f || bounds.Height > maxLineHeight)
                            {
                                fits = false;
                                break;
                            }
                        }

                        if (!fits) continue;

                        float currentY = (float)(verticalPosition * (width / 256.0));

                        foreach (var line in lines)
                        {
                            if (string.IsNullOrEmpty(line)) { currentY += font.Height * 1.1f; continue; }

                            var b = Measure(graphics, line, font);
                            var x = (width - b.Width) / 2.0f;
                            graphics.DrawString(line, font, brush, x, currentY - b.Y, drawFmt);
                            currentY += b.Height * 1.1f;
                        }

                        return;
                    }
                }
            }
        }

        // A small label with a value beneath it. Both share one font size, chosen as the largest that
        // fits the width and keeps the pair inside maxHeight (the gap to the next block).
        public static void DrawLabelAndValue(Graphics graphics, string label, string value, SolidBrush brush, double verticalPosition, int width, float maxHeight, bool bold)
        {
            if (string.IsNullOrEmpty(value)) return;

            var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;

            var startSize = (int)(64 * (width / 256.0));
            if (startSize < 8) startSize = 8;

            using (var drawFmt = new StringFormat(StringFormat.GenericTypographic))
            {
                for (int size = startSize; size >= 8; size--)
                {
                    using (var font = new Font("Arial", size, fontStyle))
                    {
                        var labelBounds = Measure(graphics, label, font);
                        var valueBounds = Measure(graphics, value, font);

                        bool fits = labelBounds.Width <= width * 0.95f && valueBounds.Width <= width * 0.95f
                            && (labelBounds.Height * 1.1f + valueBounds.Height) <= maxHeight;

                        if (!fits) continue;

                        float currentY = (float)(verticalPosition * (width / 256.0));

                        graphics.DrawString(label, font, brush, (width - labelBounds.Width) / 2.0f, currentY - labelBounds.Y, drawFmt);
                        currentY += labelBounds.Height * 1.1f;
                        graphics.DrawString(value, font, brush, (width - valueBounds.Width) / 2.0f, currentY - valueBounds.Y, drawFmt);

                        return;
                    }
                }
            }
        }
    }
}
