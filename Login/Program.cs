using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Resources;

namespace Login
{
    public class Program : Form
        {

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        
            Application.Run(new Program());
        }

        private NotifyIcon  trayIcon;

        private ContextMenuStrip trayMenu;

        public string User1 = "";
        public string Pass1 = "";
        Thread loginThread;
        Boolean loginThreadStop = false;

        DateTime lastLoginTime;
        Boolean isLogin = false;

        public Program()
        {
            Stream image = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Login.Resources.trash.png");
            Image uninstallImage = Image.FromStream(image);
            image = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Login.Resources.login.png");
            Image loginImage = Image.FromStream(image);
            image = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Login.Resources.user.png");
            Image userImage = Image.FromStream(image);
            image = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Login.Resources.about.png");
            Image aboutImage = Image.FromStream(image);
            image = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Login.Resources.exit.png");
            Image exitImage = Image.FromStream(image);
            trayMenu = new ContextMenuStrip();

            trayMenu.Items.Add("Login", loginImage).Click += new EventHandler(Login_Click);
            trayMenu.Items.Add("Kontoinformation", userImage).Click += new EventHandler(Info_Click);
            trayMenu.Items.Add("Uninstall", uninstallImage).Click += new EventHandler(Uninstall_Click);
            trayMenu.Items.Add("About", aboutImage).Click += new EventHandler(About_Click);
            trayMenu.Items.Add("Exit", exitImage).Click += new EventHandler(Exit_Click);

            trayIcon = new NotifyIcon();
            Icon icnNormal;
         
            System.IO.Stream st;
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            st = a.GetManifestResourceStream("Login.Resources.blue.ico");
            icnNormal = new System.Drawing.Icon(st);


            trayIcon.Text = "Jensen AutoLogin\nSenaste inlogging: - ";

            trayIcon.Icon = icnNormal;

            trayIcon.ContextMenuStrip = trayMenu;

            trayIcon.Visible     = true;
            
            StartUp();

            Thread thread = new Thread(() => Login(User1, Pass1));
            thread.Start();

            Thread thread2= new Thread(() => NextLogin());
            thread2.Start();

        }

