using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

public abstract class AppFormBase : Form
{
    protected virtual string HelpTopic => "Αρχική";

    protected AppFormBase(string title, Size? minimumSize = null)
    {
        Theme.ApplyForm(this, title, minimumSize);
        KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode != Keys.F1) return;
            HelpForm.ShowTopic(HelpTopic, this);
            eventArgs.Handled = true;
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
