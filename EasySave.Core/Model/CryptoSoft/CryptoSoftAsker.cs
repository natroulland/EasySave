using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Model.CryptoSoft
{
    public class CryptoSoftAsker
    {

        public int Execute(string filePath, string key, bool isEncryption)
        {
            Task<int> res = CryptoSoftExecutor.Instance.Execute(filePath, key, isEncryption);
            return 1;
        }
    }
}
