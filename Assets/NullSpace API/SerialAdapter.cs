/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using System;
using System.Collections;
using System.IO.Ports;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using NullSpace.API.Logger;

namespace NullSpace.API
{
    /// <summary>
    /// SerialAdapter provides an interface to the suit through a serial port
    /// </summary>
	public class SerialAdapter : ICommunicationAdapter
    {
        private ByteQueue dataStream;
        private SerialPort port;

        /// <summary>
        /// Connect to a serial port by name
        /// </summary>
        /// <param name="name">Name of the port. On Windows, assumes that the port name begins with a character in the set [a-zA-Z]. </param>
        /// <returns>Returns true if successful connection, false otherwise</returns>
        public bool ConnectWithPort(string name)
        {
            int p = (int)Environment.OSVersion.Platform;
            if (p != 4 && p != 128 || p != 6)
            {
                //Prepend the magic serial port string
                if (!name.Contains(@"\\.\"))
                {

                    int beginIndex = Regex.Match(name, "COM[0-9]+").Index;
                    if (beginIndex > 0)
                    {
                        Log.Warning("{0} does not look like a valid Windows serial port. Will proceed, but check spelling.", name);

                    }
                    name = string.Concat(@"\\.\", name);
                }
            }
            return this.createPort(name);
        }
        /// <summary>
        /// Connect to a serial port automatically. May not work on Unix-like systems.
        /// </summary>https://github.com/NullSpaceVR/nullspace_libs.git
        /// <returns>Returns true if successful connect, false otherwise</returns>
        public bool Connect()
        {
            return this.autoConnectPort();
        }

        /// <summary>
        /// Close the port
        /// </summary>
        public void Disconnect()
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        /// <summary>
        /// Write an array of bytes to the serial port. Disconnects from the suit if unable to write.
        /// </summary>
        /// <param name="bytes">Bytes to write to the port</param>
        public void Write(byte[] bytes)
        {
            if (port == null || !port.IsOpen)
            {
                return;
            }
            try
            {
                port.Write(bytes, 0, bytes.Length);
            }
            catch (IOException e)
            {
                //We get these a lot of we read to quickly, so ignore
                if (!e.Message.Contains("semaphore"))
                {
                    Log.Error("Suit disconnected or unresponsive.");
                    this.Disconnect();
                }
            }

        }

        /// <summary>
        /// Construct a new SerialAdapter with null port by default.
        /// </summary>
        public SerialAdapter()
        {
            dataStream = new ByteQueue();
            port = null;
        }

        /// <summary>
        /// Exposes the circular buffer containing suit data
        /// </summary>
        public ByteQueue suitDataStream
        {
            get { return this.dataStream; }
        }

        /// <summary>
        /// Returns true if the port is open
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return port != null && port.IsOpen;
            }
        }

        /// <summary>
        /// Read up to 512 bytes from the port, enqueueing in the suit data stream
        /// </summary>
        public void Read()
        {
            if (port == null || !port.IsOpen)
            {
                return;
            }

            byte[] buffer = new byte[1024];
            Action kickoffRead = null;
            kickoffRead = delegate
            {
                port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                {
                    try
                    {
                        int actualLength = port.BaseStream.EndRead(ar);
                        byte[] received = new byte[actualLength];
                        Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                        this.suitDataStream.Enqueue(received, 0, received.Length);
                    }
                    catch
                    {
                        //intentionally left blank, as this will fail almost constantly (default case is no data)
                    }
                }, null);
            };
            kickoffRead();
        }

        private string[] getPortNames()
        {
            int p = (int)Environment.OSVersion.Platform;
            List<string> serialPorts = new List<string>();

            //Unix-like
            if (p == 4 || p == 128 || p == 6)
            {
                serialPorts.AddRange(Directory.GetFiles("/dev/", "tty.*"));
                serialPorts.AddRange(Directory.GetFiles("/dev/", "cu.*"));
                return serialPorts.ToArray();
            }
            //Windows
            else
            {
                string[] winPorts = SerialPort.GetPortNames();
                for (int i = 0; i < winPorts.Length; i++)
                {
                    winPorts[i] = string.Format(@"\\.\{0}", winPorts[i]);
                }
                return winPorts;
            }
        }



        private bool autoConnectPort()
        {
            string[] portNames = this.getPortNames();
            if (portNames.Length == 0)
            {
                Log.Warning("No ports are available on the system. Is the system plugged in?");
                return false;
            }

            foreach (string name in portNames)
            {
                //try to open the port
                if (this.createPort(name))
                {
                    //write a ping, read the response. Temporary high read-timeout.
                    byte[] ping = { 0x24, 0x02, 0x02, 0x07, 0xFF, 0xFF, 0x0A };
                    port.ReadTimeout = 20;
                    port.Write(ping, 0, ping.Length);
                    System.Threading.Thread.Sleep(20);
                    try
                    {
                        port.ReadLine();
                        port.ReadTimeout = 1;

                    }
                    catch
                    {
                        port.Close();
                        //If no response, try another port
                        continue;
                    }
                    //Got a response, so set read timeout back to 1 and stop trying
                    break;
                }
            }
            return port.IsOpen;
        }


        private bool createPort(string name)
        {
            port = new SerialPort(name, 115200, Parity.None, 8, StopBits.One);
            //Better to fail the read immediately rather than block
            port.ReadTimeout = 1;
            port.WriteTimeout = 5;
            try
            {
                port.Open();
                return true;
            }
            catch (IOException e)
            {
                Log.Message("Tried to use {0} but couldn't open it: {1}", name, e.Message);
                return false;
            }
        }



        /// <summary>
        /// Get the name of the connected port
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.port.PortName;
        }


        //TODO: put in utility class
        public static void PrintByteArray(byte[] arr)
        {
            Log.Message(ByteArrayToString(arr));
        }

        public static string ByteArrayToString(byte[] arr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in arr)
            {
                sb.Append(string.Format("{0} ", b.ToString("X2")));
            }
            return sb.ToString();
        }


    }


}


