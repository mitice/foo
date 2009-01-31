using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using TouristGuide.map.obj;
using TouristGuide.map.exception;

namespace TouristGuide.map.repository
{
    public class MapPkgRepository
    {
        private MapSourceMem mapSourceMem;
        public MapSourceMem MapSourceMem
        {
            set
            {
                this.mapSourceMem = value;
            }
        }

        private MapSourceHdd mapSourceHdd;
        public MapSourceHdd MapSourceHdd
        {
            set
            {
                this.mapSourceHdd = value;
            }
        }

        private MapSourceWeb mapSourceWeb;
        public MapSourceWeb MapSourceWeb
        {
            set
            {
                this.mapSourceWeb = value;
            }
        }

        /// <summary>
        /// When loading map takes long time (source web or hdd),
        /// set this event (eg. map displayer can show loading box)
        /// </summary>
        public event LoadingMapPkg loadingEvent;
        public delegate void LoadingMapPkg(string msg);

        /// <summary>
        /// Get map package by coordinates.
        /// </summary>
        /// <returns>MapPackage instance</returns>
        public MapPackage getMapPkg(double latitude, double longitude, int zoom)
        {
            MapPackage pkg = this.mapSourceMem.findMapPkg(latitude, longitude, zoom);
            if (pkg == null)
            {
                //loadingEvent("Loading map from hard drive.");
                pkg = this.mapSourceHdd.findMapPkg(latitude, longitude, zoom);
                if (pkg == null)
                {
                    //loadingEvent("Loading map from web server.");
                    pkg = this.mapSourceWeb.findMapPkg(latitude, longitude, zoom);
                }
                if (pkg != null)
                    this.mapSourceMem.putMapPkg(pkg);
                else
                {
                    string msg = "Map package can't be found for location: ("
                        + latitude + "; " + longitude + "), zoom: " + zoom;
                    Debug.WriteLine("MapSourceManager: getMapPkg: " + msg);
                    throw new MapNotFoundException(msg);
                }
            }
            return pkg;
        }
    }
}
