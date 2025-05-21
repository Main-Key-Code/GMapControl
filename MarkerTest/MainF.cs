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
            // 
            this.FormClosing += (s, e) =>
            {
                if (map != null)
                {
                    // ������ �ð��� �ʿ� �ϴϱ�
                    // �׷��� ���� �ʿ� �ұ?
                    // �Ǵ��� �ȵǳ�. . . 

                    ShowCloseMessage();
                    map.App.Dispose();
                }
            };

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


            // ��/�浵 ǥ�� ���̺� �߰�
            map.App.Controls.Add(dynLabel = map.ShowLatLngLabel());
        }

        private void ShowCloseMessage()
        {
            Form closeMessageBoxF = new Form();
            closeMessageBoxF.Text = "Close Message Box";
            closeMessageBoxF.Size = new Size(300, 200);
            closeMessageBoxF.StartPosition = FormStartPosition.CenterParent;
            closeMessageBoxF.FormBorderStyle = FormBorderStyle.FixedDialog;
            closeMessageBoxF.MaximizeBox = false;
            closeMessageBoxF.MinimizeBox = false;
            closeMessageBoxF.ShowInTaskbar = false;
            closeMessageBoxF.TopMost = true;
            closeMessageBoxF.BackColor = Color.White;

            closeMessageBoxF.ShowDialog();

        }
        private void OnMapZoomChanged()
        {
            map.MovePoint(map.showLat, map.showLng);
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

        private void OnPositionChanged(PointLatLng point)
        {
            map.showLatLan();
            dynLabel.Text = $"{map.showLat}:{map.showLng}";
        }

    }
}
