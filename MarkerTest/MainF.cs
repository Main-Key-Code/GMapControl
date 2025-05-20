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

            dynLabel = new Label();
            //map.App.Controls.Add(dynLabel = map.ShowLatLngLabel());

            map.App.Controls.Add(dynLabel = new Label());

            dynLabel.BackColor = Color.Transparent;
            //dynLabel.Text = string.Empty;
            dynLabel.Text = $"{map.showLat} : {map.showLng}";
            dynLabel.Location = new Point(10, 10);
            dynLabel.AutoSize = true;
            //dynLabel.Size = new Size();
        }

        private Label ShowLatLngLabel()
        {
            Label label = new Label();
            label.BackColor = Color.Transparent;
            label.Text = $"{map.showLat} : {map.showLng}";
            label.Location = new Point(10, 10);
            label.AutoSize = true;
            return label;
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
