using System;
using System.IO;  //req for file operations
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
 
using Microsoft.Office.Interop.Outlook;
 
using System.Data.SqlClient;
 
 
namespace mailborg3
{
    public partial class Form1 : Form
    {
 
        Microsoft.Office.Interop.Outlook.Application outlk = new Microsoft.Office.Interop.Outlook.ApplicationClass();
        MailItem t;
        string constr = "Data Source=.\\SQLEXPRESS;Initial Catalog=mailborg3;Integrated Security=True";
        SqlDataAdapter da = new SqlDataAdapter("select * from mail", "Data Source=.\\SQLEXPRESS;Initial Catalog=mailborg3;Integrated Security=True");
        SqlDataAdapter da2 = new SqlDataAdapter("select * from attachment", "Data Source=.\\SQLEXPRESS;Initial Catalog=mailborg3;Integrated Security=True");
        DataSet ds = new DataSet();
        DataSet ds2 = new DataSet();
 
        private MAPIFolder selectedFolder = null;
 
        public Form1()
        {
            InitializeComponent();
        }
 
        private void button1_Click(object sender, EventArgs e)
        {
            getOutlook();
        }
        public void getOutlook()
        {
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            da.UpdateCommand = cb.GetUpdateCommand();
            SqlCommandBuilder cb2 = new SqlCommandBuilder(da2);
            da2.UpdateCommand = cb2.GetUpdateCommand();
            da.Fill(ds);
            da2.Fill(ds2);
            NameSpace NS = outlk.GetNamespace("MAPI");
 
            selectedFolder = NS.PickFolder();
            getFolderMail(selectedFolder, selectedFolder.Name);
 
            //MAPIFolder inboxFld = NS.GetDefaultFolder(OlDefaultFolders.olFolderInbox  );
            //getFolderMail(inboxFld, "Inbox");
            //MAPIFolder junkFld = NS.GetDefaultFolder(OlDefaultFolders.olFolderJunk)  ;
            //getFolderMail(junkFld, "Junk");
            //MAPIFolder sentFld = NS.GetDefaultFolder(OlDefaultFolders.olFolderSentMail);
            //getFolderMail(sentFld, "Sent");
            //MAPIFolder outboxFld = NS.GetDefaultFolder(OlDefaultFolders.olFolderOutbox);
            //getFolderMail(outboxFld, "Outbox");
            //MAPIFolder draftFld = NS.GetDefaultFolder(OlDefaultFolders.olFolderDrafts);
            //getFolderMail(draftFld, "Draft");
            //MAPIFolder deleteFld = NS.GetDefaultFolder(OlDefaultFolders.olFolderDeletedItems);
            //getFolderMail(deleteFld, "Delete");
 
        }
 
