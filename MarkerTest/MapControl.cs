using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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

        private bool isDragMode = false;
        private bool isDragging = false;
        private Point dragStartPoint;
        private PointLatLng dragStartPosition;

        private bool isRectSelectMode = false;
        private bool isRectSelecting = false;
        private Point rectStartPoint;
        private Point rectEndPoint;
        private GMapPolygon? previewRectPolygon = null;

        private bool isAddMarkerMode = false; // 마커 추가 모드

        private bool isRulerMode = false;
        private bool isRulerDrawing = false;
        private Point rulerStartPoint;
        private Point rulerEndPoint;
        private GMapRoute? previewRulerRoute = null;

        private bool isPolygonMode = false;
        private bool isPolygonDrawing = false;
        private List<PointLatLng> polygonPoints = new();
        private GMapPolygon? previewPolygon = null;

        private bool isCircleMode = false;
        private bool isCircleDrawing = false;
        private Point circleStartPoint;
        private Point circleEndPoint;
        private GMapPolygon? previewCirclePolygon = null;

        public MapControl(GMapControl app)
        {
            this.App = app;

            this.App.TabStop = false; // TabStop 설정
            this.App.DragButton = MouseButtons.None ; // 드래그 버튼 설정

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

            // 기존 MouseDown 핸들러 최적화
            this.App.MouseDown += (s, e) =>
            {
                // 드래그 모드
                if (isDragMode && e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    dragStartPoint = e.Location;
                    dragStartPosition = App.Position;
                    App.Cursor = Cursors.Hand;
                    return;
                }

                // 사각형 선택 모드
                if (isRectSelectMode && e.Button == MouseButtons.Left)
                {
                    isRectSelecting = true;
                    rectStartPoint = e.Location;
                    rectEndPoint = e.Location;
                    RemovePreviewRectPolygon();
                    return;
                }

                // 마커 추가 모드
                if (isAddMarkerMode && e.Button == MouseButtons.Left)
                {
                    var latlng = App.FromLocalToLatLng(e.X, e.Y);
                    string markerName = PromptForMarkerName();
                    if (!string.IsNullOrWhiteSpace(markerName))
                    {
                        AddCustomMarker(latlng.Lat, latlng.Lng, Properties.Resources.pin_24, markerName);
                    }
                    isAddMarkerMode = false;
                    App.Cursor = Cursors.Default;
                    return;
                }

                // ruler 모드
                if (isRulerMode && e.Button == MouseButtons.Left)
                {
                    if (!isRulerDrawing)
                    {
                        isRulerDrawing = true;
                        rulerStartPoint = e.Location;
                        rulerEndPoint = e.Location;
                        RemovePreviewRulerRoute();
                    }
                    else
                    {
                        rulerEndPoint = e.Location;
                        DrawFinalRulerRoute(rulerStartPoint, rulerEndPoint);
                        RemovePreviewRulerRoute();
                        isRulerDrawing = false;
                        isRulerMode = false;
                        App.Cursor = Cursors.Default;
                    }
                    return;
                }

                // polygon 모드
                if (isPolygonMode)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        var latlng = App.FromLocalToLatLng(e.X, e.Y);
                        polygonPoints.Add(latlng);
                        isPolygonDrawing = true;
                        DrawPreviewPolygon();
                        return;
                    }
                    if (e.Button == MouseButtons.Right && isPolygonDrawing && polygonPoints.Count >= 3)
                    {
                        DrawFinalPolygon();
                        isPolygonMode = false;
                        isPolygonDrawing = false;
                        polygonPoints.Clear();
                        RemovePreviewPolygon();
                        App.Cursor = Cursors.Default;
                        return;
                    }
                }

                // 원 그리기 모드
                if (isCircleMode && e.Button == MouseButtons.Left)
                {
                    if (!isCircleDrawing)
                    {
                        isCircleDrawing = true;
                        circleStartPoint = e.Location;
                        circleEndPoint = e.Location;
                        RemovePreviewCirclePolygon();
                    }
                    else
                    {
                        circleEndPoint = e.Location;
                        DrawFinalCirclePolygon(circleStartPoint, circleEndPoint);
                        RemovePreviewCirclePolygon();
                        isCircleDrawing = false;
                        isCircleMode = false;
                        App.Cursor = Cursors.Default;
                    }
                    return;
                }

                // 오른쪽 클릭: 폴리곤/루트 삭제
                if (e.Button == MouseButtons.Right)
                {
                    var polygon = FindPolygonAt(e.Location);
                    if (polygon != null)
                    {
                        if (polygon.Name.StartsWith("range_"))
                        {
                            return;
                        }
                        #region
                        // Begin regi : 혹시 몰라서 드레그 모드 해제 : 코드 삭제해도 무방
                        isDragging = false;
                        App.Cursor = Cursors.Default;
                        // End AA
                        #endregion 

                        if (MessageBox.Show("이 영역을 삭제하시겠습니까?", "영역 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            markerOverlay.Polygons.Remove(polygon);
                            App.Refresh();
                        }

                        
                        return;
                    }
                    

                    var route = FindRouteAt(e.Location);
                    if (route != null)
                    {
                        // Begin  혹시 몰라서 드레그 모드 해제 : 코드 삭제해도 무방
                        isDragging = false;
                        App.Cursor = Cursors.Default;

                        if (MessageBox.Show("이 선을 삭제하시겠습니까?", "선 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            markerOverlay.Routes.Remove(route);
                            App.Refresh();
                        }
                        return;
                    }
                }
            };

            this.App.MouseMove += (s, e) =>
            {
                if (isDragMode && isDragging && e.Button == MouseButtons.Left)
                {
                    // 이동 거리 계산
                    PointLatLng current = App.FromLocalToLatLng(e.X, e.Y);
                    PointLatLng start = App.FromLocalToLatLng(dragStartPoint.X, dragStartPoint.Y);

                    double dLat = start.Lat - current.Lat;
                    double dLng = start.Lng - current.Lng;

                    App.Position = new PointLatLng(dragStartPosition.Lat + dLat, dragStartPosition.Lng + dLng);
                    showLatLan();
                }
                else if (isRectSelectMode && isRectSelecting && e.Button == MouseButtons.Left)
                {
                    rectEndPoint = e.Location;
                    DrawPreviewRectPolygon(rectStartPoint, rectEndPoint);
                }

                // ruler 모드: 드래그 중 미리보기
                if (isRulerMode && isRulerDrawing)
                {
                    rulerEndPoint = e.Location;
                    DrawPreviewRulerRoute(rulerStartPoint, rulerEndPoint);
                }

                // polygon 모드: 미리보기(마우스 따라가기)
                if (isPolygonMode && isPolygonDrawing && polygonPoints.Count > 0)
                {
                    var tempPoints = new List<PointLatLng>(polygonPoints)
                    {
                        App.FromLocalToLatLng(e.X, e.Y)
                    };
                    DrawPreviewPolygon(tempPoints);
                }

                // 원 그리기 모드: 드래그 중 미리보기
                if (isCircleMode && isCircleDrawing)
                {
                    circleEndPoint = e.Location;
                    DrawPreviewCirclePolygon(circleStartPoint, circleEndPoint);
                }
            };

            this.App.MouseUp += (s, e) =>
            {
                if (isDragMode && e.Button == MouseButtons.Left)
                {
                    isDragging = false;
                    App.Cursor = Cursors.Default;
                }
                else if (isRectSelectMode && isRectSelecting && e.Button == MouseButtons.Left)
                {
                    isRectSelecting = false;
                    rectEndPoint = e.Location;
                    DrawFinalRectPolygon(rectStartPoint, rectEndPoint);
                    RemovePreviewRectPolygon();
                }
            };
        }

        /// <summary>
        /// 화면 우측 패널 생성 및 기능
        /// </summary>
        /// <returns></returns>
        public Panel ShowRightPanel()
        {

            Panel rightPanel = new Panel();

            rightPanel.BackColor = Color.DarkSlateGray;
            rightPanel.Size = new Size(50, (App.Height / 2));   // Panel 크기 초기 설정

            var images = new List<Bitmap>
            {
                Properties.Resources.cursor_24 ,
                Properties.Resources.select_24,
                Properties.Resources.pin_24,
                Properties.Resources.ruler_24,
                Properties.Resources.polygon_24,
                Properties.Resources.empty_24,
                Properties.Resources.home_24
            };

            var toolTips = new List<string>
            {
                "마우스 위치 표시",
                "사각 영역 선택",
                "마커 추가",
                "거리 측정",
                "다각형 영역 선택",
                "지도 초기화",
                "홈 위치로 이동"
            };

            int sizeButton = 30; // 버튼 크기

            for (int i = 0; i < images.Count; i++)
            {
                Button button = new Button();
                button.Size = new Size(sizeButton, sizeButton);
                button.BackgroundImage = images[i];
                button.BackgroundImageLayout = ImageLayout.Stretch;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Location = new Point(0, i * sizeButton);
                button.Tag = toolTips[i]; // 툴팁 텍스트 저장

                // 툴팁 설정
                ToolTip toolTip = new ToolTip();

                toolTip.SetToolTip(button, toolTips[i]);

                // 툴팁 글씨 크기 설정
                toolTip.OwnerDraw = true;
                toolTip.Draw += (sender, e) =>
                {
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                    using (var font = new Font("맑은 고딕", 8))
                    using (var brush = new SolidBrush(Color.Black))
                    {
                        e.Graphics.DrawString(e.ToolTipText, font, brush, new PointF(2, 2));
                    }
                };

                //툴팁 글씨 크기에 맞춰서 Box 크기 조절
                toolTip.Popup += (sender, e) =>
                {
                    var btn = e.AssociatedControl as Button;
                    string currentToolTipText = btn?.Tag?.ToString() ?? "";

                    using (var font = new Font("맑은 고딕", 8))
                    {
                        Size textSize = TextRenderer.MeasureText(currentToolTipText, font);
                        e.ToolTipSize = new Size(textSize.Width + 4, textSize.Height + 4);
                    }
                };

                if (i == 0) // cursor
                {
                    button.Click += (s, e) =>
                    {
                        isDragMode = true;
                        isRectSelectMode = false;
                        isAddMarkerMode = false;
                        isRulerMode = false;
                        App.Cursor = Cursors.Hand;
                    };
                }

                if (i == 1) // select
                {
                    button.Click += (s, e) =>
                    {
                        isDragMode = false;
                        isRectSelectMode = true;
                        isAddMarkerMode = false;
                        isRulerMode = false;
                        App.Cursor = Cursors.Cross; // 십자 커서로 변경
                    };
                }

                if (i == 2) // marker
                {
                    button.Click += (s, e) =>
                    {
                        isDragMode = false;
                        isRectSelectMode = false;
                        isAddMarkerMode = true;
                        isRulerMode = false;
                        App.Cursor = Cursors.Arrow; // 마커 추가 시 커서(원하는 모양으로 변경 가능)
                    };
                }

                if (i == 3) // ruler
                {
                    button.Click += (s, e) =>
                    {
                        isDragMode = false;
                        isRectSelectMode = false;
                        isAddMarkerMode = false;
                        isRulerMode = true;
                        App.Cursor = Cursors.Cross;
                    };
                }

                if (i == 4) // polygon
                {
                    button.Click += (s, e) =>
                    {
                        isDragMode = false;
                        isRectSelectMode = false;
                        isAddMarkerMode = false;
                        isRulerMode = false;
                        isPolygonMode = true;
                        isPolygonDrawing = false;
                        polygonPoints.Clear();
                        RemovePreviewPolygon();
                        App.Cursor = Cursors.Cross;
                    };
                }

                if (i == 5) // empty(원 그리기)
                {
                    button.Click += (s, e) =>
                    {
                        isDragMode = false;
                        isRectSelectMode = false;
                        isAddMarkerMode = false;
                        isRulerMode = false;
                        isPolygonMode = false;
                        isCircleMode = true;
                        isCircleDrawing = false;
                        RemovePreviewCirclePolygon();
                        App.Cursor = Cursors.Cross;
                    };
                }

                if (i == 6) // home 버튼
                {
                    button.Click += (s, e) =>
                    {
                        // 초기 위치와 줌 값으로 이동
                        App.Position = new PointLatLng(36.121054, 125.973433);
                        App.Zoom = 13;
                        showLatLan();
                        App.Cursor = Cursors.Default;

                        // 모든 모드 해제
                        isDragMode = false;
                        isRectSelectMode = false;
                        isAddMarkerMode = false;
                        isRulerMode = false;
                        isPolygonMode = false;
                        isCircleMode = false;
                    };
                }

                rightPanel.Controls.Add(button);
            }

            rightPanel.Size = new Size(sizeButton, (images.Count * sizeButton));   // Panel 크기 버튼 갯수에 맞게 조정

            return rightPanel;
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

            //var polygon = new GMapPolygon(points, $"circle_{center.Lat}_{center.Lng}_{radiusInMeters}");
            var polygon = new GMapPolygon(points, $"range_{center.Lat}_{center.Lng}_{radiusInMeters}");
            polygon.Stroke = new Pen(color, 2);
            polygon.Fill = new SolidBrush(Color.FromArgb(0, color));
            markerOverlay.Polygons.Add(polygon);

            App.Refresh();
        }

        /// <summary>
        /// 사각형 미리보기 그리기
        /// </summary>
        private void DrawPreviewRectPolygon(Point start, Point end)
        {
            RemovePreviewRectPolygon();

            var p1 = App.FromLocalToLatLng(start.X, start.Y);
            var p2 = App.FromLocalToLatLng(end.X, start.Y);
            var p3 = App.FromLocalToLatLng(end.X, end.Y);
            var p4 = App.FromLocalToLatLng(start.X, end.Y);

            var points = new List<PointLatLng> { p1, p2, p3, p4, p1 };
            previewRectPolygon = new GMapPolygon(points, "previewRect")
            {
                Stroke = new Pen(Color.Red, 2),
                Fill = new SolidBrush(Color.FromArgb(40, Color.Red))
            };
            markerOverlay.Polygons.Add(previewRectPolygon);
            App.Refresh();
        }

        /// <summary>
        /// 사각형 미리보기 제거
        /// </summary>
        private void RemovePreviewRectPolygon()
        {
            if (previewRectPolygon != null)
            {
                markerOverlay.Polygons.Remove(previewRectPolygon);
                previewRectPolygon = null;
                App.Refresh();
            }
        }

        /// <summary>
        /// 사각형 확정 그리기
        /// </summary>
        private void DrawFinalRectPolygon(Point start, Point end)
        {
            var p1 = App.FromLocalToLatLng(start.X, start.Y);
            var p2 = App.FromLocalToLatLng(end.X, start.Y);
            var p3 = App.FromLocalToLatLng(end.X, end.Y);
            var p4 = App.FromLocalToLatLng(start.X, end.Y);

            var points = new List<PointLatLng> { p1, p2, p3, p4, p1 };
            var rectPolygon = new GMapPolygon(points, $"rect_{DateTime.Now.Ticks}")
            {
                Stroke = new Pen(Color.Blue, 2),
                Fill = new SolidBrush(Color.FromArgb(40, Color.Blue))
            };
            markerOverlay.Polygons.Add(rectPolygon);
            App.Refresh();
        }

        /// <summary>
        /// 거리 측정 미리보기 그리기
        /// </summary>
        private void DrawPreviewRulerRoute(Point start, Point end)
        {
            RemovePreviewRulerRoute();

            var p1 = App.FromLocalToLatLng(start.X, start.Y);
            var p2 = App.FromLocalToLatLng(end.X, end.Y);

            var points = new List<PointLatLng> { p1, p2 };
            previewRulerRoute = new GMapRoute(points, "previewRuler")
            {
                Stroke = new Pen(Color.Orange, 2)
            };
            markerOverlay.Routes.Add(previewRulerRoute);
            App.Refresh();
        }

        /// <summary>
        /// 거리 측정 미리보기 제거
        /// </summary>
        private void RemovePreviewRulerRoute()
        {
            if (previewRulerRoute != null)
            {
                markerOverlay.Routes.Remove(previewRulerRoute);
                previewRulerRoute = null;
                App.Refresh();
            }
        }

        /// <summary>
        /// 거리 측정 확정 그리기
        /// </summary>
        private void DrawFinalRulerRoute(Point start, Point end)
        {
            var p1 = App.FromLocalToLatLng(start.X, start.Y);
            var p2 = App.FromLocalToLatLng(end.X, end.Y);

            var points = new List<PointLatLng> { p1, p2 };
            var route = new GMapRoute(points, $"ruler_{DateTime.Now.Ticks}")
            {
                Stroke = new Pen(Color.OrangeRed, 2)
            };
            markerOverlay.Routes.Add(route);
            App.Refresh();
        }

        /// <summary>
        /// 다각형 미리보기 그리기
        /// </summary>
        private void DrawPreviewPolygon(List<PointLatLng>? customPoints = null)
        {
            RemovePreviewPolygon();
            var points = customPoints ?? polygonPoints;
            if (points.Count < 2) return;
            previewPolygon = new GMapPolygon(points, "previewPolygon")
            {
                Stroke = new Pen(Color.DarkViolet, 2),
                Fill = new SolidBrush(Color.FromArgb(40, Color.Violet))
            };
            markerOverlay.Polygons.Add(previewPolygon);
            App.Refresh();
        }

        /// <summary>
        /// 다각형 미리보기 제거
        /// </summary>
        private void RemovePreviewPolygon()
        {
            if (previewPolygon != null)
            {
                markerOverlay.Polygons.Remove(previewPolygon);
                previewPolygon = null;
                App.Refresh();
            }
        }

        /// <summary>
        /// 다각형 확정 그리기
        /// </summary>
        private void DrawFinalPolygon()
        {
            if (polygonPoints.Count < 3) return;
            var closedPoints = new List<PointLatLng>(polygonPoints) { polygonPoints[0] };
            var polygon = new GMapPolygon(closedPoints, $"polygon_{DateTime.Now.Ticks}")
            {
                Stroke = new Pen(Color.Purple, 2),
                Fill = new SolidBrush(Color.FromArgb(40, Color.Purple))
            };
            markerOverlay.Polygons.Add(polygon);
            App.Refresh();
        }

        /// <summary>
        /// 원 미리보기 그리기
        /// </summary>
        private void DrawPreviewCirclePolygon(Point start, Point end)
        {
            RemovePreviewCirclePolygon();

            var center = App.FromLocalToLatLng(start.X, start.Y);
            double radius = GetDistanceInMeters(start, end);

            const int segments = 72;
            List<PointLatLng> points = new();
            double seg = 360.0 / segments;

            for (int i = 0; i < segments; i++)
            {
                double theta = Math.PI * (i * seg) / 180.0;
                double lat = center.Lat + (radius / 111320.0) * Math.Cos(theta);
                double lng = center.Lng + (radius / (111320.0 * Math.Cos(center.Lat * Math.PI / 180.0))) * Math.Sin(theta);
                points.Add(new PointLatLng(lat, lng));
            }

            previewCirclePolygon = new GMapPolygon(points, "previewCircle")
            {
                Stroke = new Pen(Color.Green, 2),
                Fill = new SolidBrush(Color.FromArgb(40, Color.Green))
            };
            markerOverlay.Polygons.Add(previewCirclePolygon);
            App.Refresh();
        }

        /// <summary>
        /// 원 미리보기 제거
        /// </summary>
        private void RemovePreviewCirclePolygon()
        {
            if (previewCirclePolygon != null)
            {
                markerOverlay.Polygons.Remove(previewCirclePolygon);
                previewCirclePolygon = null;
                App.Refresh();
            }
        }

        /// <summary>
        /// 원 확정 그리기
        /// </summary>
        private void DrawFinalCirclePolygon(Point start, Point end)
        {
            var center = App.FromLocalToLatLng(start.X, start.Y);
            double radius = GetDistanceInMeters(start, end);

            const int segments = 72;
            List<PointLatLng> points = new();
            double seg = 360.0 / segments;

            for (int i = 0; i < segments; i++)
            {
                double theta = Math.PI * (i * seg) / 180.0;
                double lat = center.Lat + (radius / 111320.0) * Math.Cos(theta);
                double lng = center.Lng + (radius / (111320.0 * Math.Cos(center.Lat * Math.PI / 180.0))) * Math.Sin(theta);
                points.Add(new PointLatLng(lat, lng));
            }

            var polygon = new GMapPolygon(points, $"range_{DateTime.Now.Ticks}")
            {
                Stroke = new Pen(Color.Green, 2),
                Fill = new SolidBrush(Color.FromArgb(40, Color.Green))
            };
            markerOverlay.Polygons.Add(polygon);
            App.Refresh();
        }

        /// <summary>
        /// 두 포인트 간 거리(픽셀)를 미터로 변환
        /// </summary>
        private double GetDistanceInMeters(Point p1, Point p2)
        {
            var latlng1 = App.FromLocalToLatLng(p1.X, p1.Y);
            var latlng2 = App.FromLocalToLatLng(p2.X, p2.Y);
            return GMap.NET.MapProviders.GMapProviders.EmptyProvider.Projection.GetDistance(latlng1, latlng2) * 1000.0;
        }

        /// <summary>
        /// 마우스 위치에 있는 폴리곤(사각형, 다각형, 원) 찾기
        /// </summary>
        private GMapPolygon? FindPolygonAt(Point mouseLocation)
        {
            foreach (var polygon in markerOverlay.Polygons)
            {
                var localPoints = polygon.Points.Select(p => App.FromLatLngToLocal(p)).ToList();
                if (localPoints.Count < 3) continue;
                using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    gp.AddPolygon(localPoints.Select(pt => new PointF((float)pt.X, (float)pt.Y)).ToArray());
                    if (gp.IsVisible(mouseLocation))
                        return polygon;
                }
            }
            return null;
        }

        /// <summary>
        /// 마우스 위치에 있는 직선(거리 측정) 찾기
        /// </summary>
        private GMapRoute? FindRouteAt(Point mouseLocation)
        {
            foreach (var route in markerOverlay.Routes)
            {
                var localPoints = route.Points.Select(p => App.FromLatLngToLocal(p)).ToList();
                for (int i = 0; i < localPoints.Count - 1; i++)
                {
                    var a = localPoints[i];
                    var b = localPoints[i + 1];
                    double dist = DistanceToSegment(mouseLocation, a, b);
                    if (dist < 8) // 8픽셀 이내 클릭 시 선택
                        return route;
                }
            }
            return null;
        }

        /// <summary>
        /// 두 점(a, b)로 이루어진 선분과 점(p)의 최소 거리
        /// </summary>
        private double DistanceToSegment(Point p, GPoint a, GPoint b)
        {
            double px = p.X, py = p.Y;
            double ax = a.X, ay = a.Y, bx = b.X, by = b.Y;
            double dx = bx - ax, dy = by - ay;
            if (dx == 0 && dy == 0) return Math.Sqrt((px - ax) * (px - ax) + (py - ay) * (py - ay));
            double t = ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            double cx = ax + t * dx, cy = ay + t * dy;
            return Math.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
        }
    }
}
