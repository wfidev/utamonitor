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
    
class Stop(UTABase):
    def __init__(self):
        super().__init__()
        self.SetValue("type", "Stop")
        self.SetValue("id", None)
        self.SetValue("code", None)
        self.SetValue("name", None)
        self.SetValue("description", None)
        self.SetValue("url", None)
        self.SetValue("latitude", None)
        self.SetValue("longitude", None)
        self.SetValue("zone", None)
        self.SetValue("locationtype", None)
        self.SetValue("parentstation", None)
        self.SetValue("timezone", None)
        self.SetValue("wheelchair", False)

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

r834 = GetFavoriteRoute("834")
Frontrunner = GetFavoriteRoute("Frontrunner")

print(r834)
print(Frontrunner)
