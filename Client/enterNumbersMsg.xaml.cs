using System;
using System.Reflection;
using System.Text;
using System.Windows;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for enterNumbersMsg.xaml
    /// </summary>
    public partial class EnterNumbersMsg : Window
    {
        public double num1, num2;
        string downloadPath;

        public EnterNumbersMsg(string downloadPath)
        {
            InitializeComponent();
            this.downloadPath = downloadPath;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            //double num1, num2;
            string res = "Please enter only numbers";

            if (Double.TryParse(xText.Text, out num1) && Double.TryParse(yText.Text, out num2))
            {
                res = ActivateReflection();
                MessageBox.Show(res);
                this.Close();
                return;
            }
            MessageBox.Show(res);
        }

        public string ActivateReflection()
        {
            StringBuilder stringBuilder = new StringBuilder();
            Assembly assembly = null;

            try
            {
                assembly = Assembly.LoadFrom(downloadPath + "\\reflection.dll");

                if (assembly == null)
                    return "reflection.dll file not found";

                Type op = assembly.GetType("MiniTorrent.OpAttribute");
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    object[] Attributes = type.GetCustomAttributes(false);

                    foreach (object obj in Attributes)
                    {
                        if (obj is Attribute Att)
                        {
                            Type type1 = Att.GetType();
                            if (type1 == op)
                            {
                                PropertyInfo pi = type1.GetProperty("Op");
                                char operatorSymbol = (char)pi.GetValue(Att, null);

                                object[] ArgsArray = new object[2];
                                ArgsArray[0] = num1;
                                ArgsArray[1] = num2;

                                object action = Activator.CreateInstance(type, ArgsArray);
                                string s;
                                MethodInfo[] mi = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly
                                                                 | BindingFlags.Public);
                                s = (string)mi[0].Invoke(action, null);
                                stringBuilder.Append(s + "\n");
                            }
                        }
                    }
                }
            }

            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
                return "Dll file not found";
            }

            return stringBuilder.ToString();
        }
    }
}
