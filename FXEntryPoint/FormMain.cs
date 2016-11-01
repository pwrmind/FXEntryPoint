﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FXEntryPoint
{
    public partial class FormMain : Form
    {
        class Timeframe
        {
            public DateTime Time { get; set; }
            public float Open { get; set; }
            public float High { get; set; }
            public float Low { get; set; }
            public float Close { get; set; }
            public int Volume { get; set; }

            public Timeframe()
            {

            }
        }

        List<Timeframe> timeframeCollection;
        //PictureBox picture;

        public FormMain()
        {
            InitializeComponent();
        }

        private void toolStripMenuItemLoad_Click(object sender, EventArgs e)
        {
            Stream csvStream = null;
            if (openFileDialogMain.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((csvStream = openFileDialogMain.OpenFile()) != null)
                    {
                        using (csvStream)
                        {
                            timeframeCollection = LoadFromCSV(csvStream);
                            dataGridViewMain.DataSource = LoadDataSource(timeframeCollection);
                            toolStripStatusLabelTimeframeCount.Text = "Count: " + timeframeCollection.Count.ToString();
                            dateTimePickerFrom.MinDate = timeframeCollection.Min(tf => tf.Time);
                            dateTimePickerFrom.MaxDate = timeframeCollection.Max(tf => tf.Time);
                            dateTimePickerFrom.Value = dateTimePickerFrom.MinDate;
                            dateTimePickerTo.MinDate = timeframeCollection.Min(tf => tf.Time);
                            dateTimePickerTo.MaxDate = timeframeCollection.Max(tf => tf.Time);
                            dateTimePickerTo.Value = dateTimePickerFrom.MaxDate;
                            dateTimePickerFrom.Enabled = true;
                            dateTimePickerTo.Enabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private List<Timeframe> LoadFromCSV(Stream csvStream)
        {
            var reader = new StreamReader(csvStream);
            List<Timeframe> timeframeCollection = new List<Timeframe>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                Timeframe tf = new Timeframe();
                tf.Time = DateTime.Parse(values[0] + " " + values[1]);
                tf.Open = float.Parse(values[2].Replace(".", ","));
                tf.High = float.Parse(values[3].Replace(".", ","));
                tf.Low = float.Parse(values[4].Replace(".", ","));
                tf.Close = float.Parse(values[5].Replace(".", ","));
                tf.Volume = int.Parse(values[6]);

                timeframeCollection.Add(tf);
            }
            return timeframeCollection;
        }

        private BindingSource LoadDataSource(List<Timeframe> timeframeCollection)
        {
            BindingList<Timeframe> bindingList = new BindingList<Timeframe>(timeframeCollection);
            BindingSource source = new BindingSource(bindingList, null);
            return source;
        }

        private void dateTimePickerFrom_ValueChanged(object sender, EventArgs e)
        {
            DateChanged();
        }

        private void dateTimePickerTo_ValueChanged(object sender, EventArgs e)
        {
            DateChanged();
        }

        private void DateChanged()
        {
            if (dateTimePickerFrom.Enabled == true)
            {
                BindingList<Timeframe> bindingList = new BindingList<Timeframe>(timeframeCollection
                    .Where(tf => tf.Time.Date >= dateTimePickerFrom.Value.Date)
                    .Where(tf => tf.Time.Date <= dateTimePickerTo.Value.Date)
                    .ToList());
                BindingSource bs = new BindingSource(bindingList, null);
                toolStripStatusLabelTimeframeCount.Text = "Count: " + bs.Count.ToString();
                dataGridViewMain.DataSource = bs;
            }
        }

        private void Draw(List<Timeframe> items,  BackgroundWorker worker)
        {
            if (items.Count == 0)
                return;

            int r = 100, g = 100, b = 0;
            

            //picture = new PictureBox();
            //picture.Size = new Size(items.Count, (int)Math.Ceiling((items.Max(i=>i.Close) - items.Min(i => i.Close)) * 100000)*2);
            //Bitmap plot = new Bitmap(picture.Size.Width, picture.Size.Height);
            Bitmap plot = new Bitmap(items.Count, (int)Math.Ceiling((items.Max(i => i.Close) - items.Min(i => i.Close)) * 100000) * 2);
            picture.Image = plot;
            int height = plot.Height;
            int width = plot.Width;
            Graphics plotGraphics = Graphics.FromImage(plot);
            plotGraphics.FillRectangle(Brushes.Black, 0, 0, plot.Width, plot.Height);

            //toolStripProgressBarDrawing.Maximum = items.Count;

            for (int i = 0; i < items.Count; i++)
            {
                //toolStripProgressBarDrawing.Value = i;
                worker.ReportProgress(i * 100 / items.Count);

                if (r != 240)
                {
                    r += 20;
                }
                else
                {
                    if (g != 240)
                    {
                        g += 20;
                        r = 0;
                    }
                    else
                    {
                        if (b != 240)
                        {
                            b += 40;
                            g = 0;
                        }
                        else
                        {
                            b = 0;
                            r = 0;
                            g = 0;
                        }
                    }
                }



                Color c = Color.FromArgb(255, r, g, b);
                for (int j = i + 1; j < items.Count; j++)
                {
                    float k = items[i].Close - items[j].Close;

                    Graphics graphicsObj = CreateGraphics();
                    Pen myPen = new Pen(c, 1);

                    //graphicsObj.DrawLine(myPen, j, (this.Height / 2) + (float)(k * 100000), j, (this.Height / 2) + (float)(k * 100000) + 1);
                    float t = (height / 2) + (float)(k * 100000);
                    plotGraphics.DrawLine(myPen, i, (height / 2), j, t);


                    //Console.WriteLine(items[i].operationDate.ToString() + " - " + items[j].operationDate.ToString() + ": " + k.ToString());
                    //File.AppendAllText(@"eurusd.csv", items[i].operationDate.ToString() + "," + items[j].operationDate.ToString() + "," + k.ToString() + Environment.NewLine);
                }

                Graphics graphicsObj02;
                graphicsObj02 = CreateGraphics();
                Pen myPen02 = new Pen(c, 2);
                //graphicsObj02.DrawLine(myPen02, i, (this.Height / 2)-4, i, (this.Height / 2) + 4);
                plotGraphics.DrawLine(myPen02, i, (height / 2) - 4, i, (height / 2) + 4);

                //if (tmpdt.Hour != items[i].operationDate.Hour)
                if (i % 60 == 0)
                {
                    //tmpdt = items[i].operationDate;
                    if (i % 1440 == 0)
                    {
                        Graphics graphicsObj03;
                        graphicsObj03 = CreateGraphics();
                        Pen myPen03 = new Pen(Color.Red, 1);
                        //graphicsObj03.DrawLine(myPen03, i, 0, i, this.Height);
                        plotGraphics.DrawLine(myPen03, i, 0, i, height);
                    }
                    else
                    {
                        Graphics graphicsObj03;
                        graphicsObj03 = CreateGraphics();
                        Pen myPen03 = new Pen(Color.Red, 1);
                        float[] dashValues = { 2, 2 };
                        myPen03.DashPattern = dashValues;
                        //graphicsObj03.DrawLine(myPen03, i, 0, i, this.Height);
                        plotGraphics.DrawLine(myPen03, i, 0, i, height);
                    }
                }
                
                //picture.Refresh();
            }

            //worker.CancelAsync();
            //plot.Dispose();
            //picture.Image.Save("./plot_20100829_20160901.png", ImageFormat.Png);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.FileName = dateTimePickerFrom.Value.ToString("yyyyMMdd") + "_" + dateTimePickerTo.Value.ToString("yyyyMMdd");
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog.FileName != "")
            {
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        picture.Image.Save(fs, ImageFormat.Jpeg);
                        break;
                    case 2:
                        picture.Image.Save(fs, ImageFormat.Bmp);
                        break;
                    case 3:
                        picture.Image.Save(fs, ImageFormat.Gif);
                        break;
                    case 4:
                        picture.Image.Save(fs, ImageFormat.Png);
                        break;
                }
                fs.Close();
            }

        }

        private void plottingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerMain.IsBusy != true)
            {
                plottingToolStripMenuItem.Enabled = false;
                backgroundWorkerMain.RunWorkerAsync();
            }
        }

        private void backgroundWorkerMain_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Draw(timeframeCollection
                .Where(tf => tf.Time.Date >= dateTimePickerFrom.Value.Date)
                .Where(tf => tf.Time.Date <= dateTimePickerTo.Value.Date)
                .ToList(), worker);
        }

        private void backgroundWorkerMain_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBarDrawing.Value = e.ProgressPercentage;
            picture.Refresh();
        }

        private void backgroundWorkerMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            plottingToolStripMenuItem.Enabled = true;
            using (SoundPlayer player = new SoundPlayer("complete.wav"))
            {
                player.Play();
            }
        }
    }

}