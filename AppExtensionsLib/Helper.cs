namespace AppExtensionsLib;

public static class Helper
{
    public static string HRSize(long size, string format = "F1", string postfix = "B")
    {
        double current = size;
        const int KiBSize = 1024;
        const int MiBSize = KiBSize * KiBSize;
        const int GiBSize = MiBSize * KiBSize;

        return size switch
        {
            < KiBSize => $"{size} {postfix}",
            < MiBSize => $"{(current / KiBSize).ToString(format)} Ki{postfix}",
            < GiBSize => $"{(current / MiBSize).ToString(format)} Mi{postfix}",
            _ => $"{(current / GiBSize).ToString(format)} Gi{postfix}",
        };
    }
}
