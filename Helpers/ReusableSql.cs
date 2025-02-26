using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotNetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper=new DataContextDapper(config);
        }

    public bool UpsertUser(UserComplete user)
    {
        //Remember to wrap a bool true/false string in '" "' so SQL changes it to 1 or 0 for the bit type, else it won't work
        string sql=@"EXEC TutorialAppSchema.spUser_Upsert
                @FirstName = @FirstNameParam,
                @LastName = @LastNameParam,
                @Email = @EmailParam,
                @Gender = @GenderParam,
                @Active = @ActiveParam,
                @JobTitle = @JobTitleParam,
                @Department = @DepartmentParam,
                @Salary = @SalaryParam,                             
                @UserId= @UserIdParam";

            DynamicParameters sqlParameters=new DynamicParameters();
            sqlParameters.Add("@FirstNameParam",user.FirstName,DbType.String);
            sqlParameters.Add("@LastNameParam",user.LastName,DbType.String);
            sqlParameters.Add("@EmailParam",user.Email,DbType.String);
            sqlParameters.Add("@GenderParam",user.Gender,DbType.String);
            sqlParameters.Add("@ActiveParam",user.Active,DbType.Boolean);
            sqlParameters.Add("@JobTitleParam",user.JobTitle,DbType.String);
            sqlParameters.Add("@DepartmentParam",user.Department,DbType.String);
            sqlParameters.Add("@SalaryParam",user.Salary,DbType.Decimal);
            sqlParameters.Add("@UserIdParam",user.UserId,DbType.Int32);


            if(_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
            {
                return true;
            }

            throw new Exception("Failed to update user exception");
        
    }
    }
}