using System.Text;

namespace TermuiX
{
    /// <summary>
    /// Internal renderer that converts widget trees into character arrays for console display.
    /// </summary>
    internal class Renderer
    {
        private int _width;
        private int _height;

        internal void Size(int width, int height)
        {
            _width = width;
            _height = height;
        }

        internal (Rune[][] chars, ConsoleColor[][] fg, ConsoleColor[][] bg) Render(IWidget widget)
        {
            if (_width == 0 || _height == 0)
            {
                throw new InvalidOperationException("Renderer size not set. Call Size(width, height) before Render().");
            }

            var output = new Rune[_height][];
            var fgColors = new ConsoleColor[_height][];
            var bgColors = new ConsoleColor[_height][];

            for (int i = 0; i < _height; i++)
            {
                output[i] = new Rune[_width];
                fgColors[i] = new ConsoleColor[_width];
                bgColors[i] = new ConsoleColor[_width];
                Array.Fill(output[i], new Rune(' '));
                Array.Fill(fgColors[i], ConsoleColor.White);
                Array.Fill(bgColors[i], ConsoleColor.Black);
            }

            RenderWidget(output, fgColors, bgColors, widget, 0, 0, _width, _height, 0, 0);

            return (output, fgColors, bgColors);
        }

        private static void RenderWidget(Rune[][] output, ConsoleColor[][] fgColors, ConsoleColor[][] bgColors, IWidget widget, int parentX, int parentY, int parentWidth, int parentHeight, int parentScrollX, int parentScrollY)
        {
            // Skip invisible widgets and their children
            if (!widget.Visible)
            {
                return;
            }

            int width = ParseSize(widget.Width, parentWidth);
            int height = ParseSize(widget.Height, parentHeight);
            int posX = ParseSize(widget.PositionX, parentWidth);
            int posY = ParseSize(widget.PositionY, parentHeight);

            posX -= parentScrollX;
            posY -= parentScrollY;

            int padLeft = ParseSize(widget.PaddingLeft, width);
            int padTop = ParseSize(widget.PaddingTop, height);
            int padRight = ParseSize(widget.PaddingRight, width);
            int padBottom = ParseSize(widget.PaddingBottom, height);

            // If widget is a Container with a border, add 1ch to all padding for the border
            if (widget is Widgets.Container container && container.HasBorder)
            {
                padLeft += 1;
                padTop += 1;
                padRight += 1;
                padBottom += 1;
            }

            int absX = parentX + posX;
            int absY = parentY + posY;

            if (absX >= parentX + parentWidth || absY >= parentY + parentHeight)
            {
                return;
            }

            if (absX + width < parentX || absY + height < parentY)
            {
                return;
            }

            var raw = widget.GetRaw();

            int contentWidth = Math.Max(0, width - padLeft - padRight);
            int contentHeight = Math.Max(0, height - padTop - padBottom);
            int contentX = absX + padLeft;
            int contentY = absY + padTop;

            int scrollX = widget.Scrollable ? (int)widget.ScrollOffsetX : 0;
            int scrollY = widget.Scrollable ? (int)widget.ScrollOffsetY : 0;

            var bgColor = widget.Disabled && widget.DisabledBackgroundColor.HasValue ? widget.DisabledBackgroundColor.Value :
                          (widget.Focussed ? widget.FocusBackgroundColor : widget.BackgroundColor);
            var fgColor = widget.Disabled ? widget.DisabledForegroundColor :
                          (widget.Focussed ? widget.FocusForegroundColor : widget.ForegroundColor);
            for (int y = 0; y < height && absY + y < output.Length; y++)
            {
                for (int x = 0; x < width && absX + x < output[0].Length; x++)
                {
                    if (absX + x >= 0 && absY + y >= 0 && absX + x >= parentX && absY + y >= parentY && absX + x < parentX + parentWidth && absY + y < parentY + parentHeight)
                    {
                        output[absY + y][absX + x] = new Rune(' ');
                        bgColors[absY + y][absX + x] = bgColor;
                        fgColors[absY + y][absX + x] = fgColor;
                    }
                }
            }

            if (raw?.Length > 0)
            {
                // First render the raw content WITHOUT scroll (for borders, static content)
                for (int y = 0; y < raw.Length && y < height; y++)
                {
                    for (int x = 0; x < raw[y].Length && x < width; x++)
                    {
                        int targetX = absX + x;
                        int targetY = absY + y;

                        if (targetY >= absY && targetY < absY + height &&
                            targetX >= absX && targetX < absX + width &&
                            targetY >= 0 && targetY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && targetY >= parentY &&
                            targetX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            output[targetY][targetX] = raw[y][x];
                        }
                    }
                }
            }

            // Children are positioned relative to content area and affected by scroll
            // Clipping region is the intersection of parent bounds and content area
            int childClipX = Math.Max(parentX, contentX);
            int childClipY = Math.Max(parentY, contentY);
            int childClipRight = Math.Min(parentX + parentWidth, contentX + contentWidth);
            int childClipBottom = Math.Min(parentY + parentHeight, contentY + contentHeight);
            int childClipWidth = Math.Max(0, childClipRight - childClipX);
            int childClipHeight = Math.Max(0, childClipBottom - childClipY);

            // Check if scrollbars will be rendered (need to check before rendering children)
            bool willRenderVerticalScrollbar = false;
            bool willRenderHorizontalScrollbar = false;

            if (widget.Scrollable && widget.Children.Count > 0)
            {
                int maxChildBottom = 0;
                int maxChildRight = 0;
                foreach (var child in widget.Children)
                {
                    int childPosY = ParseSize(child.PositionY, contentHeight);
                    int childPosX = ParseSize(child.PositionX, contentWidth);

                    // Use already computed sizes if available, otherwise calculate
                    int childHeight = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight);
                    int childWidth = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth);

                    maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
                    maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
                }

