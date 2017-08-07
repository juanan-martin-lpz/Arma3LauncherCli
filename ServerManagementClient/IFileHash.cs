using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerManagementClient
{
    public interface IFileHash
    {
        string ComputeHash(string Filename);
        Task<string> ComputeHashAsync(string Filename);
    }
}
