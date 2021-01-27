using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GoogleAuth
{
    partial class Form1 : Form
    {
        private string redirectURI = "urn:ietf:wg:oauth:2.0:oob";

        public Form1()
        {
            InitializeComponent();
            txtOutput.Text = "";
        }

        private void btnGetAuthCode_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtClientId.Text == "")
                {
                    txtClientId.Focus();
                    return;
                }

                if (txtScopes.Text == "")
                {
                    txtScopes.Focus();
                    return;
                }

                Process.Start(AuthResponse.GetAutenticationURI(this.txtClientId.Text, redirectURI, this.txtScopes.Text).ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAuth_Click(object sender, EventArgs e)
        {
            try
            {
                AuthResponse access = AuthResponse.Exchange(txtAuthCode.Text, txtClientId.Text, txtClientSecret.Text, redirectURI);

                txtOutput.AppendText("Access_token:" + access.access_token + "\r\n");
                txtOutput.AppendText("Refresh_token:" + access.refresh_token + "\r\n");

                if (DateTime.Now < access.created.AddHours(1))
                {
                    txtOutput.AppendText("Expire:" + access.created.AddHours(1).Subtract(DateTime.Now).Minutes.ToString() + "m\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtClientId.Text == "")
                {
                    txtClientId.Focus();
                    return;
                }

                if (txtClientSecret.Text == "")
                {
                    txtClientSecret.Focus();
                    return;
                }

                if (txtRefreshToken.Text == "")
                {
                    txtRefreshToken.Focus();
                    return;
                }

                string post = string.Format("client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token", txtClientId.Text, txtClientSecret.Text, txtRefreshToken.Text);
                byte[] data = Encoding.UTF8.GetBytes(post);

                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/oauth2/v4/token");
                httpRequest.Method = "POST";
                httpRequest.ContentLength = data.Length;
                httpRequest.ContentType = "application/x-www-form-urlencoded";

                Stream requestStream = httpRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream);
                string html = sr.ReadToEnd();
                sr.Close();
                responseStream.Close();

                AuthResponse resp = AuthResponse.GetResponse(html);

                txtOutput.AppendText("Access_token:" + resp.access_token + "\r\n");
                txtOutput.AppendText("Refresh_token:" + resp.refresh_token + "\r\n");

                if (DateTime.Now < resp.created.AddHours(1))
                {
                    txtOutput.AppendText("Expire:" + resp.created.AddHours(1).Subtract(DateTime.Now).Minutes.ToString() + "m\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}