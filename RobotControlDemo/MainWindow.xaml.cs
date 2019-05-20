using BLECode;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotControlDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void UpdateRobotInfoDel(TextBox textBox, string str);
        private delegate void UpdateRobotStatusDel(bool status);

        private Canvas CanvasControl;
        private Ellipse Joystick;
        private Ellipse Joystick1;
        private Ellipse Knob;

        private DateTime DateTimeLastAccess;
        private Vector JoystickPos;

        private bool RobotConnected = false;
        private int KeyDownFlag = 0x00;
        private int SpeedLevel = 0; // SpeedLevel：0-3
        private string Direction = "";

        private BluetoothLECode Bluetooth;

        private string ServiceGuid = "0000ffe0-0000-1000-8000-00805f9b34fb";
        private string WriteGuid = "0000ffe1-0000-1000-8000-00805f9b34fb";
        private string NotifyGuid = "0000ffe1-0000-1000-8000-00805f9b34fb";

        public string Forword = "*00AA0000#";
        public string Backword = "*00AB0000#";
        public string TurnLeft = "*00AC0000#";
        public string TurnRight = "*00AD0000#";
        public string StopRun = "*00AE0000#";
        public string Accelerate = "*0000V+00#";
        public string Slowdown = "*0000V-00#";

        public MainWindow()
        {
            InitializeComponent();

            Paint();

            //Test();

            DateTimeLastAccess = DateTime.Now;
        }

        private void Paint()
        {
            if (CanvasControl != null)
                gridJoystick.Children.Remove(CanvasControl);

            double height = this.ActualWidth / 3;
            double width = this.ActualWidth / 3;

            CanvasControl = new Canvas();
            CanvasControl.Height = height;
            CanvasControl.Width = width;

            Joystick = new Ellipse();
            Joystick.Fill = Brushes.Transparent;
            Joystick.StrokeThickness = 4;
            Joystick.Stroke = Brushes.White;
            Joystick.Width = (double)width;
            Joystick.Height = (double)height;
            Canvas.SetLeft(Joystick, 0);
            Canvas.SetTop(Joystick, 0);

            Joystick1 = new Ellipse();
            Joystick1.Fill = Brushes.Transparent;
            Joystick1.StrokeThickness = 4;
            Joystick1.Stroke = Brushes.White;
            Joystick1.Width = (double)width / 2;
            Joystick1.Height = (double)height / 2;
            Canvas.SetLeft(Joystick1, (double)width / 4);
            Canvas.SetTop(Joystick1, (double)height / 4);

            Knob = new Ellipse();
            Knob.Fill = Brushes.Green;
            Knob.StrokeThickness = 4;
            Knob.Stroke = Brushes.White;
            Knob.Width = (double)width * 1 / 3;
            Knob.Height = (double)height * 1 / 3;
            Canvas.SetLeft(Knob, (double)width / 3);
            Canvas.SetTop(Knob, (double)height / 3);

            //Label lblUp = new Label();
            //lblUp.Content = "Up";
            //Canvas.SetLeft(lblUp, width / 2);
            //Canvas.SetTop(lblUp, height / 8);

            //Label lblDown = new Label();
            //lblDown.Content = "Down";
            //Canvas.SetLeft(lblDown, width / 2 - 10);
            //Canvas.SetTop(lblDown, height * 7 / 8 - 10);

            //Label lblLeft = new Label();
            //lblLeft.Content = "Left";
            //Canvas.SetLeft(lblLeft, width / 8);
            //Canvas.SetTop(lblLeft, height / 2);

            //Label lblRight = new Label();
            //lblRight.Content = "Ritht";
            //Canvas.SetLeft(lblRight, width * 7 / 8 - 10);
            //Canvas.SetTop(lblRight, height / 2 - 10);

            CanvasControl.Children.Add(Joystick);
            CanvasControl.Children.Add(Joystick1);
            CanvasControl.Children.Add(Knob);
            //CanvasControl.Children.Add(lblUp);
            //CanvasControl.Children.Add(lblDown);
            //CanvasControl.Children.Add(lblLeft);
            //CanvasControl.Children.Add(lblRight);
            gridJoystick.Children.Add(CanvasControl);
        }

        private void Test()
        {
            string MAC = "a8:10:87:6a:f7:e8";
            string cmd = "*00AE0000#";

            Bluetooth = new BluetoothLECode(ServiceGuid, WriteGuid, NotifyGuid);
            Bluetooth.ValueChanged += Bluetooth_ValueChanged;
            Bluetooth.SelectDeviceFromIdAsync(MAC);

            SendCommand(cmd);
            Thread.Sleep(3000);

            Bluetooth.Dispose();
        }

        private void CheckBoxSliderOnOff_Checked(object sender, RoutedEventArgs e)
        {
            // on
            string MAC = "a8:10:87:6a:f7:e8";
            string cmd = "*00AE0000#";

            Bluetooth = new BluetoothLECode(ServiceGuid, WriteGuid, NotifyGuid);
            Bluetooth.ValueChanged += Bluetooth_ValueChanged;
            Bluetooth.SelectDeviceFromIdAsync(MAC);

            SendCommand(cmd);
        }

        private void CheckBoxSliderOnOff_Unchecked(object sender, RoutedEventArgs e)
        {
            // off
            Bluetooth.Dispose();
        }

        private void Bluetooth_ValueChanged(MsgType type, string str, byte[] data)
        {
            textBoxInfo.Dispatcher.Invoke(new UpdateRobotInfoDel(SetTextBoxInfo), textBoxInfo, str);

            if (str.Contains("-"))
            {
                str = str.Replace('-', ' ').Trim();
            }

            // Connection status
            if (str.Contains("Success"))
                RobotConnected = true;

            if (str.Contains("断开"))
                RobotConnected = false;

            Dispatcher.Invoke(new UpdateRobotStatusDel(UpdateRobotStatus), RobotConnected);
        }

        private void UpdateRobotStatus(bool status)
        {
            if (status)
                EllipseStatus.Fill = new SolidColorBrush(Colors.Green);
            else
                EllipseStatus.Fill = new SolidColorBrush(Colors.Red);
        }

        private void SetTextBoxInfo(TextBox textBox, string str)
        {
            textBox.AppendText(str);
            textBox.AppendText("\r\n");
            textBox.ScrollToEnd();
        }

        private void SendCommand(string cmd = "")
        {
            byte[] msg = System.Text.Encoding.Default.GetBytes(cmd);

            if (RobotConnected)
                Bluetooth.Write(msg);

            textBoxInfo.AppendText(cmd);
            textBoxInfo.AppendText("\r\n");
            textBoxInfo.ScrollToEnd();
        }

        private string HexToStr(string mHex)
        {
            mHex = mHex.Replace(" ", "");
            if (mHex.Length <= 0) return "";
            byte[] vBytes = new byte[mHex.Length / 2];
            for (int i = 0; i < mHex.Length; i += 2)
                if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                    vBytes[i / 2] = 0;
            return ASCIIEncoding.Default.GetString(vBytes);
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            double joystickRadius = Joystick.Height * 0.5;
            Vector joystickPos = e.GetPosition(Joystick) -
                new Point(joystickRadius, joystickRadius);
            joystickPos /= joystickRadius;

            // coordinate system
            //XMousePos.Text = joystickPos.X.ToString();
            //YMousePos.Text = joystickPos.Y.ToString();

            //Polar coord system
            double anglePI = Math.Atan2(joystickPos.Y, joystickPos.X);
            double angle = 0;
            //XPolPos.Text = angle.ToString(); //Angle
            //YPolPos.Text = joystickPos.Length.ToString(); //Radius

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double length = joystickPos.Length;
                //Console.WriteLine("Length: " + length);

                if (length > 1.0)
                {
                    joystickPos.X = Math.Cos(anglePI);
                    joystickPos.Y = Math.Sin(anglePI);

                    //Console.WriteLine(anglePI);
                }

                UpdateKnobPosition(joystickPos);

                // set time span
                TimeSpan timeSpan = DateTime.Now - DateTimeLastAccess;
                DateTimeLastAccess = DateTime.Now;

                double timeSpanSeconds = timeSpan.TotalMilliseconds;

                if (timeSpanSeconds < 15)
                    return;

                // Run cmd: *F3C58845
                string command = "*F3";

                if (length > 0 && length < 0.333)
                {
                    command += "A";
                    SpeedLevel = 1;
                }
                else if (length > 0.333 && length < 0.666)
                {
                    command += "B";
                    SpeedLevel = 2;
                }
                else if (length > 0.666)
                {
                    command += "C";
                    SpeedLevel = 3;
                }

                int range = 0;

                double x = 100 * joystickPos.X;
                double y = -100 * joystickPos.Y;

                // set forward and backward angle
                angle = Math.Atan2(y, x) * 180 / Math.PI;
                Console.WriteLine(angle);

                if ((angle > -5 && angle < 5) || (angle > 175 || angle < -175))
                    y = 0;
                if ((angle > 85 && angle < 95) || (angle > -95 && angle < -85))
                    x = 0;

                if (x > 0 && y == 0)
                    range = 0;
                else if (x > 0 && y > 0)
                    range = 1;
                else if (x == 0 && y > 0)
                    range = 2;
                else if (x < 0 && y > 0)
                    range = 3;
                else if (x < 0 && y == 0)
                    range = 4;
                else if (x < 0 && y < 0)
                    range = 5;
                else if (x == 0 && y < 0)
                    range = 6;
                else if (x > 0 && y < 0)
                    range = 7;

                command += range.ToString();

                string xPos = ((int)x).ToString("00");
                string yPos = ((int)y).ToString("00");

                if (xPos.Contains("-"))
                    xPos = xPos.Replace('-', ' ');

                if (yPos.Contains("-"))
                    yPos = yPos.Replace('-', ' ');

                command += xPos.Trim();
                command += yPos.Trim();

                command += "#";

                textBoxInfo.AppendText(command);
                textBoxInfo.AppendText("\r\n");
                textBoxInfo.ScrollToEnd();

                textBoxInfo.AppendText(((int)x).ToString("00"));
                textBoxInfo.AppendText(((int)y).ToString("00"));
                textBoxInfo.AppendText("\r\n");
                textBoxInfo.ScrollToEnd();

                SendCommand(command);

                if (!RobotConnected)
                    return;

                // Update Direction
                if (angle > -5 && angle < 5)
                    Direction = "Right";
                if (angle > 175 || angle < -175)
                    Direction = "Left";
                if (angle > -95 && angle < -85)
                    Direction = "Back";
                if (angle > 85 && angle < 95)
                    Direction = "Up";

                if (angle > 5 && angle < 85)
                    Direction = "Up Right";
                if (angle > 95 && angle < 175)
                    Direction = "Up Left";
                if (angle < -5 && angle > -85)
                    Direction = "Back Right";
                if (angle < -95 && angle > -175)
                    Direction = "Back Left";

                labelDirectionInfo.Content = Direction;

                // Update Speed
                labelSpeedInfo.Content = "Level " + SpeedLevel.ToString();
            }
        }

        private void UpdateKnobPosition(Vector joystickPos)
        {
            double fJoystickRadius = Joystick.Height * 0.5;
            double fKnobRadius = Knob.Width * 0.5;
            Canvas.SetLeft(Knob, Canvas.GetLeft(Joystick) +
                joystickPos.X * fJoystickRadius + fJoystickRadius - fKnobRadius);
            Canvas.SetTop(Knob, Canvas.GetTop(Joystick) +
                joystickPos.Y * fJoystickRadius + fJoystickRadius - fKnobRadius);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Z:
                    SendCommand(Forword);
                    break;
            }

            switch (e.Key)
            {
                case Key.Up:
                    KeyDownFlag = KeyDownFlag & 0x0e;
                    break;
                case Key.Down:
                    KeyDownFlag = KeyDownFlag & 0x0d;
                    break;
                case Key.Left:
                    KeyDownFlag = KeyDownFlag & 0x0b;
                    break;
                case Key.Right:
                    KeyDownFlag = KeyDownFlag & 0x07;
                    break;
                case Key.Z:
                    SendCommand(Accelerate);
                    break;
                case Key.X:
                    SendCommand(Slowdown);
                    break;
            }

            switch (KeyDownFlag)
            {
                case 0x01:
                    SendCommand(Forword);
                    break;
                case 0x02:
                    SendCommand(Backword);
                    break;
                case 0x04:
                    SendCommand(TurnLeft);
                    break;
                case 0x08:
                    SendCommand(TurnRight);
                    break;
                default:
                    SendCommand(StopRun);
                    SendCommand(StopRun);
                    SendCommand(StopRun);
                    break;
            }

            KeyDownFlag = 0x00;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    KeyDownFlag = KeyDownFlag | 0x01;
                    break;
                case Key.Down:
                    KeyDownFlag = KeyDownFlag | 0x02;
                    break;
                case Key.Left:
                    KeyDownFlag = KeyDownFlag | 0x04;
                    break;
                case Key.Right:
                    KeyDownFlag = KeyDownFlag | 0x08; ;
                    break;
            }

            switch (KeyDownFlag)
            {
                case 0x01:
                    SendCommand(Forword);
                    break;
                case 0x02:
                    SendCommand(Backword);
                    break;
                case 0x04:
                    SendCommand(TurnLeft);
                    break;
                case 0x08:
                    SendCommand(TurnRight);
                    break;

                case 0x05:
                    SendCommand(Forword);
                    SendCommand(TurnLeft);
                    break;
                case 0x06:
                    SendCommand(Backword);
                    SendCommand(TurnLeft);
                    break;
                case 0x09:
                    SendCommand(Forword);
                    SendCommand(TurnRight);
                    break;
                case 0x0a:
                    SendCommand(Backword);
                    SendCommand(TurnRight);
                    break;
            }
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            JoystickPos.X = 0;
            JoystickPos.Y = 0;

            UpdateKnobPosition(JoystickPos);

            for (int i = SpeedLevel; i > 0; i--)
            {
                SendCommand(Slowdown);
            }

            SendCommand(StopRun);
            SendCommand(StopRun);
            SendCommand(StopRun);

            if (!RobotConnected)
                return;

            SpeedLevel = 0;
            Direction = "None";

            labelSpeedInfo.Content = "Level " + SpeedLevel;
            labelDirectionInfo.Content = Direction;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Paint();
        }
    }
}
