namespace MayoSolutions.Framework.IO
{
    public partial class LiveVirtualFileSystem : VirtualFileSystem
    {
        private readonly LiveStub _stub;
        internal readonly IPersistence Persistence;

        public LiveVirtualFileSystem(LiveVirtualFileSystemOptions options)
        {
            if (!string.IsNullOrEmpty(options?.FileSystemLayoutFilePath))
                Persistence = new LayoutFilePersistence(this, options);

            Persistence.Load();

            _stub = new LiveStub(this);
            Drive = _stub;
            Directory = _stub;
            File = _stub;
        }



        internal void Invalidate(object signalingObject)
        {
            if (Persistence.IsLoading) return;
            if (Persistence.IsSaving) return;

            // Do the saving things
            Persistence.Save();
        }
    }
}
