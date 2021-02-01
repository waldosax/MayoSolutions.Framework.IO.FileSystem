namespace MayoSolutions.Framework.IO
{
    public interface IFileSystem : IFieSystemDescriptor
    {

        IDrive Drive{ get; }
        IDirectory Directory { get; }
        IFile File { get; }
    }
}