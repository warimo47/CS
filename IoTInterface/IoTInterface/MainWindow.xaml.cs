using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net; // HttpWebRequest, HttpWebResponse
using System.IO; // Stream
using System.Diagnostics; // Debug

namespace IoTInterface
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void GetIoTPos(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; ++i)
            {
                // Create a request using a URL that can receive a post.
                WebRequest request = WebRequest.Create("http://112.216.130.180:9904/location/latest");
                // Set the Method property of the request to POST.
                request.Method = "POST";

                // Create POST data and convert it to a byte array.
                string postData = "{\"equipmentId\" : \"YT301\",\"sensorId\" : \"scnt-location\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/json";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;

                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();

                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.
                    Debug.WriteLine(responseFromServer);
                }

                // Close the response.
                response.Close();
            }
        }

        private void GetIoTPosTemp(object sender, EventArgs e)
        {
            int errcode = 0;
            string responseText = string.Empty;

            try
            {
                string getIoTPosURL = "http://112.216.130.180:9904/location/latest";
                Stream requestStream = null;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getIoTPosURL);
                request.Method = "POST";
                request.Timeout = 30 * 1000; // 30초
                request.ContentType = "application/json; charset=UTF-8";
                // request.Headers.Add("Content-Type", "application/json");

                requestStream = request.GetRequestStream();
                // requestStream.Write(sendData, 0, sendData.Length);

                requestStream.Flush();
                requestStream.Close();

                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                HttpStatusCode status = resp.StatusCode;
                // Console.WriteLine(status); // 정상이면 "OK"

                Stream respStream = resp.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    responseText = sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                errcode = (int)((HttpWebResponse)ex.Response).StatusCode;
                responseText = ex.Message;
            }
        }
    }
}
