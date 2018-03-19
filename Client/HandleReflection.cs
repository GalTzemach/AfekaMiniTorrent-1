using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniTorrent
{
    class HandleReflection
    {

        public static string GetAuthors(string dwLoc)
        {
            StringBuilder sb = new StringBuilder();
            Assembly a = null;

            try
            {
                //Loading assembly from dll file
                a = Assembly.LoadFrom(dwLoc + "\\MyReflection.dll");
                //If loading failed
                if (a == null)
                    return "Dll file not found";

                //Getting Author type for use below
                Type op = a.GetType("MyReflection.Author");

                //Getting all types from assembly
                Type[] types = a.GetTypes();
                foreach (Type type in types)
                {
                    sb.Append("The author/s of " + type.Name + ":\n");

                    //Getting all attributes for each type 
                    object[] objects = type.GetCustomAttributes(false);
                    foreach (object obj in objects)
                    {
                        Attribute attribute = obj as Attribute;
                        if (attribute != null)
                        {
                            Type typeOfAttribute = attribute.GetType();
                            if (typeOfAttribute == op)
                            {
                                PropertyInfo pi = typeOfAttribute.GetProperty("Name");
                                //char operatorSymbol = (char)pi.GetValue(attribute, null);
                                string authorName = (string)pi.GetValue(attribute, null);

                                sb.Append("     " + authorName + "\n");
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                return err.ToString();
            }

            return sb.ToString();



            //StringBuilder stringBuilder = new StringBuilder();
            //StringBuilder stringBuilder1 = new StringBuilder();

            ////Builds assembly from the DLL file
            //Assembly assembly = null;
            //assembly = Assembly.LoadFrom("C:\\Users\\Gal Tzemach\\Desktop\\Gal Up\\Reflection.dll");///dwLoc + 
            //if (assembly == null)
            //    return "DLL file not found / does not exist";

            ////Get all types from assembly
            //Type[] types = assembly.GetTypes();
            //foreach (Type type in types)
            //{
            //    if (type.GetTypeInfo().IsClass)//Check whether the type is class
            //    {
            //        stringBuilder.Append("Author information for class " + type + ":\n");

            //        //Get all attributes from type
            //        Attribute[] attributes = Attribute.GetCustomAttributes(type);
            //        foreach (Attribute attribute in attributes)
            //        {
            //            string str = attribute.ToString();
            //            if (attribute is Author)// || attribute.ToString().Equals("Reflection.Author"))//Check whether the type has an Author feature
            //            {
            //                Author author = (Author)attribute;
            //                stringBuilder1.Append(stringBuilder);
            //                stringBuilder1.Append("  " + author.GetName() + "\n");
            //            }
            //        }
            //        stringBuilder.Clear();
            //    }
            //}

            ////stringBuilder.Append(PrintAuthorInfo(typeof(Class1)));
            ////stringBuilder.Append(PrintAuthorInfo(typeof(Class2)));
            ////stringBuilder.Append(PrintAuthorInfo(typeof(Class3)));
            ////stringBuilder.Append(PrintAuthorInfo(typeof(Class4)));

            //return stringBuilder1.ToString();
        }
    }
}
