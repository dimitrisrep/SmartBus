using SmartTouristBus.UI;

namespace SmartTouristBus.Forms;

//afto xrisimooipoiw se ola ta Forms. pairnw oti exei hdh etoimo to form twn windows, kai epipleon o,ti prosthetw egw
//abstract giati klironwmoun apo afti
public abstract class AppFormBase : Form
{
    protected virtual string HelpTopic => "Αρχική";

    protected AppFormBase(string title, Size? minimumSize = null) //mporw na dwsw o.ti timi thelw. mporei na einai ka null. episis einai to default

    {
        Theme.ApplyForm(this, title, minimumSize);
        KeyDown += (_, eventArgs) => //+= syndese ton handler sto event. 1h parametros gia sender den mas en diaferei, defteri einai gia to pliktro pou patithike
        {
            if (eventArgs.KeyCode != Keys.F1) return;
            HelpForm.ShowTopic(HelpTopic, this);
            eventArgs.Handled = true;
        };
    }

 
}
