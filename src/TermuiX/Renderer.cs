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

        // Cached frame buffers — reused across frames when size stays the same
        private Rune[][]? _cachedOutput;
        private Color[][]? _cachedFg;
        private Color[][]? _cachedBg;
        private Widgets.TextStyle[][]? _cachedStyles;
        private int _cachedWidth;
        private int _cachedHeight;

        internal void Size(int width, int height)
        {
            _width = width;
            _height = height;
        }

        internal (Rune[][] chars, Color[][] fg, Color[][] bg, Widgets.TextStyle[][] styles) Render(IWidget widget, HitTestMap? hitTestMap = null, bool focusVisible = true)
        {
            if (_width == 0 || _height == 0)
            {
                throw new InvalidOperationException("Renderer size not set. Call Size(width, height) before Render().");
            }

            // Only reallocate when terminal size changes
            if (_cachedOutput == null || _cachedWidth != _width || _cachedHeight != _height)
            {
                _cachedOutput = new Rune[_height][];
                _cachedFg = new Color[_height][];
                _cachedBg = new Color[_height][];
                _cachedStyles = new Widgets.TextStyle[_height][];

                for (int i = 0; i < _height; i++)
                {
                    _cachedOutput[i] = new Rune[_width];
                    _cachedFg[i] = new Color[_width];
                    _cachedBg[i] = new Color[_width];
                    _cachedStyles[i] = new Widgets.TextStyle[_width];
                }

                _cachedWidth = _width;
                _cachedHeight = _height;
            }

            var output = _cachedOutput;
            var fgColors = _cachedFg!;
            var bgColors = _cachedBg!;
            var styleBuffer = _cachedStyles!;

            // Clear buffers for new frame
            for (int i = 0; i < _height; i++)
            {
                Array.Fill(output[i], new Rune(' '));
                Array.Fill(fgColors[i], ConsoleColor.White);
                Array.Fill(bgColors[i], ConsoleColor.Black);
                Array.Clear(styleBuffer[i]);
            }

            hitTestMap?.Reset(_width, _height);
            RenderWidget(output, fgColors, bgColors, styleBuffer, widget, 0, 0, _width, _height, 0, 0, hitTestMap, focusVisible, Color.Black, Color.White);

            return (output, fgColors, bgColors, styleBuffer);
        }

        private static void RenderWidget(Rune[][] output, Color[][] fgColors, Color[][] bgColors, Widgets.TextStyle[][] styleBuffer, IWidget widget, int parentX, int parentY, int parentWidth, int parentHeight, int parentScrollX, int parentScrollY, HitTestMap? hitTestMap, bool focusVisible, Color inheritedBg, Color inheritedFg)
        {
            // Skip invisible widgets and their children
            if (!widget.Visible)
            {
                return;
            }

            int width = ParseSize(widget.Width, parentWidth);
            int height = ParseSize(widget.Height, parentHeight);

            // For children inside a StackPanel, save layout-pass main-axis value
            // before ResolveAutoSize runs (it may overwrite with a stale value that
            // doesn't account for margins).
            bool isInStackPanel = widget.Parent is Widgets.StackPanel;
            bool parentIsVert = isInStackPanel && widget.Parent is Widgets.StackPanel sp3 && sp3.Direction == Widgets.StackDirection.Vertical;
            int layoutMain = 0;
            if (isInStackPanel)
                layoutMain = parentIsVert ? widget.ComputedHeight : widget.ComputedWidth;

            // StackPanel auto-size: compute size from children when Width/Height is "auto"
            if (widget is Widgets.StackPanel autoStack)
            {
                var (autoW, autoH) = ResolveAutoSize(autoStack, parentWidth, parentHeight);
                if (autoW > 0) width = autoW;
                if (autoH > 0) height = autoH;
            }

            // For children inside a StackPanel, use ComputedWidth/Height.
            // Cross-axis: read AFTER ResolveAutoSize (it produces correct shrink-wrap).
            // Main-axis: use saved layout-pass value (ResolveAutoSize may miss margins).
            if (isInStackPanel)
            {
                if (parentIsVert)
                {
                    if (widget.ComputedWidth > 0) width = widget.ComputedWidth;
                    if (layoutMain > 0) height = layoutMain;
                }
                else
                {
                    if (layoutMain > 0) width = layoutMain;
                    if (widget.ComputedHeight > 0) height = widget.ComputedHeight;
                }
            }

            // Apply min/max constraints
            if (!string.IsNullOrEmpty(widget.MinWidth))
                width = Math.Max(width, ParseSize(widget.MinWidth, parentWidth));
            if (!string.IsNullOrEmpty(widget.MaxWidth))
                width = Math.Min(width, ParseSize(widget.MaxWidth, parentWidth));
            if (!string.IsNullOrEmpty(widget.MinHeight))
                height = Math.Max(height, ParseSize(widget.MinHeight, parentHeight));
            if (!string.IsNullOrEmpty(widget.MaxHeight))
                height = Math.Min(height, ParseSize(widget.MaxHeight, parentHeight));

            int posX = ParseSize(widget.PositionX, parentWidth);
            int posY = ParseSize(widget.PositionY, parentHeight);

            posX -= parentScrollX;
            posY -= parentScrollY;

            int marginLeft = ParseSize(widget.MarginLeft, parentWidth);
            int marginTop = ParseSize(widget.MarginTop, parentHeight);

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

            int absX = parentX + posX + marginLeft;
            int absY = parentY + posY + marginTop;

            // Set constrained size BEFORE GetRaw() so widgets (Container, Button)
            // can render borders/content at the correct Min/Max-constrained size.
            widget.ComputedWidth = width;
            widget.ComputedHeight = height;

            var raw = widget.GetRaw();

            // After GetRaw(), self-sizing widgets (not inside a StackPanel) may report
            // content larger than their declared size (e.g. TreeView inside a scroll container).
            // Expand width/height so the parent scroll container can see the full content.
            if (raw?.Length > 0 && !(widget is Widgets.StackPanel) && !(widget.Parent is Widgets.StackPanel))
            {
                int rawH = raw.Length;
                int rawW = 0;
                for (int i = 0; i < raw.Length; i++)
                    if (raw[i] != null && raw[i].Length > rawW) rawW = raw[i].Length;

                if (rawW > width) width = rawW;
                if (rawH > height) height = rawH;
            }

            hitTestMap?.Set(absX, absY, width, height, widget, parentX, parentY, parentWidth, parentHeight);

            if (absX >= parentX + parentWidth || absY >= parentY + parentHeight)
            {
                return;
            }

            if (absX + width < parentX || absY + height < parentY)
            {
                return;
            }

            int contentWidth = Math.Max(0, width - padLeft - padRight);
            int contentHeight = Math.Max(0, height - padTop - padBottom);
            int contentX = absX + padLeft;
            int contentY = absY + padTop;

            int scrollX = widget.ScrollX ? (int)widget.ScrollOffsetX : 0;
            int scrollY = widget.ScrollY ? (int)widget.ScrollOffsetY : 0;

            // Resolve inherited colors
            var resolvedBg = widget.BackgroundColor.IsInherit ? inheritedBg : widget.BackgroundColor;
            var resolvedFg = widget.ForegroundColor.IsInherit ? inheritedFg : widget.ForegroundColor;
            var resolvedFocusBg = widget.FocusBackgroundColor.IsInherit ? inheritedBg : widget.FocusBackgroundColor;
            var resolvedFocusFg = widget.FocusForegroundColor.IsInherit ? inheritedFg : widget.FocusForegroundColor;

            bool highlighted = (widget.Focussed && focusVisible) || widget.Hovered;
            var bgColor = widget.Disabled && widget.DisabledBackgroundColor.HasValue ? widget.DisabledBackgroundColor.Value :
                          (highlighted ? resolvedFocusBg : resolvedBg);
            var fgColor = widget.Disabled ? widget.DisabledForegroundColor :
                          (highlighted ? resolvedFocusFg : resolvedFg);
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
                var rawColors = widget.GetRawColors();
                var rawStyles = widget.GetRawStyles();

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
                            if (rawColors.HasValue)
                            {
                                var rcFg = rawColors.Value.fg[y][x];
                                var rcBg = rawColors.Value.bg[y][x];
                                // Only override cells that have an explicit color set.
                                // default(Color) (= ConsoleColor.Black, value 0) means
                                // "no override" — keep the widget's resolved color.
                                if (rcFg != default) fgColors[targetY][targetX] = rcFg;
                                if (rcBg != default) bgColors[targetY][targetX] = rcBg;
                            }
                            if (rawStyles != null && y < rawStyles.Length && x < rawStyles[y].Length)
                            {
                                styleBuffer[targetY][targetX] = rawStyles[y][x];
                            }
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

            // StackPanel layout pass: compute child positions before scrollbar checks
            if (widget is Widgets.StackPanel stackPanel && widget.Children.Count > 0)
            {
                bool isVertical = stackPanel.Direction == Widgets.StackDirection.Vertical;
                var justify = stackPanel.Justify;
                var align = stackPanel.Align;
                bool wrap = stackPanel.Wrap;

                int mainAxisSize = isVertical ? contentHeight : contentWidth;
                int crossAxisSize = isVertical ? contentWidth : contentHeight;

                // Measure all visible children with cumulative % rounding.
                // Two-pass approach: first measure non-fill children, then distribute remaining space to fill children.
                var measured = new List<(IWidget child, int mainSize, int crossSize, int mMain0, int mMain1, int mCross0, int mCross1)>();
                float cumulativePercent = 0f;
                int cumulativePixels = 0;

                // Pass 1: measure non-fill children, count fill children
                int usedMainSpace = 0;
                int fillCount = 0;
                var fillIndices = new List<int>();

                int childIndex = 0;
                foreach (var child in widget.Children)
                {
                    if (!child.Visible) continue;

                    // Pre-set ComputedWidth/Height from MaxWidth/MaxHeight constraints
                    // so nested widgets (e.g. Text inside a MaxWidth StackPanel) can
                    // calculate wrapping correctly during GetRaw().
                    // Use as upper bound — don't override a smaller value already set
                    // by ResolveAutoSize (shrink-wrap).
                    if (isVertical)
                    {
                        if (!string.IsNullOrEmpty(child.MaxWidth))
                        {
                            int maxW = Math.Min(ParseSize(child.MaxWidth, contentWidth), crossAxisSize);
                            if (child.ComputedWidth == 0 || child.ComputedWidth > maxW)
                                child.ComputedWidth = maxW;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(child.MaxHeight))
                        {
                            int maxH = Math.Min(ParseSize(child.MaxHeight, contentHeight), crossAxisSize);
                            if (child.ComputedHeight == 0 || child.ComputedHeight > maxH)
                                child.ComputedHeight = maxH;
                        }
                    }

                    // For auto-sized StackPanels, clear stale ComputedHeight/Width before
                    // GetRaw() so the intrinsic size reflects current children, not a cached
                    // value from a previous layout pass (e.g. after children were removed).
                    string mainStr = isVertical ? child.Height : child.Width;
                    bool mainIsAuto = string.IsNullOrEmpty(mainStr) || mainStr.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase);
                    if (mainIsAuto && child is Widgets.StackPanel)
                    {
                        if (isVertical) child.ComputedHeight = 0;
                        else child.ComputedWidth = 0;
                    }

                    var rawContent = child.GetRaw();

                    // Capture intrinsic size from GetRaw() before resetting.
                    // Used as fallback for children without an explicit main-axis size.
                    int intrinsicWidth = child.ComputedWidth;
                    int intrinsicHeight = child.ComputedHeight;
                    if (intrinsicWidth == 0 && rawContent.Length > 0 && rawContent[0].Length > 0)
                    {
                        intrinsicWidth = rawContent[0].Length;
                    }
                    if (intrinsicHeight == 0 && rawContent.Length > 0)
                    {
                        intrinsicHeight = rawContent.Length;
                    }

                    // Reset computed sizes so StackPanel layout uses declared sizes, not
                    // values that GetRaw() may have set (e.g. Button's internal container
                    // computing 100% of parent height instead of the declared "3ch").
                    child.ComputedWidth = 0;
                    child.ComputedHeight = 0;

                    int mLeft = ParseSize(child.MarginLeft, contentWidth);
                    int mTop = ParseSize(child.MarginTop, contentHeight);
                    int mRight = ParseSize(child.MarginRight, contentWidth);
                    int mBottom = ParseSize(child.MarginBottom, contentHeight);

                    int mMain0 = isVertical ? mTop : mLeft;
                    int mMain1 = isVertical ? mBottom : mRight;
                    int mCross0 = isVertical ? mLeft : mTop;
                    int mCross1 = isVertical ? mRight : mBottom;

                    string sizeStr = isVertical ? child.Height : child.Width;
                    bool isFill = sizeStr.Trim().Equals("fill", StringComparison.OrdinalIgnoreCase);
                    bool isAutoSize = string.IsNullOrEmpty(sizeStr) || sizeStr.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase);
                    int mainChildSize;

                    if (isFill)
                    {
                        // Placeholder — will be resolved in pass 2
                        mainChildSize = 0;
                        fillCount++;
                        fillIndices.Add(measured.Count);
                    }
                    else if (isAutoSize)
                    {
                        int intrinsic = isVertical ? intrinsicHeight : intrinsicWidth;
                        if (child.Children.Count > 0)
                        {
                            int fromMeasure = MeasureChildrenMainAxis(child, isVertical, contentWidth, contentHeight, crossAxisSize);
                            // MeasureChildrenMainAxis returns content size only.
                            // If GetRaw() was empty (no intrinsic), add this container's
                            // own border + padding on the main axis.
                            if (rawContent.Length == 0 && fromMeasure > 0 && child is Widgets.Container childContainer)
                            {
                                if (childContainer.HasBorder)
                                    fromMeasure += 2;
                                fromMeasure += isVertical
                                    ? ParseSize(child.PaddingTop, 0) + ParseSize(child.PaddingBottom, 0)
                                    : ParseSize(child.PaddingLeft, 0) + ParseSize(child.PaddingRight, 0);
                            }
                            if (fromMeasure > intrinsic)
                                intrinsic = fromMeasure;
                        }
                        mainChildSize = intrinsic;
                    }
                    else if (sizeStr.TrimEnd().EndsWith('%') && float.TryParse(sizeStr.TrimEnd()[..^1].Trim(), out float pct))
                    {
                        // Cumulative rounding for %-based sizes
                        cumulativePercent += pct;
                        int newCumulative = (int)(mainAxisSize * cumulativePercent / 100.0f);
                        mainChildSize = newCumulative - cumulativePixels;
                        cumulativePixels = newCumulative;
                    }
                    else
                    {
                        mainChildSize = isVertical
                            ? (child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, contentHeight))
                            : (child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, contentWidth));

                        // Text widgets with wrapping: GetRaw() may expand height beyond the
                        // initial declared value (XmlParser sets "1ch" for single-line text,
                        // but wrapping can produce multiple lines). Use the actual rendered
                        // height from GetRaw() when it exceeds the declared size.
                        if (child is Widgets.Text)
                        {
                            int intrinsicMain = isVertical ? rawContent.Length : (rawContent.Length > 0 ? rawContent[0].Length : 0);
                            if (intrinsicMain > mainChildSize)
                                mainChildSize = intrinsicMain;
                        }
                    }

                    // Cross-axis: "fill" means full cross axis size; otherwise clamp to available space
                    string crossStr = isVertical ? child.Width : child.Height;
                    int crossChildSize;
                    bool isCrossAuto = string.IsNullOrEmpty(crossStr) || crossStr.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase);
                    if (crossStr.Trim().Equals("fill", StringComparison.OrdinalIgnoreCase))
                    {
                        crossChildSize = crossAxisSize;
                    }
                    else if (isCrossAuto)
                    {
                        crossChildSize = isVertical ? intrinsicWidth : intrinsicHeight;
                        crossChildSize = Math.Min(crossChildSize, crossAxisSize);
                    }
                    else
                    {
                        // For explicit sizes (e.g. "80%", "30ch"), always use ParseSize.
                        // Do NOT prefer ComputedWidth/Height here — those may be stale
                        // from ResolveAutoSize running with different parent dimensions.
                        crossChildSize = isVertical
                            ? ParseSize(child.Width, contentWidth)
                            : ParseSize(child.Height, contentHeight);
                        crossChildSize = Math.Min(crossChildSize, crossAxisSize);
                    }

                    // Apply min/max constraints to layout sizes
                    if (isVertical)
                    {
                        if (!isFill)
                        {
                            if (!string.IsNullOrEmpty(child.MinHeight))
                                mainChildSize = Math.Max(mainChildSize, ParseSize(child.MinHeight, contentHeight));
                            if (!string.IsNullOrEmpty(child.MaxHeight))
                                mainChildSize = Math.Min(mainChildSize, ParseSize(child.MaxHeight, contentHeight));
                        }
                        if (!string.IsNullOrEmpty(child.MinWidth))
                            crossChildSize = Math.Max(crossChildSize, ParseSize(child.MinWidth, contentWidth));
                        if (!string.IsNullOrEmpty(child.MaxWidth))
                            crossChildSize = Math.Min(crossChildSize, ParseSize(child.MaxWidth, contentWidth));
                    }
                    else
                    {
                        if (!isFill)
                        {
                            if (!string.IsNullOrEmpty(child.MinWidth))
                                mainChildSize = Math.Max(mainChildSize, ParseSize(child.MinWidth, contentWidth));
                            if (!string.IsNullOrEmpty(child.MaxWidth))
                                mainChildSize = Math.Min(mainChildSize, ParseSize(child.MaxWidth, contentWidth));
                        }
                        if (!string.IsNullOrEmpty(child.MinHeight))
                            crossChildSize = Math.Max(crossChildSize, ParseSize(child.MinHeight, contentHeight));
                        if (!string.IsNullOrEmpty(child.MaxHeight))
                            crossChildSize = Math.Min(crossChildSize, ParseSize(child.MaxHeight, contentHeight));
                    }

                    if (!isFill)
                        usedMainSpace += mMain0 + mainChildSize + mMain1;

                    measured.Add((child, mainChildSize, crossChildSize, mMain0, mMain1, mCross0, mCross1));
                    childIndex++;
                }

                // Pass 2: distribute remaining space to fill children
                if (fillCount > 0)
                {
                    int remaining = Math.Max(0, mainAxisSize - usedMainSpace);
                    int perFill = remaining / fillCount;
                    int extra = remaining % fillCount;

                    for (int fi = 0; fi < fillIndices.Count; fi++)
                    {
                        int idx = fillIndices[fi];
                        var m = measured[idx];
                        int fillSize = perFill + (fi < extra ? 1 : 0);

                        // Apply min/max constraints to fill size
                        if (isVertical)
                        {
                            if (!string.IsNullOrEmpty(m.child.MinHeight))
                                fillSize = Math.Max(fillSize, ParseSize(m.child.MinHeight, contentHeight));
                            if (!string.IsNullOrEmpty(m.child.MaxHeight))
                                fillSize = Math.Min(fillSize, ParseSize(m.child.MaxHeight, contentHeight));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(m.child.MinWidth))
                                fillSize = Math.Max(fillSize, ParseSize(m.child.MinWidth, contentWidth));
                            if (!string.IsNullOrEmpty(m.child.MaxWidth))
                                fillSize = Math.Min(fillSize, ParseSize(m.child.MaxWidth, contentWidth));
                        }

                        measured[idx] = (m.child, fillSize, m.crossSize, m.mMain0, m.mMain1, m.mCross0, m.mCross1);
                    }
                }

                // Store computed sizes for all children
                for (int i = 0; i < measured.Count; i++)
                {
                    var m = measured[i];
                    if (isVertical)
                    {
                        m.child.ComputedHeight = m.mainSize;
                        m.child.ComputedWidth = m.crossSize;
                    }
                    else
                    {
                        m.child.ComputedWidth = m.mainSize;
                        m.child.ComputedHeight = m.crossSize;
                    }
                }

                // Split into lines (wrap) or single line (no wrap)
                var lines = new List<List<int>>(); // indices into measured
                var lineMainSizes = new List<int>();
                var lineCrossSizes = new List<int>();

                var currentLine = new List<int>();
                int currentMainUsed = 0;
                int currentMaxCross = 0;

                for (int i = 0; i < measured.Count; i++)
                {
                    var m = measured[i];
                    int totalMain = m.mMain0 + m.mainSize + m.mMain1;
                    int totalCross = m.mCross0 + m.crossSize + m.mCross1;

                    if (wrap && currentLine.Count > 0 && currentMainUsed + totalMain > mainAxisSize)
                    {
                        // Finish current line, start new one
                        lines.Add(currentLine);
                        lineMainSizes.Add(currentMainUsed);
                        lineCrossSizes.Add(currentMaxCross);

                        currentLine = new List<int>();
                        currentMainUsed = 0;
                        currentMaxCross = 0;
                    }

                    currentLine.Add(i);
                    currentMainUsed += totalMain;
                    if (totalCross > currentMaxCross) currentMaxCross = totalCross;
                }

                if (currentLine.Count > 0)
                {
                    lines.Add(currentLine);
                    lineMainSizes.Add(currentMainUsed);
                    lineCrossSizes.Add(currentMaxCross);
                }

                // Position each line
                int crossOffset = 0;
                for (int lineIdx = 0; lineIdx < lines.Count; lineIdx++)
                {
                    var line = lines[lineIdx];
                    int lineMainUsed = lineMainSizes[lineIdx];
                    int lineMaxCross = lineCrossSizes[lineIdx];
                    int lineCount = line.Count;

                    // Calculate justify spacing for this line
                    int freeSpace = Math.Max(0, mainAxisSize - lineMainUsed);
                    int startOff = 0;
                    int spacing = 0;
                    int remainder = 0;

                    switch (justify)
                    {
                        case Widgets.StackJustify.Start:
                            break;
                        case Widgets.StackJustify.End:
                            startOff = freeSpace;
                            break;
                        case Widgets.StackJustify.Center:
                            startOff = freeSpace / 2;
                            break;
                        case Widgets.StackJustify.SpaceBetween:
                            if (lineCount > 1)
                            {
                                spacing = freeSpace / (lineCount - 1);
                                remainder = freeSpace % (lineCount - 1);
                            }
                            break;
                        case Widgets.StackJustify.SpaceAround:
                            if (lineCount > 0)
                            {
                                spacing = freeSpace / lineCount;
                                remainder = freeSpace % lineCount;
                                startOff = spacing / 2;
                            }
                            break;
                        case Widgets.StackJustify.SpaceEvenly:
                            if (lineCount > 0)
                            {
                                spacing = freeSpace / (lineCount + 1);
                                remainder = freeSpace % (lineCount + 1);
                                startOff = spacing;
                            }
                            break;
                    }

                    // Position children in this line
                    int mainOffset = startOff;
                    for (int li = 0; li < line.Count; li++)
                    {
                        var m = measured[line[li]];
                        var child = m.child;

                        if (isVertical)
                        {
                            child.PositionY = $"{mainOffset}ch";
                            mainOffset += m.mMain0 + m.mainSize + m.mMain1;

                            // Cross-axis (X) position
                            int crossAvail = wrap ? lineMaxCross : crossAxisSize;
                            int crossFree = Math.Max(0, crossAvail - m.crossSize - m.mCross0 - m.mCross1);
                            int crossPos = align switch
                            {
                                Widgets.StackAlign.Center => m.mCross0 + (crossFree / 2),
                                Widgets.StackAlign.End => m.mCross0 + crossFree,
                                _ => 0
                            };
                            child.PositionX = $"{crossOffset + crossPos}ch";
                        }
                        else
                        {
                            child.PositionX = $"{mainOffset}ch";
                            mainOffset += m.mMain0 + m.mainSize + m.mMain1;

                            // Cross-axis (Y) position
                            int crossAvail = wrap ? lineMaxCross : crossAxisSize;
                            int crossFree = Math.Max(0, crossAvail - m.crossSize - m.mCross0 - m.mCross1);
                            int crossPos = align switch
                            {
                                Widgets.StackAlign.Center => m.mCross0 + (crossFree / 2),
                                Widgets.StackAlign.End => m.mCross0 + crossFree,
                                _ => 0
                            };
                            child.PositionY = $"{crossOffset + crossPos}ch";
                        }

                        // Justify spacing after each child
                        if (justify != Widgets.StackJustify.Start && justify != Widgets.StackJustify.End && justify != Widgets.StackJustify.Center)
                        {
                            bool isLast = li == line.Count - 1;
                            if (justify == Widgets.StackJustify.SpaceBetween && isLast)
                                continue;

                            mainOffset += spacing;
                            if (remainder > 0)
                            {
                                mainOffset++;
                                remainder--;
                            }
                        }
                    }

                    crossOffset += lineMaxCross;
                }
            }

            // Check if scrollbars will be rendered (need to check before rendering children)
            bool willRenderVerticalScrollbar = false;
            bool willRenderHorizontalScrollbar = false;

            if ((widget.ScrollX || widget.ScrollY) && widget.Children.Count > 0)
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

                // For wrapping StackPanels, only allow overflow on the cross axis:
                // - Horizontal wrap: content wraps down (vertical overflow only)
                // - Vertical wrap: content wraps right (horizontal overflow only)
                bool suppressHorizontal = false;
                bool suppressVertical = false;
                if (widget is Widgets.StackPanel sp && sp.Wrap)
                {
                    if (sp.Direction == Widgets.StackDirection.Horizontal)
                        suppressHorizontal = true; // wrapping prevents horizontal overflow
                    else
                        suppressVertical = true; // wrapping prevents vertical overflow
                }

                willRenderVerticalScrollbar = widget.ScrollY && !suppressVertical && contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0);
                willRenderHorizontalScrollbar = widget.ScrollX && !suppressHorizontal && contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0);
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
                RenderWidget(output, fgColors, bgColors, styleBuffer, child, childClipX, childClipY, childClipWidth, childClipHeight, childScrollX, childScrollY, hitTestMap, focusVisible, resolvedBg, resolvedFg);
            }

            // Render scrollbars AFTER children so they're always on top
            if ((widget.ScrollX || widget.ScrollY) && widget.Children.Count > 0)
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

                // For wrapping StackPanels, only allow overflow on the cross axis
                bool renderSuppressH = false;
                bool renderSuppressV = false;
                if (widget is Widgets.StackPanel spRender && spRender.Wrap)
                {
                    if (spRender.Direction == Widgets.StackDirection.Horizontal)
                        renderSuppressH = true;
                    else
                        renderSuppressV = true;
                }

                if (widget.ScrollY && !renderSuppressV && contentWidth > 0 && (maxChildBottom > contentHeight || scrollY > 0))
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

                if (widget.ScrollX && !renderSuppressH && contentHeight > 0 && (maxChildRight > contentWidth || scrollX > 0))
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

        /// <summary>
        /// Recursively resolves auto-sized dimensions for a StackPanel by measuring its children.
        /// Returns (width, height) where 0 means "not auto / use ParseSize result".
        /// </summary>
        private static (int width, int height) ResolveAutoSize(Widgets.StackPanel stack, int parentWidth, int parentHeight)
        {
            bool autoW = stack.Width.Equals("auto", StringComparison.OrdinalIgnoreCase);
            bool autoH = stack.Height.Equals("auto", StringComparison.OrdinalIgnoreCase);

            if (!autoW && !autoH)
            {
                return (0, 0);
            }

            bool isVert = stack.Direction == Widgets.StackDirection.Vertical;
            bool doWrap = stack.Wrap;

            // Compute effective available cross-axis size for children,
            // respecting MaxWidth/MaxHeight, border, and padding on this stack.
            int availCrossWidth = parentWidth;
            int availCrossHeight = parentHeight;
            if (!string.IsNullOrEmpty(stack.MaxWidth))
                availCrossWidth = Math.Min(availCrossWidth, ParseSize(stack.MaxWidth, parentWidth));
            if (!string.IsNullOrEmpty(stack.MaxHeight))
                availCrossHeight = Math.Min(availCrossHeight, ParseSize(stack.MaxHeight, parentHeight));
            int borderSize = stack.HasBorder ? 2 : 0;
            int innerWidth = availCrossWidth - borderSize - ParseSize(stack.PaddingLeft, 0) - ParseSize(stack.PaddingRight, 0);
            int innerHeight = availCrossHeight - borderSize - ParseSize(stack.PaddingTop, 0) - ParseSize(stack.PaddingBottom, 0);
            if (innerWidth < 0) innerWidth = 0;
            if (innerHeight < 0) innerHeight = 0;

            // Measure all visible children
            var items = new List<(int mainSize, int crossSize)>();
            foreach (var child in ((IWidget)stack).Children)
            {
                if (!child.Visible) continue;

                // Pre-set cross-axis ComputedWidth/Height only when the child has a
                // %-based size (needs parent as reference) or a MaxWidth/MaxHeight constraint.
                // Do NOT set for auto, fill, or fixed ch — those self-size from content.
                if (isVert)
                {
                    bool needsParentW = (!string.IsNullOrEmpty(child.Width) && child.Width.TrimEnd().EndsWith('%'))
                        || !string.IsNullOrEmpty(child.MaxWidth);
                    if (needsParentW)
                    {
                        int childCross = innerWidth;
                        if (!string.IsNullOrEmpty(child.MaxWidth))
                            childCross = Math.Min(childCross, ParseSize(child.MaxWidth, innerWidth));
                        if (childCross > 0) child.ComputedWidth = childCross;
                    }
                }
                else
                {
                    bool needsParentH = (!string.IsNullOrEmpty(child.Height) && child.Height.TrimEnd().EndsWith('%'))
                        || !string.IsNullOrEmpty(child.MaxHeight);
                    if (needsParentH)
                    {
                        int childCross = innerHeight;
                        if (!string.IsNullOrEmpty(child.MaxHeight))
                            childCross = Math.Min(childCross, ParseSize(child.MaxHeight, innerHeight));
                        if (childCross > 0) child.ComputedHeight = childCross;
                    }
                }

                // Recursively resolve nested auto-sized StackPanels first
                if (child is Widgets.StackPanel childStack)
                {
                    var (cAutoW, cAutoH) = ResolveAutoSize(childStack, innerWidth, innerHeight);
                    if (cAutoW > 0) child.ComputedWidth = cAutoW;
                    if (cAutoH > 0) child.ComputedHeight = cAutoH;
                }

                // Measure the child's actual rendered dimensions from GetRaw()
                var childRaw = child.GetRaw();
                int cw = 0, ch = 0;
                if (childRaw != null && childRaw.Length > 0)
                {
                    ch = childRaw.Length;
                    if (child is Widgets.Text)
                    {
                        // For Text: measure actual content width (excluding trailing spaces)
                        // so auto-sized parent StackPanels can shrink-wrap around short text.
                        for (int i = 0; i < childRaw.Length; i++)
                        {
                            if (childRaw[i] == null) continue;
                            int lineW = childRaw[i].Length;
                            while (lineW > 0 && childRaw[i][lineW - 1].Value == ' ')
                                lineW--;
                            if (lineW > cw) cw = lineW;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < childRaw.Length; i++)
                        {
                            if (childRaw[i] != null && childRaw[i].Length > cw)
                                cw = childRaw[i].Length;
                        }
                    }
                }
                if (cw == 0)
                    cw = child.ComputedWidth > 0 ? child.ComputedWidth : ParseSize(child.Width, innerWidth);
                if (ch == 0)
                    ch = child.ComputedHeight > 0 ? child.ComputedHeight : ParseSize(child.Height, innerHeight);

                // Apply min/max constraints
                if (!string.IsNullOrEmpty(child.MinWidth))
                    cw = Math.Max(cw, ParseSize(child.MinWidth, innerWidth));
                if (!string.IsNullOrEmpty(child.MaxWidth))
                    cw = Math.Min(cw, ParseSize(child.MaxWidth, innerWidth));
                if (!string.IsNullOrEmpty(child.MinHeight))
                    ch = Math.Max(ch, ParseSize(child.MinHeight, innerHeight));
                if (!string.IsNullOrEmpty(child.MaxHeight))
                    ch = Math.Min(ch, ParseSize(child.MaxHeight, innerHeight));

                child.ComputedWidth = cw;
                child.ComputedHeight = ch;

                int mLeft = ParseSize(child.MarginLeft, 0);
                int mTop = ParseSize(child.MarginTop, 0);
                int mRight = ParseSize(child.MarginRight, 0);
                int mBottom = ParseSize(child.MarginBottom, 0);

                int mainTotal = isVert ? (mTop + ch + mBottom) : (mLeft + cw + mRight);
                int crossTotal = isVert ? (cw + mLeft + mRight) : (ch + mTop + mBottom);
                items.Add((mainTotal, crossTotal));
            }

            int border = stack.HasBorder ? 2 : 0;
            int padH = ParseSize(stack.PaddingLeft, 0) + ParseSize(stack.PaddingRight, 0);
            int padV = ParseSize(stack.PaddingTop, 0) + ParseSize(stack.PaddingBottom, 0);

            int totalStack = 0;
            int maxCross = 0;

            if (doWrap)
            {
                // For wrap: need the fixed main-axis size to know where to break
                int mainAxisLimit = isVert
                    ? (autoH ? int.MaxValue : ParseSize(stack.Height, parentHeight) - border - padV)
                    : (autoW ? int.MaxValue : ParseSize(stack.Width, parentWidth) - border - padH);

                int lineMain = 0;
                int lineMaxCross = 0;
                int maxLineMain = 0;
                int totalCross = 0;

                foreach (var item in items)
                {
                    if (lineMain > 0 && lineMain + item.mainSize > mainAxisLimit)
                    {
                        // Finish line
                        if (lineMain > maxLineMain) maxLineMain = lineMain;
                        totalCross += lineMaxCross;
                        lineMain = 0;
                        lineMaxCross = 0;
                    }

                    lineMain += item.mainSize;
                    if (item.crossSize > lineMaxCross) lineMaxCross = item.crossSize;
                }

                // Last line
                if (lineMain > maxLineMain) maxLineMain = lineMain;
                totalCross += lineMaxCross;

                totalStack = maxLineMain;
                maxCross = totalCross;
            }
            else
            {
                // No wrap: simple sum/max
                foreach (var item in items)
                {
                    totalStack += item.mainSize;
                    if (item.crossSize > maxCross) maxCross = item.crossSize;
                }
            }

            int width = 0;
            int height = 0;

            if (isVert)
            {
                if (autoH) height = totalStack + border + padV;
                if (autoW) width = maxCross + border + padH;
            }
            else
            {
                if (autoW) width = totalStack + border + padH;
                if (autoH) height = maxCross + border + padV;
            }

            // Store computed values so GetRaw() and external code can use them
            ((IWidget)stack).ComputedWidth = width > 0 ? width : ParseSize(stack.Width, parentWidth);
            ((IWidget)stack).ComputedHeight = height > 0 ? height : ParseSize(stack.Height, parentHeight);

            return (width, height);
        }

        /// <summary>
        /// Recursively measures the main-axis size of a widget's children.
        /// Propagates MaxWidth/MaxHeight constraints so nested Text widgets
        /// can calculate wrapping correctly during GetRaw().
        /// </summary>
        private static int MeasureChildrenMainAxis(IWidget parent, bool parentIsVertical, int contentWidth, int contentHeight, int crossAxisSize)
        {
            int intrinsic = 0;
            bool childIsHoriz = parent is Widgets.StackPanel sp2 && sp2.Direction == Widgets.StackDirection.Horizontal;

            // Determine effective cross-axis size for children (respecting parent MaxWidth/MaxHeight)
            int childCrossSize = crossAxisSize;
            if (parentIsVertical && !string.IsNullOrEmpty(parent.MaxWidth))
                childCrossSize = Math.Min(ParseSize(parent.MaxWidth, contentWidth), crossAxisSize);
            else if (!parentIsVertical && !string.IsNullOrEmpty(parent.MaxHeight))
                childCrossSize = Math.Min(ParseSize(parent.MaxHeight, contentHeight), crossAxisSize);

            // Account for border + padding reducing available space
            if (parent is Widgets.Container containerParent && containerParent.HasBorder)
                childCrossSize -= 2;
            int padCross = parentIsVertical
                ? ParseSize(parent.PaddingLeft, 0) + ParseSize(parent.PaddingRight, 0)
                : ParseSize(parent.PaddingTop, 0) + ParseSize(parent.PaddingBottom, 0);
            childCrossSize -= padCross;
            if (childCrossSize < 0) childCrossSize = 0;

            // Set parent's ComputedWidth/Height so children querying parent size
            // (e.g. Text with Width="100%" calling CalculateSize) get the correct value.
            if (parentIsVertical && childCrossSize > 0)
                parent.ComputedWidth = childCrossSize;
            else if (!parentIsVertical && childCrossSize > 0)
                parent.ComputedHeight = childCrossSize;

            foreach (var gc in parent.Children)
            {
                if (!gc.Visible) continue;

                // Pre-set ComputedWidth/Height so nested GetRaw() can wrap correctly.
                // Respect MaxWidth/MaxHeight constraints on the child.
                int gcCross = childCrossSize;
                if (parentIsVertical && !string.IsNullOrEmpty(gc.MaxWidth))
                    gcCross = Math.Min(gcCross, ParseSize(gc.MaxWidth, childCrossSize));
                else if (!parentIsVertical && !string.IsNullOrEmpty(gc.MaxHeight))
                    gcCross = Math.Min(gcCross, ParseSize(gc.MaxHeight, childCrossSize));
                if (parentIsVertical && gcCross > 0)
                    gc.ComputedWidth = gcCross;
                else if (!parentIsVertical && gcCross > 0)
                    gc.ComputedHeight = gcCross;

                // Clear stale main-axis size for auto-sized containers WITH a cross-axis
                // constraint (MaxWidth/MaxHeight) so GetRaw() returns [] and we measure
                // recursively with the correct cross-axis width.
                string gcMainStr = parentIsVertical ? gc.Height : gc.Width;
                bool gcMainIsAuto = string.IsNullOrEmpty(gcMainStr) || gcMainStr.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase);
                bool gcHasCrossConstraint = parentIsVertical ? !string.IsNullOrEmpty(gc.MaxWidth) : !string.IsNullOrEmpty(gc.MaxHeight);
                if (gcMainIsAuto && gc is Widgets.StackPanel && gcHasCrossConstraint)
                {
                    if (parentIsVertical) gc.ComputedHeight = 0;
                    else gc.ComputedWidth = 0;
                }

                var gcRaw = gc.GetRaw();

                // In an auto-sized parent, use intrinsic size from GetRaw() as the
                // primary measurement. The declared size may be a static initial value
                // (e.g. XmlParser sets "1ch" for single-line text) that becomes stale
                // after wrapping expands the widget. Percentage sizes are meaningless
                // in auto-sized contexts and are skipped entirely.
                int gcMainSize = 0;
                if (gcRaw.Length > 0)
                {
                    gcMainSize = parentIsVertical ? gcRaw.Length : (gcRaw[0].Length);
                }
                if (gcMainSize == 0)
                {
                    gcMainSize = parentIsVertical ? gc.ComputedHeight : gc.ComputedWidth;
                }
                if (gcMainSize == 0)
                {
                    string gcSize = parentIsVertical ? gc.Height : gc.Width;
                    if (!gcSize.TrimEnd().EndsWith('%'))
                    {
                        gcMainSize = ParseSize(gcSize, parentIsVertical ? contentHeight : contentWidth);
                    }
                }

                // If still 0 and has children, measure recursively
                if (gcMainSize == 0 && gc.Children.Count > 0)
                    gcMainSize = MeasureChildrenMainAxis(gc, parentIsVertical, contentWidth, contentHeight, gcCross);

                // Add border + padding of the child container itself on the main axis.
                // MeasureChildrenMainAxis and recursive calls measure inner content only;
                // the container's own border/padding must be added on top.
                if (gcMainSize > 0 && gcRaw.Length == 0 && gc is Widgets.Container gcContainer)
                {
                    if (gcContainer.HasBorder)
                        gcMainSize += 2;
                    gcMainSize += parentIsVertical
                        ? ParseSize(gc.PaddingTop, 0) + ParseSize(gc.PaddingBottom, 0)
                        : ParseSize(gc.PaddingLeft, 0) + ParseSize(gc.PaddingRight, 0);
                }

                // Account for margins on the main axis (same as layout pass)
                int gcMMain0 = parentIsVertical ? ParseSize(gc.MarginTop, contentHeight) : ParseSize(gc.MarginLeft, contentWidth);
                int gcMMain1 = parentIsVertical ? ParseSize(gc.MarginBottom, contentHeight) : ParseSize(gc.MarginRight, contentWidth);

                if (childIsHoriz == !parentIsVertical)
                    intrinsic += gcMMain0 + gcMainSize + gcMMain1;
                else
                    intrinsic = Math.Max(intrinsic, gcMMain0 + gcMainSize + gcMMain1);
            }

            return intrinsic;
        }

        private static int ParseSize(string size, int parentSize)
        {
            if (string.IsNullOrEmpty(size))
            {
                return 0;
            }

            size = size.Trim();

            if (size.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (size.Equals("fill", StringComparison.OrdinalIgnoreCase))
            {
                // In non-StackPanel contexts, fill behaves like 100%.
                // StackPanel layout overrides this with the actual remaining space.
                return parentSize;
            }

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

            throw new FormatException($"Invalid size value: '{size}'. Size must end with 'ch', '%', or be 'auto'/'fill' (e.g., '10ch', '50%', 'auto', 'fill')");
        }

    }
}
