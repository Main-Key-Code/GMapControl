using GMap.NET;
using GMap.NET.WindowsForms;

namespace MarkerTest
{
    public partial class MainF : Form
    {
        MapControl map;

        Label dynLabel;

        TrackBar zoomTrackBar;

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

            //TrackBar Control Panel Create
            Panel panel = map.ShowTrackBarPanel();
            map.App.Controls.Add(panel);

            //트랙바 컨트롤 생성 및 설정
            zoomTrackBar = map.ShowTrackBar();
            zoomTrackBar.Parent = panel;

            // 버튼으로 지도 확대 이벤트 핸들러 추가
            Button zoomInButton = map.TrackBarControlZoomIn();
            zoomInButton.Parent = panel;

            // 버튼으로 지도 축소 이벤트 핸들러 추가
            Button zoomOutButton = map.TrackBarControlZoomOut();
            zoomOutButton.Parent = panel;

            // 지도 확대 및 축소 이벤트 핸들러 추가
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

            // 방위표시용 PictureBox 추가
            PictureBox pictureBox = new PictureBox
            {
                Image = Properties.Resources.compass,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(50, 50),
                Location = new Point(10, 10),
                BackColor = Color.Transparent
            };

            map.App.Controls.Add(pictureBox);

            // 오른쪽 선택 메뉴 추가
            Panel pnlRight = map.ShowRightPanel();
            map.App.Controls.Add(pnlRight);

            int rightPanelTop = 70; // 오른쪽 패널의 상단 위치

            int rightPanelLeft()
            {
                return (map.App.Width - pnlRight.Width) - 10; // 오른쪽 패널의 왼쪽 위치
            }

            // 패널 위치 설정
            pnlRight.Location = new Point(rightPanelLeft(), rightPanelTop);

            // Form 크기 조정 시 오른쪽 패널 위치 조정
            map.App.Resize += (s, e) =>
            {
                pnlRight.Location = new Point(rightPanelLeft(), rightPanelTop);
            };





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
