using System.IO;

namespace TQM.SfPdfViewer
{
    public interface ISave
    {
        string Save(MemoryStream fileStream);
    }
}
