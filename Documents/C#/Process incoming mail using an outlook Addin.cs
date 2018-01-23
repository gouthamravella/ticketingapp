//Process incoming mail using an outlook Addin

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;

namespace WatchIncomingMailAddin
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Outlook.MAPIFolder inbox = Application.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);    
           inbox.Items.ItemAdd += new Outlook.ItemsEvents_ItemAddEventHandler(InboxFolderItemAdded);        }
        private void InboxFolderItemAdded(object Item)
        {
            if (Item is Outlook.MailItem)
            {
                // New mail item in inbox folder 
                MessageBox.Show("you got mail");
            }
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        #endregion
    }
}

//The code above :

// - sets up namespace to allow us to use the message box (system.windows.forms)

// - set up a session to determine the correct folder that we want to process (inbox)

// - set up an event handler to fire when a new item arrives in the inbox (InboxFolderItemAdded)

 //- In our event handler we set up a message box in the event to alert us

 

//Make sure the outlook process is shut down before running this code as visual studio will process it will automatically at runtime by registering it and tehn fire up outlook

//- Run the Addin

//- wait for a new mail (or send yourself one)

//- You should now see the message fire "You got mail"

 

//As it's an outlook.mailitem, you can do lots of things to this, here are a list of members that the mailitem exposes

//http://msdn.microsoft.com/en-us/library/microsoft.office.interop.outlook.mailitem_members.aspx