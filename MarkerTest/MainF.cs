using GMap.NET;
using GMap.NET.WindowsForms;
using System.Diagnostics;

namespace MarkerTest
{
    public partial class MainF : Form
    {
        MapControl map;

        Label dynLabel;

        public MainF()
        {
            InitializeComponent();

            //GMapControl gMapCtrl = new GMapControl();
            //gMapCtrl.Dock = DockStyle.Fill;
            //this.Controls.Add(gMapCtrl);

            //map = new MapControl(gMapCtrl);

            map = new MapControl(new GMapControl());

            map.App.Dock = DockStyle.Fill;
            this.Controls.Add(map.App);

            map.App.OnMapZoomChanged += OnMapZoomChanged;
            map.App.OnPositionChanged += OnPositionChanged;

            // 위/경도 표시 레이블 추가
            map.App.Controls.Add(dynLabel = map.ShowLatLngLabel());
        }

        private void OnMapZoomChanged()
        {
            map.SetPointSave(map.showLat, map.showLng);
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

        private void OnPositionChanged(PointLatLng point)
        {
            map.showLatLan();
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

    }
}
