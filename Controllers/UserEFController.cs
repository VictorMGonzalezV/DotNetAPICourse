using AutoMapper;
using DotnetAPI;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using DotNetAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace DotNetAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class UserEFController: ControllerBase
{
    //DataContextEF _entityFramework;
    IUserRepository _userRepository;

    IMapper _mapper;

    public UserEFController(IConfiguration config,IUserRepository userRepository)
    {

        //_entityFramework= new DataContextEF(config);

        _userRepository=userRepository;
        
        //If more than one  mapping is needed, add them all together like this
        _mapper=new Mapper(new MapperConfiguration(cfg=>
        {cfg.CreateMap<UserToAddDTO,User>();
         cfg.CreateMap<UserSalary,UserSalary>();
         cfg.CreateMap<UserJobInfo,UserJobInfo>();
        }));
        //This works in .NET>6, where the ConnectionString is read directly from appsettings.json, in older versions the string must be injected from Startup.cs
      //Console.WriteLine(config.GetConnectionString("DefaultConnection"));
    }



    
//This defines a GET request that will be accepted by the controller, so to speak, endpoint ahead.
// Adding /{parameterName} makes a URL parameter explicit, so Swagger or  similar UI's will show it and make it required
[HttpGet("GetUsers")]
     //Catch-all in case we don't need to return a particular type of data from a call
    //public IActionResult Test()

//If a URL parameter hasn't been made explicit, we pass it as an argument by adding  ...test?testValue=myString to the URL
//If it's explicit we must add ...test/myString to the URL
    public IEnumerable<User> GetUsers()
    {
        
       IEnumerable<User> users=_userRepository.GetUsers();
       return users;

    }

[HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        //This is not needed anymore since the exception will be thrown from the repository  
         /*User? user=_entityFramework.Users.Where(u=>u.UserId==userId).FirstOrDefault<User>();       
         if(user!=null)
         {
            return user;
         }

         throw new Exception("Cannot into get of user");*/

         return _userRepository.GetSingleUser(userId);
       
    }

[HttpPut("EditUser")]
//By declaring the explicit URL parameter as the model, we enforce the type, with FromBody, the method will accept anything from the request body as argument

public IActionResult EditUser(User user)
{
    //The Edit method now gets the user from the GetSingleUser method instead of the EF Data Context
        User? userDb=_userRepository.GetSingleUser(user.UserId);

        if(userDb!=null)
        {
            userDb.Active=user.Active;
            userDb.FirstName=user.FirstName;
            userDb.LastName=user.LastName;
            userDb.Email=user.Email;
            userDb.Gender=user.Gender;
            //DataContextEF.SaveChanges() returns the number of rows affected by the changes
            // we replaced the old if (_entityFramework.SaveChanges()>0) with a call to the interface method
            if (_userRepository.SaveChanges())
        {
             return Ok();
        }

        throw new Exception("Failed to edit user exception");
        }

        throw new Exception("Get user is of fail");
}


[HttpPost("AddUser")]

public IActionResult AddUser(UserToAddDTO user)
{
    User userDb=_mapper.Map<User>(user);

    _userRepository.AddEntity<User>(userDb);

            if (_userRepository.SaveChanges())
        {
             return Ok();
        }

        throw new Exception("Failed to add user exception");
      
}

[HttpDelete("DeleteUser/{userId}")]

public IActionResult DeleteUser(int userId)
{
        User? userDb=_userRepository.GetSingleUser(userId);

        if(userDb!=null)
        {
            _userRepository.RemoveEntity<User>(userDb);
            if(_userRepository.SaveChanges())
            {
                return Ok();
            }

         throw new Exception("Cannot into delete");
        }


        throw new Exception("Cannot into finding user");
}

//UserSalary CRUD Functionality

[HttpGet("GetUserSalary/{userId}")]

public UserSalary GetSingleUseralary(int userId)
{

     UserSalary? salary=_userRepository.GetSingleUseralary(userId);       
         if(salary!=null)
         {
            return salary;
         }

         throw new Exception("Cannot into get of user salary");
}

[HttpPost("PostSalary")]

public IActionResult PostSalary(UserSalary salary)
{
   /*This works without a mapper, but it allows manual insertion of a userId key, which is not ideal, fits the course objective of
   building a POST request, but isn't an ideal RL situation.
   */

    _userRepository.AddEntity<UserSalary>(salary);

            if (_userRepository.SaveChanges())
        {
             return Ok();
        }

        throw new Exception("Failed to add user exception");
}

[HttpPut("UpdateSalary")]
public IActionResult UpdateSalary (UserSalary userForUpdate)
{
        UserSalary? userToUpdate=_userRepository.GetSingleUseralary(userForUpdate.UserId);

        if(userToUpdate!=null)
        {
            _mapper.Map(userForUpdate,userToUpdate);
            if(_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Update Salary countered on resolution");
        }

        throw new Exception("Cannot into find user to update");
}

[HttpDelete("DeleteSalary/{userId}")]

public IActionResult DeleteSalary(int userId)
{
    UserSalary? userSalary=_userRepository.GetSingleUseralary(userId);

    if(userSalary!=null)
    {
        _userRepository.RemoveEntity<UserSalary>(userSalary);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Delete Salary countered on resolution");
    }

    throw new Exception("Cannot into find salary to delete");
}

//UserJobInfo CRUD Functionality

[HttpPost("PostJobInfo")]

public IActionResult PostJobInfo(UserJobInfo jobInfo)
{
   /*This works without a mapper, but it allows manual insertion of a userId key, which is not ideal, fits the course objective of
   building a POST request, but isn't an ideal RL situation.
   */

    _userRepository.AddEntity<UserJobInfo>(jobInfo);

            if (_userRepository.SaveChanges())
        {
             return Ok();
        }

        throw new Exception("Failed to add job information exception");
}

[HttpDelete("DeleteJobInfo/{userId}")]

public IActionResult DeleteJobInfo(int userId)
{
    UserJobInfo? userJobInfo=_userRepository.GetSingleUserJobInfo(userId);

    if(userJobInfo!=null)
    {
        _userRepository.RemoveEntity<UserJobInfo>(userJobInfo);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Delete Job Information countered on resolution");
    }

    throw new Exception("Cannot into find job info to delete");
}

[HttpGet("GetUserJobInfo/{userId}")]
public UserJobInfo GetSingleUserJobInfo(int userId)
{
   return _userRepository.GetSingleUserJobInfo(userId);

}


[HttpGet("UpdateJobInfo")]
public IActionResult UpdateJobInfo (UserJobInfo userForUpdate)
{
        UserJobInfo? userToUpdate=_userRepository.GetSingleUserJobInfo(userForUpdate.UserId);

        if(userToUpdate!=null)
        {
            _mapper.Map(userForUpdate,userToUpdate);
            if(_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Update Job info countered on resolution");
        }

        throw new Exception("Cannot into find user to update");
}


}