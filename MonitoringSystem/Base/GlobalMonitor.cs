using Communication.Modbus;
using Communication1;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Base
{
    public class GlobalMonitor
    {
        public static List<StorageModel> StorageList { get; set; }
        public static List<DeviceModel> DeviceList { get; set; }
        public static ObservableCollection<LogModel> LogList { get; set; } = new ObservableCollection<LogModel>();
        public static ObservableCollection<LogModel> AllLogList { get; set; } = new ObservableCollection<LogModel>();
        public static string CurrentUsername { get; set; }
        public static SerialInfo SerialInfo { get; set; }
        public static ObservableCollection<UserModel> UserList { get; set; }

        static bool isRunning = true;
        static Task mainTask = null;
        static RTU rtu;

        public static void Start(Action successAction, Action<string> faultAction)
        {
            mainTask = Task.Run(async () =>
            {
                // 获取串口信息
                MonitorSystemBLL bll = new MonitorSystemBLL();
                var si = bll.InitSerialInfo();
                if (si.State)
                {
                    SerialInfo = si.Data;
                }
                else
                {
                    faultAction(si.Message);
                    return;
                }
                
                
                // 获取用户信息
                var user = bll.InitUsers();
                if (user.State)
                {
                    UserList = new ObservableCollection<UserModel>(user.Data);
                }
                else
                {
                    faultAction(user.Message); return;
                }
                // 获取存储区信息
                var sa = bll.InitStorageArea();
                if (sa.State)
                {
                    StorageList = sa.Data;
                }
                else
                {
                    faultAction(sa.Message);
                    return;
                }
                // 初始化设备
                var dr = bll.InitDevice();
                if (dr.State)
                {
                    DeviceList = dr.Data;
                    if (DeviceList != null)
                    {
                        foreach (var d in DeviceList) {
                            
                        LogList.Add(new LogModel
                        {
                            RowNumber = d.DeviceId,
                            DeviceName = d.DeviceName,
                            LogInfo = d.IsRuning ? "已启动":"已关闭" ,
                            LogType = Base.LogType.Info,
                        });

                            // 浅绑定
                            AllLogList = new ObservableCollection<LogModel>(LogList);

                            // ★核心：订阅当前设备的运行状态变更事件
                            d.RunningStateChanged += OnDeviceRunningStateChanged;
                        }

                    }
                    
                }
                else
                {
                    faultAction(dr.Message);
                    return;
                }

                // 初始化串口
                rtu = RTU.GetInstance(SerialInfo);
                rtu.ResponseData = new Action<int, List<byte>>(ParsingData);
                if (rtu.Connection())
                {
                    successAction();

                    int startAddr = 0;
                    while (isRunning)
                    {
                        foreach (var item in StorageList)
                        {
                            if (item.Length <= 0)
                            {
                                // 记录警告日志，跳过此配置
                                continue;
                            }
                            // modbus rtu 最长能响应回来256字节，一个寄存器占用两个字节
                            if (item.Length > 100)
                            {
                                startAddr = int.Parse(item.StartAddress);
                                int readCount = item.Length / 100;
                                for (int i = 0; i <= readCount; i++)
                                {
                                    int readLen = i == readCount ? item.Length - 100 * i : 100;
                                    await rtu.Send(int.Parse(item.SlaveAddress), (byte)int.Parse(item.FuncCode)
                                        , startAddr + 100 * i, readLen);
                                }
                            }
                            else
                            {
                                await rtu.Send(int.Parse(item.SlaveAddress),
                                               (byte)int.Parse(item.FuncCode),
                                               startAddr, item.Length);
                            }

                        }
                    }
                }
            
            
                else
                {
                    faultAction("程序无法启动，串口初始化失败....");
                }
            });
        }

        private static void ParsingData(int start_addr,List<byte> byteList)
        {
            if (byteList != null && byteList.Count > 0)
            {
                // 查找设备监控点位与当前返回报文相关的监控数据列表
                // 根据从站地址/功能码/起始地址
                /*  var mvl = (from q in DeviceList
                             from m in q.MonitorValueList
                             where m.StorageAreaId == (byteList[0].ToString() + byteList[1].ToString("00")
                             + start_addr.ToString())
                             && q.IsRuning
                             select m*/
             
                var  mvl = DeviceList
                        .Where(q => q.IsRuning)
                        .SelectMany(q => q.MonitorValueList)
                        .Where(m => m.StorageAreaId == (byteList[0].ToString() + byteList[1].ToString("00")
                           + start_addr.ToString()))
                        .ToList();

                int startByte;
                byte[] res = null;
                foreach (var item in mvl)
                {
                    switch (item.DataType)
                    {
                        case "Float":
                            startByte = 3 + item.StartAddress * 2;  // 跳过帧头（3字节）后，按寄存器地址*2定位
                            if (startByte < start_addr + byteList.Count)
                            {
                                res = new byte[4] { byteList[startByte], byteList[startByte + 1],
                                byteList[startByte+2], byteList[startByte+3]};
                                item.CurrentValue = Convert.ToDouble(res.ByteArrsyToFolat());
                            }
                            break;
                        case "Bool":
                            break;
                    }
                }
            }
        }

        // ViewModel 中的事件处理（更新单条日志）
        private static void OnDeviceRunningStateChanged(int DeviceId, bool isRunning)
        {
            foreach (var lt in GlobalMonitor.LogList.Where(m => m.RowNumber == DeviceId))
            {
                lt.LogInfo = isRunning ? "已启动" : "已关闭";
            }
        }

        public static void Dispose()
        {
            isRunning = false;
            if(rtu != null)
            {
                rtu.Dispose();
            }
            if(mainTask != null)
            {
                mainTask.Wait();
            }
        }
    }
    }
