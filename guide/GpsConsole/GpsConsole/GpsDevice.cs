﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GpsConsole
{
    class GpsDevice
    {


        private SerialPort gpsPort = new SerialPort();
        private System.Threading.Thread gpsListenerThread = null;

        //public delegate void LocationChangedEventHandler(object sender, LocationChangedEventArgs args);
        //public delegate void SatellitesChangedEventHandler(object sender, SatellitesChangedEventArgs args);
        //event LocationChangedEventHandler locationChanged;
        //public event LocationChangedEventHandler LocationChanged
        //{
        //    add
        //    {
        //        locationChanged += value;
        //    }
        //    remove
        //    {
        //        locationChanged -= value;
        //    }
        //}
        //event SatellitesChangedEventHandler satellitesChanged;
        //public event SatellitesChangedEventHandler DeviceStateChanged
        //{
        //    add
        //    {
        //        satellitesChange += value;
        //    }
        //    remove
        //    {
        //        satellitesChanged -= value;
        //    }
        //}


        private IntPtr locationChange;
        private IntPtr satelliteChange;

        private NmeaParser nmea = new NmeaParser();

        public GpsDevice(IntPtr satelliteChange, IntPtr locationChange)
        {
            this.locationChange = locationChange;
            this.satelliteChange = satelliteChange;
        }

        public GpsDevice()
        {
            this.locationChange = new IntPtr(0);
            this.satelliteChange = new IntPtr(0);
        }

        public void Open()
        {
            FindPort();
            CreateGpsListenerThread();
        }

        private void FindPort()
        {
            gpsPort.ReadTimeout = 300;
            Regex rxGps = new Regex("^[$]GP");
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    gpsPort.PortName = "COM" + i;
                    gpsPort.Open();
                    for (int j = 0; j < 3; j++)
                    {
                        try
                        {
                            string data = gpsPort.ReadLine();
                            if (gpsPort.IsOpen && rxGps.IsMatch(data))
                            {
                                Debug.WriteLine(gpsPort.PortName + ": data: " + data, this.ToString());
                                Debug.WriteLine(gpsPort.PortName + ": connected to gps device.", this.ToString());
                                return;
                            }
                        }
                        catch (TimeoutException ex)
                        {
                            //Debug.WriteLine(gpsPort.PortName + ": " + ex.Message, this.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(gpsPort.PortName + ": " + ex.Message, this.ToString());
                }
                gpsPort.Close();
            }
            Debug.WriteLine("couldn't connect to Gps device! " + gpsPort.PortName, this.ToString());
            return;
        }

        private void CreateGpsListenerThread()
        {
            // we only want to create the thread if we don't have one created already 
            // and we have opened the gps device
            if (gpsListenerThread == null && gpsPort.IsOpen)
            {
                // Create and start thread to listen for GPS events
                gpsListenerThread = new System.Threading.Thread(new System.Threading.ThreadStart(Listen));
                gpsListenerThread.Start();
            }
        }

        private void Listen()
        {
            gpsPort.ReadTimeout = 50;
            lock (this)
            {
                bool listening = true;
                while (listening)
                {
                    string gpsMessage = "";
                    try
                    {
                        gpsMessage = gpsPort.ReadLine();
                        short status = nmea.Parse(gpsMessage);
                        switch (status)
                        {
                            case NmeaParser.LOCATION:
                                //int a = Marshal.ReadIntPtr(locationChange);
                                //Marshal.WriteIntPtr(locationChange, 
                                //locationChange;
                                //Marshal.WriteIntPtr(locationChange, new IntPtr(locationChange.ToInt32() + 1));
                                break;
                            case NmeaParser.SATELLITE:
                                //satelliteChange;
                                break;
                            case NmeaParser.UNRECOGNIZED:
                                break;
                            case NmeaParser.CHECKSUM_INVALID:
                                break;
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.WriteLine(gpsPort.PortName + ": device disconnected?", this.ToString());
                        // TODO
                        // this.Close();
                        // this.Open(); ?
                    }
                }
            }
        } 

    }
}
