/// <summary>
/// Класс для работы с источниками рентгена Yolong.
/// </summary>
/// <author>
/// Молотоалиев А.О (northaxeler@gmail.com)
/// </author>
/// <created>02.05.2024</created>
/// <requestNumber>1</requestNumber>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using NLog;

/// <summary>
/// Пространство имен высоковольтных источников рентгена
/// </summary>
namespace HighVoltageModules {
    
    /// <summary>
    /// Класс для работы с логированием
    /// </summary.
    static public class Logger {
        static public NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
    }
    /// <summary>
    /// Класс для работы с источниками рентгена Yolong
    /// </summary>
    public class Yolong{ 
       private SerialPort _port;
       private byte[] _data;
       private byte[] _dataR;
       private string _dataStringR;
       bool _connected;

       /// <summary>
       /// Метод для получения открытого порта в системе
       /// </summary>
       /// <returns>Возвращает открытый порт(SerialPort) в системе или null  </returns>
       public SerialPort GetSerialPort() {
           return _port;
       }
       
       /// <summary>
       /// Конструктор класса
       /// </summary>
       /// <returns>Метод ничего не возвращает</returns>
       public Yolong() {
            _dataR = new byte[6000];
        }
       
        /// <summary>
        /// Метод для получения массива принятых байт после выполнения команды
        /// </summary>
        /// <returns>Массив принятых баит(byte[])</returns>
        public byte[] GetBytesReceived(){
            return _dataR;
        }
        
        /// <summary>
        /// Метод для получения названий портов в системе
        /// </summary>
        /// <returns>Метод возвращает массив названий портов в системе</returns>
       static public string[] GetPorts(){
            return SerialPort.GetPortNames();
        }
        
        /// <summary>
        /// Метод для подключения к порту в системе
        /// </summary>
        /// <param name="portName">Название порта в системе</param>
        /// <param name="baudRate">Скорость передачи данных</param>
        /// <param name="dataBits">Количество бит данных</param>
        /// <param name="parity">Четность</param>
        /// <param name="stopBits">Стоповые биты</param>
        /// <param name="readTimeout">Таймаут приема данных в миллисекундах</param>
        /// <param name="writeTimeout">Таймаут передачи данных в миллисекундах</param>
        /// <returns>Метод возвращает true если подключение произошло успешно</returns>
      public bool ConnectToSeralPort(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits, int readTimeout = 6000, int writeTimeout = 6000) {
            if(_port != null){_port.Close();}
            if(_port == null){_port = new SerialPort();}
            _port.PortName = portName;
            _port.BaudRate = baudRate;
            _port.DataBits = dataBits;
            _port.Parity = parity;
            _port.ReadTimeout = readTimeout;
            _port.WriteTimeout = writeTimeout;
            _port.ReadBufferSize = 6000;
            _port.WriteBufferSize = 6000;
            _port.Encoding = Encoding.ASCII;
             if (_port.IsOpen ){_port.Close();}
             _port.Open();
            _connected = _port.IsOpen;
            return _connected;
        }
        
        /// <summary>
        /// Метод для отключения от порта
        /// </summary>
        /// <returns>Метод возвращает true если отключение произошло успешно</returns>
      public  bool Disconnect(){
            _port.Close();
            _connected = _port.IsOpen;
            return _connected;
        }
        
        /// <summary>
        /// Метод для получения принятых байт
        /// </summary>
        /// <returns>Метод возвращает массив принятых байт(byte[])</returns>
        public byte[] ReceiveResponce()
        {
            Array.Clear(_dataR, 0, 20);
            _port.ReadExisting();
            _port.Read(_dataR, 0, 20);
            _port.DiscardInBuffer();
            return _dataR;
        }
        
        /// <summary>
        /// Метод для получения принятых данных в виде строки
        /// <returns>Метод возвращает принятую строку(string)</returns>
        public string ReceiveResponceString(){
            Array.Clear(_dataR, 0, 20);
            _dataStringR = _port.ReadExisting();
            return _dataStringR;
        }
        
        /// <summary>
        /// Метод для отправки команды
        /// </summary>
        /// <param name="command">Команда(мнемоника)</param>
        /// <returns>Метод ничего не возвращает</returns>
        public void SendCommand(string command) {
            _port.Write("\n" + command + "\r");
        }
        
