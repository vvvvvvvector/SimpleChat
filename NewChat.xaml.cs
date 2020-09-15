using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Globalization;
using System.Windows.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class NewChat : Window
    {
        Socket accept;
        Socket socket;
        bool _isHost;
        public NewChat(bool isHost, string Ip = null)
        {
            InitializeComponent();

            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += ((sender, e) => 
            {
                ChatBox.Height += 10;

                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    scrollViewer.ScrollToEnd();
                }

            });
            timer.Start();

            if (isHost)
            {
                _isHost = true;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 8005));
                socket.Listen(0);
                ChatBox.Text = "Server Started...\nWaiting for incoming client connections...";

                Task.Factory.StartNew(() =>
                {
                    accept = socket.Accept();

                    Dispatcher.Invoke(() =>
                    {
                        ChatBox.Text += "\nConnection accepted!";
                    });

                    while (true)
                    {
                        try
                        {
                            var buffer = new byte[256];
                            var receivedData = accept.Receive(buffer, 0, buffer.Length, 0);

                            if (receivedData <= 0)
                            {
                                throw new SocketException();
                            }

                            Array.Resize(ref buffer, receivedData);

                            Dispatcher.Invoke(() =>
                            {
                                ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} Client: {Encoding.Default.GetString(buffer)}";
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            Application.Current.Shutdown();
                        }
                    }
                });

            }
            else
            {
                _isHost = false;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(new IPEndPoint(IPAddress.Parse(Ip), 8005));
                    ChatBox.Text += $"Your are connected to: {Ip}";

                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                var buffer = new byte[256];
                                var receivedData = socket.Receive(buffer, 0, buffer.Length, 0);

                                if (receivedData <= 0)
                                {
                                    throw new SocketException();
                                }

                                Array.Resize(ref buffer, receivedData);

                                Dispatcher.Invoke(() =>
                                {
                                    ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} Host: {Encoding.Default.GetString(buffer)}";
                                });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    socket.Close();
                    Application.Current.Shutdown();
                }
            }
        }

        void SendButton(object sender, RoutedEventArgs e)
        {
            var dataToSend = Encoding.Default.GetBytes(BoxForMessage.Text);
            if (_isHost)
            {
                accept.Send(dataToSend, 0, dataToSend.Length, 0);
                ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} Host: {BoxForMessage.Text}";
            }
            else
            {
                socket.Send(dataToSend, 0, dataToSend.Length, 0);
                ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} Client: {BoxForMessage.Text}";
            }
        }

        void BoxForMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = "";
            textBox.GotFocus -= BoxForMessage_GotFocus;
        }

        private void BoxForMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                sendButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}
