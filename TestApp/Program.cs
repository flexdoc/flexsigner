using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlexSignerService;

namespace TestApp
{
    class Program
    {        
        static void Main(string[] args)
        {
            FlexSigner w = new FlexSigner();
            w.Init();
            System.Threading.Thread.Sleep(Timeout.Infinite);
        }
    }
}
