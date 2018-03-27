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
using System.Net;
using System.IO;

using System.Xml;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jsonWeatherAPItry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string configPath = "config.xml";

        public string location = "shanghai";

        public string username = "HE1802061017581755";

        public string key = "3117847d0b66484f88da525cdc405865";

        public string t = "1477455132";

        public string link = "https://free-api.heweather.com/s6/weather/forecast?";

        string basicStockMarketUrl = "http://stock.liangyee.com/bus-api/stock/freeStockMarketData/getDailyKBar?";


        private Stock SelectedStock;
        public MainWindow()
        {
            InitializeComponent();   
        }

        public static string GetHttpResponse(string url, int Timeout)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = Timeout;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
          //  string userKey = "userKey=291072bc56234a6aa952625adccee3bc&";
            if(textBoxUserKey.Text==string.Empty)
            {
                MessageBox.Show("请输入user key");
                return;
            }
            try
            {
                string userKey = "userKey=" + textBoxUserKey.Text + "&";

                string symbol = "symbol=" + textBoxStockID.Text + "&";
                string startDate = "startDate=" + datePickerStart.SelectedDate.Value.ToString("yyyy-MM-dd") + "&";

                string endDate = "endDate=" + datePickerEnd.SelectedDate.Value.ToString("yyyy-MM-dd") + "&";

                TimeSpan t1 = new TimeSpan(datePickerStart.SelectedDate.Value.Ticks);
                TimeSpan t2 = new TimeSpan(datePickerEnd.SelectedDate.Value.Ticks);
                TimeSpan span = t1.Subtract(t2).Duration();

                int days = span.Days;
                string type = "type=" + comboBox.SelectedIndex.ToString();
                string requestURLString = basicStockMarketUrl + userKey + symbol + startDate + endDate + type;

                string responsed = GetHttpResponse(requestURLString, 5000);
                JObject JObj = JObject.Parse(responsed);
                //string resultString = (string)JObj["result"][0];
                //string[] spilted = resultString.Split(',');
                //textBlock0.Text = spilted[0];
                //textBlock1.Text = spilted[1];
                //textBlock2.Text = spilted[2];
                //textBlock3.Text = spilted[3];
                //textBlock4.Text = spilted[4];
                //textBlock5.Text = spilted[5];
                //textBlock6.Text = spilted[6];

                SelectedStock = new Stock();
                SelectedStock.Type = comboBox.SelectedIndex;
                SelectedStock.StockID = textBoxStockID.Text;

                tabControl.SelectedIndex = 1;
                textBlockCurrentStockID.Text = "当前股票号码: " + SelectedStock.StockID.ToString();
                dataGridStockData.ItemsSource = GetData(JObj);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //abandoned
        //private StockData[] GetData(JObject jsonOBJ,int duration)
        //{
        //    List<StockData> data = new List<StockData>();

        //    for (int i = 0; i < duration; i++)
        //    {
        //        string resultString = (string)jsonOBJ["result"][i];

        //        string[] toStockData = resultString.Split(',');
        //        StockData single = new StockData();
        //        single.TradeTime = toStockData[0];
        //        single.StartPrice = Convert.ToDouble(toStockData[1]);
        //        single.HighestPrice = Convert.ToDouble(toStockData[2]);
        //        single.LowestPrice = Convert.ToDouble(toStockData[3]);
        //        single.EndPrice = Convert.ToDouble(toStockData[4]);
        //        single.TradeValue = Convert.ToInt32(toStockData[5]);
        //        single.ChangeRate = Convert.ToDouble(toStockData[6]);
                

        //        data.Add(single);
        //    }
        //    return data.ToArray<StockData>();
        //}


        private StockData[] GetData(JObject jsonOBJ)
        {
            List<StockData> data = new List<StockData>();

            string[] resultString = jsonOBJ.SelectToken("result").Select(s => (string)s).ToArray();
            for (int i = 0; i < resultString.Length; i++)
            {
                string[] singleStockData = resultString[i].Split(',');
                StockData single = new StockData();
                single.TradeTime = singleStockData[0];
                single.StartPrice = Convert.ToDouble(singleStockData[1]);
                single.HighestPrice = Convert.ToDouble(singleStockData[2]);
                single.LowestPrice = Convert.ToDouble(singleStockData[3]);
                single.EndPrice = Convert.ToDouble(singleStockData[4]);
                single.TradeValue = Convert.ToInt32(singleStockData[5]);
                single.ChangeRate = Convert.ToDouble(singleStockData[6]);


                data.Add(single);
            }
            return data.ToArray<StockData>();

        }

        private string GetUserKey()
        {
            try
            {
                XmlDocument config = new XmlDocument();
                config.Load(configPath);

                string xPathUserKey = "//UserKey";
                XmlNode nodeUserKey = config.SelectSingleNode(xPathUserKey);
                return nodeUserKey.InnerText;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }

        }

        private Stock[] GetPreferredStock()
        {
            try
            {
                XmlDocument config = new XmlDocument();
                config.Load(configPath);

                string xPathPreferredStock = "//PreferredStock";
                XmlNodeList nodeListPreferredStock = config.SelectNodes(xPathPreferredStock);
                List<Stock> stockList = new List<Stock>();
                foreach(XmlNode node in nodeListPreferredStock)
                {
                    Stock single = new Stock();
                    single.Type = Convert.ToInt32(node.Attributes["type"].Value);
                    single.StockID = node.InnerText;
                    stockList.Add(single);
                }

                return stockList.ToArray<Stock>();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void SetDefaultUserKey(string toSet)
        {
            try
            {
                XmlDocument config = new XmlDocument();
                config.Load(configPath);

                string xPathUserKey = "//UserKey";
                XmlNode nodeUserKey = config.SelectSingleNode(xPathUserKey);
                nodeUserKey.InnerText = toSet;

                config.Save(configPath);
                MessageBox.Show("设置成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("fail to set " + ex.Message);
            }
        }

        private void AddPreferredStock(int type,string stockID)
        {
            XmlDocument config = new XmlDocument();
            config.Load(configPath);
            XmlElement root = config.DocumentElement;

            XmlElement newStock = config.CreateElement("PreferredStock");
            XmlAttribute stockTypeAttribute = config.CreateAttribute("type");
            stockTypeAttribute.Value = type.ToString();
            XmlText stockIDText = config.CreateTextNode(stockID);

            newStock.Attributes.Append(stockTypeAttribute);
            newStock.AppendChild(stockIDText);
            root.InsertAfter(newStock, root.LastChild);

            config.Save(configPath);
        }


        private void buttonSetAsDefault_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultUserKey(textBoxUserKey.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string url = link + "location=" + location + "&key=" + key;

            string responsed = GetHttpResponse(url, 5000);

            //    string jsonResult = System.Text.RegularExpressions.Regex.Replace(responsed, "[\\]", "");

            JObject JObj = JObject.Parse(responsed);

            string city = (string)JObj["HeWeather6"][0]["basic"]["location"];

            string highTmp = (string)JObj["HeWeather6"][0]["daily_forecast"][0]["tmp_max"];

            string lowTmp = (string)JObj["HeWeather6"][0]["daily_forecast"][0]["tmp_min"];

            textBlockLocation.Text = city;
            textBlockTMPMax.Text = highTmp + "°C";
            textBlockTMPMin.Text = lowTmp + "°C";
            textBoxUserKey.Text = GetUserKey();

            datePickerStart.Text = DateTime.Now.AddDays(-1).ToShortDateString();
            datePickerEnd.Text = DateTime.Now.ToShortDateString();

            dataGridPreferred.ItemsSource = GetPreferredStock();


        }

        private void dataGridPreferred_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedStock = (Stock)dataGridPreferred.SelectedItem;
            
            comboBox.SelectedIndex = SelectedStock.Type;
            textBoxStockID.Text = SelectedStock.StockID;

        }

        private void buttonAddPreferredStock_Click(object sender, RoutedEventArgs e)
        {
            AddPreferredStock(comboBox.SelectedIndex, textBoxStockID.Text);
        }

        private void HyperlinkGetFreeUserKey_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.liangyee.com/apiDetail/a1cc7246-c806-453b-9cbe-022b599360b8");
        }

        private void hyperLinkAuthorsGitHub_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/tongWQ/StockMarketAndWeatherAPITry");
        }

        private void buttonRefreshDataGridPreferred_Click(object sender, RoutedEventArgs e)
        {   
            dataGridPreferred.ItemsSource = GetPreferredStock();
        }
    }

    public class StockData
    { 
        public string TradeTime { get; set; }
        public double StartPrice { get; set; }
        public double HighestPrice { get; set; }
        public double LowestPrice { get; set; }
        public double EndPrice { get; set; }
        public int TradeValue { get; set; }
        public double ChangeRate { get; set; }
    }

    public class Stock
    {
        public int Type { get; set; }
        public string StockID { get; set; }
        //public string StockName { get; set; }
    }

}
