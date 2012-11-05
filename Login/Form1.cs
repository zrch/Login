using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login
{
    public partial class Form1 : Form
    {
        public Form1()
        {
          

            InitializeComponent();

           
        }

        public class MyClass {
            Action<String> labelSetter;

            public MyClass(Action<String> labelSetter)
            {
                this.labelSetter = labelSetter;
            }

            public void MyMethod() { 
            
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
