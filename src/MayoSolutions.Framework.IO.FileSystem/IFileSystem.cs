using System;
using System.Runtime.InteropServices;

namespace MayoSolutions.Framework.IO
{
    public interface IFileSystem
    {
        OSPlatform Platform { get; }
        StringComparer PathComparer { get; }
        StringComparison StringComparison { get; }
        char DirectorySeparatorChar { get; }

        IDrive Drive{ get; }
        IDirectory Directory { get; }
        IFile File { get; }
    }
}