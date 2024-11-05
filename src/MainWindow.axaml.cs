/// <summary>
/// Класс окна программы.
/// </summary>
/// <author>
/// Молотоалиев А.О (northaxeler@gmail.com)
/// </author>
/// <created>02.05.2024</created>
/// <requestNumber>1</requestNumber>

using Avalonia.Controls;
using System.IO.Ports;
using System.Threading;
using Avalonia.Interactivity;
using System.Text;
using Avalonia.Rendering;
using Avalonia.Threading;
using HighVoltageModules;
using System.Globalization;
using System.Text.RegularExpressions;
namespace YolongHVPult;

/// <summary>
/// Класс окна программы.
/// </summary>
public partial class MainWindow : Window
{
    private Yolong _yolong;
    private string[] _ports;
    private bool _mobile = false;
    private string _trace = "";
    
    /// <summary>
    /// Конструктор класса окна
    /// </summary>
    /// <returns>Метод ничего не возвращает</returns>
    public MainWindow()
    {
        _yolong = new Yolong();
        InitializeComponent();
        _ports = Yolong.GetPorts();
        
        if (_ports.Length > 0)
        {
            this.Title = Yolong.GetPorts()[0];
        }
        else
        {
            this.Title = "Connect TTL";
        }

        FocusType.ItemsSource = new string[]
            { "Big", "Small" };
     
        DeviceMode.ItemsSource = new string[]
            { "Automatic mode", "Time Messured mode", "mAs mode" };
        
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки "Connect" (подключиться к стационарному источнику)
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничёго не возвращает</returns>
    private void DeviceXRRP_OnClick(object? sender, RoutedEventArgs e)
    {
        _mobile = false;
        _yolong.ConnectToLineHV(portName: _ports[0]);
        FocusType.SelectedIndex = 1;
        DeviceMode.SelectedIndex = 0;
        _yolong.GetSerialPort().DataReceived += SerialDataReceivedEventHandler;
        _yolong.LightOn();
        _yolong.GetHVLevel();
        _yolong.GetRadiationCurrent();
        _yolong.GetRadiationmAs();
        _yolong.GetTimeRadiationMs();
    }

    /// <summary>
    /// Метод обработки события приема данных
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    public void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e)
    {
        string data = _yolong.ReceiveResponceString();
        string Responce = data.ToString();
        _trace += Responce;
        Dispatcher.UIThread.Post(() => this.Receive.Text = _trace);
        string hvLevelInt = _yolong.GetKVSFromString(Responce);
        string timeRadiationMs = _yolong. GetMSSFromString(Responce);
        string mAsRadiation = _yolong.GetMXSFromString(Responce);
        string currentRadiation = _yolong.GetMASFromString(Responce);
        int value_hv = 0;
        int value_current = 0;
        float value_time = 0;
        float value_mAs = 0;
        decimal value_mAs_dec = 0;
        
        if (_mobile)
        {
            if (int.TryParse(hvLevelInt, out value_hv))
            {
                Dispatcher.UIThread.Post(() => this.Hv.Text = value_hv.ToString());
            }

            if (float.TryParse(timeRadiationMs, out value_time))
            {
                Dispatcher.UIThread.Post(() => this.Ms.Text = (value_time / 100).ToString());
            }

            if (float.TryParse(mAsRadiation, out value_mAs))
            {
                Dispatcher.UIThread.Post(() => this.Mas.Text = (value_mAs * 0.001).ToString());
            }

            if (int.TryParse(currentRadiation, out value_current))
            {
                Dispatcher.UIThread.Post(() => this.Ma.Text = (value_current * 0.1).ToString());
            }
        }
        else
        {
         
            if (int.TryParse(hvLevelInt, out value_hv))
            {
                Dispatcher.UIThread.Post(() => this.Hv.Text = value_hv.ToString());
            }

            if (float.TryParse(timeRadiationMs, out value_time))
            {
                Dispatcher.UIThread.Post(() => this.Ms.Text = (value_time).ToString());
            }

            if (float.TryParse(mAsRadiation.Replace('.', ','), out value_mAs))
            {
                Dispatcher.UIThread.Post(() => this.Mas.Text = (value_mAs).ToString());
            }

            if (int.TryParse(currentRadiation, out value_current))
            {
                Dispatcher.UIThread.Post(() => this.Ma.Text = (value_current).ToString());
            }
            
        }

    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки "LightOn" (включить освещение)
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void LightOn_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.LightOn();
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки подключения к портативному источнику
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void DeviceGGF_OnClick(object? sender, RoutedEventArgs e)
    {
        _mobile = true;
        _yolong.ConnectToPortableHV(_ports[0]);
        _yolong.GetSerialPort().DataReceived += SerialDataReceivedEventHandler;
        _yolong.LightOn();
        FocusType.SelectedIndex = 1;
        DeviceMode.SelectedIndex = 0;
        
        _yolong.GetHVLevel();
        _yolong.GetRadiationCurrent();
        _yolong.GetRadiationmAs();
        _yolong.GetTimeRadiationMs();
        
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для безусловной экспозиции
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void ImitateXRay_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.ShotXrayUnConditional();
    }

    /// <summary>
    /// Метод для экспозиции с установленными параметрами и условиям
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void RealXray_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.PrepRadiationInittion();
        _yolong.ShotXrayConditional();
    }

