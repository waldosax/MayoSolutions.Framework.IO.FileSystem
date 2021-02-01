using System;
using System.Runtime.InteropServices;

namespace MayoSolutions.Framework.IO
{
    public interface IFieSystemDescriptor
    {
        OSPlatform Platform { get; }
        StringComparer PathComparer { get; }
        StringComparison StringComparison { get; }
        char DirectorySeparatorChar { get; }
    }
}