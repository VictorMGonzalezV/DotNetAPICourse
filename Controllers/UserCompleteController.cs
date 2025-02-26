using DotnetAPI;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration.UserSecrets;
using Dapper;
using System.Data;
using DotNetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]

public class UserCompleteController: ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;

    public UserCompleteController(IConfiguration config)
    {

        _dapper= new DataContextDapper(config);
        _reusableSql=new ReusableSql(config);
        //This works in .NET>6, where the ConnectionString is read directly from appsettings.json, in older versions the string must be injected from Startup.cs
      //Console.WriteLine(config.GetConnectionString("DefaultConnection"));
    }
/*[HttpGet("TestConnection")]

public DateTime TestConnection()
{
    return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
}*/
    
//This defines a GET request that will be accepted by the controller, so to speak, endpoint ahead.
// Adding /{parameterName} makes a URL parameter explicit, so Swagger or  similar UI's will show it and make it required
[HttpGet("GetUsers/{userId}/{isActive}")]
     //Catch-all in case we don't need to return a particular type of data from a call
    //public IActionResult Test()

//If a URL parameter hasn't been made explicit, we pass it as an argument by adding  ...test?testValue=myString to the URL
//If it's explicit we must add ...test/myString to the URL
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        //This sql query string for Dapper must be <4000 char
       string sql=@"EXEC TutorialAppSchema.spUsers_Get";
       DynamicParameters sqlParameters= new DynamicParameters();

       string stringParameters="";
       //This setup solves the issue of adding a comma after each additional parameter in a simple way that avoids unnecessary steps,we add a comma before any parameter
       if(userId!=0)
       {
            stringParameters+=", @UserId=@UserIdParam";
            sqlParameters.Add("@UserIdParam",userId,DbType.Int32);
       }

       if(isActive)
       {
            stringParameters+=", @Active=@ActiveParam";
            sqlParameters.Add("@ActiveParam",isActive,DbType.Boolean);
       }

       if(stringParameters.Length>0)
       {
            //Then we use Substring to get the parameters string except the unnecessary comma before the first parameter
            sql+=stringParameters.Substring(1);
       }

       IEnumerable<UserComplete> users=_dapper.LoadDataWithParams<UserComplete>(sql,sqlParameters);
       return users;

    }

[HttpPut("UpsertUser")]
//By declaring the explicit URL parameter as the model, we enforce the type, with FromBody, the method will accept anything from the request body as argument

public IActionResult UpsertUser(UserComplete user)
{
    //Remember to wrap a bool true/false string in '" "' so SQL changes it to 1 or 0 for the bit type, else it won't work
    /*string sql=@"EXEC TutorialAppSchema.spUser_Upsert
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
        sqlParameters.Add("@UserIdParam",user.UserId,DbType.Int32);*/


        if(_reusableSql.UpsertUser(user))
        {
             return Ok();
        }

        throw new Exception("Failed to update user exception");
    
}




[HttpDelete("DeleteUser/{userId}")]

public IActionResult DeleteUser(int userId)
{
        string sql=@"EXEC TutorialAppSchema.spUser_Delete 
        @UserId=@UserIdParam";

        DynamicParameters sqlParameters=new DynamicParameters();
        sqlParameters.Add("@UserIdParam",userId,DbType.Int32);

        if(_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
        {
            return Ok();
        }

        throw new Exception("Cannot into delete");
}

}



