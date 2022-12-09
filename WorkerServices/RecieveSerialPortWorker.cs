using Newtonsoft.Json;
using ProductionEntryWorkerService.Models;
using ProductionEntryWorkerService.Modules;
using System.Globalization;
using System.IO.Ports;
using System.Timers;

namespace ProductionEntryWorkerService.WorkerServices
{
    public class RecieveSerialPortWorker : BackgroundService
    {
        private static readonly SerialPort serialPort1 = new SerialPort();
        private System.Timers.Timer _timer = new System.Timers.Timer();

        private static string ReadingText1 = null!;

        private HttpClient client;

        private readonly ILogger<RecieveSerialPortWorker> _logger;

        public RecieveSerialPortWorker(ILogger<RecieveSerialPortWorker> logger)
        {
            _logger = logger;
            client = null!;

            _timer.Interval = 100;
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsedAsync!);
            //_timer.AutoReset = false;
            //_timer.Start();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new HttpClient();

            InitialDirectory();

            LoadDataPattern();

            LoadUrl();

            Setting.InitSerialPort();

            LoadSettingAndOpenSerialPort(1, Param.SerialPortFile, serialPort1);


            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();

            Setting.ClosePort(serialPort1);

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                bool result = false;
                int retryCount = 3;
                while (!result) // true : entry in the loop
                {
                    try
                    {
                        CultureInfo ci = new CultureInfo("en-US");
                        Thread.CurrentThread.CurrentCulture = ci;
                        Thread.CurrentThread.CurrentUICulture = ci;

                        string path = $"{Param.BinPath}\\";

                        string[] getFiles = Directory.GetFiles(path);
                        if (getFiles.Length > 0)
                        {
                            List<ProdRecordReq> production = new List<ProdRecordReq>();

                            foreach (string file in getFiles)
                            {
                                string readdata = File.ReadAllText(file);

                                if (readdata == "")
                                {
                                    File.Delete(file);
                                }

                                string[] parts = File.ReadAllText(file).Split(',');

                                var data = new ProdRecordReq()
                                {
                                    ChildNumber = Param.ChildNumer,
                                    ProductId = parts[0],
                                    MachineAssetNo = $"{Param.ChildNumer}FINAL",
                                    PartNumber = parts[1],
                                    CurrentDateTime = Convert.ToDateTime(parts[2]),
                                    Judgement = "OK",
                                    CycleTime = 0,
                                };

                                production.Add(data);
                            };

                            string json = JsonConvert.SerializeObject(production, Formatting.Indented);

                            StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                            var response = await client.PostAsync(Param.UploadUrl, httpContent);

                            if (response.IsSuccessStatusCode)
                            {
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {

                                    foreach (var file in getFiles)
                                    {
                                        File.Delete(file);
                                    }
                                    result = true; // exit while loop

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.HResult.ToString();
                        if (msg != "-2147024864")
                        {
                            _logger.LogError($"{ex.Message} of \n { Param.UploadUrl} \n");

                            Task.Delay(30_000).Wait();

                        }
                            result = retryCount == 0 ? true : false;
                    }

                }
                await Task.Delay(1_000, stoppingToken);

            }

        }


        private void InitialDirectory()
        {
            if (!Directory.Exists(Param.BasePath))
                Directory.CreateDirectory(Param.BasePath);

            if (!Directory.Exists(Param.SettingPath))
                Directory.CreateDirectory(Param.SettingPath);

            if (!Directory.Exists(Param.BinPath))
                Directory.CreateDirectory(Param.BinPath);

            _logger.LogInformation("Create folder => OK ");

        }

        private void LoadDataPattern()
        {
            try
            {


                string path = Param.PatternFile;

                if (File.Exists(path))
                {
                    string data = File.ReadAllText(path);

                    string[] parts = data.Split(',');
                    if (parts.Length >= 5)
                    {
                        Param.Pattern.TotalLength = int.Parse(parts[0]);
                        Param.Pattern.Start1 = Convert.ToInt32(parts[1]);
                        Param.Pattern.Length1 = Convert.ToInt32(parts[2]);
                        Param.Pattern.Start2 = Convert.ToInt32(parts[3]);
                        Param.Pattern.Length2 = Convert.ToInt32(parts[4]);

                        _logger.LogInformation($"Total = { Param.Pattern.TotalLength} \n ProductId start : {Param.Pattern.Start1}, ProductId amount : { Param.Pattern.Length1}\n PartNumber start : {Param.Pattern.Start1},PartNumber amount : { Param.Pattern.Length1}");
                    }
                }
                else
                {
                    string dumy = "47,0,13,20,5";
                    File.WriteAllText(path, dumy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        private void LoadUrl()
        {
            try
            {
                string path = Param.UrlFile;

                if (File.Exists(path))
                {
                    Param.UploadUrl = File.ReadAllText(path);

                    _logger.LogInformation($"Upload Url = { Param.UploadUrl} ");
                }
                else
                {
                    string dumy = "http://localhost:8085/api/v1/Board/production-record";
                    File.WriteAllText(path, dumy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private void LoadSettingAndOpenSerialPort(int port, string destination, SerialPort serialPort)
        {
            try
            {
                string setting = File.ReadAllText(destination);

                string[] parts = setting.Split(',');
                if (parts.Length == 5)
                {
                    string comport = parts[0];
                    string BaudRate = parts[1];
                    string DataBits = parts[3];
                    string stopbit = parts[4];
                    string parity = parts[2];

                    serialPort.PortName = comport;
                    serialPort.BaudRate = Convert.ToInt32(BaudRate);
                    serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                    serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbit);
                    serialPort.DataBits = Convert.ToInt16(DataBits);

                    serialPort.Handshake = Handshake.None;
                    int maxRetries = 3;
                    const int sleepTimeInMs = 500;
                    while (maxRetries > 0)
                    {
                        try
                        {
                            serialPort.Open();
                            if (serialPort.IsOpen)
                            {
                                serialPort.DiscardInBuffer();
                                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler1);
                                _timer.Start();

                                _logger.LogInformation($"{comport},{BaudRate},{DataBits},{stopbit},{parity} \n Comport ready !!!");
                                return;
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            maxRetries--;
                            Thread.Sleep(sleepTimeInMs);
                        }
                        catch (Exception ex)
                        {
                            maxRetries--;
                            _logger.LogError($"{ex.Message}");
                        }
                    }

                    if (maxRetries != 3)
                    {
                        _logger.LogError($"maxRetries:{maxRetries}");

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");

            }

        }

        private static void DataReceivedHandler1(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            ReadingText1 = sp.ReadExisting().Trim('\r');

            serialPort1.DiscardInBuffer();
        }

        private void OnTimerElapsedAsync(object sender, ElapsedEventArgs e)
        {
            if (ReadingText1 != null && ReadingText1 != "")
            {
                _logger?.LogInformation($"receive data => {ReadingText1}");

                if (ReadingText1.Length == Param.Pattern.TotalLength)
                {
                    AsyncInsertFile(ReadingText1);
                }
                ReadingText1 = null!;
            }
        }

        private async void AsyncInsertFile(string readtxt)
        {
            string partNumber = readtxt.Substring(Param.Pattern.Start1, Param.Pattern.Length1);
            string productId = readtxt.Substring(Param.Pattern.Start2, Param.Pattern.Length2);

            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            string filename = $"{Param.BinPath}\\{Guid.NewGuid()}.txt";

            string data = $"{productId},{partNumber},{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

            await File.WriteAllTextAsync(filename, data);

        }



    }
}
