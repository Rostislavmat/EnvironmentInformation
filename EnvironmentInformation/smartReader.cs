using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentInformation
{
    class smartReader
    {
        private char[] endOfString;

        private System.IO.StreamReader file;
        private int pointer;
        const int max_len = 100;
        private int credit;


        public smartReader(string path)
        {
            file = new System.IO.StreamReader(path);
            credit = 0;
            pointer = 0;
            endOfString = new char[max_len];
        }

        public string nextChar()
        {

            return "";
        }

    }
}
