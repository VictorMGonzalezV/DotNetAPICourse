using DotnetAPI;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class UserController: ControllerBase
{
    DataContextDapper _dapper;

    public UserController(IConfiguration config)
    {

        _dapper= new DataContextDapper(config);
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
[HttpGet("GetUsers")]
     //Catch-all in case we don't need to return a particular type of data from a call
    //public IActionResult Test()

//If a URL parameter hasn't been made explicit, we pass it as an argument by adding  ...test?testValue=myString to the URL
//If it's explicit we must add ...test/myString to the URL
    public IEnumerable<User> GetUsers()
    {
        //This sql query string for Dapper must be <4000 char
       string sql=@"
       SELECT [UserId],
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active] FROM TutorialAppSchema.Users";


       IEnumerable<User> users=_dapper.LoadData<User>(sql);
       return users;

    }

[HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string sql=@"SELECT [UserId],
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active] FROM TutorialAppSchema.Users WHERE UserId ="+userId;

        User user=_dapper.LoadDataSingle<User>(sql);       
    return user;
    }

[HttpPut("EditUser")]
//By declaring the explicit URL parameter as the model, we enforce the type, with FromBody, the method will accept anything from the request body as argument

public IActionResult EditUser(User user)
{
    //Remember to wrap a bool true/false string in '" "' so SQL changes it to 1 or 0 for the bit type, else it won't work
    string sql=@"
    UPDATE TutorialAppSchema.Users
        SET  [FirstName]='"+user.FirstName+
             "', [LastName]='"+user.LastName+
             "', [Email]='"+user.Email+
             "', [Gender]='"+user.Gender+
             "', [Active]='"+user.Active+           
        "' WHERE UserId="+user.UserId;
        if(_dapper.ExecuteSql(sql))
        {
             return Ok();
        }

        throw new Exception("Failed to update user exception");
    
}


[HttpPost("AddUser")]

public IActionResult AddUser(UserToAddDTO userToAddDTO)
{
   
    string sql=@"INSERT INTO TutorialAppSchema.Users(
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
            ) VALUES ("+
            "'"+userToAddDTO.FirstName+
            "','"+userToAddDTO.LastName+
            "','"+userToAddDTO.Email+
            "','"+userToAddDTO.Gender+
            "','"+userToAddDTO.Active+
        "')";

    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Cannot into add user");
    
}

[HttpDelete("DeleteUser/{userId}")]

public IActionResult DeleteUser(int userId)
{
        string sql="DELETE FROM TutorialAppSchema.Users WHERE UserId="+userId.ToString();

        if(_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Cannot into delete");
}

//ASSIGNMENT:Replicate CRUD functionality for the 2 peripheral tables UserJobInfo and UserSalary

//UserJobInfo CRUD methods: 

[HttpGet("GetJobs")]

public IEnumerable<UserJobInfo> GetJobs()
{
    string sql=@"SELECT [UserId],
                        [JobTitle],
                        [Department] 
                         FROM TutorialAppSchema.UserJobInfo";

    IEnumerable<UserJobInfo> jobs=_dapper.LoadData<UserJobInfo>(sql);
    return jobs;

}

[HttpGet("GetUserJobInfo/{userId}")]
/*Case discrepancies between the HTML attribute and the method parameter show up as "invalid value: {attribute}" errors, ALWAYS ensure
that they are matched exactly.
*/
public UserJobInfo GetUserJobInfo (int userId)
{
    string sql=@"SELECT UserJobInfo.UserId,
                UserJobInfo.JobTitle,
                UserJobInfo.Department FROM TutorialAppSchema.UserJobInfo WHERE UserId="+userId.ToString();
 
    UserJobInfo jobInfo;
    jobInfo=_dapper.LoadDataSingle<UserJobInfo>(sql);
    return jobInfo;
}

[HttpPost("PostNewJob")]

/*This creates a new job with NULL as user Id, works for the challenge but I don't think it'd be a good implementation IRL, this should
take an actual UserJobInfo object as argument so a UserId is enforced, same applies to a UserSalary*/
public IActionResult PostNewJob(UserJobInfoToAddDTO jobToAdd)
{
    string sql=@"INSERT INTO TutorialAppSchema.UserJobInfo(
            [JobTitle],
            [Department]
            ) VALUES ("+
            "'"+jobToAdd.JobTitle+
            "','"+jobToAdd.Department+
            
        "')";
    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Cannot into addings of job");
}

[HttpPut("EditJobInfo")]

public IActionResult EditJobInfo(UserJobInfo jobInfo)
{
    string sql=@"UPDATE TutorialAppSchema.UserJobInfo
        SET [JobTitle]='"+jobInfo.JobTitle+ 
        "',  [Department]='"+jobInfo.Department+
        "'WHERE UserID="+jobInfo.UserId;

    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Cannot into editing info of job");

}

[HttpDelete("DeleteJobInfo/{userId}")]
public IActionResult DeleteJobInfo(int userId)
{
    string sql=@"DELETE FROM TutorialAppSchema.UserJobInfo WHERE UserId="+userId;

    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Cannot into delete info of job");
}

//UserSalary CRUD Methods
[HttpGet("GetSalaries")]

public IEnumerable<UserSalary> GetSalaries()
{
    string sql=@"SELECT * FROM TutorialAppSchema.UserSalary";

    IEnumerable<UserSalary> salaries=_dapper.LoadData<UserSalary>(sql);
    return salaries;
}

[HttpGet("GetSalary/{userId}")]

public UserSalary GetUserSalary(int userId)
{
    string sql=@"SELECT * FROM TutorialAppSchema.UserSalary
                    WHERE UserId="+userId.ToString();

    UserSalary salary=_dapper.LoadDataSingle<UserSalary>(sql);

    return salary;
    
    
}

[HttpPost("AddSalary")]

public IActionResult AddSalary(UserSalaryToAddDTO salaryToAdd)
{
    string sql=@"INSERT INTO TutorialAppSchema.UserSalary(
    [Salary],
    [AvgSalary])VALUES(
    "+"'"+salaryToAdd.Salary+
    "','"+salaryToAdd.AvgSalary+
    "')";

    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Sorry buddy, no can do (AddSalary)");
}

[HttpPut("EditSalary")]

public IActionResult EditSalary(UserSalary userSalary)
{
    string sql=@"UPDATE TutorialAppSchema.UserSalary
                    SET [Salary]='"+userSalary.Salary+
                    "', [AvgSalary]='"+userSalary.AvgSalary+
                    "' WHERE UserID="+userSalary.UserId;
    
    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Edit Salary is of fail");
}

[HttpDelete("DeleteSalary/{userId}")]

public IActionResult DeleteSalary(int userId)
{
    string sql=@"DELETE FROM TutorialAppSchema.UserSalary
                    WHERE UserId="+userId;

    if(_dapper.ExecuteSql(sql))
    {
        return Ok();
    }

    throw new Exception("Cannot delete a salary, that's slavery you moron");
}

}



