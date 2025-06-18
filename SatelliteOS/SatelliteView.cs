using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    float fuel = 60;
    float energy = 100;
    float angle = 0;
    float dangle = 0;
    float friction = 0.8f;
    float t = 0f;
    List<PointF> stars = [];
    List<float> startSizes = [];
    List<float> startOffsets = [];
    Queue<float> signal = [];

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
            t += 0.02f;
            for (int i = 0; i < N && Move(0.02f / N); i++);
            var antenna = OS.Actuators[3];

            signal.Enqueue(antenna);
            if (signal.Count > 250)
                signal.Dequeue();
            g.Clear(Color.Black);
            Draw();
            pb.Refresh();
        };
        form.Load += (o, e) => timer.Start();
        form.FormClosed += (o, e) => timer.Stop();

        for (int i = 0; i < 12; i++)
        {
            float theta = Random.Shared.NextSingle() * MathF.Tau;
            float pho = 180 + Random.Shared.NextSingle() * 220;
            var x = pho * MathF.Cos(theta) + 400;
            var y = pho * MathF.Sin(theta) + 400;
            stars.Add(new PointF(x, y));
            startSizes.Add(0.5f + Random.Shared.NextSingle() * 0.5f);
            startOffsets.Add(Random.Shared.NextSingle() * 5);
        }
    }

    bool Move(float dt)
    {
        var dx = xPos - 400;
        var dy = yPos - 400;
        var dist2 = dx * dx + dy * dy;
        var dist = MathF.Sqrt(dist2);
        var height = dist - EarthRadius;

        if (height < 30 || height > 70)
        {
            timer.Stop();
            MessageBox.Show("The mission failed!");
            form.Close();
            return false;
        }

        var mxForce = 400 * float.Clamp(OS.Actuators[0], -1f, 1f);
        var myForce = 400 * float.Clamp(OS.Actuators[1], -1f, 1f);
        var mgForce = 45 * float.Clamp(OS.Actuators[2], -1f, 1f);

        if (float.Abs(mxForce) > 10)
            energy -= dt / 2;
        if (float.Abs(myForce) > 10)
            energy -= dt / 2;
        if (float.Abs(mgForce) > 10)
            energy -= dt / 2;
        if (energy < 0)
        {
            timer.Stop();
            MessageBox.Show("The mission failed!");
            form.Close();
            return false;
        }

        fuel -= (float.Abs(mxForce) + float.Abs(myForce)) / 800 * dt;
        if (fuel < 0)
        {
            timer.Stop();
            MessageBox.Show("The mission failed!");
            form.Close();
            return false;
        }

        float mass = 40 + fuel;

        xPos += xVel * dt;
        yPos += yVel * dt;

        var gravity = EarthMass * mass / dist2;
        var ux = dx / dist;
        var uy = dy / dist;

        friction = float.Clamp(friction + 0.1f * Random.Shared.NextSingle() - 0.05f, 0.6f, 1f);

        var fx = (myForce-gravity) * ux + mxForce * uy - xVel * friction;
        var fy = (myForce-gravity) * uy - mxForce * ux - yVel * friction;

        xVel += fx * dt / mass;
        yVel += fy * dt / mass;

        angle += dangle * dt;
        dangle += (mgForce + 0.1f * Random.Shared.NextSingle()) * dt / mass;
        var deltaAngle = (angle % 360) - 180 + 45 * MathF.Sin(t / 60);
        var charge = float.Clamp(2000 / (deltaAngle * deltaAngle), 0f, 1f);

        energy = float.Clamp(energy + charge * dt, 0, 100);

        var TanVel = uy * xVel - ux * yVel;
        OS.Sensors[0] = TanVel;
        OS.Sensors[1] = height;
        OS.Sensors[2] = fuel;
        OS.Sensors[3] = energy;
        OS.Sensors[4] = charge;
        OS.Sensors[5] = MathF.Sin(5 * t);

        return true;
    }

    void Draw()
    {
        foreach (var ((point, size), offset) in stars.Zip(startSizes).Zip(startOffsets))
        {
            var pixelSize = 10 * size * (MathF.Sin(MathF.Tau * t / 5 + offset) / 4 + 0.5f);
            g.FillEllipse(
                Brushes.Yellow,
                point.X - pixelSize / 2, point.Y - pixelSize / 2,
                pixelSize, pixelSize
            );
        }

        List<(string text, float y)> infos = [
            ($"Velocide Tangencial: {Math.Round(OS.Sensors[0], 2)}", 0),
            ($"Altura: {Math.Round(OS.Sensors[1], 2)}", 20),
            ($"Combustível: {Math.Round(OS.Sensors[2], 2)}", 40),
            ($"Energia: {Math.Round(OS.Sensors[3], 2)}", 60),
            ($"Recarga: {Math.Round(OS.Sensors[4], 10)}", 80),
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
            new SolidBrush(Color.FromArgb(0, 20, 0)),
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

        var points = signal.Select((v, i) => new PointF(
            20f + 200f * i / 250f, 480 + 120f * (v + 1)
        )).ToArray();
        if (points.Length > 1)
            g.DrawLines(Pens.Red, points);
        g.DrawRectangle(Pens.White, 20, 480, 200, 240);
    }

    public void Show()
    {
        foreach (var opened in Application.OpenForms)
            if (opened == form)
                return;
        form.Show();
    }
}