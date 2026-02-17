using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class DirectoryTree
{
    private readonly TermuiXLib _termui;
    private TreeView? _treeView;
    private Container? _scrollContainer;
    private string _currentDirectory;

    private long _lastGoodScroll;

    public event EventHandler<string>? DirectorySelected;

    public DirectoryTree(TermuiXLib termui)
    {
        _termui = termui;
        _currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public string BuildXml()
    {
        return @"
<StackPanel Name='treePanel' Direction='Vertical' Width='28ch' Height='100%' BackgroundColor='#141414'>
    <Text Width='100%' Height='1ch' BackgroundColor='#141414' ForegroundColor='#808080'
        PaddingLeft='1ch' Style='Bold'>Explorer</Text>
    <Container Name='treeScroll' Width='28ch' Height='fill' ScrollY='true' BackgroundColor='#141414'>
        <TreeView Name='dirTree' Width='28ch' Height='auto'
            BackgroundColor='#141414' ForegroundColor='#909090' />
    </Container>
</StackPanel>";
    }

    public void Initialize()
    {
        _treeView = _termui.GetWidget<TreeView>("dirTree");
        _scrollContainer = _termui.GetWidget<Container>("treeScroll");
        if (_treeView is null) return;

        // Set colors programmatically (could also use XML attributes)
        _treeView.BackgroundColor = global::TermuiX.Color.Parse("#141414");
        _treeView.ForegroundColor = global::TermuiX.Color.Parse("#909090");
        _treeView.FocusBackgroundColor = global::TermuiX.Color.Parse("#141414");
        _treeView.FocusForegroundColor = global::TermuiX.Color.Parse("#909090");
        _treeView.HighlightBackgroundColor = global::TermuiX.Color.Parse("#1a3050");
        _treeView.HighlightForegroundColor = global::TermuiX.Color.Parse("#c0c0c0");

        _treeView.NodeSelected += OnNodeSelected;
        _treeView.NodeToggled += OnNodeToggled;

        PopulateRoot();
    }

    private void PopulateRoot()
    {
        if (_treeView is null) return;

        // Add home directory as the primary root
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var homeNode = _treeView.Root.AddChild($"🏠 {Path.GetFileName(homePath)}");
        homeNode.Tag = homePath;
        LoadChildren(homeNode);
        homeNode.IsExpanded = true;

        // Add filesystem root
        var rootNode = _treeView.Root.AddChild("💽 /");
        rootNode.Tag = "/";
        AddPlaceholder(rootNode);
    }

    private void LoadChildren(TreeNode parent)
    {
        var path = parent.Tag as string;
        if (path is null) return;

        // Remove existing children (placeholder or old)
        while (parent.Children.Count > 0)
            parent.RemoveChild(parent.Children[0]);

        try
        {
            var dirs = Directory.GetDirectories(path)
                .Select(d => new DirectoryInfo(d))
                .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
                .OrderBy(d => d.Name)
                .ToList();

            foreach (var dir in dirs)
            {
                var node = parent.AddChild($"📁 {dir.Name}");
                node.Tag = dir.FullName;

                // Add placeholder if directory has subdirectories (for expand arrow)
                try
                {
                    if (Directory.GetDirectories(dir.FullName).Length > 0)
                        AddPlaceholder(node);
                }
                catch
                {
                    // Permission denied — no expand arrow
                }
            }
        }
        catch
        {
            var errorNode = parent.AddChild("⚠ Access denied");
            errorNode.Tag = null;
        }
    }

    private static void AddPlaceholder(TreeNode node)
    {
        var placeholder = node.AddChild("...");
        placeholder.Tag = "__placeholder__";
    }

    private void OnNodeSelected(object? sender, TreeNode node)
    {
        var path = node.Tag as string;
        if (path is null or "__placeholder__") return;

        _currentDirectory = path;
        DirectorySelected?.Invoke(this, path);
    }

    private void OnNodeToggled(object? sender, TreeNode node)
    {
        var path = node.Tag as string;
        if (path is null or "__placeholder__") return;

        if (node.IsExpanded)
        {
            // Check if children are just placeholder
            if (node.Children.Count == 1 && node.Children[0].Tag as string == "__placeholder__")
                LoadChildren(node);
        }
    }

    public void SelectDirectory(string path)
    {
        _currentDirectory = path;
    }

    /// <summary>
    /// Expands the tree to the given directory path and selects it.
    /// </summary>
    public void ExpandToPath(string targetPath)
    {
        if (_treeView is null) return;

        targetPath = Path.GetFullPath(targetPath);
        _currentDirectory = targetPath;

        // Determine which root node to start from
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        TreeNode? startNode = null;
        string startPath;

        if (targetPath.StartsWith(homePath, StringComparison.Ordinal))
        {
            // Under home — use the home node (first root child)
            startNode = _treeView.Root.Children.Count > 0 ? _treeView.Root.Children[0] : null;
            startPath = homePath;
        }
        else
        {
            // Under / — use the root filesystem node (second root child)
            startNode = _treeView.Root.Children.Count > 1 ? _treeView.Root.Children[1] : null;
            startPath = "/";

            // Ensure / node is loaded
            if (startNode is not null && startNode.Children.Count == 1
                && startNode.Children[0].Tag as string == "__placeholder__")
            {
                LoadChildren(startNode);
            }
            startNode!.IsExpanded = true;
        }

        if (startNode is null) return;

        // Already at target?
        if (string.Equals(targetPath, startPath, StringComparison.Ordinal))
        {
            _treeView.SelectNode(startNode);
            return;
        }

        // Get relative segments from startPath to targetPath
        var relativePath = Path.GetRelativePath(startPath, targetPath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var currentNode = startNode;
        foreach (var segment in segments)
        {
            // Ensure children are loaded
            if (currentNode.Children.Count == 1
                && currentNode.Children[0].Tag as string == "__placeholder__")
            {
                LoadChildren(currentNode);
            }

            currentNode.IsExpanded = true;

            // Find matching child by directory name
            TreeNode? match = null;
            foreach (var child in currentNode.Children)
            {
                if (child.Tag is string childPath)
                {
                    var childName = Path.GetFileName(childPath);
                    if (string.Equals(childName, segment, StringComparison.Ordinal))
                    {
                        match = child;
                        break;
                    }
                }
            }

            if (match is null) break; // Path segment not found (hidden dir, permission denied, etc.)
            currentNode = match;
        }

        // Select the deepest node we reached
        _treeView.SelectNode(currentNode);
    }

    public TreeView? GetTreeView() => _treeView;
    public Container? GetScrollContainer() => _scrollContainer;

    /// <summary>
    /// Call every frame in the render loop. Detects spurious scroll resets to 0
    /// (caused by ScrollToWidget using TreeView.PositionY="0ch") and reverts them.
    /// Legitimate scrolls to 0 are allowed when the previous position was already near 0.
    /// </summary>
    public void GuardScrollPosition()
    {
        if (_scrollContainer is null) return;

        long current = ((IWidget)_scrollContainer).ScrollOffsetY;

        if (current == 0 && _lastGoodScroll > 3)
        {
            // Spurious reset — restore previous good value
            ((IWidget)_scrollContainer).ScrollOffsetY = _lastGoodScroll;
        }
        else
        {
            // Legitimate scroll — remember it
            _lastGoodScroll = current;
        }
    }
}