        /// <summary>
        /// Метод для получения версии прошивки
        /// </summary>
        /// <returns>Метод возвращает версию прошивки(string)</returns>
        public string GetVersionFW(){
            string commnad = "VER?";
            SendCommand(commnad);
            return ReceiveResponceString();
        }
        
        /// <summary>
        /// Метод для увеличения напряжения источника кВ на 1 отсчет  
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetUpKV(){
            string commnad = "KV+";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для уменьшения напряжения источника кВ на 1 отсчет  
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetDownKV(){
            string commnad = "KV-";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для получения значения напряжения кВ
        /// </summary>
        /// <returns>Метод возвращает строку со значением напряжения кВ</returns>
         public String GetHVLevel(){
             string commnad = "KV?";
             SendCommand(commnad);
             return ReceiveResponceString();
         }
        
        /// <summary>
        /// Метод для переключения на малый фокус источника 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetSmallFocus(){
            string commnad = "FS";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для переключения на большой фокус источника
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetBigFocus(){
            string commnad = "FL";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для программного сброса источника
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void SystemReset(){
            string commnad = "RF";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для включения подсветки коллиматора источника
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
      public void LightOn(){
          string commnad = "LY";
          SendCommand(commnad);
      }

      /// <summary>
      /// Метод для переключения в режим уставок в зависмости от установленного времени
      /// </summary>
      /// <returns>Метод ничего не возвращает</returns>
      public void TimeMeasureMethod(){
          string commnad = "US";
          SendCommand(commnad);
      }

      /// <summary>
      /// Метод для переключения в режим уставок в зависмости от установленного мАс(время на ток анода)
      /// </summary>
      /// <returns>Метод ничего не возвращает</returns>
      public void mAsMesureMethod(){
          string commnad = "UX";
          SendCommand(commnad);
      }
      
      /// <summary>
      /// Метод для получения значения напряжения кВ 
      /// </summary>
      /// <param name="input">Входная строка</param>
      /// <returns>Метод возвращает строку со значением напряжения кВ</returns>
  public string GetKVSFromString(string input)
   {
       
       int startIndex = input.IndexOf("KVS");
       if (startIndex != -1)
       {
           // Extract the substring starting from "KVS" until "\r" is encountered
           int endIndex = input.IndexOf("\r", startIndex);
           if (endIndex != -1)
           {
               string extractedValue = input.Substring(startIndex + 3, endIndex - startIndex - 3);
               return extractedValue;
           }
           else
           {
               return string.Empty;
           }
       }
       else
       {
           return string.Empty;
       }
   }
      
   /// <summary>
   /// Метод для получения значения времени экспозиции в миллисекундах 
   /// </summary>
   /// <param name="input">Входная строка</param>
   /// <returns>Метод возвращает строку со значением времени в миллисекундах</returns>
   public string GetMSSFromString(string input)
   {
       
       int startIndex = input.IndexOf("MSS");
       if (startIndex != -1)
       {
           // Extract the substring starting from "MSS" until "\r" is encountered
           int endIndex = input.IndexOf("\r", startIndex);
           if (endIndex != -1)
           {
               string extractedValue = input.Substring(startIndex + 3, endIndex - startIndex - 3);
               return extractedValue;
           }
           else
           {
               return string.Empty;
           }
       }
       else
       {
           return string.Empty;
       }
   }

   /// <summary>
   /// Метод для получения значения мАс 
   /// </summary>
   /// <param name="input">Входная строка</param>
   /// <returns>Метод возвращает строку со значением мАс</returns>
   public string GetMASFromString(string input)
   {
       
       int startIndex = input.IndexOf("MAS");
       if (startIndex != -1)
       {
           int endIndex = input.IndexOf("\r", startIndex);
           if (endIndex != -1)
           {
               string extractedValue = input.Substring(startIndex + 3, endIndex - startIndex - 3);
               return extractedValue;
           }
           else
           {
               return string.Empty;
           }
       }
       else
       {
           return string.Empty;
       }
   }

   /// <summary>
   /// Метод для получения значения тока анода в мА 
   /// </summary>
   /// <param name="input">Входная строка</param>
   /// <returns>Метод возвращает строку со значением мА</returns>
   public string GetMXSFromString(string input)
   {
       int startIndex = input.IndexOf("MXS");
       if (startIndex != -1)
       {
           // Extract the substring starting from "MSS" until "\r" is encountered
           int endIndex = input.IndexOf("\r", startIndex);
           if (endIndex != -1)
           {
               string extractedValue = input.Substring(startIndex + 3, endIndex - startIndex - 3);
               return extractedValue;
           }
           else
           {
               return string.Empty;
           }
       }
       else
       {
           return string.Empty;
       }
   }
   
   /// <summary>т
   /// Метод для получения режима измения уставок в источнике 
   /// </summary>
   /// <returns>Метод возвращает строку со значением времени в миллисекундах</returns>
      public string InquiryMeasureMethod (){
          string commnad = "U?";
          SendCommand(commnad);
          return Encoding.ASCII.GetString(ReceiveResponce());
      }

      /// <summary>
      /// Метод для установки автоматического режима уставок 
      /// </summary>
      /// <returns>Метод ничего не возвращает</returns>
      public void AutomaticMeasureMethod(){
          string commnad = "UAEC";
          SendCommand(commnad);
      }

      /// <summary>
      /// Метод для получения установленного режима фокуса источника
      /// </summary>
      /// <returns>Метод возвращает строку значения установленного режима фокуса источниках</returns>
        public String GetFocus(){
            string commnad = "F?";
            SendCommand(commnad);
            return Encoding.ASCII.GetString(ReceiveResponce());
        }

        /// <summary>
        /// Метод для установки напряжения в кВ источника 
        /// </summary>
        /// <param name="HV">Напряжение в кВ</param>
        /// <returns>Метод ничего не возвращает</returns>
       public void SetHv(int HV){
            string commnad = $"KVN{HV}";
            SendCommand(commnad);
       }

        /// <summary>
        /// Метод для установки тока в мА источника 
        /// </summary>
        /// <param name="MA">Ток в мА</param>
        /// <returns>Метод ничего не возвращает</returns>
       public void SetRadiationCurrent(int MA){
            int CorrectedMas = MA * 10;
            string commnad = $"MAN{CorrectedMas}";
            SendCommand(commnad);

       }
       
        /// <summary>
        /// Метод для увеличения тока в мА источника на шаг в таблице допустимых значений
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns> 
       public void RadiationCurrentUp(){
           string commnad = $"MA+";
           SendCommand(commnad);
       }
       
        /// <summary>
        /// Метод для уменьшения тока в мА источника на шаг в таблице допустимых значений 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void RadiationCurrentDown(){
           string commnad = $"MA-";
           SendCommand(commnad);
       }

        /// <summary>
        /// Метод для получения значения тока в мА источника 
        /// </summary>
        /// <returns>Метод возвращает строку со значением мА"</returns>
       public string GetRadiationCurrent(){
            string commnad = $"MA?";
            SendCommand(commnad);
            return ReceiveResponceString();
       }
        
        /// <summary>
        /// Метод для установки времени в миллисекундах источника 
        /// </summary>
        /// <param name="MS">Время в миллисекундах</param>
        /// <returns>Метод ничего не возвращает</returns>
      public void SetTimeRadiationMs(int MS){
            int CorrectedMs = MS * 100;            
            string commnad = $"MSN{CorrectedMs}";
            SendCommand(commnad);
       }
   
        /// <summary>
        /// Метод для увеличения времени в миллисекундах источника на шаг в таблице допустимых значений
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void TimeRadiationMsUp(){
           string commnad = $"MS+";
           SendCommand(commnad);
       }
       
        /// <summary>
        /// Метод для уменьшения времени в миллисекундах источника на шаг в таблице допустимых значений
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void TimeRadiationMsDown(){
           string commnad = $"MS-";
           SendCommand(commnad);
       }
      
        /// <summary>
        /// Метод для получения значения времени в миллисекундах источника 
        /// </summary>
        /// <returns>Метод возвращает строку со значением миллисекунд" c префиксом MS</returns>
       public string GetTimeRadiationMs(){
            string commnad = $"MS?";
            SendCommand(commnad);
            return ReceiveResponceString();
       }

        /// <summary>
        /// Метод для установки мАс источника  
        /// </summary>
        /// <param name="MAS">значение мАс </param>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetRadiationmAs(float MAS){
            int CorrectedMas = (int)(MAS * 1000);
            string commnad = $"MXN{CorrectedMas}";
            SendCommand(commnad);
       }
        
        /// <summary>
        /// Метод для увеличения мАс источника на шаг в таблице допустимых значений
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void RadiationmAsUp(){
            string commnad = $"MX+";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для уменьшения мАс источника на шаг в таблице допустимых значений
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void RadiationmAsDown(){
            string commnad = $"MX-";
            SendCommand(commnad);
        } 
        
        /// <summary>
        /// Метод для установки плотности обьекта  
        /// </summary>
        /// <param name="number">Номер плотности обьекта</param>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetAECDensity(int number){
            string commnad = $"DEN{number}";
            SendCommand(commnad);
        }

        /// <summary>
        /// Метод для получения плотности обьекта  
        /// </summary>
        /// <returns>Метод возвращает строку со значением плотности обьекта c префиксом DENS</returns>
        public string GetAECDensity(){
            string commnad = $"DEN?";
            SendCommand(commnad);
            return Encoding.ASCII.GetString(ReceiveResponce());
        }
        
        /// <summary>
        /// Метод для получения мАс источника 
        /// </summary>
        /// <returns>Метод возвращает строку со значением мАс с префиксом MXS</returns>
        public string GetRadiationmAs(){
            string commnad = $"MX?";
            SendCommand(commnad);
            return ReceiveResponceString();
       }

        /// <summary>
        /// Метод для  подготовки источника к экспозиции 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void PrepRadiationInittion(){
            string commnad = $"PRI";
            SendCommand(commnad);
       }
       
        /// <summary>
        /// Метод для подготовки источника к завершению экспозиции 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void PrepRadiationTermination(){
            string commnad = $"PRO";
            SendCommand(commnad);
       }

