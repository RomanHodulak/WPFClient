using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klient1TabulkaDB
{
    public static class DB
    {
        public static string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "lsd.spsejecna.net";
                builder.UserID = "hodular";
                builder.Password = "databaze";
                builder.InitialCatalog = "hodular";
                return builder.ConnectionString;
            }
        }
        public static SqlConnection Connection
        {
            get
            {
                return con;
            }
            set
            {
                con = value;
            }
        }
        static SqlConnection con;

        public static void InitializeConnection()
        {
            CloseConnection();
            Console.WriteLine("Establishing connection to the database...");
            con = new SqlConnection(ConnectionString);
            try
            {
                con.Open();
                Console.WriteLine("Connection established.");
            }
            catch (Exception e)
            {
                if (con != null)
                {
                    con.Close();
                    con = null;
                }
                Console.WriteLine(e.Message);
            }
        }

        public static void CloseConnection()
        {
            if (con != null)
            {
                con.Close();
                con = null;
                Console.WriteLine("Connection closed.");
            }
        }

        public static void ResetDatabase()
        {
            SqlCommand command0 = new SqlCommand("IF OBJECT_ID('EngineerTable', 'U') IS NOT NULL \nDROP TABLE EngineerTable", con);
            ExecuteCommand(command0);

            SqlCommand command1 = new SqlCommand("Create table EngineerTable (engineer_id int IDENTITY(1,1) PRIMARY KEY, name nvarchar(32), surname nvarchar(32), birthdate date, wage int, employed_since date);", con);
            ExecuteCommand(command1);

            InsertRecord(new EmployeeItem(0, "Stanislav", "Novák", new DateTime(1986, 10, 14), 25000, new DateTime(2009, 6, 25)));
        }

        public static void ExecuteCommand(SqlCommand cmd)
        {
            try
            {
                int rows2 = cmd.ExecuteNonQuery();
                Console.WriteLine("Rows affected: " + rows2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void InsertRecord(EmployeeItem item)
        {
            if (IntegrityCheck(item))
            {
                SqlCommand command2 = new SqlCommand("Insert into EngineerTable (name, surname, birthdate, wage, employed_since) values (@a, @b, @c, @d, @e);", con);
                command2.Parameters.AddWithValue("@a", item.Name);
                command2.Parameters.AddWithValue("@b", item.Surname);
                command2.Parameters.AddWithValue("@c", ToSQLDate(item.Date_of_birth));
                command2.Parameters.AddWithValue("@d", item.Wage);
                command2.Parameters.AddWithValue("@e", ToSQLDate(item.Employed_since));
                ExecuteCommand(command2);
                SqlCommand command3 = new SqlCommand("Select Top 1 * from EngineerTable ORDER BY engineer_id DESC", con);
                SqlDataReader sdr = command3.ExecuteReader();
                while (sdr.Read())
                {
                    item.ID = (int)sdr["engineer_id"];
                }
                if (!sdr.IsClosed)
                    sdr.Close();
            }
            else
                throw new Exception("You can't enter a person employed in age of 14 or less.");
        }

        public static void DeleteRecord(EmployeeItem item)
        {
            if (item != null)
            {
                SqlCommand command = new SqlCommand("Delete from EngineerTable where engineer_id=" + item.ID, con);
                ExecuteCommand(command);
            }
        }

        public static void UpdateRecord(EmployeeItem item)
        {
            if (IntegrityCheck(item))
            {
                SqlCommand command = new SqlCommand("Update EngineerTable Set " + 
                    "name='" + item.Name + "', surname='" + item.Surname + "', birthdate='" + ToSQLDate(item.Date_of_birth) + 
                    "', wage=" + item.Wage + ", employed_since='" + ToSQLDate(item.Employed_since) + "' where engineer_id=" + item.ID, con);
                Console.WriteLine(command.CommandText);
                ExecuteCommand(command);
            }
        }

        public static bool IntegrityCheck(EmployeeItem item)
        {
            if (item == null)
                return false;
            DateTime birth = item.Date_of_birth;
            DateTime employed = item.Employed_since;
            TimeSpan ageTS = employed.Subtract(birth);
            double age = (ageTS.TotalDays / 365.25);
            if (age < 15)
                return false;
            if (item.Name.Length < 1 | item.Surname.Length < 1 | item.Wage < 0)
                return false;
            return true;
        }

        static string ToSQLDate(DateTime date)
        {
            return date.Month + "-" + date.Day + "-" + date.Year;
        }

        public static EmployeeItem[] LoadData()
        {
            List<EmployeeItem> list = new List<EmployeeItem>();
            try
            {

                SqlCommand command3 = new SqlCommand("Select * from EngineerTable", con);
                SqlDataReader sdr = command3.ExecuteReader();
                while (sdr.Read())
                {
                    EmployeeItem item = new EmployeeItem();
                    item.ID = (int)sdr["engineer_id"];
                    item.Name = sdr["name"].ToString();
                    item.Surname = sdr["surname"].ToString();
                    item.Date_of_birth = DateTime.Parse(sdr["birthdate"].ToString());
                    item.Wage = (int)sdr["wage"];
                    item.Employed_since = DateTime.Parse(sdr["employed_since"].ToString());
                    list.Add(item);
                }
                sdr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return list.ToArray();
        }
    }
}
