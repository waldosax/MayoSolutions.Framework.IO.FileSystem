namespace MayoSolutions.Framework.IO
{
    public interface IFileSystem
    {
        IDrive Drive{ get; }
        IDirectory Directory { get; }
        IFile File { get; }
    }
}