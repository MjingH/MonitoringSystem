using Communication1;
using MonitoringSystem.Base;
using MonitoringSystem.DAL;
using MonitoringSystem.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonitoringSystem.BLL
{
    public class MonitorSystemBLL
    {
        // 创建全局DataAccess对象
        DataAccess da = new DataAccess();

        // 获取串口信息
        public DataResult<SerialInfo> InitSerialInfo()
        {
            DataResult<SerialInfo> result = new DataResult<SerialInfo>();
            result.State = false;
            try
            {
                SerialInfo si = new SerialInfo();
                si.PortName = ConfigurationManager.AppSettings["port"].ToString();
                si.BaudRate = int.Parse(ConfigurationManager.AppSettings["baud"].ToString());
                si.DataBit = int.Parse(ConfigurationManager.AppSettings["data_bit"].ToString());
                si.Parity = (Parity)Enum.Parse(typeof(Parity), ConfigurationManager.AppSettings["parity"].ToString(), true);
                si.StopBits = (StopBits)Enum.Parse(typeof(StopBits), ConfigurationManager.AppSettings["stopbit"].ToString(), true);

                result.State = true;
                result.Data = si;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        // 解析数据库表登录User数据信息
        public DataResult<List<UserModel>> LoginUser(string username,string password)
        {
            DataResult<List<UserModel>> result = new DataResult<List<UserModel>>();

            try
            {

                DataResult<DataTable> user = da.CheckUserInfo(username,password);
                result.Message = user.Message;
                if (user.Data == null) return result;
                var values = (from q in user.Data.AsEnumerable()
                              select new UserModel
                              {
                                  Id = q.Field<int>("id"),
                                  Status = q.Field<bool>("status"),
                                  UserName = q.Field<string>("user_name"),
                                  Sex = q.Field<bool>("sex"),
                                  Password = q.Field<string>("password"),
                                  CreateTime = q.Field<string>("create_time"),
                                  UpdateTime = q.Field<string>("updata_time")
                              }).ToList();
                result.State = user.State;
                result.Data = values;
            }
            catch (Exception ex)
            {

                result.Message=ex.Message;
            }
            return result;
        }
        // 解析数据库表Users数据信息
        public DataResult<List<UserModel>> InitUsers()
        {
            DataResult<List<UserModel>> result = new DataResult<List<UserModel>>();

            try
            {
                var user = da.GetUsers();

                var values = (from q in user.AsEnumerable()
                              select new UserModel
                              {
                                  Id = q.Field<int>("id"),
                                  Status = q.Field<bool>("status"),
                                  UserName = q.Field<string>("user_name"),
                                  Sex = q.Field<bool>("sex"),
                                  Password = q.Field<string>("password"),
                                  CreateTime = q.Field<string>("create_time"),
                                  UpdateTime = q.Field<string>("updata_time")
                              }).ToList();
                result.State = true;
                result.Data = values;
            }
            catch (Exception ex)
            {

                result.Message=ex.Message;
            }
            return result;
        }
 
        // 解析数据库表StorageArea数据信息
        public DataResult<List<StorageModel>> InitStorageArea()
        {
            DataResult<List<StorageModel>> result = new DataResult<List<StorageModel>>();
            try
            {
                var sa = da.GetStorageArea();
                // 因为 sa 是 DataTable，它本身不支持直接使用 from...in... 循环，
                //调用 .AsEnumerable() 将其变为可枚举的集合，这样你就可以遍历表格的每一行（DataRow）。q 就代表一行数据。
                var values = (from q in sa.AsEnumerable()
                              select new StorageModel
                              {
                                  id = q.Field<int>("id"),
                                  SlaveAddress = q.Field<string>("slave_address"),
                                  FuncCode = q.Field<string>("func_code"),
                                  StartAddress = q.Field<string>("start_address"),
                                  Length = q.Field<int>("length")

                              }).ToList();

                /*   List<StorageModel> values = new List<StorageModel>();
                   foreach (DataRow q in sa.Rows)
                   {
                       StorageModel model = new StorageModel();
                       model.id = Convert.ToInt32(q["id"]);
                       model.SlaveAddress = q["slave_address"].ToString();
                       model.FuncCode = q["func_code"].ToString();
                       model.StartAddress = q["start_address"].ToString();
                       model.Length = Convert.ToInt32(q["length"]);
                       values.Add(model);
                   }*/
                result.State = true;
                result.Data = values;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        // 解析数据库表DeviceModel数据信息
        public DataResult<List<DeviceModel>> InitDevice()
        {
            DataResult<List<DeviceModel>> result = new DataResult<List<DeviceModel>>();
            try
            {
                var dv = da.GetDevices();
                var monitorValues = da.GetMonitorValues();

              /*  var values = (from q in dv.AsEnumerable()
                              select new DeviceModel
                              {
                                  DeviceId = q.Field<int>("d_id"),
                                  DeviceName = q.Field<string>("d_name")
                              }).ToList();
              */
                List<DeviceModel> deviceModels = new List<DeviceModel>();
                foreach (var dr in dv.AsEnumerable()) 
                { 
                DeviceModel model = new DeviceModel(); 
                    deviceModels.Add(model);
                    model.DeviceId = dr.Field<int>("d_id");
                    model.DeviceName = dr.Field<string>("d_name");
                    model.IsRuning = Convert.ToBoolean(dr["is_runing"]);
                    model.IsWarning = Convert.ToBoolean(dr["is_warning"]);

                    foreach (var mv in monitorValues.AsEnumerable().Where(m => 
                    m.Field<int>("d_id") == model.DeviceId))
                    {
                        MonitorValueModel monitorValueModel = new MonitorValueModel();
                        model.MonitorValueList.Add(monitorValueModel);

                       
                        monitorValueModel.ValueId = mv.Field<string>("value_id");
                        monitorValueModel.ValueName = mv.Field<string>("value_name");
                        monitorValueModel.DeviceId = mv.Field<int>("d_id");
                        monitorValueModel.StorageAreaId = mv.Field<string>("area_id");
                        monitorValueModel.StartAddress = mv.Field<int>("start_address");
                        monitorValueModel.DataType = mv.Field<string>("data_type");
                        monitorValueModel.IsAlarm = mv.Field<bool>("is_alarm");
                        monitorValueModel.ValueDesc = mv.Field<string>("description");
                        monitorValueModel.Unit = mv.Field<string>("unit");

                        // 警戒值
                        var column = mv.Field<double>("alarm_lolo");
                        monitorValueModel.LoLoAlarm = column;
                        column = mv.Field<double>("alarm_low");
                        monitorValueModel.LowAlarm = column;
                        column = mv.Field<double>("alarm_high");
                        monitorValueModel.HighAlarm = column;
                        column = mv.Field<double>("alarm_hihi");
                        monitorValueModel.HiHiAlarm = column;


                        monitorValueModel.ValueStorageChanged = (state, msg, value_id) =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var index = model.WarningMessageList.ToList().FindIndex(m => m.ValueId == value_id);
                                if (index > -1)
                                    model.WarningMessageList.RemoveAt(index);
                                if (state != Base.MonitorValueState.OK)
                                {
                                    model.IsWarning = true;
                                    
                                   



                                    // 添加报警日志
                                    model.WarningMessageList.Add(new WarningMessageModel
                                    {
                                        ValueId = value_id,
                                        Message = msg
                                    });

                                    // 给logType赋值
                                    foreach (var lt in GlobalMonitor.LogList.Where(m => m.RowNumber == model.DeviceId))
                                    {
                                        lt.LogType = Base.LogType.Warn;
                                        lt.Message = msg;

                                        GlobalMonitor.AllLogList.Insert(0,new LogModel
                                        {
                                            RowNumber = lt.RowNumber,
                                            DeviceName = lt.DeviceName,
                                            LogInfo = lt.LogInfo,
                                            LogType = lt.LogType,
                                            Message = msg
                                            //WarningMessageList  = new ObservableCollection<WarningMessageModel>
                                            //{

                                            //}
                                            //WarningMessageList = model.WarningMessageList
                                        });
                                    }
                                    
                                }
                                
                                var ss = model.WarningMessageList.Count > 0;
                                if (model.IsWarning != ss)
                                {
                                    model.IsWarning = ss;
                                }
                            });
                        };
                    }

                }
                result.State = true;
                result.Data = deviceModels;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;

        }
    }
}
