using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;
using TracerLibrary;
using System.Diagnostics;

namespace WorkflowLibrary
{
    public class Server
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);

        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }

        public event MessageReceivedHandler MessageReceived;

        public delegate void MessageReceivedHandler(Client client, string message);

        public const int BUFFER_SIZE = 4096;

        string pipeName;
        Thread listenThread;
        bool running;
        List<Client> clients;

        public string PipeName
        {
            get
            {
                return this.pipeName;
            }
            set
            {
                this.pipeName = value;
            }
        }

        public bool Running
        {
            get
            {
                return this.running;
            }
        }

        public Server()
        {
            this.clients = new List<Client>();
        }

        /// <summary>
        /// Starts the pipe server
        /// </summary>
        public void Start()
        {
            //start the listening thread
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            this.running = true;
        }

        /// <summary>
        /// Listens for client connections
        /// </summary>
        private void ListenForClients()
        {
            while (true)
            {
                SafeFileHandle clientHandle =
                CreateNamedPipe(
                     this.pipeName,                     // The unique pipe name
                     DUPLEX | FILE_FLAG_OVERLAPPED,     // The pipe is overlapped
                     0,                                 // Pipe Mode 
                     255,                               // Maximum Instances
                     BUFFER_SIZE,                       // Out buffer size
                     BUFFER_SIZE,                       // In buffer size
                     0,                                 // Default Timeout
                     IntPtr.Zero);
                
                //could not create named pipe
                if (clientHandle.IsInvalid)
                    return;

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                //could not connect client
                if (success == 0)
                    return;

                Client client = new Client();
                client.handle = clientHandle;

                lock (clients)
                    this.clients.Add(client);

                Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.Start(client);
            }
        }

        /// <summary>
        /// Reads incoming data from connected clients
        /// </summary>
        /// <param name="clientObj"></param>
        private void Read(object clientObj)
        {
            Client client = (Client)clientObj;
            client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();

            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = client.stream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    //read error has occurred
                    break;
                }

                //client has disconnected
                if (bytesRead == 0)
                    break;

                //fire message received event
                if (this.MessageReceived != null)
                    this.MessageReceived(client, encoder.GetString(buffer, 0, bytesRead));
            }

            //clean up resources
            client.stream.Close();
            client.handle.Close();
            lock (this.clients)
                this.clients.Remove(client);
        }

        /// <summary>
        /// Sends a message to all connected clients
        /// </summary>
        /// <param name="message">the message to send</param>
        public void SendMessage(string message)
        {
            lock (this.clients)
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                
                byte[] messageBuffer = encoder.GetBytes(message);
                foreach (Client client in this.clients)
                {
                    client.stream.Write(messageBuffer, 0, messageBuffer.Length);
                    client.stream.Flush();
                }
            }
        }
    }

    public class Client
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
           String pipeName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr lpSecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplate);

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public delegate void MessageReceivedHandler(string message);
        public event MessageReceivedHandler MessageReceived;

        public const int BUFFER_SIZE = 4096;

        string pipeName;
        private FileStream stream;
        private SafeFileHandle handle;
        Thread readThread;
        bool connected = false;

        public bool Connected
        {
            get
            {
                return this.connected;
            }
        }

        public string PipeName
        {
            get
            {
                return this.pipeName;
            }

            set
            {
                this.pipeName = value;
            }
        }

        /// <summary>
        /// Connects to the server
        /// </summary>
        public void Connect()
        {
            this.handle =
               CreateFile(
                  this.pipeName,                    // pipe name
                  GENERIC_READ | GENERIC_WRITE,     // read and write access
                  0,                                // no sharing
                  IntPtr.Zero,                      // default security attributes
                  OPEN_EXISTING,                    // open existing pipe
                  FILE_FLAG_OVERLAPPED,             // overlapped 
                  IntPtr.Zero);                     // no template file

            //could not create handle - server probably not running
            if (this.handle.IsInvalid)
                return;

            this.connected = true;

            //start listening for messages
            this.readThread = new Thread(new ThreadStart(Read));
            this.readThread.Start();
        }

        public void Disconnect()
        {
            try
            {
                if (readThread != null)
                {
                    try
                    {
                        readThread.Abort();
                    }
                    catch { }
                    try
                    {
                        readThread.Join();
                    }
                    catch { }
                }
                this.stream.Close();        // Now close the stream
                this.connected = false;
            }
            catch (Exception e1)
            {
                TraceInternal.TraceVerbose("Cannot disconnect " + e1.ToString());
            }
        }

        /// <summary>
        /// Reads data from the server
        /// </summary>
        public void Read()
        {
            this.stream = new FileStream(this.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] readBuffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();
            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = this.stream.Read(readBuffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    //read error occurred
                    break;
                }

                //server has disconnected
                if (bytesRead == 0)
                    break;

                //fire message received event
                if (this.MessageReceived != null)
                    this.MessageReceived(encoder.GetString(readBuffer, 0, bytesRead));
            }

            //clean up resource
            this.stream.Close();
            this.handle.Close();
        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] messageBuffer = encoder.GetBytes(message);

            this.stream.Write(messageBuffer, 0, messageBuffer.Length);
            this.stream.Flush();
        }
    }

    public class Cell

    {
       
        int cellContents;         // Cell contents
        bool readerFlag = false;  // State flag
       
        public int ReadFromCell( )
        {
            lock (this)   // Enter synchronization block
            {
                if (!readerFlag)
                {            // Wait until Cell.WriteToCell is done producing
                    try
                    {
                        // Waits for the Monitor.Pulse in WriteToCell
                        Monitor.Wait(this);
                    }
                    catch (SynchronizationLockException e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                    catch (ThreadInterruptedException e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }

                Console.WriteLine("Consume: {0}", cellContents);
                readerFlag = false;     // Reset the state flag to say consuming is done.
                Monitor.Pulse(this);    // Pulse tells Cell.WriteToCell that Queue.ReadFromCell is done.
            }                           // Exit synchronization block
            return cellContents;
        }
       
        public void WriteToCell(int n)
        {
           lock(this)  // Enter synchronization block
           {
              if (readerFlag)
              {      // Wait until Cell.ReadFromCell is done consuming.
                 try
                 {
                    Monitor.Wait(this);   // Wait for the Monitor.Pulse in
                                          // ReadFromCell
                 }
                 catch (SynchronizationLockException e)
                 {
                    Console.WriteLine(e);
                 }
                 catch (ThreadInterruptedException e)
                 {
                    Console.WriteLine(e);
                 }
              }
              cellContents = n;
              Console.WriteLine("Produce: {0}",cellContents);
              readerFlag = true;    // Reset the state flag to say producing
                                    // is done
              Monitor.Pulse(this);  // Pulse tells Cell.ReadFromCell that 
                                    // Cell.WriteToCell is done.
           }   // Exit synchronization block
        }
    }
}