using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RaceCalc
{
    public partial class Form1 : Form
    {
        //methods

        public void ReadInputs()
        {
            //testing inputs
            consump = 6;
            fuel_multiplier = 1;
            tanksize = 100;
            laptime_text = "1:30";
            dur_laps = 40;
            dur_time = "1:30";

            //check radio buttons again (just in case...)
            if (radioButton1.Checked)
            {
                inputmode = "laps";
            }
            else if (radioButton2.Checked)
            {
                inputmode = "time";
            }

            //inputs
            consump = Convert.ToDouble(textBox1.Text);
            fuel_multiplier = Convert.ToDouble(numericUpDown1.Value);
            tanksize = Convert.ToInt32(textBox2.Text);
            laptime_text = maskedTextBox1.Text;

            if (max_tires)
            {
                max_laps_tires = Convert.ToDouble(textBox3.Text);
            }

            laptime_num = TimeStr2Num(laptime_text); //convert laptime to num

            //race duration input (laps / time)
            switch (inputmode)
            {
                case "laps":
                    dur_laps = Convert.ToDouble(durLapsUpDown.Value);
                    dur_num = dur_laps * laptime_num;
                    dur_time = TimeNum2Str(Math.Round(dur_num, 2)); //convert duration to time
                    break;
                case "time":
                    dur_time = durTimeTextBox.Text;
                    dur_num = TimeStr2Num(dur_time); //convert duration to num
                    dur_laps = dur_num / laptime_num;
                    break;
            }
        }
        public void ReadSaves()
        {
            string path = @AppDomain.CurrentDomain.BaseDirectory + @"data\";

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = Path.GetFileNameWithoutExtension(files[i]);
                }

                loadComboBox.DataSource = files;
            }
        }

        static string[] SaveArray(string savename, double consump, int tanksize, double fuel_multiplier, string laptime_text, string inputmode, double dur_laps, string dur_time, bool max_tires, double max_laps_tires) //put all input values in a string to save
        {
            string[] values = new string[10];

            values[0] = Convert.ToString(consump);
            values[1] = Convert.ToString(tanksize);
            values[2] = Convert.ToString(fuel_multiplier);
            values[3] = laptime_text;
            values[4] = inputmode;
            switch (inputmode)
            {
                case ("laps"):
                    values[5] = Convert.ToString(dur_laps);
                    break;
                case ("time"):
                    values[5] = dur_time;
                    break;
            }
            values[6] = Convert.ToString(max_tires);
            values[7] = Convert.ToString(max_laps_tires);
            values[8] = "";

            return values;
        }
        public void LoadFromFile(string path)
        {
            string[] values = File.ReadAllLines(path);

            string inputmode = values[4];
            bool max_tires = Convert.ToBoolean(values[6]);

            textBox1.Text = values[0];
            textBox2.Text = values[1];
            numericUpDown1.Value = Convert.ToDecimal(values[2]);
            maskedTextBox1.Text = values[3];
            switch (inputmode)
            {
                case ("laps"):
                    radioButton1.Checked = true;

                    durTimeTextBox.Visible = false;
                    durLapsUpDown.Visible = true;
                    label10.Text = "laps";
                    durLapsUpDown.Value = Convert.ToDecimal(values[5]);
                    durTimeTextBox.Text = "60:00";
                    break;
                case ("time"):
                    radioButton2.Checked = true;

                    durTimeTextBox.Visible = true;
                    durLapsUpDown.Visible = false;
                    label10.Text = "minutes";
                    durTimeTextBox.Text = values[5];
                    durLapsUpDown.Value = 43;
                    break;
            }
            if (max_tires)
            {
                checkBox1.Checked = true;
                textBox3.Text = values[7];
            }
            else
            {
                checkBox1.Checked = false;
                textBox3.Text = "";
            }
        }

        static double TimeStr2Num(string laptime_text) //convert laptime or duration string to number
        {
            double laptime_num;

            double laptime_min = Convert.ToDouble(laptime_text.Substring(0, 2));
            double laptime_sec = Convert.ToDouble(laptime_text.Substring(3)) / 60;

            return laptime_num = laptime_min + laptime_sec;
        }
        static string TimeNum2Str(double laptime_num) //convert laptime or duration number to string
        {
            string laptime_text; //laptime in text form (01:30)
            string laptime_min; //laptime minutes (1)
            double laptime_min_temp;
            string laptime_sec; //laptime seconds (30)

            laptime_min_temp = Math.Floor(laptime_num);
            laptime_min = Convert.ToString(laptime_min_temp);
            laptime_sec = Convert.ToString(Math.Round(((laptime_num - laptime_min_temp) * 60)));

            if (laptime_min_temp < 10) //two-digit-minutes
            {
                laptime_min = "0" + laptime_min;
            }
            if (Convert.ToInt32(laptime_sec) < 10) //two-digit-seconds
            {
                laptime_sec = "0" + laptime_sec;
            }

            laptime_text = laptime_min + ":" + laptime_sec; //build laptime_text string
            return laptime_text;
        }

        public void CalculateOutput()
        {
            if (dur_num < laptime_num)
            {
                MessageBox.Show("Race duration is smaller than laptime");
                return;
            }

            consump_notires = consump;
            consump *= fuel_multiplier; //apply fuel multiplier


            //calculations
            max_laps = tanksize / consump;

            //determine limiting factor (fuel / tires)
            if (max_tires == true && max_laps_tires < max_laps)
            {
                max_laps = max_laps_tires;
            }

            //stop if you cant complete a full lap on one tank
            if (max_laps < 1)
            {
                MessageBox.Show("Can't complete a full lap with these values");
                return;
            }

            max_time_num = max_laps * laptime_num;
            max_time = TimeNum2Str(Math.Round(max_time_num, 2)); //convert max time to time

            min_stints = dur_laps / max_laps;
            min_pits = min_stints;

            //stint arrays
            double[] stint_laps = new double[Convert.ToInt32(Math.Ceiling(min_stints))];
            string[] stint_time = new string[Convert.ToInt32(Math.Ceiling(min_stints))];
            double[] stint_fuel = new double[Convert.ToInt32(Math.Ceiling(min_stints))];
            //pitstop arrays
            double[] pit_laps = new double[Convert.ToInt32(Math.Ceiling(min_pits))];
            double[] pit_at_lap = new double[Convert.ToInt32(Math.Ceiling(min_pits))];
            string[] pit_time = new string[Convert.ToInt32(Math.Ceiling(min_pits))];
            string[] pit_at_time = new string[Convert.ToInt32(Math.Ceiling(min_pits))];
            double[] pit_fuel = new double[Convert.ToInt32(Math.Ceiling(min_pits))];


            //stint detail
            for (int i = 0; i < stint_laps.Length; i++)
            {
                stint_laps[i] = max_laps; //stint laps
                stint_time[i] = max_time;

                if (i == (stint_laps.Length - 1)) //last stint case
                {
                    stint_laps[i] = dur_laps - (max_laps * i);
                    stint_time[i] = TimeNum2Str(stint_laps[i] * laptime_num);
                }

                stint_fuel[i] = stint_laps[i] * consump;
            }

            //pitstop detail
            for (int i = 0; i < pit_laps.Length; i++)
            {
                pit_at_lap[i] = i * max_laps;
                pit_at_time[i] = TimeNum2Str(i * max_time_num);

                pit_laps[i] = max_laps;
                pit_time[i] = max_time;

                if (i == (pit_laps.Length - 1)) //last pitstop case
                {
                    pit_laps[i] = dur_laps - (i * max_laps);
                    pit_time[i] = TimeNum2Str(pit_laps[i] * laptime_num);
                }

                pit_fuel[i] = pit_laps[i] * consump;
            }

            //outputs
            double min_pits_output = min_pits - 1;
            if (min_pits_output < 0)
            {
                min_pits_output = 0;
            }
            min_pits_output = Math.Round(min_pits_output, 2);

            textBox4.Text = Convert.ToString(Math.Round(max_laps, 2));
            textBox5.Text = max_time;
            textBox7.Text = Convert.ToString(min_pits_output);
            label16.Text = "min stints: " + Convert.ToString(Math.Round(min_stints, 2));
            label17.Text = "min pits: " + Convert.ToString(min_pits_output);

            switch (inputmode)
            {
                case ("laps"):
                    label14.Text = "race duration: ";
                    textBox6.Text = dur_time;
                    break;
                case ("time"):
                    label14.Text = "race duration in laps: ";
                    textBox6.Text = Convert.ToString(Math.Round(dur_laps, 2));
                    break;
            }


            //grid string array
            double min_stints_temp = Convert.ToDouble(min_stints);
            min_stints_temp = Math.Ceiling(min_stints_temp);
            string[,] results = new string[Convert.ToInt32(min_stints_temp), 10];
            for (int i = 0; i < Math.Ceiling(min_stints); i++)
            {
                results[i, 0] = (i + 1).ToString();
                results[i, 1] = Math.Round(stint_laps[i], 2).ToString();
                results[i, 2] = stint_time[i];
                results[i, 3] = Math.Round(stint_fuel[i], 2).ToString();
                results[i, 4] = (i + 1).ToString();
                results[i, 5] = Math.Round(pit_at_lap[i], 2).ToString();
                results[i, 6] = pit_at_time[i].ToString();
                results[i, 7] = Math.Round(pit_laps[i], 2).ToString();
                results[i, 8] = pit_time[i].ToString();
                results[i, 9] = Math.Round(pit_fuel[i], 2).ToString();
            }

            //pitstop details DataGridView
            for (int i = 0; i < min_stints; i++)
            {
                string rowname = i.ToString();
                dataGridView1.Rows.Add(rowname);

                for (int j = 0; j < 10; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = results[i, j];
                }
            }

            //values without tires limiting
            if (max_tires)
            {
                double max_laps_notires = Math.Round(tanksize / consump_notires, 2);
                double max_time_num_notires = Math.Round(max_laps_notires + laptime_num, 2);
                string max_time_notires = TimeNum2Str(Math.Round(max_time_num_notires, 2));
                double min_stints_notires = Math.Round(dur_laps / max_laps_notires, 2);
                double min_pits_notires = Math.Round(min_stints_notires - 1, 2);

                if (min_stints_notires < 0)
                {
                    min_pits_notires = 1;
                }
                string message = string.Format("Values without tires as limiting factor:\r\nmax laps on 1 tank: {0}\r\nmax time on 1 tank: {1}\r\nmin number of stints: {2}\r\nmin number of pitstops: {3}", max_laps_notires, max_time_notires, min_stints_notires, min_pits_notires);
                MessageBox.Show(message);
            }

        }



        //declaring input variables
        bool max_tires = false;
        double consump;
        double consump_notires;
        int tanksize;
        double fuel_multiplier;
        string laptime_text;
        double laptime_num;
        string inputmode = "laps";
        double dur_laps = 0;
        string dur_time = "00:00";
        double dur_num;
        double max_laps_tires = 999;
        //declaring internal variables
        double max_time_num; // max time on 1 tank num
        //declaring saving variables
        string savename;
        //declaring output variables
        double max_laps;
        string max_time;
        double min_pits;
        double min_stints;


        public Form1()
        {
            InitializeComponent();

            //fill default values
            textBox1.Text = "6";
            numericUpDown1.Value = 1;
            textBox2.Text = "100";
            maskedTextBox1.Text = "01:30";
            durLapsUpDown.Value = 43;
            durTimeTextBox.Text = "60:00";

            ReadSaves();

            checkBox2.Checked = false; //hide pitstop details
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            ReadInputs();

            CalculateOutput();
            
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox3.Visible = true;
                max_tires = true;
            }
            else
            {
                textBox3.Visible = false;
                max_tires = false;
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox2.Checked)
            {
                this.Size = new Size(800, 700);
                this.MaximumSize = new Size(800, 700);
                this.MinimumSize = new Size(800, 700);
                panel2.Height = 200;
                dataGridView1.Visible = true;
            }
            else
            {
                this.Size = new Size(800, 545);
                this.MaximumSize = new Size(800, 545);
                this.MinimumSize = new Size(800, 545);
                panel2.Height = 50;
                dataGridView1.Visible = false;

            }
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                inputmode = "laps";
                durLapsUpDown.Visible = true;
                durTimeTextBox.Visible = false;
                label10.Text = "laps";
            }
            else
            {
                inputmode = "time";
                durLapsUpDown.Visible = false;
                durTimeTextBox.Visible = true;
                label10.Text = "minutes";
            }
        }

        private void MaskedTextBox1_Click(object sender, EventArgs e)
        {
            maskedTextBox1.ResetText();
        }

        private void DurTimeTextBox_Click(object sender, EventArgs e)
        {
            durTimeTextBox.ResetText();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            //input name
            savename = loadComboBox.Text;

            //input variables
            ReadInputs();

            //save all input variables to string array
            string[] values = new string[10];
            values = SaveArray(savename, consump, tanksize, fuel_multiplier, laptime_text, inputmode, dur_laps, dur_time, max_tires, max_laps_tires);

            //write values to file
            string path;
            path = @AppDomain.CurrentDomain.BaseDirectory + @"data\";


            //create data folder if it doesn't exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); //Verzeichnis erstellen
            }
            
            //save values array to textfile
            path += @savename + @".txt";
            if (File.Exists(path)) //Messagebox if filename already exists
            {
                DialogResult result;
                result = MessageBox.Show("Replace existing entry?", "Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    File.WriteAllLines(path, values, Encoding.UTF8);
                }
            }
            else
            {
                File.WriteAllLines(path, values, Encoding.UTF8);
            }
            
            if (!File.Exists(path))
            {
                path = @AppDomain.CurrentDomain.BaseDirectory + @"data\";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path += @"data.txt";
                File.WriteAllLines(path, values, Encoding.UTF8);
            }

            string currentName = loadComboBox.Text;
            ReadSaves(); //re-read saved entries
            loadComboBox.Text = currentName;
        }

        private void DelButton_Click(object sender, EventArgs e)
        {
            //delete selected entry from file
            string filename = loadComboBox.Text;
            string path = @AppDomain.CurrentDomain.BaseDirectory + @"data\" + @filename + @".txt";
            DialogResult result = MessageBox.Show("Are you sure you want to delete " + path + "?", "Delete", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                File.Delete(path);
            }


            //re-read file
            ReadSaves();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to load this entry?\r\nAll unsaved values will be lost", "load entry", MessageBoxButtons.YesNo);
            string path = @AppDomain.CurrentDomain.BaseDirectory + @"data\" + @loadComboBox.Text + @".txt";

            if (result == DialogResult.Yes)
            {
                LoadFromFile(path);
            }

        }

    }
}
