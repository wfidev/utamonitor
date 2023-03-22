import requests

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
        return r.json()

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
j = Frontrunner.GetStatus(AuthToken)
print(j)


