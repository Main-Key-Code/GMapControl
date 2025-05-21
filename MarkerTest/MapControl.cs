using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkerTest
{
    internal class MapControl
    {
        // gmapcontrol 필드 추가
        public GMapControl App;

        public MapControl(GMapControl app)
        {
            this.App = app;

            //this.App.MapProvider = GMapProviders.OpenStreetMap;
            this.App.MapProvider = GMapProviders.GoogleKoreaSatelliteMap;

            this.App.MaxZoom = 20;
            this.App.MinZoom = 6;

            this.App.Position = new GMap.NET.PointLatLng(37.678830, 126.779361);

            showLatLan();

            this.App.Zoom = 13;

            this.App.MouseClick += (s, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    //AddCustomMarker(showLat, showLng, Properties.Resources.)
                    // 오른쪽 클릭 시 현재 위치로 이동
                    MovePoint(showLat, showLng);
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
        /// 위치 이동
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public void MovePoint(double lat, double lng)
        {
            this.App.Position = new PointLatLng(showLat, showLng);
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

        // 마커 오버레이 필드 추가
        private GMapOverlay markerOverlay;

        /// <summary>
        /// 사용자 지정 이미지로 마커 추가
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="imagePath"></param>
        public void AddCustomMarker(double lat, double lng, string imagePath)
        {
            // 이미지 로드
            Bitmap markerImage = new Bitmap(imagePath);

            // 커스텀 마커 생성
            var marker = new GMarkerGoogle(
                new PointLatLng(lat, lng),
                markerImage
            );

            markerOverlay.Markers.Add(marker);
        }
    }
}
