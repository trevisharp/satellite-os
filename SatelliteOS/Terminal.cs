using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SatelliteOS;

public class Terminal
{
    readonly Form form;
    readonly List<string> content = [];
    public Terminal()
    {
        form = new Form {
            FormBorderStyle = FormBorderStyle.SizableToolWindow,
            BackColor = Color.Black
        };
    }

    public void Show()
    {
        form.Show();
    }
}