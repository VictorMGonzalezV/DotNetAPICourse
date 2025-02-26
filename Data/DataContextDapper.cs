using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _config;

        public DataContextDapper(IConfiguration config)
        {
            _config=config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql)>0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }

        public bool ExecuteSqlWithParameters(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql,parameters)>0;
            
           /* SqlCommand commandWithParams=new SqlCommand(sql);
            foreach(SqlParameter param in parameters)
            {
                commandWithParams.Parameters.Add(param);
            }

            //For this to work the DB connection must be SqlConnection, instead of an IDbConnection!
            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            dbConnection.Open();
            commandWithParams.Connection=dbConnection;

            int rowsAffected=commandWithParams.ExecuteNonQuery();

            dbConnection.Close();
            
            return rowsAffected>0;*/
        }
        //The List<sqlParameter> method does NOT work if Dapper runs a Query/QuerySingle instruction, in that case wee need to use a DynamicParameters object

        public IEnumerable<T> LoadDataWithParams<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql,parameters);
        }

        public T LoadDataSingleWithParams<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql, parameters);
        }
    }
}