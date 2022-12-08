using ProductionEntryWorkerService.Models;

namespace ProductionEntryWorkerService.Modules
{
    public static class Param
    {
        public static string BasePath = Environment.CurrentDirectory + "\\www";

        public static string SettingPath = $"{BasePath}\\setting";

        public static string PatternFile = $"{SettingPath}\\pattern.txt";

        public static string SerialPortFile = $"{SettingPath}\\port.txt";


        public static string BinPath = $"{BasePath}\\bin";

        public static string ChildNumer = "4319-01";

        public static string UploadUrl = "http://localhost:8085/api/v1/Board/production-record";


        public static Pattern Pattern = new Pattern() 
        {
        };
    
    
    
    
    
    
    
    
    }
}
