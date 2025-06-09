using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MarkerTest
{
    internal class MapControl
    {
        public GMapControl App;
        private GMapOverlay markerOverlay;
        private ContextMenuStrip contextMenu;
        private ContextMenuStrip markerContextMenu; // 추가: 마커 전용 컨텍스트 메뉴
        private GMapMarker lastRightClickedMarker;

        private PointLatLng lastRightClickLatLng;

        public MapControl(GMapControl app)
        {
            this.App = app;

            //this.App.MapProvider = GMapProviders.OpenStreetMap;
            //this.App.MapProvider = GMapProviders.TryGetProvider("GoogleMap") ?? GMapProviders.OpenStreetMap;
             this.App.MapProvider = GMapProviders.TryGetProvider("OpenStreetMap") ?? GMapProviders.GoogleMap;

            this.App.MaxZoom = 20;
            this.App.MinZoom = 6;

            // 타일 구분 그리드 라인 및 위도 / 경도 표시
            this.App.ShowTileGridLines = false;
            this.App.ShowCenter = true;
            //this.App.Position = new GMap.NET.PointLatLng(37.678830, 126.779361);
            this.App.Position = new GMap.NET.PointLatLng(36.121054, 125.973433);
            showLatLan();

            this.App.Zoom = 13;

            markerOverlay = new GMapOverlay("markers");
            this.App.Overlays.Add(markerOverlay);

            InitializeContextMenu();
            InitializeMarkerContextMenu(); // 마커 전용 메뉴 초기화

            this.App.MouseClick += (s, e) =>
            {
                //if (e.Clicks > 1) { return; }

                PointLatLng p = App.FromLocalToLatLng(e.X, e.Y);

                if (e.Button == MouseButtons.Right)
                {
                    // 마커가 있는지 확인
                    var marker = FindMarkerAt(e.Location);
                    if (marker != null)
                    {
                        lastRightClickedMarker = marker;
                        // 마커 전용 컨텍스트 메뉴 표시
                        markerContextMenu.Show(App, e.Location);
                    }
                    else
                    {
                        // 마커가 없으면 컨텍스트 메뉴 표시
                        lastRightClickLatLng = p;
                        contextMenu.Show(App, e.Location);
                    }
                }
            };
        }

        // 위/경도 표시 필드 추가
        public double showLat { set; get; }
        public double showLng { set; get; }

        /// <summary>
        /// 현재 위/경도 값 세팅
        /// </summary>
        public void showLatLan()
        {
            showLat = this.App.Position.Lat;
            showLng = this.App.Position.Lng;
        }

        /// <summary>
        /// 위/경도 표시 레이블 세팅 
        /// </summary>
        /// <returns></returns>
        public Label ShowLatLngLabel()
        {
            Label label = new Label();
            label.BackColor = Color.Transparent;
            label.Location = new Point(10, 10);
            label.AutoSize = true;
            label.Font = new Font("Arial", 12, FontStyle.Bold);
            label.ForeColor = Color.White;

            label.Text = $"{showLat} : {showLng}";

            // 오른쪽 상단에 위치하도록 Location 설정
            // GMapControl의 Width가 충분히 확보된 후 위치를 잡아야 하므로, Resize 이벤트로 동기화
            void UpdateLabelPosition(object? sender, EventArgs? e)
            {
                if (App.Width > label.Width + 20)
                    label.Location = new Point(App.Width - label.Width - 20, 10);
                else
                    label.Location = new Point(10, 10); // 너무 좁으면 왼쪽
            }
            App.Resize += UpdateLabelPosition;
            label.SizeChanged += (s, e) => UpdateLabelPosition(null, null);
            // 최초 위치 지정
            UpdateLabelPosition(null, null);
            return label;
        }

        /// <summary>
        /// 지도 확대 축소 제어 패널 생성
        /// </summary>
        /// <returns></returns>
        public Panel ShowTrackBarPanel()
        {
            Panel panel = new Panel();
            //panel.BackColor = Color.Transparent;
            panel.BackColor = Color.White;
            panel.Size = new Size(45, 200);
            panel.Location = new Point(10, 70); // 패널 위치 조정

            return panel;
        }

        /// <summary>
        /// 지도 확대 버튼 생성 (패널 상단에 위치)
        /// </summary>
        /// <returns></returns>
        public Button TrackBarControlZoomIn()
        {
            Button zoomInButton = new Button();
            zoomInButton.Text = "확대";
            zoomInButton.Size = new Size(80, 30);
            //zoomInButton.Location = new Point(10, 10);
            zoomInButton.Dock = DockStyle.Top; // 패널 상단에 위치

            zoomInButton.Click += (s, e) =>
            {
                if (App.Zoom < App.MaxZoom)
                {
                    App.Zoom++;
                    showLatLan();
                }
            };
            return zoomInButton;
        }

        /// <summary>
        /// 지도 축소 버튼 생성 (패널 하단에 위치)
        /// </summary>
        /// <returns></returns>
        public Button TrackBarControlZoomOut()
        {
            Button zoomOutButton = new Button();
            zoomOutButton.Text = "축소";
            zoomOutButton.Size = new Size(80, 30);
            //zoomOutButton.Location = new Point(10, 30+50);
            zoomOutButton.Dock = DockStyle.Bottom; // 패널 하단에 위치

            zoomOutButton.Click += (s, e) =>
            {
                if (App.Zoom > App.MinZoom)
                {
                    App.Zoom--;
                    showLatLan();
                }
            };
            return zoomOutButton;
        }

        /// <summary>
        /// 지도 축소/확대 트랙바 생성
        /// </summary>
        /// <returns></returns>
        public TrackBar ShowTrackBar()
        {
            TrackBar trackBar = new TrackBar();
            trackBar.Orientation = Orientation.Vertical;
            trackBar.Minimum = (int)App.MinZoom;
            trackBar.Maximum = (int)App.MaxZoom;
            trackBar.Value = (int)App.Zoom;
            trackBar.TickStyle = TickStyle.Both;
            trackBar.Dock = DockStyle.Fill;

            // 폼 크기 기준 5%로 동적 크기 설정
            SetTrackBarSizeAndPosition(trackBar, 3.0);

            // 폼 리사이즈 시 TrackBar 크기 재조정
            App.Resize += (s, e) => SetTrackBarSizeAndPosition(trackBar, 3.0);

            trackBar.ValueChanged += (s, e) =>
            {
                if (App.Zoom != trackBar.Value)
                    App.Zoom = trackBar.Value;
            };

            return trackBar;
        }

        /// <summary>
        /// 트랙바 크기 및 위치 설정
        /// </summary>
        /// <param name="trackBar"></param>
        /// <param name="size"></param>
        private void SetTrackBarSizeAndPosition(TrackBar trackBar, double size)
        {
            int height = (int)(trackBar.Height * size);
            if (height < 30) height = 30; // 최소 높이 보장
            trackBar.Height = height;
            trackBar.Left = 10;
            trackBar.Top = 10;
        }

        /// <summary>
        /// 사용자 지정 이미지로 마커 추가 (이름 포함)
        /// </summary>
        /// <param name="lat">위도</param>
        /// <param name="lng">경도</param>
        /// <param name="markerImage">이미지</param>
        /// <param name="markerName">마커 명칭</param>
        public void AddCustomMarker(double lat, double lng, Bitmap markerImage, string markerName)
        {
            var marker = new GMarkerGoogle(new PointLatLng(lat, lng), markerImage);
            marker.ToolTipText = markerName;
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            markerOverlay.Markers.Add(marker);
        }

        /// <summary>
        /// 마커 이름 입력 폼
        /// </summary>
        private string PromptForMarkerName()
        {
            using (var form = new MarkerNameInputForm())
            {
                if (form.ShowDialog(App.FindForm()) == DialogResult.OK)
                {
                    return "\n" + form.MarkerName;
                }
            }
            return null;
        }

        /// <summary>
        /// 마우스 오른쪽 클릭 시 컨텍스트 메뉴 초기화
        /// </summary>
        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            var moveHereItem = new ToolStripMenuItem("이 위치로 이동");
            moveHereItem.Click += (s, e) =>
            {
                this.App.Position = lastRightClickLatLng;
                showLatLan();
            };

            var addMarkerItem = new ToolStripMenuItem("마커 추가");
            addMarkerItem.Click += (s, e) =>
            {
                string markerName = PromptForMarkerName();
                if (!string.IsNullOrWhiteSpace(markerName))
                {
                    AddCustomMarker(lastRightClickLatLng.Lat, lastRightClickLatLng.Lng, Properties.Resources.rtarget_24, markerName);
                }
            };

            var clearMarkersItem = new ToolStripMenuItem("마커 모두 삭제");
            clearMarkersItem.Click += (s, e) => { markerOverlay.Markers.Clear(); };

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                moveHereItem,
                addMarkerItem,
                clearMarkersItem
            });
        }

        /// <summary>
        /// 마커 전용 컨텍스트 메뉴 초기화
        /// </summary>
        private void InitializeMarkerContextMenu()
        {
            markerContextMenu = new ContextMenuStrip();

            var deleteMarkerItem = new ToolStripMenuItem("마커 삭제");
            deleteMarkerItem.Click += (s, e) =>
            {
                if (lastRightClickedMarker != null)
                {
                    var result = MessageBox.Show($"이 마커를 삭제하시겠습니까?\n{lastRightClickedMarker.ToolTipText}", "마커 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        markerOverlay.Markers.Remove(lastRightClickedMarker);
                    }

                    lastRightClickedMarker = null;
                }
            };

            markerContextMenu.Items.Add(deleteMarkerItem);
        }

        /// <summary>
        /// 마우스 위치에 있는 마커 찾기
        /// </summary>
        private GMapMarker FindMarkerAt(Point mouseLocation)
        {
            foreach (var marker in markerOverlay.Markers)
            {
                // 마커의 화면 좌표 계산
                var localPos = App.FromLatLngToLocal(marker.Position);

                // 마커 이미지 크기(기본 32x32, 중심이 이미지 중앙)
                var markerSize = marker is GMarkerGoogle gmg && gmg.Bitmap != null ? gmg.Bitmap.Size : new Size(32, 32);
                var rect = new Rectangle((int)(localPos.X - markerSize.Width / 2), (int)(localPos.Y - markerSize.Height / 2), markerSize.Width, markerSize.Height);

                if (rect.Contains(mouseLocation))
                {
                    return marker;
                }
            }

            return null;
        }

        /// <summary>
        /// 미터 단위로 원 그리기
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radiusInMeters"></param>
        public void DrawCircleOnMap(PointLatLng center, double radiusInMeters)
        {
            const int segments = 72;
            List<PointLatLng> points = new List<PointLatLng>();
            double seg = 360.0 / segments;

            for (int i = 0; i < segments; i++)
            {
                double theta = Math.PI * (i * seg) / 180.0;
                double lat = center.Lat + (radiusInMeters / 111320.0) * Math.Cos(theta);
                double lng = center.Lng + (radiusInMeters / (111320.0 * Math.Cos(center.Lat * Math.PI / 180.0))) * Math.Sin(theta);
                points.Add(new PointLatLng(lat, lng));
            }

            var color = Color.Silver;

            var polygon = new GMapPolygon(points, $"circle_{center.Lat}_{center.Lng}_{radiusInMeters}");
            polygon.Stroke = new Pen(color, 2);
            polygon.Fill = new SolidBrush(Color.FromArgb(0, color));
            markerOverlay.Polygons.Add(polygon);
        }

        public Panel ShowRightPanel()
        {
            Panel rightPanel = new Panel
            {
                Size = new Size(200, App.Height - 20), // 패널 높이를 폼 높이에 맞춤
                BackColor = Color.LightGray
            };

            button MarkerButton = new Button
            {
                BackgroundImage = Image,
                Dock = DockStyle.Top,
                Height = 40
            };





            return rightPanel;
        }
}
