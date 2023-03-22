import requests
from geopy.geocoders import Nominatim

class UTABase:
    def __init__(self):                 
        self.attributes = {}

    def SetValue(self, Label, Value):
        self.attributes[Label] = Value

    def GetValue(self, Label):
        return self.attributes[Label]

class Route(UTABase):
    def __init__(self):
        super().__init__()
        self.SetValue("type", "Route")
        self.SetValue("id", None)
        self.SetValue("shortname", None)
        self.SetValue("longname", None)
        self.SetValue("description", None)
        self.SetValue("url", None)

    def __repr__(self):
        return f"Route {self.GetValue('shortname')} ({self.GetValue('longname')})"
    
    def GetApi(self, Token):
        return f"http://api.rideuta.com/utartapi/vehiclemonitor/ByRoute?route={self.GetValue('shortname')}&onwardcalls=true&usertoken={Token}&format=json"

    def GetStatus(self, Token):
        uri = Frontrunner.GetApi(AuthToken)
        r = requests.get(uri)
        return r
    
    def GetTrains(self, Token):
        geolocator = Nominatim(user_agent="geoapiExercises")
        Status = self.GetStatus(Token).json()
        Vehicles = Status['serviceDelivery']['vehicleMonitoringDelivery']['vehicleActivity']
        for v in Vehicles:
            mva = v['monitoredVehicleJourney']
            latitude =  mva['vehicleLocation']['latitude']
            longitude = mva['vehicleLocation']['longitude']
            location = geolocator.reverse(f"{latitude},{longitude}")
            address = location.raw['address']
            Building = address.get('building', '')
            HouseNum = address.get('house_number', '')
            Road = address.get('road', '')
            City = address.get('city', '')
            Zip = address.get('postcode')
            #print(address.keys())
            #print(address)
            print(f"Extensions:            {mva['extensions']}")
            print(f"Line reference:        {mva['lineRef']}")
            print(f"Journey reference:     {mva['framedVehicleJourneyRef']}")
            print(f"Line name:             {mva['publishedLineName']}")
            print(f"Direction:             {mva['directionName']}")
            print(f"Origin:                {mva['originRef']}")
            print(f"Destination:           {mva['destinationRef']}")
            print(f"Destination name:      {mva['destinationName']}")
            print(f"Monitored:             {mva['monitored']}")
            print(f"Location:              {Building}, {HouseNum} {Road}, {City} {Zip}")
            print(f"Bearing:               {mva['bearing']}")
            print(f"Progress Status:       {mva['progressStatus']}")
            print(f"Course:                {mva['courseOfJourneyRef']}")
            print(f"Vehicle Reference:     {mva['vehicleRef']}")
            print()
            print()

        #status['serviceDelivery']['vehicleMonitoringDelivery']['vehicleActivity'][0]['monitoredVehicleJourney'].keys()
        #dict_keys(['extensions', 'lineRef', 'framedVehicleJourneyRef', 'publishedLineName', 'directionName', 'originRef', 'destinationRef', 'destinationName', 
        #           'monitored', 'vehicleLocation', 'bearing', 'progressStatus', 'courseOfJourneyRef', 'vehicleRef'])
        return

def GetFavoriteRoute(Label):
    r = Route()
    l = Label.lower()
    if l ==  "frontrunner":
        r = Route()
        r.SetValue("id", 41065)
        r.SetValue("shortname", "750")
        r.SetValue("longname", "FrontRunner")
        r.SetValue("url", "https://www.rideuta.com/Rider-Tools/Schedules-and-Maps/750-FrontRunner")
    elif l == "834":
        r.SetValue("id", 39024)
        r.SetValue("shortname", "834")
        r.SetValue("longname", "VINEYARD/RIVERWOODS/ PROVO STATION")
        r.SetValue("url", "https://www.rideuta.com/Rider-Tools/Schedules-and-Maps/834-Riverwoods---Provo-Station")

    return r

def PrintStatus(StatusJson):
    j = StatusJson

# Load in a couple of routes
#
r834 = GetFavoriteRoute("834")
Frontrunner = GetFavoriteRoute("Frontrunner")
print(r834)
print(Frontrunner)

# Load in the token
#
with open('token.cfg') as f:
    lines = f.readlines()
AuthToken = lines[0].strip()
print(AuthToken)

# Get the status for the frontrunner
#
Frontrunner.GetTrains(AuthToken)


