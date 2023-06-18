using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;


namespace LR29_Sydorenco_202TN
{
    public partial class Form1 : Form
    {
        bool alive = false; // чи буде працювати потік для приймання
        UdpClient client;
        int LOCALPORT = 8001; // порт для приймання повідомлень        
        int REMOTEPORT = 8001; // порт для передавання повідомлень
        const int TTL = 20;
        const string HOST = "235.5.5.1"; // хост для групового розсилання
        IPAddress groupAddress; // адреса для групового розсилання
        string userName; // ім’я користувача в чаті
        public Form1()
        {
            InitializeComponent();
            saveFileDialog1.Filter = "Text File(*.txt)|*.txt| Text Redactor(*.trt)|*.trt";
            loginButton.Enabled = true; // кнопка входу
            logoutButton.Enabled = false; // кнопка виходу
            sendButton.Enabled = false; // кнопка отправки
            chatTextBox.ReadOnly = true; // поле для повідомлень
            groupAddress = IPAddress.Parse(HOST);
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            loginButton.Text = "Вхід";
            logoutButton.Text = "Вихід";
            sendButton.Text = "Відправити";
            button1.Text = "Зберегти";
            button2.Text = "Змінити шрифт";
            textBox1.Text = Convert.ToString(8001);
        }
        private void loginButton_Click(object sender, EventArgs e)
        {
            userName = userNameTextBox.Text;
            userNameTextBox.ReadOnly = true;
            if (textBox1.Text != Convert.ToString(8001))
            {
                LOCALPORT = Convert.ToInt32(textBox1.Text);
                REMOTEPORT = Convert.ToInt32(textBox1.Text);
                chatTextBox.AppendText($"[Порт змінено на {textBox1.Text}]"); 
            }
            try
            {               
                client = new UdpClient(LOCALPORT);
                //підєднання до групового розсилання
                client.JoinMulticastGroup(groupAddress, TTL);

                // задача на приймання повідомлень
                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start();
                // перше повідомлення про вхід нового користувача
                string message = userName + " вошел в чат";
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                loginButton.Enabled = false;
                logoutButton.Enabled = true;
                sendButton.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // метод приймання повідомлення
        private void ReceiveMessages()
        {
            alive = true;
            try
            {
                while (alive)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);
                    // добавляем полученное сообщение в текстовое поле
                    this.Invoke(new MethodInvoker(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();
                        chatTextBox.Text = time + " " + message + "\r\n"
                        + chatTextBox.Text;
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive)
                    return;
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // обробник натискання кнопки sendButton

    
        private void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", userName,
                messageTextBox.Text);
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                messageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // обробник натискання кнопки logoutButton
        private void logoutButton_Click(object sender, EventArgs e)
        {
               ExitChat();
        }
        // вихід з чату
        private void ExitChat()
        {
               string message = userName + " покидает чат";
               byte[] data = Encoding.Unicode.GetBytes(message);
               client.Send(data, data.Length, HOST, REMOTEPORT);
               client.DropMulticastGroup(groupAddress);
               alive = false;
               client.Close();
               loginButton.Enabled = true;
               logoutButton.Enabled = false;
               sendButton.Enabled = false;
        }
        // обработчик события закрытия формы
        private void Form1_FormClosing(object sender,
        FormClosingEventArgs e)
        {
            if (alive)
            ExitChat();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowDialog();
            chatTextBox.Font = fontDialog1.Font;
            chatTextBox.AppendText($"[Шрифт змінено на {chatTextBox.Font}]") ;
            // запускаємо метод fontDialog1.ShowDialog() там встановлюємо переметри полів та присвоюємо ці значення полю TextBox1.Font
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            //Якщо при відкритті натиснемо кнопку ЗАКРИТИ то повернемось назад
            File.WriteAllText(saveFileDialog1.FileName, chatTextBox.Text);
            MessageBox.Show($" Файл був збережений в {saveFileDialog1.FileName} ");
            // зчитуємо текст з TextBox.Text в saveFileDialog1.FileName і виводимо повідомлення
        }
    }
}

