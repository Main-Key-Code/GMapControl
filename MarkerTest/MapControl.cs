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
            this.App.MapProvider = GMapProviders.TryGetProvider("GoogleMap") ?? GMapProviders.OpenStreetMap;

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

            return label;
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
        /// 위도 / 경도 입력해서 이벤트 실행 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radiusInMeters"></param>
        public void DrawCircleOnMap(PointLatLng center, double radiusInMeters)
        {
            int segments = 36; // 원을 구성하는 점의 개수
            List<PointLatLng> points = new List<PointLatLng>();

            for (int i = 0; i < segments; i++)
            {
                double theta = 2.0 * Math.PI * i / segments; // 각도 계산
                double dx = radiusInMeters * Math.Cos(theta); // 원의 x 좌표
                double dy = radiusInMeters * Math.Sin(theta); // 원의 y 좌표

                // 위도/경도 변환
                double lat = center.Lat + (180 / Math.PI) * (dy / 6378137); // 6378137은 지구의 반지름(미터)
                double lng = center.Lng + (180 / Math.PI) * (dx / 6378137) / Math.Cos(center.Lat * Math.PI / 180);

                points.Add(new PointLatLng(lat, lng));
            }

            // GMapPolygon 생성
            GMapPolygon circle = new GMapPolygon(points, "Circle")
            {
                Stroke = new Pen(Color.DarkGray, 2), // 테두리 색상과 두께
                Fill = new SolidBrush(Color.FromArgb(0, Color.Red)) // 내부 색상 및 투명도
            };

            // 오버레이 추가
            GMapOverlay overlay = new GMapOverlay("circleOverlay");
            overlay.Polygons.Add(circle);
            App.Overlays.Add(overlay);
        }
    }
}
