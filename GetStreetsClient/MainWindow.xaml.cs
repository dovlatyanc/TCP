using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TcpLib;

namespace GetStreetsClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient server = new TcpClient();
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void FindButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {

                await server.ConnectAsync("127.0.0.1", 2048);

                NetworkStream stream = server.GetStream();

                string strIndex = textBoxIndex.Text;

                await server.SendString(strIndex);//отправляем строковый индекс

                string response = await server.ReceiveString();// получаем строку

                string[] streets = response.Split(['\n']);
                foreach (var street in streets)
                {
                    listBoxStreets.Items.Add(street + "\n");//записываем в листбокс построчно
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                server.Close();
            }
        }


    }
}