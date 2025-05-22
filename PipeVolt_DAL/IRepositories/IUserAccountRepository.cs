using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_DAL.IRepositories
{
    public interface IUserAccountRepository
    {
        Task<UserAccount?> FindByUsernameAsync(string username);
      
    }
}