        public void getFolderMail(MAPIFolder folder, string foldername)
        {
 
 
            int mailID = 0;
            int nAttachCount = 0;
 
            for (int i = 1; i <= folder.Items.Count; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                try
                {
                    DataRow dr = ds.Tables[0].NewRow();
                    t = (MailItem)folder.Items[i];
 
                    int size = t.Size / 1000;
                    string sizeinK = size.ToString() + "K";
                    dr["folder"] = foldername;
                    dr["fromName"] = t.SenderName;
                    dr["fromID"] = t.SenderEmailAddress;
                    dr["toName"] = t.ReceivedByName;
                    dr["toID"] = t.To;
                    dr["cc"] = t.CC;
                    dr["bcc"] = t.BCC;
                    dr["subject"] = t.Subject;
                    dr["body"] = t.Body;
                    dr["date"] = t.SentOn ;
                    dr["attachment"] = t.Attachments.Count;
                    dr["size"] = sizeinK;
                    dr["readStatus"] = t.UnRead;
 
                    ds.Tables [0].Rows.Add(dr);
                    da.Update(ds);
 
                    textBox3.Text = Convert.ToString(i); //update mail count
                    //System.Windows.Forms.Application.DoEvents();
 
                    if (t.Attachments.Count > 0)
                    {
                        mailID = getMailID();
                        for (int j = 1; j <= t.Attachments.Count; j++)
                        {
                            DataRow dra = ds2.Tables[0].NewRow();
 
                            dra["mailID"] = mailID;
 
                            dra["Name"] = t.Attachments[j].DisplayName;
                            string filePath =Path.GetDirectoryName(System.Windows.Forms.Application.StartupPath ) +t.Attachments[j].FileName;  // @"G:/prabu"
                            t.Attachments[j].SaveAsFile(filePath);
                            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                            int length = (int)fs.Length;
                            byte[] content = new byte[length];
                            fs.Read(content, 0, length);
                            dra["contents"] = content;
                            dra["contentSize"] = length;
 
                            fs.Close();
                            FileInfo f = new FileInfo(filePath);
                            f.Delete();
                            ds2.Tables[0].Rows.Add(dra);
                            da2.Update(ds2);
 
                            nAttachCount += 1;
                            textBox2.Text = Convert.ToString(nAttachCount); //update attachment count
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                }
                catch (System .Exception  ex)
                {
                    Console.WriteLine(ex.ToString());
                }
 
            }
        }
 
        public int getMailID()
        {
            int mailID = 0;
            SqlDataAdapter da1 = new SqlDataAdapter("select max(mailID) as newMailID from mail", constr);
            DataSet ds1 = new DataSet();
            da1.Fill(ds1);
            foreach (DataRow dr in ds1.Tables[0].Rows)
            {
                mailID = Convert.ToInt32(dr["newMailID"]);
            }
            return mailID;
        }
 
        static void initDB()
        {
            //SqlConnection thisConnection = new SqlConnection("server=(local)\\SQLEXPRESS;database=MyDatabase;Integrated Security=SSPI");
            SqlConnection thisConnection = new SqlConnection("server=.\\SQLEXPRESS;Integrated Security=True");
            SqlCommand nonqueryCommand = thisConnection.CreateCommand();
 
            try
            {
                thisConnection.Open();
 
                nonqueryCommand.CommandText = "DROP DATABASE mailborg3";
                Console.WriteLine(nonqueryCommand.CommandText);
 
                nonqueryCommand.ExecuteNonQuery();
                Console.WriteLine("Existing DataBase Destroyed");
 
                nonqueryCommand.CommandText = "CREATE DATABASE mailborg3";
                Console.WriteLine(nonqueryCommand.CommandText);
 
                nonqueryCommand.ExecuteNonQuery();
                Console.WriteLine("Database created, now switching");
                thisConnection.ChangeDatabase("mailborg3");
 
                nonqueryCommand.CommandText = "CREATE TABLE mail("
                    //+ "pindex INT PRIMARY KEY,"
                    //+ "mailID int REFERENCES attachment(mailID) PRIMARY KEY," // foreign key to attachment DB
                                            + "mailID INT IDENTITY(1,1) PRIMARY KEY,"
                                            + "folder VARCHAR(MAX),"
                                            + "fromName VARCHAR(MAX),"
                                            + "fromID VARCHAR(MAX),"
                                            + "toName VARCHAR(MAX),"
                                            + "toID VARCHAR(MAX),"
                                            + "cc VARCHAR(MAX),"
                                            + "bcc VARCHAR(MAX),"
                                            + "subject VARCHAR(MAX),"
                                            + "body VARCHAR(MAX),"
                                            + "date DATETIME,"
                                            + "attachment INT,"
                                            + "size VARCHAR(MAX),"
                                            + "readStatus BIT"
                                            + ")";
                Console.WriteLine(nonqueryCommand.CommandText);
                Console.WriteLine("Number of Rows Affected is: {0}", nonqueryCommand.ExecuteNonQuery());
                    //+ " GO"
                nonqueryCommand.CommandText = " CREATE TRIGGER trgDateTimeUNQ"
                                            + " ON mail FOR INSERT, UPDATE"
                                            + " AS"
                                            + " IF EXISTS(SELECT I.date"
                                            + " FROM inserted AS I JOIN mail AS C"
                                            + " ON I.date = C.date"
                                            + " WHERE I.date <> ''"
                                            + " GROUP BY I.date"
                                            + " HAVING COUNT(*) > 1)"
                                            + " BEGIN"
                                            + "  RAISERROR('Duplicates found. Transaction rolled back.', 10, 1)"
                                            + "  ROLLBACK TRAN"
                                            + " END";
                                            //+ " GO";
 
 
                Console.WriteLine(nonqueryCommand.CommandText);
                Console.WriteLine("Number of Rows Affected is: {0}", nonqueryCommand.ExecuteNonQuery());
 
 
 
                nonqueryCommand.CommandText = "CREATE TABLE attachment("
                                            + "pindex INT IDENTITY(1,1) PRIMARY KEY,"
                                            + "mailID int REFERENCES mail(mailID)," // foreign key to mail DB
                                            //+ "mailID INT PRIMARY KEY,"
                                            + "Name VARCHAR(MAX),"
                                            + "contents VARCHAR(MAX),"
                                            + "contentSize VARCHAR(MAX)"
                                            + ")";
                Console.WriteLine(nonqueryCommand.CommandText);
                Console.WriteLine("Number of Rows Affected is: {0}", nonqueryCommand.ExecuteNonQuery());
 
 
 
 
 
                //nonqueryCommand.CommandText = "INSERT INTO mailID VALUES (99)";
                //Console.WriteLine(nonqueryCommand.CommandText);
                //Console.WriteLine("Number of Rows Affected is: {0}", nonqueryCommand.ExecuteNonQuery());
 
                }
                catch (SqlException ex)
                {
 
                    Console.WriteLine(ex.ToString());
 
                }
                finally
                {
 
                    thisConnection.Close();
                    Console.WriteLine("Connection Closed.");
 
                }
            }
 
 
 
        private void Form1_Load(object sender, EventArgs e)
        {
 
        }
 
        private void button2_Click(object sender, EventArgs e)
        {
            initDB();
        }
 
        private void button3_Click(object sender, EventArgs e)
        {
 
            SqlConnection thisConnection = new SqlConnection("server=.\\SQLEXPRESS;Integrated Security=True");
            //create a new sql command object of type dynamic sql 
            SqlCommand cmd = thisConnection.CreateCommand(); 
            cmd.CommandType = CommandType.Text;
            //textBox1.Text = "";
            dataGrid1.DataSource = null;
            dataGrid1.DataMember = null;
 
            try
            {
                thisConnection.Open();
 
                //set the sql command to whatever is in the textbox 
                cmd.CommandText = textBox1.Text;
 
                //create a new data adapter and bind it to the command obejct 
                SqlDataAdapter daQuery = new SqlDataAdapter();
                daQuery.SelectCommand = cmd;
 
                //create a new dataset and fill it with results based adapter 
                DataSet dsQuery = new DataSet();
                daQuery.Fill(dsQuery, "Results");
 
                //bind the local variable dataset to the datagrid on the form 
                dataGrid1.DataSource = dsQuery;
                dataGrid1.DataMember = "Results";
            }
            catch (SqlException ex)
            {
 
                Console.WriteLine(ex.ToString());
 
            }
            catch (System.Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
 
                finally
                {
 
                    thisConnection.Close();
                    Console.WriteLine("Connection Closed.");
 
                }
        }
 
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
 
        }
 
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
 
        }
    }
}
 