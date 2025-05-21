using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarkerTest
{
    internal class MapControl
    {
        public GMapControl App;
        private GMapOverlay markerOverlay;
        private ContextMenuStrip contextMenu;

        private PointLatLng lastRightClickLatLng;

        public MapControl(GMapControl app)
        {
            this.App = app;

            this.App.MapProvider = GMapProviders.OpenStreetMap;
            //this.App.MapProvider = GMapProviders.GoogleMap;

            this.App.MaxZoom = 20;
            this.App.MinZoom = 6;

            // 타일 구분 그리드 라인 및 위도 / 경도 표시
            this.App.ShowTileGridLines = true;
            this.App.Position = new GMap.NET.PointLatLng(37.678830, 126.779361);

            showLatLan();

            this.App.Zoom = 13;

            markerOverlay = new GMapOverlay("markers");
            this.App.Overlays.Add(markerOverlay);

            InitializeContextMenu();

            this.App.MouseClick += (s, e) =>
            {
                //if (e.Clicks > 1) { return; }

                PointLatLng p = App.FromLocalToLatLng(e.X, e.Y);

                if (e.Button == MouseButtons.Right)
                {
                    // 오른쪽 클릭 시 컨텍스트 메뉴 표시
                    lastRightClickLatLng = p;
                    contextMenu.Show(App, e.Location);
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
            clearMarkersItem.Click += (s, e) =>
            {
                markerOverlay.Markers.Clear();
            };

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                moveHereItem,
                addMarkerItem,
                clearMarkersItem
            });
        }
    }
}
