using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace BillBlacklist
{
    public static class BlacklistAddon //i dont know how to declare a static type inside the dll assembly code... so have to use static class instead
    { 
        public static int checkInstance(Bill myBill)
        {
            Map map = myBill.Map;
            MapComponent_BlacklistHold b = map.GetComponent<MapComponent_BlacklistHold>();
            if(b == null)
            {
                b = new MapComponent_BlacklistHold(map);
                map.components.Add(b);
            }
            return b.checkInstance(myBill);
        }
        public static int changeInstance(Bill myBill, int change)
        {
            Verse.Log.Warning("Fires");
            Verse.Log.Warning("Fired");
            Map map = myBill.Map;
            MapComponent_BlacklistHold b = map.GetComponent<MapComponent_BlacklistHold>();
            if (b == null)
            {
                Verse.Log.Warning("Had to create instance");
                b = new MapComponent_BlacklistHold(map);
                map.components.Add(b);
            }
            return b.changeInstance(myBill, change);
        }
        public static int deleteInstance(Bill myBill)
        {
            Map map = myBill.Map;
            MapComponent_BlacklistHold b = map.GetComponent<MapComponent_BlacklistHold>();
            if (b == null)
            {
                b = new MapComponent_BlacklistHold(map);
                map.components.Add(b);
            }
            else
            {
                b.deleteInstance(myBill);
            }
            return 0;
        }


    }

    public class MapComponent_BlacklistHold : MapComponent
    {
        public MapComponent_BlacklistHold(Map map) : base(map) 
        {
            blackListedReferences = new Dictionary<Bill, int>();//int = 0 if not blacklisted, 1 if blacklisted
        }

        private Dictionary<Bill, int>blackListedReferences;
        List<Bill> fake;
        List<int> fake2;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref blackListedReferences, "blackListedBills", LookMode.Reference, LookMode.Value, ref fake, ref fake2);//i doubt this will save our dictionary correctly, will probably have to make something a bit more complicated, which just
            //holds the data in two seperate arrays and references the two by id instead of being a dictionary that does that automatically
        }

        public int createInstance(Bill myBill)//creates a blacklist class to assign to a bill. has to be done this way because we cannot create variables inside source code, just adjust methods
        {
            if (!blackListedReferences.ContainsKey(myBill))
            {
                blackListedReferences.Add(myBill, 0);
                return blackListedReferences[myBill];
            }
            blackListedReferences[myBill] = 0;
            return blackListedReferences[myBill];
        }
        public int checkInstance(Bill myBill)//retrieves current value, and if not defined creates a new blacklist
        {
            if (blackListedReferences.ContainsKey(myBill))
            {
                return blackListedReferences[myBill];
            }
            else
            {
                return createInstance(myBill);
            }
        }
        public int changeInstance(Bill myBill, int change)
        {
            if (blackListedReferences.ContainsKey(myBill))
            {
                blackListedReferences[myBill] = change;
                return blackListedReferences[myBill];
            }
            else
            {
                return 0;
            }
        }
        public int deleteInstance(Bill myBill)
        {
            blackListedReferences.Remove(myBill);
            return 0;
        }
    }
}
