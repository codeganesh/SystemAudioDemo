using System;
using System.Windows.Forms;
using NAudio.Wave;
using System.IO;
using System.Diagnostics;

namespace SystemAudioDemo
{
    public partial class Form1 : Form
    {
        private string outputFileName;
        private WasapiLoopbackCapture capture;
        public Form1()
        {
            InitializeComponent();
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Wave files | *.wav";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            outputFileName = dialog.FileName;

            RecordButton.Enabled = false;
            StopButton.Enabled = true;

            capture = new WasapiLoopbackCapture();
            var writer = new WaveFileWriter(outputFileName, capture.WaveFormat);

            capture.DataAvailable += async (s, e) =>
            {
                if (writer != null)
                {
                    await writer.WriteAsync(e.Buffer, 0, e.BytesRecorded);
                    await writer.FlushAsync();
                }
            };

            capture.RecordingStopped += (s, e) =>
            {
                if (writer != null)
                {
                    writer.Dispose();
                    writer = null;
                }

                RecordButton.Enabled = true;
                capture.Dispose();
            };

            capture.StartRecording();



        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopButton.Enabled = false;
            capture.StopRecording();

            if (outputFileName == null)
                return;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetDirectoryName(outputFileName),
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
        }
    }
}
