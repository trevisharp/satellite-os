using System;
using System.Drawing;
using System.Windows.Forms;

namespace SatelliteOS;

internal class View
{
    readonly Form form;
    readonly Bitmap image;
    readonly Graphics g;
    readonly PictureBox pb;
    readonly Timer timer;

    float xPos = 400;
    float yPos = 250;
    float xVel = 40;
    float yVel = 0;
    float mass = 100;

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

        timer = new Timer {
            Interval = 50
        };
        timer.Tick += (o, e) =>
        {
            Move();
            g.Clear(Color.Black);
            Draw();
            pb.Refresh();
        };
        form.Load += (o, e) => timer.Start();
    }

    void Move()
    {
        // xVel * xVel = M / 150
        var dt = 0.05f;
        var dx = xPos - 400;
        var dy = yPos - 400;
        var dist2 = dx * dx + dy * dy;
        var force = 150 * 1600 * mass / dist2;
        var ux = dx / MathF.Sqrt(dist2);
        var uy = dy / MathF.Sqrt(dist2);

        xVel -= force * ux * dt / mass;
        yVel -= force * uy * dt / mass;

        xPos += xVel * dt;
        yPos += yVel * dt;
    }

    void Draw()
    {
        g.FillEllipse(
            new SolidBrush(Color.FromArgb(120, 120, 255)),
            new RectangleF(300, 300, 200, 200)
        );
        g.FillRectangle(
            new SolidBrush(Color.FromArgb(220, 220, 220)),
            new RectangleF(xPos, yPos, 10, 10)  
        );
    }

    public void Show()
    {
        foreach (var opened in Application.OpenForms)
            if (opened == form)
                return;
        form.Show();
    }
}