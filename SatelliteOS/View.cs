using System.Drawing;
using System.Windows.Forms;

namespace SatelliteOS;

internal class View
{
    readonly Form form;
    readonly Bitmap image;
    readonly Graphics g;
    readonly PictureBox pb;

    public View()
    {
        form = new Form {
            Width = 800,
            Height = 800,
            FormBorderStyle = FormBorderStyle.FixedToolWindow
        };

        image = new Bitmap(800, 800);
        g = Graphics.FromImage(image);
        g.Clear(Color.Black);

        pb = new PictureBox {
            Dock = DockStyle.Fill,
            Image = image
        };
        form.Controls.Add(pb);
    }

    public void Show()
    {
        foreach (var opened in Application.OpenForms)
            if (opened == form)
                return;
        form.Show();
    }
}