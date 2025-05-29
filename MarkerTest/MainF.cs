using GMap.NET;
using GMap.NET.WindowsForms;

namespace MarkerTest
{
    public partial class MainF : Form
    {
        MapControl map;

        Label dynLabel;

        public MainF()
        {
            // 
            this.FormClosing += (s, e) =>
            {
                if (map != null)
                {
                    map.App.Dispose();
                }
            };

            InitializeComponent();

            map = new MapControl(new GMapControl());

            map.App.Dock = DockStyle.Fill;
            this.Controls.Add(map.App);

            map.App.OnMapZoomChanged += OnMapZoomChanged;
            map.App.OnPositionChanged += OnPositionChanged;

            // 위/경도 표시 레이블 추가
            // 위/경도 표시 레이블 추가
            map.App.Controls.Add(dynLabel = map.ShowLatLngLabel());
        }

        private void OnMapZoomChanged()
        {
            map.App.Position = new PointLatLng(map.showLat, map.showLng);
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

        private void OnPositionChanged(PointLatLng point)
        {
            map.showLatLan();
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

    }
}
