using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

namespace XamlIslandsNet9
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            // We aren't even using Bing tile source, so let's just hide the warning
            TryHideBingWarning(mapControl);

            HttpMapTileDataSource dataSource = new("http://c.tile.openstreetmap.org/{zoomlevel}/{x}/{y}.png");

            MapTileSource tileSource = new(dataSource)
            {
                Visible = true,
                Layer = MapTileLayer.BackgroundReplacement,
                IsFadingEnabled = false
            };

            mapControl.TileSources.Add(tileSource);
        }

        private static void TryHideBingWarning(MapControl mapControl)
        {
            var mapGrid = VisualTreeHelper.GetChild(mapControl, 0) as Grid;
            if (mapGrid is not null)
                mapGrid = mapGrid.FindName("MapGrid") as Grid;

            if (mapGrid is not null)
            {
                var mapBrorder = mapGrid.Children.FirstOrDefault(x => x is Border);

                if (mapBrorder is not null)
                    mapBrorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
