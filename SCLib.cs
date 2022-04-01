using System;
using System.IO.Ports;
using System.Threading;
using SCLibrary.DeviceMgmt;

namespace SCLibrary
{
     public static class SmartController
    {
        public class Connection : Win32DeviceMgmt
        {
            private SerialPort _port;
            private string _buffer;
            private bool _reading;
            private string _hr;
            private Thread readThread;

            /// <summary>
            /// SerialPort object
            /// </summary>
            public SerialPort Port { get { return _port; } private set { } }

            /// <summary>
            /// Read buffer
            /// </summary>
            public string Buffer 
            { 
                get 
                {
                    if (_buffer.Contains("29:"))
                        return _buffer;
                    else
                    {
                        return null;
                    }
                } 
                private set 
                { 
                } 
            }

            /// <summary>
            /// Get Heart Rate from buffer
            /// </summary>
            public string GetHeartRate 
            {
                get 
                {
                    if (_buffer.Substring(0,1) == ",")
                    {
                        return _hr;
                    }
                    else
                    {
                        var dataStream = _buffer.Split(',');
                        _hr = dataStream[0];
                        return dataStream[0];
                    }
                }
                private set { } 
            }

            /// <summary>
            /// Get reading status
            /// </summary>
            public bool IsReading { get { return _reading; } private set { } }

            /// <summary>
            /// Create new Connection object
            /// </summary>
            public Connection()
            {
                _buffer = "";
                _hr = "";
                _reading = false;
                readThread = new Thread(Read);
            }

            private void Read()
            {
                while (_reading)
                {
                    try
                    {
                        _buffer = _port.ReadLine();
                    }
                    catch (TimeoutException) { }
                }
            }
            private string DiscoverComPort()
            {
                foreach (var item in Win32DeviceMgmt.GetAllCOMPorts())
                {
                    if (item.decsription.Contains("USB to UART Bridge"))
                    {
                        return item.name;
                    }
                }

                return null;
            }

            /// <summary>
            /// Setup COM port.
            /// Defaults to "auto,115200,None,8,One"
            /// </summary>
            /// <param name="port">COM Port name or "auto" to discover</param>
            /// <param name="baudRate">Port Baudrate</param>
            /// <param name="parity">Port Parity</param>
            /// <param name="dataBit">Port Databits</param>
            /// <param name="stopBit">Port Stopbits</param>
            public void PortSetup(string port="auto", int baudRate=115200, Parity parity=Parity.None, int dataBit=8, StopBits stopBit=StopBits.One)
            {
                if (port == "auto")
                {
                    string foundPort = DiscoverComPort();
                    if (!string.IsNullOrEmpty(foundPort))
                    {
                        _port = new SerialPort(foundPort, baudRate, parity, dataBit, stopBit);
                    }
                    else
                    {
                        throw new Exception("Unable to Detect PORT");
                    }
                    
                }
                else
                {
                    _port = new SerialPort(port, baudRate, parity, dataBit, stopBit);
                }
            }

            /// <summary>
            /// Open COM port for communication
            /// </summary>
            public void Open()
            {
                try
                {
                    _port.Open();
                }
                catch
                {
                    throw new Exception("Unable to open port.");
                }
                
            }

            /// <summary>
            /// Close COM port
            /// </summary>
            public void Close()
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
                else
                {
                    throw new Exception("Port not opened.");
                }
                
            }

            /// <summary>
            /// Start Reading from COM port.
            /// Resets buffer content when starting.
            /// </summary>
            public void ReadStart()
            {
                if (_port.IsOpen)
                {
                    _buffer = "";
                    _reading = true;

                    try
                    {
                        readThread.Start();
                    }
                    catch
                    {
                        _reading = false;
                        throw new Exception("Failed to open reading thread.");
                    }
                }
                else
                {
                    throw new Exception("Port not opened.");
                }
                
            }

            /// <summary>
            /// Stop Reading from COM port
            /// </summary>
            public void ReadStop()
            {
                if (_reading)
                {
                    _reading = false;
                    readThread.Join();
                }
            }

            /// <summary>
            /// Set the controller's light color and flash interval.
            /// Set offInterval to 0 to keep light ON.
            /// </summary>
            /// <param name="red">0-255</param>
            /// <param name="green">0-255</param>
            /// <param name="blue">0-255</param>
            /// <param name="onInterval">0-2550</param>
            /// <param name="offInterval">0-2550</param>
            public void setLight(int red=0, int green=0, int blue=255, int onInterval=2550, int offInterval=0)
            {
                if ((red < 0 || red >255) || (green < 0 || green > 255) || (blue < 0 || blue > 255) || (onInterval < 0 || onInterval > 2550) || (offInterval < 0 || offInterval > 2550))
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_port.IsOpen)
                {
                    string expression = "" + red + "," + green + "," + blue + "," + onInterval + "," + offInterval + "*";
                    _port.Write(expression);
                }
            }
        }
    }
}
