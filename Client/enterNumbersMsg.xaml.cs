using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for enterNumbersMsg.xaml
    /// </summary>
    public partial class enterNumbersMsg : Window
    {
        public double x, y;
        string dwLoc;
        public enterNumbersMsg(string dwLoc)
        {
            InitializeComponent();
            this.dwLoc = dwLoc;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            double x, y;
            string res = "Please enter only numbers";
            if (Double.TryParse(xText.Text, out x))
            {
                if (Double.TryParse(yText.Text, out y))
                {
                    res = activateReflection(dwLoc, x, y);
                    MessageBox.Show(res);
                    this.Close();
                    return;
                }                    
                else
                    MessageBox.Show(res);
            }
            MessageBox.Show(res);
        }

        public string activateReflection(string dwLoc, double x, double y)
        {
            StringBuilder sb = new StringBuilder();
            Assembly a = null;

            try
            {
                a = Assembly.LoadFrom(dwLoc + "\\reflection.dll");

                if (a == null)
                    return "Dll file not found";


                Type op = a.GetType("MiniTorrent.OpAttribute");
                Type[] types = a.GetTypes();
                foreach (Type t in types)
                {
                    object[] Attributes = t.GetCustomAttributes(false);
                    foreach (object obj in Attributes)
                    {
                        Attribute Att = obj as Attribute;
                        if (Att != null)
                        {
                            Type t1 = Att.GetType();
                            if (t1 == op)
                            {
                                PropertyInfo pi = t1.GetProperty("Op");
                                char operatorSymbol = (char)pi.GetValue(Att, null);
                                
                                object[] ArgsArray = new object[2];
                                ArgsArray[0] = x;
                                ArgsArray[1] = y;
                                    
                                object action = Activator.CreateInstance(t, ArgsArray);
                                string s;
                                MethodInfo[] mi = t.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly
                                                                 |BindingFlags.Public);                                
                                s = (string)mi[0].Invoke(action, null);
                                sb.Append(s + "\n");
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
            return sb.ToString();
        }
    }
}
