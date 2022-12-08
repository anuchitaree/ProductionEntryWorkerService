using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionEntryWorkerService.Modules
{
    public static class Setting
    {
        public async static void InitSerialPort()
        {
            string file = Param.SerialPortFile;

            if (!File.Exists(file))
            {
                string str = "COM1,19200,None,8,One";

                await File.WriteAllTextAsync(file, str);
            }



        }


        public static void ClosePort(SerialPort serialPort)
        {
            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.DiscardOutBuffer();
                    serialPort.DiscardInBuffer();
                    serialPort.Close();
                    serialPort = null!;
                }
            }
        }



    }
}
