using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;

namespace lab_facial_recognition_forms
{
    public partial class Form1 : Form
    {
        //declaring variables to use them in the project
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, byte> Frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> Users = new List<string>();
        int Count, NumLables, t;
        string name, names = null;
        bool isFaceDetected;
        int counter;

        static List<User> turdsUser;
        static string connectionString = @"Data Source=(localdb)\MSSQLLocalDB; Initial Catalog= TURDSDB; Integrated Security= True;";
        SqlConnection con = new SqlConnection(connectionString);

        public Form1()
        {
            InitializeComponent();
            this.Text = "T.U.R.D.S. (Tiny User Recognition and Designator System)";
            this.Size = new System.Drawing.Size(770, 750);
            this.MaximizeBox = false;
            failedWebcamLabel.Hide();

            //Adding that string to the list on execution to make it simpler for the Excel File creation
            currentUserListBox.Items.Add("User Name, Date Logged on");

            








            //haarcascade is for face detection
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                //labelsinf reads text from herever the file startsup + the folder Faces and the file txt
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(','); //splits each object in Labels array with ','
                //the first label before ',' will be the number of faces saved.
                NumLables = Convert.ToInt16(Labels[0]);
                Count = NumLables;
                string FacesLoad;
                for (int i = 1; i < NumLables + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + $"/Faces/{FacesLoad}"));
                    labels.Add(Labels[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No match in the Database");
                //throw;
            }
        }







        //BUTONS==============================================================


        private void saveButton_Click(object sender, EventArgs e)
        {
            Count = Count + 1;
            grayFace = camera.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] DetectedFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
            foreach (MCvAvgComp f in DetectedFaces[0])
            {
                TrainedFace = Frame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }
            TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            trainingImages.Add(TrainedFace);
            labels.Add(textName.Text);
            File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");
            for (int i = 1; i < trainingImages.ToArray().Length +1; i++)
            {
                trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", labels.ToArray() [i-1] + ", ");

            }
            MessageBox.Show(textName.Text + ": " + "Added Successfully!");

            
        }




        private void userButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"This many users exist: {labels.Count}. \n\n This is the first label in the Array: {labels[0]}" +
                $"" +
                $"\n\n number of labels: {NumLables}");
            //currentUserListBox.Items.Add("added to listbox");
        }

        private void cameraBox_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void imageBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void logInButton_Click(object sender, EventArgs e)
        {

            var newTurdsUser = new User()
            {
                UserName = name,
                SpartaId = counter++,
                TimeArrived = DateTime.Now
            };

            using (var db = new TURDSDBEntities1())
            {
                //turdsUser.Add(newTurdsUser);
                db.Users.Add(newTurdsUser);
                db.SaveChanges();
            }
            SqlShowAll();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void displayDataButton_Click(object sender, EventArgs e)
        {
            SqlShowAll();
        }

        private void start_Click(object sender, EventArgs e)
        {
            //if when connecting to webcam an exception comes then show the message box to try again
            try
            {
                camera = new Capture();
                camera.QueryFrame();
                Application.Idle += new EventHandler(FrameProcedure);
                failedWebcamLabel.Hide();

            }
            catch (Exception)
            {

                //MessageBox.Show("No Webcam detected!\n\n Please connect one to continue.");
                failedWebcamLabel.Show();
            }
            
            
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            //labelsinf reads text from herever the file startsup + the folder Faces and the file txt
            string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
            string[] Labels = Labelsinf.Split(','); //splits each object in Labels array with ','

            con.Open();
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DELETE from Users where UserName='"+textboxDelete.Text+"'";
            cmd.ExecuteNonQuery();
            con.Close();
            SqlShowAll();
            MessageBox.Show($"All Log-ons for User: {textboxDelete.Text} \n\nDeleted successfully");
            textboxDelete.Clear();

            //subtracts 1  from NumLables
            //NumLables -= 1;
            //var results = Array.FindAll(Labels, s => s.Equals(textboxDelete.Text));
            

        }

        private void exportToExcel_Click(object sender, EventArgs e)
        {
            List<string> currentUserList = new List<string>();
            var tempString = "";

            foreach (var item in currentUserListBox.Items)
            {
                currentUserList.Add(item.ToString());
                tempString += item.ToString();
                tempString += ", " + "\n\n";
            }
            //tempString is now the whole list
            //currentUserListBox.
            //MessageBox.Show(tempString);
            File.WriteAllLines("data.txt", currentUserList.ToArray());
            File.WriteAllLines("data.csv", currentUserList.ToArray());

            Console.WriteLine("Data Written");
            Process.Start("EXCEL.exe", "data.csv");
        }

        private void currentUserListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var form1 = new Form1();
            var loginForm = new LoginForm();
            this.Close();
            
            

            
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }




        //=========================================================================================
        //EXTRA METHODS=============================================================================

        private void FrameProcedure(object sender, EventArgs e)
        {
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100,100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(Count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new System.Drawing.Point(f.rect.X -2, f.rect.Y -2), new Bgr(Color.Red));

                    
                    isFaceDetected = true;
                    if (name == "")
                    {
                        //donothing
                    }
                    else if (name != "")
                    {
                        currentUserListBox.Items.Add(name + ", " + DateTime.Now);
                    }


                    //if (currentUserListBox.Items.Count > 0)
                    //{
                    //    currentUserListBox.SetSelected(0, true);
                        
                    //}
                    //make it so that after 3milisseconds it clears the list




                }
                //Users[t - 1] = name;
                //currentUserListBox.Items.Clear();
                isFaceDetected = false;
                Users.Add("");
            }
            
            cameraBox.Image = Frame;
            names = "";
            Users.Clear();
            //currentUserListBox.Items.Clear();
            


        }

    private void SqlShowAll()
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Users", sqlCon);
                DataTable dtbl = new DataTable();
                sqlDa.Fill(dtbl);
                dgv1.DataSource = dtbl;

            }
        }

    private void UpdateListBox1()
        {
            //if (isFaceDetected == true)
            //{
            //    currentUserListBox.Items.Add(name);
            //    System.Threading.Thread.Sleep(500);
            //    return;
            //}
            //else
            //{
            //    currentUserListBox.Items.Clear();
            //    System.Threading.Thread.Sleep(500);
            //    return;
            //}
            
            currentUserListBox.Items.Add(name);
            System.Threading.Thread.Sleep(500);
            



        }
    }
}
