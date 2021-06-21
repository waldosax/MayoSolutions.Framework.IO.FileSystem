namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem
    {
        internal interface IPersistence
        {
            bool IsLoading { get; }
            bool IsSaving { get; }
            void Load();
            void Save();
        }
    }
}