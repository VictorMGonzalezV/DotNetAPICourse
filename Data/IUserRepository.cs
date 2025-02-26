using DotnetAPI.Models;

namespace DotNetAPI.Data
{
    public interface IUserRepository
    {
        //Unlike in C++, you don't have to mark the interface methods as abstract, they are understood to be that.
        public bool SaveChanges();

        public void AddEntity<T>(T entityToAdd);

        public void RemoveEntity<T>(T entityToRemove);

         public IEnumerable<User> GetUsers();

         public User GetSingleUser(int userId);

         public UserSalary GetSingleUseralary(int userId);

         public UserJobInfo GetSingleUserJobInfo(int userId);
    }
}