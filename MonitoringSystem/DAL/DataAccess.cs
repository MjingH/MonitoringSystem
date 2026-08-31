using MonitoringSystem.Base;
using MonitoringSystem.BLL;
using MonitoringSystem.Model;
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
        String dbConfig = ConfigurationManager.ConnectionStrings["db_config"].ToString();
        MySqlConnection conn; //表示与 MySQL 数据库服务器之间的一个物理连接。它是所有数据库操作的第一步，负责打开、关闭连接，并管理连接状态
        MySqlCommand cmd;  // 表示要对数据库执行的一条 SQL 语句或一个存储过程。它负责发送命令并接收执行结果。
        MySqlDataAdapter adapter; // 它的唯一作用是填充（Fill）——把查回来的数据塞进本地的 DataTable 或 DataSet 里，方便离线操作。
        MySqlTransaction trans; // 表示一个数据库事务，用于将多个数据库操作组合成一个原子工作单元，保证这些操作要么全部成功提交，要么全部回滚。

        // 销毁数据
        private void Dispose()
        {
            if (conn != null)
            {
                conn.Dispose(); conn = null;
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
                conn = new MySqlConnection(dbConfig);
                conn.Open();

                adapter = new MySqlDataAdapter(sql, conn);
                adapter.Fill(dt);
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

            return dt;
        }
/*
        private bool DBConnection()
        {
            if (conn == null)
            {
                conn = new MySqlConnection(dbConfig);
            }
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("数据库连接异常");
                return false;

            }
            return true;
        }*/

        public DataResult<DataTable> CheckUserInfo(string username, string password)
        {
            DataResult<DataTable> result = new DataResult<DataTable>();
            result.State = false;
            string strsql = "select * from users where user_name=@user_name and password = @pwd";
            DataTable dt = new DataTable();

            using (MySqlConnection conn = new MySqlConnection(dbConfig))
            using (MySqlCommand cmd = new MySqlCommand(strsql, conn))
            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
            {
                // 显式指定参数类型
                cmd.Parameters.Add(new MySqlParameter("@user_name", MySqlDbType.VarChar, 50) { Value = username });
                var m = MD5Provider.GetMD5String(password + "@" + username);
                cmd.Parameters.Add(new MySqlParameter("@pwd", MySqlDbType.VarChar, 100) { Value = m });

                conn.Open();
                adapter.Fill(dt);

                // 检查结果
                if (dt.Rows.Count == 0)
                {
                    result.Message = "用户名或密码错误";
                    return result;
                }


                if (!dt.Rows[0].Field<bool>("status"))
                {
                    result.Message = "你没有权限使用平台";
                    return result;
                }
                result.State = true;
                result.Data = dt;
                return result;
            } // 所有 using 结束后，连接、命令、适配器自动关闭和释放
        }

        /*  public DataTable CheckUserInfo(string username, string password)
          {
              try
              {
                  if (DBConnection())
                  {
                      // 1
                      string strsql = "select * from users where user_name=@user_name and password = @pwd";
                      *//*adapter = new MySqlDataAdapter();
                      cmd = new MySqlCommand(strsql, conn);
                      cmd.Parameters.AddWithValue("@user_name",username); // MySQL 要自己去猜 username 和 password 是什么类型
                      cmd.Parameters.AddWithValue("@pwd",password);
                      adapter.SelectCommand = cmd;*//*
                      // 2
                      adapter = new MySqlDataAdapter(strsql, conn);
                      adapter.SelectCommand.Parameters.Add(new MySqlParameter("@user_name", MySqlDbType.VarChar) { Value = username });
                      adapter.SelectCommand.Parameters.Add(new MySqlParameter("@pwd", MySqlDbType.VarChar) { Value = password });

                      DataTable dt = new DataTable();

                      int count = adapter.Fill(dt);

                      if (count < 0)
                      {
                          throw new Exception("用户名或密码不正确");
                      }
                      var dr = dt.Rows[0];
                      if (!(dr.Field<bool>("status")))
                      {
                          throw new Exception("你没有权限使用平台");
                      }
                      return dt;
                  }
              }
              catch (Exception ex)
              {

                  throw ex;

              }
              finally
              {
                  this.Dispose();
              }

              return null;

          }*/

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

        public DataTable GetUsers()
        {
            String strsql = "select * from users";
            return this.GetDatas(strsql);
        }

        /// <summary>更新用户的启用/禁用状态</summary>
        public bool UpdateUserStatus(int id, bool status)
        {
            string strsql = "update users set status = @status where id = @id";
            using (MySqlConnection conn = new MySqlConnection(dbConfig))
            using (MySqlCommand cmd = new MySqlCommand(strsql, conn))
            {
                cmd.Parameters.Add(new MySqlParameter("@status", MySqlDbType.Bit) { Value = status });
                cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = id });
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>删除用户</summary>
        public bool DeleteUser(int id)
        {
            string strsql = "delete from users where id = @id";
            using (MySqlConnection conn = new MySqlConnection(dbConfig))
            using (MySqlCommand cmd = new MySqlCommand(strsql, conn))
            {
                cmd.Parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = id });
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>检查用户名是否已存在</summary>
        public bool CheckUserNameExists(string username)
        {
            string strsql = "select count(*) from users where user_name=@user_name";
            using (MySqlConnection conn = new MySqlConnection(dbConfig))
            using (MySqlCommand cmd = new MySqlCommand(strsql, conn))
            {
                cmd.Parameters.Add(new MySqlParameter("@user_name", MySqlDbType.VarChar, 50) { Value = username });
                conn.Open();
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>新增用户（注册），passwordMd5 为已加盐 MD5 后的密码</summary>
        public bool InsertUser(string username, string passwordMd5, bool sex)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string strsql = "insert into users(user_name,password,status,sex,create_time,updata_time) " +
                            "values(@user_name,@pwd,1,@sex,@create_time,@updata_time)";
            using (MySqlConnection conn = new MySqlConnection(dbConfig))
            using (MySqlCommand cmd = new MySqlCommand(strsql, conn))
            {
                cmd.Parameters.Add(new MySqlParameter("@user_name", MySqlDbType.VarChar, 50) { Value = username });
                cmd.Parameters.Add(new MySqlParameter("@pwd", MySqlDbType.VarChar, 100) { Value = passwordMd5 });
                cmd.Parameters.Add(new MySqlParameter("@sex", MySqlDbType.Byte) { Value = sex ? (byte)1 : (byte)0 });
                cmd.Parameters.Add(new MySqlParameter("@create_time", MySqlDbType.VarChar, 50) { Value = now });
                cmd.Parameters.Add(new MySqlParameter("@updata_time", MySqlDbType.VarChar, 50) { Value = now });
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
