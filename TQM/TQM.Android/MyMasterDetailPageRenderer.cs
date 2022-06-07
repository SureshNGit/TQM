using System.Threading.Tasks;
using TQM;
using TQM.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(flyoutNavigation), typeof(MyMasterDetailPageRenderer))]
namespace TQM.Droid.Renderers
{
    public class MyMasterDetailPageRenderer : MasterDetailPageRenderer
    {
        public override bool OnTouchEvent(Android.Views.MotionEvent e)
        {
            if (e.Action == Android.Views.MotionEventActions.Up)
                new Task(CloseDrawers).Start(TaskScheduler.FromCurrentSynchronizationContext());

            return base.OnTouchEvent(e);
        }
    }
}