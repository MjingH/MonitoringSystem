using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.DAL
{
    internal class DataAccess
    {
        String dbCofig = ConfigurationManager.ConnectionStrings["db_config"].ToString();
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataAdapter adapter;
        MySqlTransaction trans;

        // 销毁数据
        private void Dispose()
        {
            if (conn != null)
            {
                conn.Dispose(); adapter = null;
            }
            if (cmd != null)
            {
                cmd.Dispose(); cmd = null;
            }
            if (adapter != null)
            {
                adapter.Dispose(); adapter = null;
            }
            if (trans != null)
            {
                trans.Dispose(); trans = null;
            }
        }

        private DataTable GetDatas(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                conn = new MySqlConnection(dbCofig);
                conn.Open();

                // --- 添加诊断输出 ---
                Console.WriteLine("连接状态: " + conn.State);
                Console.WriteLine("服务器版本: " + conn.ServerVersion);
                Console.WriteLine("当前数据库: " + conn.Database);
                // --------------------

                adapter = new MySqlDataAdapter(sql, conn);
                adapter.Fill(dt);
                Console.WriteLine("查询返回行数1: " + dt.Rows.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("数据库操作异常: " + ex.Message);
                throw;
            }
            finally
            {
                this.Dispose();
            }
            DataRow row = dt.Rows[0];
            foreach (var item in row.ItemArray)
                Console.Write(item + "\t");
            Console.WriteLine();
            return dt;
        }

        public DataTable GetStorageArea()
        {
            String strsql = "select * from storage_area";
            var dt1 = this.GetDatas(strsql);
            DataRow row1 = dt1.Rows[0];
            foreach (var item in row1.ItemArray)
                Console.Write(item + "\t");
            Console.WriteLine();
            return dt1;
        }
        public DataTable GetMonitorValues()
        {
            String strsql = "select * from monitor_values";
            
            return this.GetDatas(strsql);   
        }
        public DataTable GetDevices()
        {
            String strsql = "select * from devices";
            return this.GetDatas(strsql);   
        }
    }
}
