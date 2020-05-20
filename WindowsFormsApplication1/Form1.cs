using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Iai.V20180301;
using TencentCloud.Iai.V20180301.Models;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        private Thread main_Thread = null;
        public String path = Application.StartupPath + "\\" ;
        public int count = 1;
        VideoCapture video;
        Mat frame = new Mat();
     
        

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBoxIpl1_Click(object sender, EventArgs e)
        {
          

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            try
            {
                video = new VideoCapture(0);
                video.FrameWidth = 640;
                video.FrameHeight = 480;
            }
            catch
            {
                timer2.Enabled = true;
            }
        }




       
        private void timer2_Tick(object sender, EventArgs e)
        {


            try
            {
                int sleepTime = (int)Math.Round(1000 / video.Fps);

                String filenameFaceCascade = "haarcascade_frontalface_alt.xml";
                CascadeClassifier faceCascade = new CascadeClassifier();

                if (!faceCascade.Load(filenameFaceCascade))
                {
                    Console.WriteLine("error");
                    return;
                }

                video.Read(frame);


               // Rect[] faces = faceCascade.DetectMultiScale(frame);

                //if (faces.Count() > 0)
                //{
                //    Console.WriteLine("faceswww : " + faces[0]);
                //    Cv2.Rectangle(frame, faces[0], Scalar.Red);
                //}

                //foreach (var item in faces)
                //{
                //    Cv2.Rectangle(frame, item, Scalar.Red); // add rectangle to the image
                //    Console.WriteLine("faces : " + item);
                //}

                // display
                pictureBoxIpl1.ImageIpl = frame;

               
                Console.WriteLine("count:" + count);
                int j = count % 10;
                if (j == 0)
                {
                    Cv2.ImWrite("./aaa.png", frame);
                    cloudCheck();
                }
                count++;
                Cv2.WaitKey(sleepTime);
          
            }
            catch (Exception)
            {

            }

           
        }
        public void cloudCheck()
        {
            String aaaa = path + "aaa.png";
            Console.WriteLine(aaaa);
            String ImagBase64 = ImgToBase64String(aaaa);
            //String ImagBase64 = "";
            //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Log.txt", true, Encoding.Default);
            //sw.WriteLine(ImagBase64);
            //sw.Close();
          //  Console.WriteLine(ImagBase64);

  
            try
            {

                Credential cred = new Credential
                {
                    SecretId = "XXXX",
                    SecretKey = "yyyy"
                };

                ClientProfile clientProfile = new ClientProfile();
                HttpProfile httpProfile = new HttpProfile();
                httpProfile.Endpoint = ("iai.tencentcloudapi.com");
                clientProfile.HttpProfile = httpProfile;

                IaiClient client = new IaiClient(cred, "ap-seoul", clientProfile);
                SearchFacesRequest req = new SearchFacesRequest();
                string strParams = "{\"GroupIds\":[\"hjtest\"],\"Image\":\"" + ImagBase64 + "\"}";
                req = SearchFacesRequest.FromJsonString<SearchFacesRequest>(strParams);
                SearchFacesResponse resp = client.SearchFacesSync(req);
                String tt = AbstractModel.ToJsonString(resp);
                //   Console.WriteLine(tt);
                String temp = GetPerson(tt);
                if (temp.Equals("error"))
                {
                    //MessageBox.Show("존재하지 않는 Person");
                }
                else
                {

                }
                // Console.WriteLine(temp);
                ListBoxItemAdd(this, this.listBox1, temp);

            }
            catch (Exception e)
            {
                ListBoxItemAdd(this, this.listBox1, "존재하지 않는 Person");
                Console.WriteLine(e.ToString());
            }
            Console.Read();

        }


        public static string GetPerson(string jsonText)
        {
            String temp = "error";
            JObject jsonObj = JObject.Parse(jsonText);
            // String a = jsonObj["Candidates"].ToString();
            JObject jo = JObject.Parse(jsonText);
            var a = jo.SelectToken("Results");
            foreach (var item in a)
            {
                var Candidates = item.SelectToken("Candidates");
                   Console.WriteLine(Candidates.ToString());
                foreach (var item2 in Candidates)
                {
                    var PersonId = item2.SelectToken("PersonId").ToString();
                    var FaceId = item2.SelectToken("FaceId").ToString();
                    var Score = item2.SelectToken("Score").ToString();
                    Console.WriteLine(PersonId + "  " + FaceId + "  " + Score);
                    temp = PersonId + " " + Score;
                   // MessageBox.Show(temp);
                    break;

                }
            }

            return temp;
        }

        public string ImgToBase64String(string Imagefilename)
        {
            //try
            //{
            Bitmap bmp = new Bitmap(Imagefilename);

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return Convert.ToBase64String(arr);
            //}
            //catch (Exception ex)
            //{
            //
            //    return null;
            //}
        }

        public void Test() { 
        
        }
        public void myStaticThreadMethod()
        {
            Test();
            System.Windows.Forms.Application.Exit();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            main_Thread = new Thread(myStaticThreadMethod);
            main_Thread.Start();
        }


        public static void ListBoxItemAdd(Form frm, ListBox lstbox, string lstitem)
        {
            frm.Invoke(new MethodInvoker(delegate
            {
                if (lstbox.Items.Count + 1 > 200)
                {
                    lstbox.Items.RemoveAt(0);
                }
                lstbox.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " | " + lstitem);
                lstbox.SelectedIndex = lstbox.Items.Count - 1;
                lstbox.Refresh();

            }));
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //button1.Enabled = true;
            //button2.Enabled = false;
            if (this.main_Thread != null)
            {
                this.main_Thread.Abort();
                this.main_Thread = null;
            }
        }



    }
}