        /// <summary>
        /// Метод для условной экспозиции источника(по команде) 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void ConditionalRadiation(){
            string commnad = $"XRII";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для установки режима рабочей станции 
        /// </summary>
        /// <param name="number">Номер режима рабочей станции</param>
        /// <returns>Метод ничего не возвращает</returns>
        public void SetWorkStationSettings(int number){
            string commnad = $"WS{number}";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для получения режима рабочей станции 
        /// </summary>
        /// <returns>Метод возвращает строку со значением режима рабочей станции с префиксом WS</returns>
        public string GetWorkStationSettings(){
            string commnad = $"WS?";
            SendCommand(commnad);
            return Encoding.ASCII.GetString(ReceiveResponce());
        }

        /// <summary>
        /// Метод для выполнения безусловной экспозиции источника 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void UnconditionalRadiation(){
            string commnad = $"XRIO";
            SendCommand(commnad);
        }

        /// <summary>
        /// Метод для завершения экспозиции источника 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void FullStopRadiation(){
            string commnad = $"XROO";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для полного цикла условной экспозиции источника  
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void ShotXrayConditional(){
            PrepRadiationInittion();
            ConditionalRadiation();
            PrepRadiationTermination();
            FullStopRadiation();
        }
        
        /// <summary>
        /// Метод для полного цикла безусловной экспозиции источника 
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
        public void ShotXrayUnConditional(){
            PrepRadiationInittion();
            UnconditionalRadiation();
            PrepRadiationTermination();
            FullStopRadiation();
        }
         
        /// <summary>
        /// Метод для установки обычного режима работы экспозиции источника  
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void NormalMode(){
            string commnad = $"EXPM0";
            SendCommand(commnad);
        }
       
        /// <summary>
        /// Метод для установки конфигурированного режима работы экспозиции источника  
        /// </summary>
        /// <returns>Метод ничего не возвращает</returns>
       public void ConfigureMode(){
            string commnad = $"EXPM3";
            SendCommand(commnad);
        }
        
        /// <summary>
        /// Метод для подключения к портативному источнику Yolong   
        /// </summary>
        /// <param name="portName">Название порта</param>
        /// <returns>Метод ничего не возвращает</returns>
       public void ConnectToPortableHV(string portName){
          ConnectToSeralPort(portName, 19200, 8, System.IO.Ports.Parity.Even, System.IO.Ports.StopBits.One);
        }
      
        /// <summary>
        /// Метод для подключения к стационарному источнику Yolong 
        /// </summary>
        /// <param name="portName">Название порта</param>
        /// <returns>Метод ничего не возвращает</returns>
        public void ConnectToLineHV(string portName){
            ConnectToSeralPort(portName, 4800, 8, System.IO.Ports.Parity.None, System.IO.Ports.StopBits.One);
        }
    }        

}