    /// <summary>
    /// Метод обработки события выбора типа фокусировки
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void FocusType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (FocusType.SelectedIndex)
        {
            case 0:
                _yolong.SetBigFocus();
                break;
            case 1:
                _yolong.SetSmallFocus();
                break;
        }
    }
    

    /// <summary>
    /// Метод обработки установки параметров экспозиции при подключении к портативному источнику"
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void HvPrepare_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_mobile)
        {
            int Hv = this.Hv.Text == "" ? 0 : int.Parse(this.Hv.Text);
            int Ma = this.Ma.Text == "" ? 0 : int.Parse(this.Ma.Text);
            float Mas = this.Mas.Text == "" ? 0 : float.Parse(this.Mas.Text);
            int Ms = this.Ms.Text == "" ? 0 : int.Parse(this.Ms.Text);
            
            switch (DeviceMode.SelectedIndex)
            {
                case 0:
                    _yolong.AutomaticMeasureMethod();
                    _yolong.SetRadiationCurrent(Ma);
                    _yolong.SetTimeRadiationMs(Ms);
                    _yolong.SetHv(Hv);
                    break;
                case 1:
                    _yolong.TimeMeasureMethod();
                    _yolong.SetRadiationCurrent(Ma);
                    _yolong.SetTimeRadiationMs(Ms);
                    _yolong.SetHv(Hv);
                    break;
                case 2:
                    _yolong.mAsMesureMethod();
                    _yolong.SetHv(Hv);
                    _yolong.SetRadiationmAs(Mas);
                    break;

            }
        }
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для увеличения напряжения источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void HvInc_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.SetUpKV();
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для уменьшения напряжения источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void HvDec_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.SetDownKV();
       
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для установки напряжения в кВ источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void HvSet_OnClick(object? sender, RoutedEventArgs e)
    {
        int Hv = int.Parse(this.Hv.Text);
        _yolong.SetHv(Hv);
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для установки безусловной экспозиции
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void UnconditionalXRay_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.UnconditionalRadiation();
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для установки условной экспозиции
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void ConditionalXRay_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.ConditionalRadiation();
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для увеличения тока источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void MaInc_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.RadiationCurrentUp();
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для уменьшения тока источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void MaDec_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.RadiationCurrentDown();
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для увеличения времени в миллисекундах источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void MsInc_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.TimeRadiationMsUp();
        
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для уменьшения времени в миллисекундах источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void MsDec_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.TimeRadiationMsDown();
       
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для увеличения мАс источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void MasInc_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.RadiationmAsUp();
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для уменьшения мАс источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void MasDec_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.RadiationmAsDown();
      
    }
    
    /// <summary>
    /// Метод обработки события нажатия кнопки для сброса источника
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void DeviceReset_OnClick_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.SystemReset();
        _yolong.GetHVLevel();
        _yolong.GetRadiationCurrent();
        _yolong.GetRadiationmAs();
        _yolong.GetTimeRadiationMs();
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для отправки команды на управление источником
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void SendCommand_OnClick(object? sender, RoutedEventArgs e)
    { 
        _yolong.SendCommand(Command.Text);
    }

    /// <summary>
    /// Метод обработки события нажатия кнопки для отправки команды на управление источником
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    /// <returns>Метод ничего не возвращает</returns>
    private void HvPreparePure_OnClick(object? sender, RoutedEventArgs e)
    {
        _yolong.PrepRadiationInittion();
    }
} 