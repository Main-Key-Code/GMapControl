using GMap.NET;
using GMap.NET.WindowsForms;

namespace MarkerTest
{
    public partial class MainF : Form
    {
        MapControl map;

        Label dynLabel;

        TrackBar zoomTrackBar; // TrackBar 추가

        public MainF()
        {
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

            // TrackBar 생성 및 설정
            zoomTrackBar = new TrackBar();
            zoomTrackBar.Orientation = Orientation.Vertical;
            zoomTrackBar.Minimum = (int)map.App.MinZoom;
            zoomTrackBar.Maximum = (int)map.App.MaxZoom;
            zoomTrackBar.Value = (int)map.App.Zoom;
            zoomTrackBar.TickStyle = TickStyle.Both;

            map.App.Controls.Add(zoomTrackBar = map.ShowTrackBar()); // GMapControl의 Controls에 추가하여 TrackBar가 맵 위에 표시되도록 함

            map.App.OnMapZoomChanged += OnMapZoomChanged;
            map.App.OnPositionChanged += OnPositionChanged;

            // 위/경도 표시 레이블 추가
            map.App.Controls.Add(dynLabel = map.ShowLatLngLabel());

            // 미터 단위로 원 그리기
            double[] distances = { 8000, 16000, 24000, 30000 };

            foreach (var dist in distances)
            {
                map.DrawCircleOnMap(new PointLatLng(map.showLat, map.showLng), dist);
            }
            
         
        }



        private void OnMapZoomChanged()
        {
            map.App.Position = new PointLatLng(map.showLat, map.showLng);
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
            // TrackBar와 동기화
            if (zoomTrackBar.Value != (int)map.App.Zoom)
                zoomTrackBar.Value = (int)map.App.Zoom;
        }

        private void OnPositionChanged(PointLatLng point)
        {
            map.showLatLan();
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

    }
}
