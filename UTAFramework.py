import requests
from math import sin, cos, sqrt, atan2, radians

class Route:
    def __init__(self, ID, number, name, type):
        self.ID = ID
        self.number = number
        self.name = name
        self.type = type
        #self.uri = f"https://www.rideuta.com/Rider-Tools/Schedules-and-Maps/{self.number}-{self.name}"

    def __repr__(self):
        return f"{self.type} {self.number}: {self.name}"
    
    def GetNumber(self):
        return self.number
    
    def GetType(self):
        return self.type

class Vehicle:
    def __init__(self, route, location, speed, direction, destination, bearing, status, ref):
        self.route = route
        self.location = location
        self.speed = speed
        self.direction = direction
        self.destination = destination
        self.bearing = bearing
        self.status = status
        self.ref = ref

    def __repr__(self):
        return f"{self.route.GetNumber()} {self.route.GetType()} #{self.ref}: {self.status} traveling {self.direction} to {self.destination} at {self.speed:.1f} mph bearing {self.bearing:.0f}, {self.DescribeLocation()}"

    def DescribeLocation(self):
        MurrayCentral = [40.659902519348506, -111.89437902728629]
        
        R = 6373.0  # Approximate radius of earth in km
        lat1 = radians(MurrayCentral[0])
        lon1 = radians(MurrayCentral[1])
        lat2 = radians(float(self.location[0]))
        lon2 = radians(float(self.location[1]))

        dlon = lon2 - lon1
        dlat = lat2 - lat1

        a = sin(dlat / 2)**2 + cos(lat1) * cos(lat2) * sin(dlon / 2)**2
        c = 2 * atan2(sqrt(a), sqrt(1 - a))

        distancekm = R * c                          # distance in kilometers
        distance = distancekm * 0.621371            # 0.621371 miles in a kilometer
        direction = 'north' if lat1 < lat2 else 'south'

        return f"{distance:.1f} miles {direction} of Murray Central"

def GetMvaValue(mva, key, key2, default):
    if key in mva:
        if key2 in mva[key]:
            return mva[key][key2]
    return default

def GetMvaValueFromList(mva, key, key2, default):
    if key in mva:
        if key2 in mva[key][0]:
            return mva[key][0][key2]
    return default

def GetVehicleStatus(route, authtoken):
    apiuri = f"http://api.rideuta.com/utartapi/vehiclemonitor/ByRoute?route={route.number}&onwardcalls=true&usertoken={authtoken}&format=json"
    j = requests.get(apiuri).json()
    statuses = j['serviceDelivery']['vehicleMonitoringDelivery']['vehicleActivity']
    VehicleStatus = []
 
    for s in statuses:
        mva = s['monitoredVehicleJourney']
        #print(mva)
        latitude =  mva['vehicleLocation']['latitude']
        longitude = mva['vehicleLocation']['longitude']
        location = [latitude, longitude]
        bearing = float(mva['bearing'])
        speed = float(GetMvaValue(mva, 'extensions', 'speed', '0.0'))
        direction = GetMvaValueFromList(mva, 'directionName', 'value', "Unknown")
        destination = GetMvaValueFromList(mva, 'destinationName', 'value', 'Unknown')
        status = GetMvaValueFromList(mva, 'progressStatus', 'value', "On time")
        ref = GetMvaValue(mva, 'vehicleRef', 'value', 'Unknown')
        if 'monitoredCall' in mva:
            v = Vehicle(route, location, speed, direction, destination, bearing, status, ref)
            VehicleStatus.append(v)
    return  sorted(VehicleStatus, key=lambda x: x.direction, reverse=False)

FavoriteRoutes = {
    750: Route(41065, 750, "Frontrunner", "Train"),
    834: Route(39024, 834, "VINEYARD/RIVERWOODS/ PROVO STATION", "Bus")
}

# Load in a couple of routes
#
Route834 = FavoriteRoutes[834]
Frontrunner = FavoriteRoutes[750]
print()
print(Route834)
print(Frontrunner)
print()

# Load in the token
#
with open('token.cfg') as f:
    lines = f.readlines()
AuthToken = lines[0].strip()
# print(AuthToken)

# Get the status for the frontrunner
#
VehicleStatus = GetVehicleStatus(Frontrunner, AuthToken)
print(f"{len(VehicleStatus)} active Frontrunner trains:")
print(f"----------------")
for Train in VehicleStatus:
    print(Train)
print()
    
# status['serviceDelivery']['vehicleMonitoringDelivery']['vehicleActivity'][0]['monitoredVehicleJourney'].keys()
# dict_keys(['extensions', 'lineRef', 'framedVehicleJourneyRef', 'publishedLineName', 'directionName', 'originRef', 'destinationRef', 'destinationName', 
#           'monitored', 'vehicleLocation', 'bearing', 'progressStatus', 'courseOfJourneyRef', 'vehicleRef'])


