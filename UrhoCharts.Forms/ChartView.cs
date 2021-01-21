using System.Threading.Tasks;
using Urho.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace UrhoCharts.Forms
{
    public class ChartView : UrhoSurface
    {
        public static readonly BindableProperty ChartProperty
            = BindableProperty.Create(
                propertyName      : nameof(Chart),
                returnType        : typeof(SurfaceChart),
                declaringType     : typeof(ChartView),
                defaultValue      : null,
                defaultBindingMode: BindingMode.OneWay,
                propertyChanged   : OnChartPropertyChanged);

        public SurfaceChart Chart
        {
            get => GetValue(ChartProperty) as SurfaceChart;
            set => SetValue(ChartProperty, value);
        }

        private ChartApplication _application;

        private static void OnChartPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var view = bindable as ChartView;
                if (view._application == null)
                {
                    await Task.Delay(300);
                    view._application = await view.Show<ChartApplication>(
                        new Urho.ApplicationOptions
                        {
                            Orientation = Urho.ApplicationOptions.OrientationType.Landscape,
                            NoSound     = true,
                        });
                }

                view._application.Chart = view.Chart;
            });
        }
    }
}
