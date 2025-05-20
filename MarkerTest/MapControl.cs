using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkerTest
{
    internal class MapControl
    {
        public GMapControl App;

        public double showLat { set; get; }
        public double showLng { set; get; }

        public MapControl(GMapControl app)
        {
            this.App = app;

            this.App.MapProvider = GMapProviders.OpenStreetMap;
            //this.App.MapProvider = GMapProviders.GoogleKoreaSatelliteMap;



            this.App.MaxZoom = 20;
            this.App.MinZoom = 6;

            this.App.Position = new GMap.NET.PointLatLng(37.678830, 126.779361);

            showLatLan();

            this.App.Zoom = 13;
        }

        public void showLatLan()
        {
            showLat = this.App.Position.Lat;
            showLng = this.App.Position.Lng;
        }

        public void SetPointSave(double lat, double lng)
        {

            this.App.Position = new PointLatLng(showLat, showLng);
        }

        public Label ShowLatLngLabel()
        {
            Label label = new Label();
            label.BackColor = Color.Transparent;
            label.Text = $"{showLat} : {showLng}";
            label.Location = new Point(10, 10);
            label.AutoSize = true;
            label.Font = new Font("Arial", 12, FontStyle.Bold);
            label.ForeColor = Color.White;
            return label;
        }
    }
}