        public static DialogResult UserInfo(ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();

            Label label2 = new Label();
            TextBox textBox2 = new TextBox();
            textBox2.PasswordChar = '*';

            Button buttonOk = new Button();


            form.Text = "Jensen AutoLogin - Kontoinformation";
            label.Text = "Användarnamn:";
 
            label2.Text = "Lösenord:";
 
            buttonOk.Text = "OK";
        
            buttonOk.DialogResult = DialogResult.OK;
    

            label.SetBounds(9, 20, 220, 13);
            textBox.SetBounds(12, 36, 220, 20);

            label2.SetBounds(9, 60,220, 13);
            textBox2.SetBounds(12, 76, 220, 20);

            buttonOk.SetBounds(12, 100, 75, 23);


            label.AutoSize = true;


            form.ClientSize = new Size(250, 150);
            form.Controls.AddRange(new Control[] { label, textBox, label2, textBox2, buttonOk });
            form.ClientSize = new Size(Math.Max(250, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
      

            DialogResult dialogResult = form.ShowDialog();

            value = textBox.Text;
            value = value.Replace(" ", "+");
            value = value + ":" + textBox2.Text;

            return dialogResult;
        }

        public void StartUp()
        {
           lastLoginTime = DateTime.Now;
           RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft");
           regKey = regKey.OpenSubKey("Jensen AutoLogin");

           if (regKey == null)
           {
               regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft", true);
               regKey = regKey.CreateSubKey("Jensen AutoLogin");
               regKey.SetValue("User", "");
               regKey.SetValue("Pass", "");
               regKey.Close();
           }

           else
           {
               String tmpvar = (String)regKey.GetValue("User");
               String tmpvar2 = "";
               byte[] data = Convert.FromBase64String(tmpvar);
               tmpvar2 = ASCIIEncoding.ASCII.GetString(data);
               User1 = tmpvar2;

               tmpvar = (String)regKey.GetValue("Pass");
               tmpvar2 = "";
               data = Convert.FromBase64String(tmpvar);
               tmpvar2 = ASCIIEncoding.ASCII.GetString(data);
               Pass1 = tmpvar2;
           }

           regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
           String res = (String)regKey.GetValue("Jensen AutoLogin");
           if (res != Application.ExecutablePath)
           {
               regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
               regKey.SetValue("Jensen AutoLogin", Application.ExecutablePath);           
           }

           regKey.Close();
        }

        public void NextLogin()
        {
            while (!loginThreadStop)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan span = currentTime.Subtract(lastLoginTime);
          
                if (span.Days >= 1  && isLogin == false)
                {
                    loginThread = new Thread(() => Login(User1, Pass1));
                    loginThread.Start();
                }
                Thread.Sleep(50);
            }
        }

        public void Login(string User1, string Pass1)
        {

            if(isLogin)
            {
                MessageBox.Show("Du loggas redan in", "Jensen AutoLogin - Login");
                return;
            }
            else
            {
                isLogin = true;
            }

            System.Net.ServicePointManager.Expect100Continue = false;

            CookieContainer cookieContainer = new CookieContainer();

            String User = User1;
            String Pass = Pass1;

            if (User1 == "")
            {
                MessageBox.Show("Ingen kontoinformation hittades\n1.Fyll i Kontoinformationen\n2.Klicka på Login\n\n*Jensen AutoLogin, loggar automatiskt in var 24:e timma\n efter den första lyckade inloggningen samt vid varje uppstart av programmet", "Jensen AutoLogin - Login");
                isLogin = false;
                return;
            }

            String Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*";
            String UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";

            /////////////////LOGIN//////////////////////////////////////////////////
            WebRequest rqst;
            try
            {
                rqst = HttpWebRequest.Create("https://yh.jenseneducation.se/login.asp?login=y");

                ((HttpWebRequest)rqst).Referer = "https://yh.jenseneducation.se/start.asp?";
                ((HttpWebRequest)rqst).Accept = Accept;
                ((HttpWebRequest)rqst).UserAgent = UserAgent;
                ((HttpWebRequest)rqst).CookieContainer = cookieContainer;

         
            rqst.Method = "POST";

                String postdata = "namn=" + User + "&losen=" + Pass;

                if (!String.IsNullOrEmpty(postdata))
                {
              
                    rqst.ContentType = "application/x-www-form-urlencoded";

                    byte[] byteData = UTF8Encoding.UTF8.GetBytes(postdata);
                    rqst.ContentLength = byteData.Length;
                    using (Stream postStream = rqst.GetRequestStream())
                    {
                        postStream.Write(byteData, 0, byteData.Length);
                        postStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                string b = ex.Message; 
                MessageBox.Show("Sidan kunde inte kontaktas", "Jensen AutoLogin - Login");
                isLogin = false;
                return;
            }

            ((HttpWebRequest)rqst).KeepAlive = true;

            StreamReader rsps = new StreamReader(rqst.GetResponse().GetResponseStream());
          
            string strRsps = rsps.ReadToEnd();

 
            int loggedIn = strRsps.IndexOf("loginform");

            if (loggedIn != -1)
            {
                MessageBox.Show("Inloggning misslyckades", "Jensen AutoLogin - Login");
                isLogin = false;
                return;
            }

            else 
            {
                lastLoginTime = DateTime.Now;
                trayIcon.Text = "Jensen AutoLogin\nSenaste inlogging: " + lastLoginTime;
            }
            /////////////////NEXT REQUEST//////////////////////////////////////////////////

            WebRequest rqst1 = HttpWebRequest.Create("https://yh.jenseneducation.se/default.asp");
            ((HttpWebRequest)rqst1).Accept = Accept;
            ((HttpWebRequest)rqst1).UserAgent = UserAgent;
            ((HttpWebRequest)rqst1).CookieContainer = cookieContainer;

            rqst1.Method = "GET";


            ((HttpWebRequest)rqst1).KeepAlive = true;
            StreamReader rsps1 = new StreamReader(rqst1.GetResponse().GetResponseStream());
            string strRsps1 = rsps1.ReadToEnd();

            ///////////////NEXT REQUEST////////////////////////////////////////////////////

            WebRequest rqst2 = HttpWebRequest.Create("https://yh.jenseneducation.se/includes/top1.asp");
            ((HttpWebRequest)rqst2).Referer = "https://yh.jenseneducation.se/default.asp";
            ((HttpWebRequest)rqst2).Accept = Accept;
            ((HttpWebRequest)rqst2).UserAgent = UserAgent;
            ((HttpWebRequest)rqst2).CookieContainer = cookieContainer;

            rqst2.Method = "GET";

            ((HttpWebRequest)rqst2).KeepAlive = true;
            StreamReader rsps2 = new StreamReader(rqst2.GetResponse().GetResponseStream());
            string strRsps2 = rsps2.ReadToEnd();


            ///////////////NEXT REQUEST////////////////////////////////////////////////////

            WebRequest rqst3 = HttpWebRequest.Create("https://yh.jenseneducation.se/start.asp?");
            ((HttpWebRequest)rqst3).Referer = "https://yh.jenseneducation.se/default.asp";
            ((HttpWebRequest)rqst3).Accept = Accept;
            ((HttpWebRequest)rqst3).UserAgent = UserAgent;
            ((HttpWebRequest)rqst3).CookieContainer = cookieContainer;

            rqst3.Method = "GET";


            ((HttpWebRequest)rqst3).KeepAlive = true;
            StreamReader rsps3 = new StreamReader(rqst3.GetResponse().GetResponseStream());
            string strRsps3 = rsps3.ReadToEnd();

            ///////////////SLEEP///////////////////////////////////////////////////

            Random random = new Random();
            int rand = random.Next(3, 10);
            rand = rand * 1000;
            Thread.Sleep(rand);

            ///////////////LOG_OUT////////////////////////////////////////////////////

            WebRequest rqst4 = HttpWebRequest.Create("https://yh.jenseneducation.se/login.asp?login=n");
            ((HttpWebRequest)rqst4).Referer = "https://yh.jenseneducation.se/includes/top1.asp";
            ((HttpWebRequest)rqst4).Accept = Accept;
            ((HttpWebRequest)rqst4).UserAgent = UserAgent;
            ((HttpWebRequest)rqst4).CookieContainer = cookieContainer;

            rqst4.Method = "GET";


            ((HttpWebRequest)rqst4).KeepAlive = true;
            StreamReader rsps4 = new StreamReader(rqst4.GetResponse().GetResponseStream());
            string strRsps4 = rsps4.ReadToEnd();

            isLogin = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible       = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void Login_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(() => Login(User1, Pass1));
            thread.Start();

            Thread.Sleep(30);
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            loginThreadStop = true;
            Thread.Sleep(100);

            Application.Exit();
        }

        private void Uninstall_Click(object sender, EventArgs e)
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            regKey.DeleteValue("Jensen AutoLogin");

            regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft", true);
            regKey.DeleteSubKey("Jensen AutoLogin");

            MessageBox.Show("Programmet  är borttaget!\nKör det igen för att installera om", "Jensen AutoLogin - Uninstall");
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("©2012 by Mattias Karlsson. All rights reserved", "Jensen Autologin - About");
        }

        private void Info_Click(object sender, EventArgs e)
        {
            String value = "";

            UserInfo(ref value);

            String[] credentials = value.Split(':');

            User1 = credentials[0];
            Pass1 = credentials[1];

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft", true);
            regKey = regKey.OpenSubKey("Jensen AutoLogin", true);

            String tmpvar = User1;
            String tmpvar2 = "";
            byte[] data1 = ASCIIEncoding.ASCII.GetBytes(tmpvar);
            tmpvar2 = Convert.ToBase64String(data1);
            regKey.SetValue("User", tmpvar2);

            tmpvar = Pass1;
            data1 = ASCIIEncoding.ASCII.GetBytes(tmpvar);
            tmpvar2 = Convert.ToBase64String(data1);
            regKey.SetValue("Pass", tmpvar2);

            regKey.Close();

            MessageBox.Show("Kontoinformation sparad!", "Jensen AutoLogin - Kontoinformation");
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
               
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Program";
            this.Load += new System.EventHandler(this.Program_Load);
            this.ResumeLayout(false);

        }

        private void Program_Load(object sender, EventArgs e)
        {
        
        }

    }
}
