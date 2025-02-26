using DotnetAPI.Models;

namespace DotNetAPI.Data
{
    public class UserRepository: IUserRepository
    {
        DataContextEF _entityFramework;

     //A mapper isn't needed when using the Repository workflow since CRUD methods will become generic
    //IMapper _mapper;

    public UserRepository(IConfiguration config)
        {

            _entityFramework= new DataContextEF(config);
        }
    
    //Unlike in C++, remember that in C# you don't have to override the interface methods, you just implement them.
    public bool SaveChanges()
    {
         return _entityFramework.SaveChanges()>0;
    }

    public void AddEntity<T>(T entityToAdd)
    {
        //Here we can add a bool return type if we want to make sure that we know whether we added an entity or not.
        if(entityToAdd!=null)
        {
           _entityFramework.Add(entityToAdd);
           //return true;
        }
        //return false;
    }

     public void RemoveEntity<T>(T entityToRemove)
    {
        //Here we can add a bool return type for the same reason.
        if(entityToRemove!=null)
        {
           _entityFramework.Remove(entityToRemove);
           //return true;
        }
        //return false;
    }

     public IEnumerable<User> GetUsers()
    {
        
       IEnumerable<User> users=_entityFramework.Users.ToList();
       return users;

    }

    public User GetSingleUser(int userId)
    {
       
         User? user=_entityFramework.Users.Where(u=>u.UserId==userId).FirstOrDefault<User>();       
         if(user!=null)
         {
            return user;
         }

         throw new Exception("Cannot into get of user");
       
    }

    public UserJobInfo GetSingleUserJobInfo(int userId)  
    {
    UserJobInfo? user=_entityFramework.UserJobInfo
        .Where(u=>u.UserId==userId)
        .FirstOrDefault<UserJobInfo>();

    if(user!=null)
    {
        return user;
    }

    throw new Exception("Cannot into edit info of job of user");

    }

    public UserSalary GetSingleUseralary(int userId)
    {

     UserSalary? salary=_entityFramework.UserSalary.Where(u=>u.UserId==userId).FirstOrDefault<UserSalary>();       
         if(salary!=null)
         {
            return salary;
         }

         throw new Exception("Cannot into get of user salary");
    }

    }
}