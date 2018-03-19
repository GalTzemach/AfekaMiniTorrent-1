using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyReflection
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Author : Attribute
    {
        public string Name { get; set; }

        public Author(string name)
        {
            this.Name = name;
        }
    }

    struct MyStruct
    {
        string str;
        public string Str { get => str; set => str = value; }
    }

    [Serializable]
    [Author("Gal")]
    public class Class1
    {

    }

    [Serializable]
    [Author("Tal")]
    public class Class2
    {

    }

    [Serializable]
    [Author("Gal"), Author("Tal")]
    public class Class3
    {

    }

    [Serializable]
    public class Class4
    {

    }
}
