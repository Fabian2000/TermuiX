using TermuiX.Widgets;
using TermuiXLib = TermuiX.TermuiX;

namespace TermuiX.FileManager2.Components;

public class PreviewPanel
{
    private readonly TermuiXLib _termui;
    private Container? _container;
    private Text? _titleText;
    private Text? _contentText;
    private Line? _separator;

    // Extension → syntax language for fenced code blocks (rendered as markdown)
    private static readonly Dictionary<string, string> CodeLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        // C# / .NET
        [".cs"] = "csharp", [".csx"] = "csharp", [".fs"] = "fsharp", [".fsx"] = "fsharp",
        [".vb"] = "vb", [".razor"] = "razor", [".cshtml"] = "razor", [".axaml"] = "xml",
        [".xaml"] = "xml", [".csproj"] = "xml", [".fsproj"] = "xml", [".vbproj"] = "xml",
        [".props"] = "xml", [".targets"] = "xml", [".nuspec"] = "xml", [".sln"] = "text",
        // JavaScript / TypeScript / Web
        [".js"] = "javascript", [".mjs"] = "javascript", [".cjs"] = "javascript",
        [".ts"] = "typescript", [".mts"] = "typescript", [".cts"] = "typescript",
        [".jsx"] = "jsx", [".tsx"] = "tsx",
        [".vue"] = "vue", [".svelte"] = "svelte", [".astro"] = "astro",
        // Python
        [".py"] = "python", [".pyw"] = "python", [".pyi"] = "python", [".pyx"] = "python",
        // Ruby
        [".rb"] = "ruby", [".erb"] = "ruby", [".rake"] = "ruby", [".gemspec"] = "ruby",
        // Go / Rust / Zig
        [".go"] = "go", [".rs"] = "rust", [".zig"] = "zig",
        // C / C++
        [".c"] = "c", [".h"] = "c", [".cpp"] = "cpp", [".hpp"] = "cpp", [".cc"] = "cpp",
        [".cxx"] = "cpp", [".hxx"] = "cpp", [".hh"] = "cpp", [".inl"] = "cpp",
        // Java / Kotlin / Scala / Groovy
        [".java"] = "java", [".kt"] = "kotlin", [".kts"] = "kotlin",
        [".scala"] = "scala", [".sc"] = "scala", [".groovy"] = "groovy", [".gradle"] = "groovy",
        // Swift / Objective-C
        [".swift"] = "swift", [".m"] = "objectivec", [".mm"] = "objectivec",
        // Shell
        [".sh"] = "bash", [".bash"] = "bash", [".zsh"] = "bash", [".fish"] = "fish",
        [".ps1"] = "powershell", [".psm1"] = "powershell", [".psd1"] = "powershell",
        [".bat"] = "batch", [".cmd"] = "batch",
        // Markup / Config
        [".json"] = "json", [".jsonc"] = "json", [".json5"] = "json",
        [".xml"] = "xml", [".xsl"] = "xml", [".xsd"] = "xml", [".svg"] = "xml", [".plist"] = "xml",
        [".html"] = "html", [".htm"] = "html", [".xhtml"] = "html",
        [".css"] = "css", [".scss"] = "scss", [".less"] = "less", [".sass"] = "sass",
        [".yml"] = "yaml", [".yaml"] = "yaml", [".toml"] = "toml",
        // Data / Query
        [".sql"] = "sql", [".graphql"] = "graphql", [".gql"] = "graphql",
        [".proto"] = "protobuf",
        // PHP / Perl / Lua / Ruby
        [".php"] = "php", [".lua"] = "lua",
        [".pl"] = "perl", [".pm"] = "perl", [".t"] = "perl",
        // R / Julia / Matlab
        [".r"] = "r", [".R"] = "r", [".jl"] = "julia", [".mat"] = "matlab",
        // Haskell / Elixir / Erlang / Clojure / Lisp
        [".hs"] = "haskell", [".lhs"] = "haskell",
        [".ex"] = "elixir", [".exs"] = "elixir", [".erl"] = "erlang", [".hrl"] = "erlang",
        [".clj"] = "clojure", [".cljs"] = "clojure", [".cljc"] = "clojure",
        [".el"] = "lisp", [".lisp"] = "lisp", [".scm"] = "scheme",
        // Dart / V / Nim / OCaml
        [".dart"] = "dart", [".v"] = "v", [".nim"] = "nim",
        [".ml"] = "ocaml", [".mli"] = "ocaml",
        // LaTeX / TeX
        [".tex"] = "latex", [".sty"] = "latex", [".cls"] = "latex", [".bst"] = "latex",
        // Build / DevOps
        [".cmake"] = "cmake", [".makefile"] = "makefile",
        [".dockerfile"] = "dockerfile", [".tf"] = "hcl", [".hcl"] = "hcl",
        [".nix"] = "nix", [".dhall"] = "dhall",
        // Misc
        [".diff"] = "diff", [".patch"] = "diff",
        [".asm"] = "asm", [".s"] = "asm",
        [".glsl"] = "glsl", [".hlsl"] = "hlsl", [".wgsl"] = "wgsl",
        [".sol"] = "solidity", [".vy"] = "python",
    };

    // Plain text extensions — shown without code wrapping
    private static readonly HashSet<string> PlainTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".csv", ".tsv", ".ini", ".cfg", ".conf", ".env",
        ".gitignore", ".gitattributes", ".gitmodules",
        ".editorconfig", ".dockerignore", ".npmignore", ".eslintignore",
        ".rst", ".adoc", ".bib", ".org",
        ".properties", ".lock",
    };

    // Well-known filenames without extensions that map to a code language
    private static readonly Dictionary<string, string> SpecialFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dockerfile"] = "dockerfile", ["Makefile"] = "makefile", ["CMakeLists.txt"] = "cmake",
        ["Rakefile"] = "ruby", ["Gemfile"] = "ruby", ["Vagrantfile"] = "ruby",
        ["Jenkinsfile"] = "groovy", ["Justfile"] = "makefile",
        [".bashrc"] = "bash", [".zshrc"] = "bash", [".profile"] = "bash",
        [".bash_profile"] = "bash", [".bash_aliases"] = "bash",
    };

    private static readonly HashSet<string> MarkdownExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".markdown", ".mdown", ".mkd",
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".svg", ".webp", ".tiff", ".tif",
    };

    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".so", ".dylib", ".o", ".a", ".lib",
        ".zip", ".tar", ".gz", ".bz2", ".xz", ".7z", ".rar",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".mp3", ".mp4", ".avi", ".mkv", ".mov", ".wav", ".flac", ".ogg",
        ".bin", ".dat", ".iso", ".img", ".dmg",
        ".class", ".pyc", ".pyo", ".wasm",
        ".snupkg", ".nupkg",
    };

    public PreviewPanel(TermuiXLib termui)
    {
        _termui = termui;
    }

    public string BuildXml()
    {
        return @"
<Line Name='previewSep' Orientation='Vertical' Type='Solid' Height='fill'
    BackgroundColor='#191919' ForegroundColor='#333333' Visible='false' />
<Container Name='previewPanel' Width='35ch' Height='100%' ScrollY='true'
    BackgroundColor='#191919' Visible='false'>
    <StackPanel Direction='Vertical' Width='35ch' Height='auto' BackgroundColor='#191919'>
        <Text Name='previewTitle' Width='100%' Height='1ch' BackgroundColor='#191919'
            ForegroundColor='#5588cc' PaddingLeft='1ch' Style='Bold'>Preview</Text>
        <Line Name='previewTitleSep' Orientation='Horizontal' Type='Solid'
            BackgroundColor='#191919' ForegroundColor='#333333' />
        <Text Name='previewContent' Width='33ch' Height='auto' BackgroundColor='#191919'
            ForegroundColor='#909090' PaddingLeft='1ch' PaddingRight='1ch' />
    </StackPanel>
</Container>";
    }

    public void Initialize()
    {
        _container = _termui.GetWidget<Container>("previewPanel");
        _titleText = _termui.GetWidget<Text>("previewTitle");
        _contentText = _termui.GetWidget<Text>("previewContent");
        _separator = _termui.GetWidget<Line>("previewSep");
    }

    public void Show()
    {
        if (_container is not null) _container.Visible = true;
        if (_separator is not null) _separator.Visible = true;
    }

    public void Hide()
    {
        if (_container is not null) _container.Visible = false;
        if (_separator is not null) _separator.Visible = false;
    }

    public void UpdatePreview(string? path)
    {
        if (_contentText is null || _titleText is null) return;

        if (path is null)
        {
            _titleText.Content = "Preview";
            _contentText.Content = "No file selected";
            _contentText.Markdown = false;
            return;
        }

        string name = Path.GetFileName(path);
        string ext = Path.GetExtension(path);

        // Directory
        if (Directory.Exists(path))
        {
            _titleText.Content = $"📁 {name}";
            _contentText.Markdown = false;
            try
            {
                var entries = Directory.GetFileSystemEntries(path);
                int dirCount = entries.Count(e => Directory.Exists(e));
                int fileCount = entries.Length - dirCount;
                _contentText.Content = $"Directory\n\n{dirCount} folders\n{fileCount} files";
            }
            catch
            {
                _contentText.Content = "Directory\n\nAccess denied";
            }
            return;
        }

        if (!File.Exists(path))
        {
            _titleText.Content = "Preview";
            _contentText.Content = "File not found";
            _contentText.Markdown = false;
            return;
        }

        _titleText.Content = $"📄 {name}";

        // Image
        if (ImageExtensions.Contains(ext))
        {
            _contentText.Markdown = false;
            _contentText.Content = $"[Image: {ext}]\n\nBinary preview\nnot available";
            return;
        }

        // Known binary
        if (BinaryExtensions.Contains(ext))
        {
            _contentText.Markdown = false;
            long size = new FileInfo(path).Length;
            _contentText.Content = $"[Binary: {ext}]\n\n{FormatSize(size)}\n\nBinary preview\nnot available";
            return;
        }

        // Determine file type
        bool isMarkdown = MarkdownExtensions.Contains(ext);
        bool isCode = CodeLanguages.ContainsKey(ext) || SpecialFileNames.ContainsKey(name);
        bool isPlainText = PlainTextExtensions.Contains(ext) || string.IsNullOrEmpty(ext);

        if (!isMarkdown && !isCode && !isPlainText)
        {
            // Unknown extension — try to detect if text
            if (IsBinaryFile(path))
            {
                _contentText.Markdown = false;
                long size = new FileInfo(path).Length;
                _contentText.Content = $"[Binary: {ext}]\n\n{FormatSize(size)}\n\nBinary preview\nnot available";
                return;
            }
        }

        // Read text content
        try
        {
            string content = File.ReadAllText(path);

            // Limit preview size
            const int maxChars = 4000;
            const int maxLines = 100;
            string truncMsg = "";

            var lines = content.Split('\n');
            if (lines.Length > maxLines)
            {
                content = string.Join('\n', lines.Take(maxLines));
                truncMsg = $"\n\n… ({lines.Length - maxLines} more lines)";
            }
            else if (content.Length > maxChars)
            {
                content = content[..maxChars];
                truncMsg = "\n\n… (truncated)";
            }

            if (isMarkdown)
            {
                _contentText.Markdown = true;
                _contentText.Content = content + truncMsg;
            }
            else if (isCode)
            {
                // Wrap in fenced code block for syntax-highlighted markdown rendering
                string lang = CodeLanguages.TryGetValue(ext, out var extLang) ? extLang
                    : SpecialFileNames.TryGetValue(name, out var nameLang) ? nameLang : "text";
                _contentText.Markdown = true;
                _contentText.Content = $"```{lang}\n{content}\n```{truncMsg}";
            }
            else
            {
                // Plain text — no markdown
                _contentText.Markdown = false;
                _contentText.Content = content + truncMsg;
            }
        }
        catch
        {
            _contentText.Markdown = false;
            _contentText.Content = "Cannot read file";
        }
    }

    private static bool IsBinaryFile(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var buffer = new byte[512];
            int read = stream.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < read; i++)
            {
                byte b = buffer[i];
                if (b == 0) return true; // NUL byte = binary
                if (b < 7 && b != 0) return true; // control chars (except NUL, BEL+)
            }
            return false;
        }
        catch { return true; }
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = bytes;
        int i = 0;
        while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
        return $"{size:0.#} {units[i]}";
    }
}
