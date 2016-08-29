using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace ActualizaBDD
{
    // Clase para comprobar que ciertas cosas "de cajon" estan bien configuradas
    //
    // Micro
    // Servicio Steam (status y reinicio opcional)
    // Conectividad al servidor
    // 
    public class Diagnosticos : INotifyPropertyChanged
    {
        private Dictionary<string, int> _micros;
        private float lastPeak;
        private bool _grabando;
        private WaveIn waveIn;
        
        public event PropertyChangedEventHandler PropertyChanged;


        // multiply by 100 because the Progress bar's default maximum value is 100 
        public float CurrentInputLevel { get { return lastPeak * 100; } }

        public string[] Microfonos { get { return _micros.Keys.ToArray<string>(); } }

        public Diagnosticos()
        {
            _micros = new Dictionary<string, int>();
            _grabando = false;
            
        }

        public void probar_microfono(string micro)
        {
            if (!_grabando)
            {
                waveIn = new WaveIn();
                waveIn.DeviceNumber = _micros[micro];
                waveIn.DataAvailable += waveIn_DataAvailable;
                int sampleRate = 8000; // 8 kHz
                int channels = 1; // mono
                waveIn.WaveFormat = new WaveFormat(sampleRate, channels);
                _grabando = true;
                waveIn.StartRecording();
            }
            else
            {
                waveIn.StopRecording();
            }
        }

        
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) |
                                    e.Buffer[index + 0]);
                float sample32 = sample / 32768f;

                lastPeak = sample32 * 100;

                OnPropertyChanged("CurrentInputLevel");
            }
        }
        

        public void obtener_estado_microfonos()
        {
            int waveInDevices = WaveIn.DeviceCount;
            int i = 0;

            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                _micros.Add(deviceInfo.ProductName, i);
                i++;
            }
        }


        public void obtener_estador_serv_steam()
        {

        }

        public void obtener_conectividad_servidor()
        {

        }


        protected void OnPropertyChanged(string name)
        {

            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool ping_servidor12bdi(string ip,int packets,int timeout)
        {
            Ping ping = new Ping();

            for (int i = 1; i < packets; i++)
            {
                PingReply pr = ping.Send(ip, timeout);

                if (pr.Status == IPStatus.Success) return true;
            }

            return false;
        }

        
    }
}
