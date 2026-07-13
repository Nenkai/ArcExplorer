# ArcExplorer

A generic game archive explorer using Avalonia. Intended to be modular and expandable through plugins.

Mainly because I was getting tired of not having UIs for my archive extractors.

<img width="1542" height="746" alt="ArcExplorer Desktop_OTA5vkVOcY" src="https://github.com/user-attachments/assets/6548e8f5-1dc8-4302-b39a-2d6460ec9440" />


## Example Plugin (`FaithGameArchivePlugin.cs`)

First, define your plugin.

```cs
using FF16Tools.Pack;

using Microsoft.Extensions.Logging;

using NenTools.ArchiveService.Abstractions;
using NenTools.ArchiveService.Implementations;

using System.ComponentModel;

namespace FF16Tools.ArcExplorerPlugin;

public class FaithGameArchivePlugin : GameArchivePluginBase
{
    public override string Name => $"Faith Archive Plugin";
    public override string Version => "1.0.0";
    public override string Author => "Nenkai";
    public override string? Website => "https://github.com/Nenkai/FF16Tools";

    // Supported files (for display) goes here.
    public override IReadOnlyCollection<ISupportedPluginFileType>? SupportedFileTypes => 
    [
        new PluginSupportedFileType()
        {
            Name = "Faith Pack Archive",
            Extension = ".pac"
        }
    ];

    public override void Initialize()
    {
        base.Initialize();

        // You also have access to a ILogger (Microsoft.Extensions.Logging) here.
        Logger.LogInformation("FaithGameArchivePlugin: Initializing...");

        // Register additional plugin settings.
        RegisterSetting(new ArchiveSettingDescriptor()
        {
            Name = "GameType",
            DisplayName = "Game Type",
            Description = "Game type.",
            ValueType = typeof(FaithGameTypeSetting),        // Enums are also supported. ArcExplorer turns these into a dropdown.
            DefaultValue = null,
            IsRequired = true
        });
    }

    protected override IGameArchive OpenArchiveCore(string path, ArchiveLoadParameters? parameters = null)
    {
        // Incase you need specific archive handling that you can't otherwise determine though absent versioning
        if (!Settings.TryGetValue("GameType", out FaithGameTypeSetting? gameTypeValue))
            throw new InvalidOperationException("FaithPlugin: Game type is not specified.");

        string codeName = gameTypeValue switch
        {
            FaithGameTypeSetting.FFXVI => "faith",
            FaithGameTypeSetting.FFTTIC => "ffto",
            _ => throw new ArgumentException("FaithPlugin: Invalid game type."),
        };

        FF16Pack pack = FF16Pack.Open(path, codeName);
        string packName = Path.GetFileName(path);

        var faithGameArchive = new FaithGameArchive(pack)
        {
            Name = packName,
            Path = path,
        };

        return faithGameArchive;
    }

    public override IReadOnlyDictionary<string, IAttributeMetadata<IGameArchiveFile>> GetFileAttributes()
    {
        // Per-file specific metadata, define anything that your archive supplies here
        List<IAttributeMetadata<IGameArchiveFile>> list = [
            AttributeMetadata.Create("Name", "Name", AttributeDisplayFormat.FileName, accessor: (IGameArchiveFile file) => file.Name),
            AttributeMetadata.Create("Size", "Size", AttributeDisplayFormat.ByteSize, accessor: (IGameArchiveFile file) => file.Size),
            AttributeMetadata.Create("Source", "Source", AttributeDisplayFormat.Default, accessor: (IGameArchiveFile file) => file.SourceArchive?.Name ?? "<none>"),

            AttributeMetadata.Create(nameof(FF16PackFile.DataOffset), "Offset", AttributeDisplayFormat.Hex,
                accessor: (IGameArchiveFile file) => file.AdditionalProperties.GetValueOrDefault(nameof(FF16PackFile.DataOffset))),

            AttributeMetadata.Create(nameof(FF16PackFile.CRC32Checksum), "CRC32", AttributeDisplayFormat.Hex,
                accessor: (IGameArchiveFile file) => file.AdditionalProperties.GetValueOrDefault(nameof(FF16PackFile.CRC32Checksum))),

            AttributeMetadata.Create(nameof(FF16PackFile.ChunkedCompressionFlags), "Compression", AttributeDisplayFormat.Default,
                accessor: (IGameArchiveFile file) => file.AdditionalProperties.GetValueOrDefault(nameof(FF16PackFile.ChunkedCompressionFlags))),
        ];

        return list.ToDictionary(e => e.Name, e => e);
    }

    public override bool IsSupported(Stream stream)
    {
        // Put code to check if an archive is supported here
    }

    public override bool IsSupported(string path)
    {
        // This should be left to path checks only. Use the other IsSupported for actual header checks.
        return Path.GetExtension(path).Equals(".pac", StringComparison.OrdinalIgnoreCase);
    }

    public enum FaithGameTypeSetting
    {
        [Description("FINAL FANTASY XVI (faith)")]
        FFXVI,

        [Description("FINAL FANTASY TACTICS: The Ivalice Chronicles (ffto)")]
        FFTTIC,
    }
}

```

Then, your archive handler (`FaithGameArchive.cs`):

```cs
using FF16Tools.Pack;

using NenTools.ArchiveService.Abstractions;
using NenTools.ArchiveService.Implementations;

namespace FF16Tools.ArcExplorerPlugin;

public class FaithGameArchive : GameArchiveBase
{
    public FF16Pack PackFile { get; set; }
    public override bool SupportsAsync => false;

    private IFileSystemTree? _tree;

    public FaithGameArchive(FF16Pack packFile)
    {
        PackFile = packFile;
    }

    // Returns a file tree of the current archive.
    public override IFileSystemTree GetTree()
    {
        if (_tree is not null)
            return _tree;

        List<IGameArchiveFile> files = [];
        foreach (var (name, file) in PackFile.Files)
        {
            string path = name.Replace('\\', '/');
            var gameArchiveFile = new GameArchiveFile(System.IO.Path.GetFileName(path), path, this)
            {
                Size = file.DecompressedFileSize,
            };

            gameArchiveFile.Properties[nameof(FF16PackFile.CompressedFileSize)] = file.CompressedFileSize;

            files.Add(gameArchiveFile);
        }


        _tree = FileSystemTree.Parse(this, files);
        return _tree;
    }

    public override void ExtractFile(IGameArchiveFile file, Stream outputStream)
    {
        PackFile.ExtractFile(file.Path, outputStream);
    }

    public override Task<IFileSystemTree> GetTreeAsync(CancellationToken ct = default)
    {
        // If async is needed.
    }

    public override void Dispose()
    {
        PackFile.Dispose();
    }

    public override IReadOnlyDictionary<string, IAttributeMetadata<IGameArchive>> GetAttributes()
    {
        // Define attributes of your archive (stuff like encryption key, version and whatnot here)
        return new Dictionary<string, IAttributeMetadata<IGameArchive>>();
    }

    public override ValueTask DisposeAsync()
    {
        // ...
    }

    public override Task ExtractFileAsync(IGameArchiveFile file, Stream outputStream, CancellationToken ct = default)
    {
        // Whether you need async extraction, plug it here
    }
}
```

This is all that is required to build a plugin.

Just make sure to **publish** your plugin so that the dependencies are also copied, as ArcExplorer makes uses of AssemblyLoadContexts to separate plugins.
