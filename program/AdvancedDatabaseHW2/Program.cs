using System;
using System.Threading;
using System.Text;
using System.Data.SqlClient;

namespace AdvancedDatabaseHW2
{
    public class Program
    {
        public static Thread T1, T2, T3;
        public void thread1()
        {
            while (true)
            {
                Record C = new Record(1);
                RecordManager recordManagerB = new RecordManager(C, C);
                recordManagerB.Detect();
                // Sleep for 4 seconds
                Thread.Sleep(4000);
            }
        }
        public static void Main()
        {
            Console.WriteLine("Main Started");
            while (true)
            {
                Random rd = new Random();
                Record A = new Record(rd.Next(1, 10));
                Record B = new Record(rd.Next(1, 10));

                Record C = new Record(rd.Next(1, 10));
                Record D = new Record(rd.Next(1, 10));

                RecordManager recordManagerA = new RecordManager(A, B);
                T1 = new Thread(recordManagerA.Transfer);
                T1.Name = "1";

                RecordManager recordManagerB = new RecordManager(C, D);
                T2 = new Thread(recordManagerB.Transfer);
                T2.Name = "2";

                Program obj = new Program();
                T3 = new Thread(obj.thread1);
                T3.Name = "3";

                T1.Start();
                T2.Start();
                T3.Start();

                T1.Join();
                T2.Join();

                Console.WriteLine("Main Completed");
            }
        }
    }

    public class Record
    {
        int _id;

        public Record(int id)
        {
            this._id = id;
        }

        public int ID
        {
            get
            {
                return _id;
            }
        }
    }

    public class RecordManager
    {
        Record _first;
        Record _second;

        static int cntr;

        public RecordManager(Record frst, Record scnd)
        {
            this._first = frst;
            this._second = scnd;
        }

        public void Transfer()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString =
              "Data Source=DESKTOP-ADGLMB7;" +
              "Initial Catalog=myDatabase;" +
              "Integrated Security=true;";
            conn.Open();
            StringBuilder sb = new StringBuilder();
            string sql = sb.ToString();
            using var cmnd = new SqlCommand();
            cmnd.Connection = conn;
            int a = 0, b = 0;
            int f = 0;
            while (f == 0)
            {
                sql = "SELECT ID FROM htw WHERE ID = " + _first.ID + " AND Lock !=" + ((5 - Int32.Parse(Thread.CurrentThread.Name)) / 2) + " ;";
                using (SqlCommand command2 = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            a = reader2.GetInt32(0);
                        }
                    }
                }
                if (a == _first.ID)
                {
                    Console.WriteLine(Thread.CurrentThread.Name + " trying to acquire lock on " + _first.ID.ToString());
                    lock (_first)
                    {
                        cmnd.CommandText = "UPDATE htw SET Lock = " + Int32.Parse(Thread.CurrentThread.Name) + " WHERE ID = " + _first.ID + ";";
                        cmnd.ExecuteNonQuery();
                        Console.WriteLine(Thread.CurrentThread.Name + " acquires lock on " + _first.ID);
                        f = 1;
                        Console.WriteLine(Thread.CurrentThread.Name + " suspended for 1 second");
                        Thread.Sleep(1000);
                        Console.WriteLine(Thread.CurrentThread.Name + " back in action and trying to acquire lock on " + _second.ID.ToString());
                        f = 0;
                        while (f == 0)
                        {
                            cntr++;
                            Detect();
                            sql = "SELECT ID FROM htw WHERE ID = " + _second.ID + " AND Lock !=" + ((5 - Int32.Parse(Thread.CurrentThread.Name)) / 2) + " ;";
                            using (SqlCommand command2 = new SqlCommand(sql, conn))
                            {
                                using (SqlDataReader reader2 = command2.ExecuteReader())
                                {
                                    while (reader2.Read())
                                    {
                                        b = reader2.GetInt32(0);
                                    }
                                }
                            }
                            if (b == _second.ID)
                            {
                                lock (_second)
                                {
                                    cntr = 0;
                                    cmnd.CommandText = "UPDATE htw SET Lock = " + Int32.Parse(Thread.CurrentThread.Name) + " WHERE ID = " + _second.ID + ";";
                                    cmnd.ExecuteNonQuery();
                                    Console.WriteLine(Thread.CurrentThread.Name + " acquires lock on " + _second.ID);
                                    f = 1;
                                    Console.WriteLine(Thread.CurrentThread.Name + " suspended for 1 second");
                                    cmnd.CommandText = "UPDATE htw SET Lock = 0 WHERE Lock = " + Int32.Parse(Thread.CurrentThread.Name) + ";";
                                    cmnd.ExecuteNonQuery();
                                    Console.WriteLine("Thread " + Thread.CurrentThread.Name + " released all records ");
                                }
                            }
                        }
                    }
                }
            }
        }
        public void Detect()
        {
            if (cntr > 12000)
            {
                cntr = 0;
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString =
                  "Data Source=DESKTOP-ADGLMB7;" +
                  "Initial Catalog=myDatabase;" +
                  "Integrated Security=true;";
                conn.Open();
                using var cmnd = new SqlCommand();
                cmnd.Connection = conn;
                int a = 0, b = 0;
                StringBuilder sb = new StringBuilder();
                string sql = sb.ToString();

                Console.WriteLine("**** DeadLock Detected!");
                sql = "SELECT ID FROM htw WHERE Lock =1 ;";
                using (SqlCommand command2 = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            a = reader2.GetInt32(0);
                            Console.WriteLine("Record " + a + " is locked by thread 1");
                        }
                    }
                }
                sql = "SELECT ID FROM htw WHERE Lock =2 ;";
                using (SqlCommand command2 = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader2 = command2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            b = reader2.GetInt32(0);
                            Console.WriteLine("Record " + b + " is locked by thread 2");
                        }
                    }
                }

                Console.WriteLine("Thread 1 is waiting for thread 2 to release lock on record " + b);
                Console.WriteLine("Thread 2 is waiting for thread 1 to release lock on record " + a);
                Environment.Exit(0);
            }
        }
    }
}