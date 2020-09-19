using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class NewChat : Window
    {
        string nickname;
        bool _isHost;
        Socket socket;
        List<Socket> clients = new List<Socket>();
        public NewChat(bool isHost, string name, string Ip = null)
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
                nickname = name;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, 8005));
                socket.Listen(0);

                ChatBox.Text = "Server Started...\nWaiting for incoming client connections...";

                socket.BeginAccept(AcceptCallBack, null); 
            }
            else
            {
                _isHost = false;
                nickname = name;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(new IPEndPoint(IPAddress.Parse(Ip), 8005));
                    ChatBox.Text += $"Welcome to the chat room {nickname}!";

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
                                    ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} {Encoding.Default.GetString(buffer)}";
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
            var dataToSend = Encoding.Default.GetBytes($"{nickname}: {BoxForMessage.Text}");
            if (_isHost)
            {
                foreach (var client in clients)
                {
                    client.Send(dataToSend, 0, dataToSend.Length, 0);
                }
                ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} {nickname}: {BoxForMessage.Text}";
                BoxForMessage.Text = "";
            }
            else
            {
                socket.Send(dataToSend, 0, dataToSend.Length, 0);
                ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} {nickname}: {BoxForMessage.Text}";
                BoxForMessage.Text = "";
            }
        }

        void AcceptCallBack(IAsyncResult asyncResult)
        {
            var acceptedClient = socket.EndAccept(asyncResult);
            clients.Add(acceptedClient);

            Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatBox.Text += "\nNew connection accepted!";
                });

                while (true)
                {
                    try
                    {
                        var buffer = new byte[256];
                        var receivedData = acceptedClient.Receive(buffer, 0, buffer.Length, 0);

                        if (receivedData <= 0)
                        {
                            throw new SocketException();
                        }

                        Array.Resize(ref buffer, receivedData);

                        Dispatcher.Invoke(() =>
                        {
                            ChatBox.Text += $"\n{DateTime.Now.ToShortTimeString()} {Encoding.Default.GetString(buffer)}";
                        });

                        // Send to other clients here

                        foreach (var client in clients)
                        {
                            if (client != acceptedClient)
                            {
                                client.Send(buffer, 0, buffer.Length, 0);
                            }
                        }
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
            socket.BeginAccept(AcceptCallBack, null);
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
