using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string? message;
        string host = "127.0.0.1";
        int port = 8888;
        StreamReader? Reader = null;
        StreamWriter? Writer = null;
        TcpClient client = new TcpClient();    
        public MainWindow()
        {
            InitializeComponent();
            ChatText.Text += "\n";
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NameOk.IsEnabled = !string.IsNullOrEmpty(NameBox.Text);
        }

        private void NameOk_Click(object sender, RoutedEventArgs e)
        {
            message = NameBox.Text;
            NameOk.IsEnabled = false;
            BeginChat();
        }

        private async void BeginChat()
        {
            try
            {
                client.Connect(host, port);
                Reader = new StreamReader(client.GetStream());
                Writer = new StreamWriter(client.GetStream());
                if (Writer is null || Reader is null) return;
                Task.Run(() => ReciveMessageAsync(Reader));
                await SendMessageAsync(Writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        async Task SendMessageAsync(StreamWriter writer)
        {
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();           
        }

        async Task ReciveMessageAsync(StreamReader reader)
        {
            while (true)
            {
                try
                {
                    string? messageReceive = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(messageReceive)) continue;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { ChatText.Text += $"{messageReceive}\n"; });
                }
                catch { break; }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            client.Close();
            Writer?.Close();
            Reader?.Close();
            this.Close();
        }

        private void MessageText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SendMessage.IsEnabled = !string.IsNullOrEmpty(MessageText.Text);            
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            message = MessageText.Text;
            MessageText.Text = "";
            await SendMessageAsync(Writer);
        }        
    }
}