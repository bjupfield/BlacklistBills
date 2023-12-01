using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace BillBlacklist
{
    public class Blacklisted
    {
        public int blackListed = 0;
    }
    public static class BlacklistAddon
    {
        private static Dictionary<Bill, Blacklisted>blackListedReferences = new Dictionary<Bill, Blacklisted>();

        public static Blacklisted createInstance(Bill myBill)//creates a blacklist class to assign to a bill. has to be done this way because we cannot create variables inside source code, just adjust methods
        {
            Blacklisted blackListInstance= new Blacklisted();
            if (!blackListedReferences.ContainsKey(myBill))
            {
                blackListedReferences.Add(myBill, blackListInstance);
                return blackListedReferences[myBill];
            }
            blackListedReferences[myBill] = blackListInstance;
            return blackListedReferences[myBill];
        }
        public static int checkInstance(Bill myBill)//retrieves current value, and if not defined creates a new blacklist
        {
            if (blackListedReferences.ContainsKey(myBill))
            {
                return blackListedReferences[myBill].blackListed;
            }
            else
            {
                return createInstance(myBill).blackListed;
            }
        }
        public static int changeInstance(Bill myBill, int change)
        {
            if (blackListedReferences.ContainsKey(myBill))
            {
                blackListedReferences[myBill].blackListed = change;
                return blackListedReferences[myBill].blackListed;
            }
            else
            {
                return 0;
            }
        }
        public static int deleteInstance(Bill myBill)
        {
            blackListedReferences.Remove(myBill);
            return 0;
        }
    }
}
