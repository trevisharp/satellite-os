using System;
using System.Drawing;
using System.Windows.Forms;

namespace SatelliteOS;

internal class SatelliteView
{
    readonly Form form;
    readonly Bitmap image;
    readonly Graphics g;
    readonly PictureBox pb;
    readonly Timer timer;

    const float EarthMass = 150 * 1600;
    const float EarthRadius = 100;
    float xPos = 400;
    float yPos = 250;
    float xVel = 0;
    float yVel = 0;
    float mass = 100;

    public SatelliteView()
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
        var dt = 0.05f;
        var dx = xPos - 400;
        var dy = yPos - 400;
        var dist2 = dx * dx + dy * dy;
        var dist = MathF.Sqrt(dist2);
        
        if (dist < EarthRadius)
        {
            timer.Stop();
            MessageBox.Show("A missão falhou!");
            form.Close();
            return;
        }

        var force = EarthMass * mass / dist2;
        var ux = dx / dist;
        var uy = dy / dist;

        xVel -= force * ux * dt / mass;
        yVel -= force * uy * dt / mass;

        xPos += xVel * dt;
        yPos += yVel * dt;
    }

    void Draw()
    {
        g.FillEllipse(
            new SolidBrush(Color.FromArgb(120, 120, 255)),
            new RectangleF(
                400 - EarthRadius, 
                400 - EarthRadius,
                2 * EarthRadius,
                2 * EarthRadius)
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