                willRenderVerticalScrollbar = contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0);
                willRenderHorizontalScrollbar = contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0);
            }

            // Reduce clip region if scrollbars will be rendered
            if (willRenderVerticalScrollbar)
            {
                childClipWidth = Math.Max(0, childClipWidth - 1);
            }
            if (willRenderHorizontalScrollbar)
            {
                childClipHeight = Math.Max(0, childClipHeight - 1);
            }

            // Adjust scroll to account for content offset relative to clip region
            int childScrollX = scrollX + (childClipX - contentX);
            int childScrollY = scrollY + (childClipY - contentY);

            foreach (var child in widget.Children)
            {
                RenderWidget(output, fgColors, bgColors, child, childClipX, childClipY, childClipWidth, childClipHeight, childScrollX, childScrollY);
            }

            // Render scrollbars AFTER children so they're always on top
            if (widget.Scrollable && widget.Children.Count > 0)
            {
                int maxChildBottom = 0;
                int maxChildRight = 0;
                foreach (var child in widget.Children)
                {
                    int childPosY = ParseSize(child.PositionY, contentHeight);
                    int childPosX = ParseSize(child.PositionX, contentWidth);

                    // Use already computed sizes if available, otherwise calculate
                    int childHeight = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight);
                    int childWidth = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth);

                    maxChildBottom = Math.Max(maxChildBottom, childPosY + childHeight);
                    maxChildRight = Math.Max(maxChildRight, childPosX + childWidth);
                }

                // Reset scrollbar flags before checking
                widget.HasVerticalScrollbar = false;
                widget.HasHorizontalScrollbar = false;

                if (contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0))
                {
                    // Set flag indicating vertical scrollbar is rendered in this frame
                    widget.HasVerticalScrollbar = true;

                    int scrollbarHeight = Math.Max(1, (contentHeight * contentHeight) / maxChildBottom);
                    int scrollbarPos = maxChildBottom > contentHeight ? (int)((float)scrollY / (maxChildBottom - contentHeight) * (contentHeight - scrollbarHeight)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentHeight - scrollbarHeight));

                    int scrollbarX = contentX + contentWidth - 1;

                    for (int y = 0; y < contentHeight; y++)
                    {
                        int targetY = contentY + y;

                        if (targetY >= 0 && targetY < output.Length &&
                            scrollbarX >= 0 && scrollbarX < output[0].Length &&
                            scrollbarX >= parentX && targetY >= parentY &&
                            scrollbarX < parentX + parentWidth && targetY < parentY + parentHeight)
                        {
                            if (y >= scrollbarPos && y < scrollbarPos + scrollbarHeight)
                            {
                                output[targetY][scrollbarX] = new Rune('▐');
                            }
                            else
                            {
                                output[targetY][scrollbarX] = new Rune('┊');
                            }
                        }
                    }
                }

                if (contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0))
                {
                    // Set flag indicating horizontal scrollbar is rendered in this frame
                    widget.HasHorizontalScrollbar = true;

                    int scrollbarWidth = Math.Max(1, (contentWidth * contentWidth) / maxChildRight);
                    int scrollbarPos = maxChildRight > contentWidth ? (int)((float)scrollX / (maxChildRight - contentWidth) * (contentWidth - scrollbarWidth)) : 0;
                    scrollbarPos = Math.Max(0, Math.Min(scrollbarPos, contentWidth - scrollbarWidth));

                    int scrollbarY = contentY + contentHeight - 1;

                    for (int x = 0; x < contentWidth; x++)
                    {
                        int targetX = contentX + x;

                        if (scrollbarY >= 0 && scrollbarY < output.Length &&
                            targetX >= 0 && targetX < output[0].Length &&
                            targetX >= parentX && scrollbarY >= parentY &&
                            targetX < parentX + parentWidth && scrollbarY < parentY + parentHeight)
                        {
                            if (x >= scrollbarPos && x < scrollbarPos + scrollbarWidth)
                            {
                                output[scrollbarY][targetX] = new Rune('▄');
                            }
                            else
                            {
                                output[scrollbarY][targetX] = new Rune('┈');
                            }
                        }
                    }
                }
            }
        }

        private static int ParseSize(string size, int parentSize)
        {
            if (string.IsNullOrEmpty(size))
            {
                return 0;
            }

            size = size.Trim();

            if (size.EndsWith("ch"))
            {
                var value = size[..^2].Trim();
                if (int.TryParse(value, out int result))
                {
                    return result;
                }

                throw new FormatException($"Invalid size value: '{size}'. Expected format: '{{number}}ch' (e.g., '10ch')");
            }
            else if (size.EndsWith('%'))
            {
                var value = size[..^1].Trim();
                if (float.TryParse(value, out float percent))
                {
                    return (int)(parentSize * percent / 100.0f);
                }

                throw new FormatException($"Invalid size value: '{size}'. Expected format: '{{number}}%' (e.g., '50%')");
            }

            throw new FormatException($"Invalid size value: '{size}'. Size must end with 'ch' or '%' (e.g., '10ch' or '50%')");
        }
    }
}