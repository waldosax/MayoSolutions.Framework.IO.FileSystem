namespace MayoSolutions.Framework.IO
{
    public interface IFileSystem
    {
        IDirectory Directory { get; }
        IFile File { get; }
    }
}