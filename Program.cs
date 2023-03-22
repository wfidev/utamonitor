/*
    COPYRIGHT 2012 - Utah Transit Authority
    This program (UTA SIRI Demo) is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

    
    This file is part of UTA SIRI Demo.

    UTA SIRI Demo is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    UTA SIRI Demo is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with UTA SIRI Demo.  If not, see <http://www.gnu.org/licenses/>.
 */




using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;

namespace SIRIDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Here we startup the UI shell. This UI merely acts as a helper wrapper for calling 'publishToGE()' for us.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


        /// <summary>
        /// This is where all the magic happens. This function accepts and then passes parameters up to the public API.
        /// It then downloads and parses the resulting XML. Vehicle and VehicleStop convenience containers are used to help
        /// keep track of the data and group it accordingly. After, the data is written to a KML file and then launches
        /// the system's default program for reading KML files.
        /// </summary>
        /// <param name="stopId"></param>
        /// <param name="fileName"></param>
        /// <param name="betaId"></param>
        /// <param name="minutesOut"></param>
        public static void publishToKML(String stopId, String fileName, String betaId, String minutesOut)
        {
            //Prepare the URI, and then contact the helper function to download the results of calling the URI.
            String builtURI = "http://Api.Rideuta.com/SIRI/SIRI.svc/StopMonitor?stopid=" + stopId + "&minutesout=" + minutesOut + "&onwardcalls=true&filterroute=&usertoken=" + betaId;
            String result = getURIResult(builtURI);

            //Prepare an XML parser object with the XML results from the public API.
            XmlDocument xmlDoc = new XmlDocument();
            if (result == null || result.Length <= 0)
            {
                throw new Exception("No result returned from the request!!");
            }
            xmlDoc.LoadXml(result);

            //Instantiate collections for the grouping containers. These will be filled as we traverse the XML nodes.
            List<Vehicle> vehicles = new List<Vehicle>();
            VehicleStop currentStop = new VehicleStop();
           
            //And now we go through the result, creating a list of vhicles and currentStops from the data.
            //This would be much more elegant by using LINQ to XML; however, as a demonstration of how it may
            //be written in a cross-platform format, we'll be using a bunch of "foreach" loops and "if" statements.
            foreach (XmlNode siriNode in xmlDoc.ChildNodes)
            {
                foreach (XmlNode siriChildNode in siriNode)
                {
                    
                    if (siriChildNode.Name.Equals("StopMonitoringDelivery"))
                    {
                        foreach (XmlNode stopMonitoringDeliveryChildNode in siriChildNode.ChildNodes)
                        {

                            //Handle the current stop information
                            if (stopMonitoringDeliveryChildNode.Name.Equals("Extensions"))
                            {
                                //Take care of the current stop's location
                                foreach (XmlNode extensionChildNode in stopMonitoringDeliveryChildNode)
                                {
                                    if (extensionChildNode.Name.Equals("StopName"))
                                    {
                                        currentStop.Name = extensionChildNode.InnerText;
                                    }
                                    else if (extensionChildNode.Name.Equals("StopLatitude"))
                                    {
                                        currentStop.Latitude = extensionChildNode.InnerText;
                                    }
                                    else if (extensionChildNode.Name.Equals("StopLongitude"))
                                    {
                                        currentStop.Longitude = extensionChildNode.InnerText;
                                    }
                                }
                            }

                            //Handle bus information
                            else if (stopMonitoringDeliveryChildNode.Name.Equals("MonitoredStopVisit"))
                            {
                                foreach (XmlNode monitoredStopVisitChildNode in stopMonitoringDeliveryChildNode.ChildNodes)
                                {

                                    if (monitoredStopVisitChildNode.Name.Equals("MonitoredVehicleJourney"))
                                    {
                                        //Handle vehicle attributes
                                        Vehicle vehicle = new Vehicle();
                                        foreach (XmlNode monitoredVehicleJourneyChildNode in monitoredStopVisitChildNode.ChildNodes)
                                        {
                                            if (monitoredVehicleJourneyChildNode.Name.Equals("PublishedLineName"))
                                            {
                                                vehicle.LineName = monitoredVehicleJourneyChildNode.InnerText;
                                            }
                                            else if (monitoredVehicleJourneyChildNode.Name.Equals("VehicleRef"))
                                            {
                                                vehicle.Name = monitoredVehicleJourneyChildNode.InnerText;
                                            }

                                            //Handle vehicle location
                                            else if (monitoredVehicleJourneyChildNode.Name.Equals("VehicleLocation"))
                                            {
                                                foreach (XmlNode vehicleLocationChildNode in monitoredVehicleJourneyChildNode)
                                                {
                                                    if (vehicleLocationChildNode.Name.Equals("Longitude"))
                                                    {
                                                        vehicle.Longitude = vehicleLocationChildNode.InnerText;
                                                    }
                                                    else if (vehicleLocationChildNode.Name.Equals("Latitude"))
                                                    {
                                                        vehicle.Latitude = vehicleLocationChildNode.InnerText;
                                                    }
                                                }
                                            }

                                            //Handle current stop's departure time estimate for the vehicle
                                            else if (monitoredVehicleJourneyChildNode.Name.Equals("MonitoredCall"))
                                            {
                                                //Handle the current stop's location
                                                foreach (XmlNode monitoredCallChildNode in monitoredVehicleJourneyChildNode)
                                                {
                                                    if (monitoredCallChildNode.Name.Equals("Extensions"))
                                                    {
                                                        foreach (XmlNode extensionChildNode in monitoredCallChildNode)
                                                        {
                                                            if (extensionChildNode.Name.Equals("EstimatedDepartureTime"))
                                                            {
                                                                vehicle.EstimatedDepartureTime = extensionChildNode.InnerText;
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                            //Take care of OnwardCalls.
                                            else if (monitoredVehicleJourneyChildNode.Name.Equals("OnwardCalls"))
                                            {
                                                foreach (XmlNode onwardCallsChildNode in monitoredVehicleJourneyChildNode)
                                                {
                                                    if (onwardCallsChildNode.Name.Equals("OnwardCall"))
                                                    {
                                                        VehicleStop vehicleStop = new VehicleStop();
                                                        foreach (XmlNode onwardCallChildNode in onwardCallsChildNode)
                                                        {
                                                            if (onwardCallChildNode.Name.Equals("StopPointRef"))
                                                            {
                                                                vehicleStop.Id = onwardCallChildNode.InnerText;
                                                            }
                                                            else if (onwardCallChildNode.Name.Equals("StopPointName"))
                                                            {
                                                                vehicleStop.Name = onwardCallChildNode.InnerText;
                                                            }
                                                        }
                                                        vehicle.OnwardCalls.Add(vehicleStop);
                                                    }
                                                }
                                            }
                                        }
                                        //For debugging reasons it's always nice to have console output...
                                        Console.WriteLine("Vehicle: " + vehicle);
                                        vehicles.Add(vehicle);
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //
            ////And now write file! For this example, we will be using KML as our target format.
            //According to Wikipedia, "Keyhole Markup Language (KML) is an XML notation for expressing geographic 
            //annotation and visualization within Internet-based, two-dimensional maps and three-dimensional Earth 
            //browsers. KML was developed for use with Google Earth, which was originally named Keyhole Earth Viewer."
            // - http://en.wikipedia.org/wiki/Kml
            //

            //Make sure our .kml export file ends with the correct suffix.
            if (!fileName.EndsWith(".kml"))
            {
                fileName += ".kml";
            }
            TextWriter tw = new StreamWriter(fileName);

            
            tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");

            //Go through all the vehicles in our collection and generate the appropriate KML
            foreach (Vehicle vehicle in vehicles)
            {
                String latitude = vehicle.Latitude;
                String longitude = vehicle.Longitude;

                //Take care of extra formatting for lat/lon so that it works well with the Google kml file
                if (latitude.StartsWith("+"))
                {
                    latitude = latitude.Substring(1);
                }
                if (longitude.StartsWith("+"))
                {
                    longitude = longitude.Substring(1);
                }
                //It's always good practice to do some error checking of our data. If this were going to be released commercially though,
                //we'd definitely have a little stronger error checking than this. But for our example, this will suffice.
                if (longitude.Trim().Equals("0")
                    || latitude.Trim().Equals("0")
                    || (!latitude.StartsWith("40") && !latitude.StartsWith("41"))
                    || (!longitude.StartsWith("-111") && !longitude.StartsWith("-112")))
                { //Must be bad GPS data. Skip it so it doesn't skew the results!
                    continue;
                }

                //First do a placemark for the vehicle
                String outputString = "" +
                "\r\n <Placemark>" +
                "\r\n   <name>" + vehicle.ToString() + "</name>" +
                "\r\n   <description>Next Stops:\n" + vehicle.getOnwardCallsString(10, currentStop.Name) + "</description>" +
                "\r\n <Style id=\"normalPlacemark\">" +
                "\r\n  <IconStyle>" +
                "\r\n    <Icon>" +
                "\r\n      <href>http://maps.google.com/mapfiles/kml/shapes/truck.png</href>" +
                "\r\n    </Icon>" +
                "\r\n    <scale>0.9</scale>" + // was 0.5
                "\r\n  </IconStyle>" +
                "\r\n</Style>" +
                "\r\n  <Point>" +
                "\r\n    <coordinates>" + longitude + "," + latitude + "</coordinates>" +
                "\r\n  </Point>" +
                "\r\n </Placemark>";

                //String outputString = "";
                //Now do an additional placemark to draw a line from the vehicle to the stop
                if (currentStop.Latitude != null && currentStop.Longitude != null)
                {
                    outputString += "" +
                    "\r\n <Placemark>" +
                    "\r\n  <name></name>" +
                    "\r\n  <description></description>" +
                    "\r\n  <styleUrl>#yellowLineGreenPoly</styleUrl>" +
                    "\r\n  <LineString>" +
                    "\r\n   <extrude>1</extrude>" +
                    "\r\n   <tessellate>1</tessellate>" +
                    "\r\n   <altitudeMode>absolute</altitudeMode>" +
                    "\r\n   <coordinates> " + longitude + "," + latitude + "," + 1500 + "\n\r" +
                        currentStop.Longitude + "," + currentStop.Latitude + "," + (1500) + "\n\r" +
                    "\r\n   </coordinates>" +
                    "\r\n  </LineString>" +
                    "\r\n </Placemark>";
                }

                    

                tw.WriteLine(outputString);

            }

            //And now place the current stop associated with all these vehicles.
            if (currentStop.Name != null)
            {
                tw.WriteLine(
                    "\r\n<Placemark>"
                    + "\r\n<description>" + getVehicleNamesForVehiclesForCurrentStop(vehicles) + "\n\n.</description>"
                    + "\r\n<name>" + currentStop.Name + "</name>"
                    + "\r\n<visibility>1</visibility>"
                    + "\r\n<Style>"
                    + "\r\n    <IconStyle>"
                    + "\r\n<Icon>"
                    + "\r\n    <href>http://maps.google.com/mapfiles/kml/shapes/flag.png</href>"
                    + "\r\n</Icon>"
                    + "\r\n<scale>1.3</scale>"
                    + "\r\n</IconStyle>"
                    + "\r\n</Style>"
                    + "\r\n  <Point>"
                    + "\r\n    <coordinates>" + currentStop.Longitude + "," + currentStop.Latitude + "</coordinates>"
                    + "\r\n  </Point>"
                    + "\r\n</Placemark>"
                );
            }

            tw.WriteLine("</Document></kml>");
            tw.Close();

            //And now call the system to launch whatever program it has that can read the file.
            System.Diagnostics.Process.Start(fileName);

        }

        /// <summary>
        /// Helper function for use within publishToKML(). This will give us a (slightly) formatted string representing the names of all the vehicles passed in. Breaking it out here makes the code more human-readable.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <returns></returns>
        public static String getVehicleNamesForVehiclesForCurrentStop(List<Vehicle> vehicles)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < vehicles.Count; i++)
            {
                result.Append("\n");
                result.Append(vehicles[i].ToString());
            }

            return result.ToString();
        }



        /// <summary>
        /// Get the result of a URI request as a String value.
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        private static String getURIResult(String uriString)
        {
            HttpWebRequest req = WebRequest.Create(uriString) as HttpWebRequest;
            string result = null;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}
