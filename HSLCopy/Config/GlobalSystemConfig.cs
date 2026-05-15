using HSLCopy.Tools;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace HSLCopy
{
    public class GlobalSystemConfig
    {
        private static readonly object _lock = new();
        private static GlobalSystemConfig? _instance;

        public static GlobalSystemConfig Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new();
                        _instance.LoadFromFile();
                    }
                    return _instance;
                }
            }
        }

        [JsonIgnore]
        public string SystemPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BDU系统");

        // === 基础路径配置 ===
        public int PerformanceLevel { get; set; } = 50;
        public string LogFilePath { get; set; } = @"D:\BDU\日志\";
        public string DLLFilePath { get; set; } = @"D:\HSLCopy\指令\";
        public string SubProgramFilePath { get; set; } = @"D:\BDU\子程序\";
        public string PreDefineDevicesPath { get; set; } = @"D:\BDU\设备预设\PreDefineDevices.json";

        public string ReportFilePath { get; set; } = @"D:\BDU\报告\";
        public string DefaultSubProgramFilePath { get; set; } = "";



        // === 新增：CAN 相关统一路径管理 ===

        /// <summary>
        /// 全局唯一的 DBC 文件路径 (全系统共用这一个，不跟具体设备绑定)
        /// </summary>
        public string DBCFilePath { get; set; } = @"D:\BDU\DBC文件\ZSDB123500_HY11_HS11_800V_ADCU20_HVEnergeCAN_230901_Fit.dbc";

        /// <summary>
        /// CAN 设备基础配置路径 (存设备类型等)
        /// </summary>
        public string CanBaseConfigPath { get; set; } = @"D:\BDU\CAN配置\CanDeviceConfig.json";

        /// <summary>
        /// CAN 通道详细配置路径 (存波特率等，由弹窗写入)
        /// </summary>
        public string CanChannelConfigPath { get; set; } = @"D:\BDU\CAN配置\CanChannelConfig.json";

        /// <summary>
        /// 标识当前 CAN 通道是否真实处于运行中 (供后台初始化和设备管理窗口共享状态)
        /// </summary>
        [JsonIgnore]
        public bool IsCanChannelRunning { get; set; } = false;
        // === 加载与保存方法 (保持不变) ===

        public void LoadFromFile()
        {
            string configPath = Path.Combine(SystemPath, "system.config");

            if (!File.Exists(configPath))
            {
                string json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
                File.WriteAllText(configPath, json);
                return;
            }
            try
            {
                string json = File.ReadAllText(configPath);
                var loadedConfig = JsonConvert.DeserializeObject<GlobalSystemConfig>(json);

                PropertyInfo[] properties = typeof(GlobalSystemConfig).GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.CanWrite && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
                    {
                        prop.SetValue(this, prop.GetValue(loadedConfig));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"配置加载失败: {ex.Message}");
            }
        }

        public void SaveToFile()
        {
            lock (_lock)
            {
                try
                {
                    if (!Directory.Exists(SystemPath))
                        Directory.CreateDirectory(SystemPath);

                    string configPath = Path.Combine(SystemPath, "system.config");
                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText(configPath, json);
                    Log.Info("系统配置已保存。");
                }
                catch (Exception ex)
                {
                    Log.Error($"配置保存失败: {ex.Message}");
                }
            }
        }
    }
}