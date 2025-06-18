using System;
using System.Collections.Generic;
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
    float xVel = 40;
    float yVel = 0;
    float mass = 100;
    float angle = 0;
    float dangle = 0;
    float friction = 0.8f;

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
            Interval = 20
        };
        timer.Tick += (o, e) =>
        {
            int N = 100;
            for (int i = 0; i < N && Move(0.02f / N); i++);
            g.Clear(Color.Black);
            Draw();
            pb.Refresh();
        };
        form.Load += (o, e) => timer.Start();
        form.FormClosed += (o, e) => timer.Stop();
    }

    bool Move(float dt)
    {;
        var dx = xPos - 400;
        var dy = yPos - 400;
        var dist2 = dx * dx + dy * dy;
        var dist = MathF.Sqrt(dist2);
        var height = dist - EarthRadius;

        if (height < 30 || height > 70)
        {
            timer.Stop();
            MessageBox.Show("A missão falhou!");
            form.Close();
            return false;
        }

        var mxForce = OS.Actuators[0];
        var mgForce = OS.Actuators[1];
        var antenna = OS.Actuators[2];

        xPos += xVel * dt;
        yPos += yVel * dt;

        var gravity = EarthMass * mass / dist2;
        var ux = dx / dist;
        var uy = dy / dist;

        friction = float.Clamp(friction + 0.1f * Random.Shared.NextSingle() - 0.05f, 0.6f, 1f);

        var fx = -gravity * ux + mxForce * uy - xVel * friction;
        var fy = -gravity * uy - mxForce * ux - yVel * friction;

        xVel += fx * dt / mass;
        yVel += fy * dt / mass;

        angle += dangle * dt;
        dangle += mgForce * dt / mass;

        // Velocidade Tangencial
        var TanVel = uy * xVel - ux * yVel;
        OS.Sensors[0] = TanVel;

        // Altura
        OS.Sensors[1] = height;

        return true;
    }

    void Draw()
    {
        List<(string text, float y)> infos = [
            ($"Velocide Tangencial: {Math.Round(OS.Sensors[0], 2)}", 0),
            ($"Altura: {Math.Round(OS.Sensors[1], 2)}", 20),
        ];
        foreach (var (text, y) in infos)
        {
            g.DrawString(
                text,
                SystemFonts.MenuFont,
                Brushes.White,
                new PointF(0, y)
            );
        }

        g.FillEllipse(
            new SolidBrush(Color.FromArgb(0, 40, 0)),
            new RectangleF(
                400 - EarthRadius - 70, 
                400 - EarthRadius - 70,
                2 * EarthRadius + 140,
                2 * EarthRadius + 140)
        );
        g.FillEllipse(
            new SolidBrush(Color.Black),
            new RectangleF(
                400 - EarthRadius - 30, 
                400 - EarthRadius - 30,
                2 * EarthRadius + 60,
                2 * EarthRadius + 60)
        );

        g.FillEllipse(
            new SolidBrush(Color.FromArgb(20, 120, 255)),
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