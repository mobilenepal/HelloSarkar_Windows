using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Xml;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.IO;
using System.Device.Location;
using System.Net.NetworkInformation;
using System.Text;




namespace HelloSarkar
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        GeoCoordinateWatcher watcher;


        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            DateTime date = DateTime.Now;
            textBoxDate.Text = date.ToShortDateString();
            watcher = new GeoCoordinateWatcher();

            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

            watcher.Start();        

        }
       
        public void saveData()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
        }
        
        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            textBoxGPS.Text = (e.Position.Location.Latitude.ToString()) + "," + (e.Position.Location.Longitude.ToString());
        }

        public class Person
        {
            string Name;
            string Category;
            string District;
            string DetailAddress;
            string MobileNumber;
            DateTime DateofIncident;
            GeoCoordinate GPS;
            string Complaints;

            public string name
            {
                get { return Name; }
                set { Name = value; }
            }
            public string category
            {
                get { return Category; }
                set { Category = value; }
            }
            public string district
            {
                get { return District; }
                set { District = value; }

            }
            public string detailaddress
            {
                get { return DetailAddress; }
                set { DetailAddress = value; }
            }
            public string mobilenumber
            {
                get { return MobileNumber; }
                set { MobileNumber = value; }
            }
            public DateTime dateofincident
            {
                get { return DateofIncident; }
                set { DateofIncident = value; }
            }            
            public string complaints
            {
                get { return Complaints; }
                set { Complaints = value; }
            }
        }
       
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {

           bool isAvailable = NetworkInterface.GetIsNetworkAvailable();
           
            if (isAvailable == true)
            {
                MessageBox.Show("Sent");
                //send data to server
                var url = "http://apps.mobilenepal.net/hellosarkar/public/complain/receive";

                // Create the web request object 
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // Start the request 
                webRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), webRequest);



                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;

                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Data.xml", FileMode.Create))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Person>));
                        using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                        {
                            serializer.Serialize(xmlWriter, GeneratePersonData());
                        }
                    }
                }

            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Connection not available!", "Save your information?", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                    xmlWriterSettings.Indent = true;

                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Data.xml", FileMode.Create))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(List<Person>));
                            using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                            {
                                serializer.Serialize(xmlWriter, GeneratePersonData());
                            }
                        }
                    }
                }

            }



        }

        //XmlDocument xml = new XmlDocument(); 
        //xml.LoadXml("your string of xml");
        //XmlNode xNode = xml.SelectSingleNode("xpath to a single node"); 

        void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {

            HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
            // End the stream request operation 
            Stream postStream = webRequest.EndGetRequestStream(asynchronousResult);

            // Create the post data 
            // Demo POST data  

            //string postData = "name="+textBoxName.Text+"&complain_type=ed&district_id=40&address="+textBoxAddress.Text+"&complain_text="+textBoxComplaint.Text+"&date="+textBoxDate.Text+"&mobile="+textBoxMobile.Text+"&gps="+textBoxGPS.Text+"&mobile_info=N/A";
            string postData = "name=" + textBoxName.Text + "&complain_type=ed&district_id=40&address=" + textBoxAddress.Text + "&complain_text=" + textBoxComplaint.Text + "&date=" + textBoxDate.Text + "&mobile="  + "&gps=" + textBoxGPS.Text + "&mobile_info=N/A";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());

            // Add the post data to the web request 
            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request 
            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
        }

        void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response;
                //MessageBox.Show(asynchronousResult.ToString());
                // End the get response operation 
                response = (HttpWebResponse)webRequest.EndGetResponse(asynchronousResult);
                Stream streamResponse = response.GetResponseStream();

                StreamReader streamReader = new StreamReader(streamResponse);
                var Response = streamReader.ReadToEnd();
                //MessageBox.Show(Response.ToString());
                //textBox1.Text = Response;
                responsedata.Text = "Response code" + Response.ToString();
                streamResponse.Close();
                streamReader.Close();
                response.Close();

            }
            catch (WebException e)
            {
                // Error                 
            }
        } 



        private List<Person> GeneratePersonData()
        {
            List<Person> data = new List<Person>();
            string gpss = textBoxGPS.Text;
            data.Add(new Person() { name = textBoxName.Text, category = textBoxCategory.Text, district = textBoxDistrict.Text, detailaddress = textBoxAddress.Text, mobilenumber = textBoxMobile.Text, complaints = textBoxComplaint.Text });
            return data;
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Data.xml", FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Person>));
                        List<Person> data = (List<Person>)serializer.Deserialize(stream);
                        this.listBox.ItemsSource = data;
                    }
                }
            }
            catch
            {
                
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            textBoxName.Text = "";
            textBoxCategory.Text = "";
            textBoxDistrict.Text = "";
            textBoxAddress.Text = "";
            
        }

        
    }
    
}


       
        
       

      
       
    


