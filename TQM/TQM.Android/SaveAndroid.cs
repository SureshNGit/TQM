using System.IO;
using TQM.SfPdfViewer;
using Xamarin.Forms;

[assembly: Dependency(typeof(TQM.Droid.SaveAndroid))]
namespace TQM.Droid
{
    public class SaveAndroid : ISave
    {
        public string Save(MemoryStream stream, string fileName)
        {
            string root = null;
            //string fileName = "TQM_Report.pdf";
            root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            Java.IO.File myDir = new Java.IO.File(root + "/SVYADownloads");
            if (myDir.Exists()) { myDir.Delete(); }
            myDir.Mkdir();
            Java.IO.File file = new Java.IO.File(myDir, fileName);
            string filePath = file.Path;
            if (file.Exists()) file.Delete();
            Java.IO.FileOutputStream outs = new Java.IO.FileOutputStream(file);
            outs.Write(stream.ToArray());
            outs.Flush();
            outs.Close();
            return filePath;
        }
    }
}