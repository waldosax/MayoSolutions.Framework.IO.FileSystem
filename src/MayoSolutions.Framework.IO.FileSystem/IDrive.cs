namespace MayoSolutions.Framework.IO
{
    public interface IDrive
    {
        string[] GetDrives();
        VolumeInfo GetVolumeInfo(string drive);
    }
}