using System;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ionic.Zlib;
using static System.Net.Mime.MediaTypeNames;
using Verse.Sound;
using BillBlacklist;

namespace BillBlacklist
{
    [StaticConstructorOnStartup]
    [HarmonyDebug]
    public static class BillBlackList
    {
        static BillBlackList()
        {

            Harmony harmony = new Harmony("rimworld.mod.Pelican.BillBlackList");
            Harmony.DEBUG = true;
            harmony.PatchAll();
            Verse.Log.Warning("My Thing has loaded");

        }
    }
}
[HarmonyPatch(typeof(Dialog_BillConfig))]
[HarmonyPatch(nameof(Dialog_BillConfig.DoWindowContents))]
public static class Dialog_BillConfig_ChangeDialog_Patch
{

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
    {
        var lineList = new List<CodeInstruction>(lines);

        int startInt = -1;
        int endInt = -1;
        bool foundVirt = false;
        bool foundCodeSection = false;
        //this for loop looks for the section that renders the bill skill range
        //look at rim notes for the il lines to see what it is referencing, but it just checks
        
        for(int i = 0; i < lineList.Count; i++)
        {
            if (lineList[i].opcode == OpCodes.Brtrue_S)
            {
                startInt = i + 1;
            }
            if (foundCodeSection == true)
            {
                if (lineList[i].opcode == OpCodes.Callvirt)
                {
                    if (foundVirt)
                    {
                        endInt = i;
                        break;
                    }
                    foundVirt = true;
                }
            }
            else
            {
                if (lineList[i].operand as string == "AllowedSkillRange")
                {
                    foundCodeSection = true;
                }
            }
        }
        int adjustPoint = 2;
        while (!(lineList[adjustPoint].ToString().Contains("ldloc") && lineList[adjustPoint - 1].ToString().Contains("pop") && lineList[adjustPoint - 2].ToString().Contains("Listing_Standard::IntRange")))
        {
            adjustPoint++;
        }
        int copyPoint = 2;
        while (!(lineList[copyPoint + 1].ToString().Contains("ldloc") && lineList[copyPoint].ToString().Contains("brtrue") && lineList[copyPoint + 2].ToString().Contains("AllowedSkillRange")))
        {
            copyPoint++;
        }
        //start int = startint - 1 = copypoint
        int jt3 = copyPoint;//need it to add il code to later
        while (!(lineList[copyPoint].ToString().Contains("ldarg")))
        {
            copyPoint++;
        }
        CodeInstruction secondCopy = new CodeInstruction(OpCodes.Add, null);
        secondCopy.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
        secondCopy.opcode = lineList[copyPoint].opcode;
        while (!(lineList[copyPoint].ToString().Contains("get_PawnRestriction")))
        {
            copyPoint--;
        }
        CodeInstruction get_pawnRestriction = new CodeInstruction(OpCodes.Add, null);
        get_pawnRestriction.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
        get_pawnRestriction.opcode = lineList[copyPoint].opcode;
        while (!(lineList[copyPoint].ToString().Contains("Dialog_BillConfig::bill")))
        {
            copyPoint++;
        }
        CodeInstruction ldfld_billconfig_bill = new CodeInstruction(OpCodes.Add, null);
        ldfld_billconfig_bill.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
        ldfld_billconfig_bill.opcode = lineList[copyPoint].opcode;
        while (!(lineList[copyPoint].ToString().Contains("ButtonText")))
        {
            copyPoint++;
        }
        CodeInstruction list_button_text = new CodeInstruction(OpCodes.Add, null);
        list_button_text.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
        list_button_text.opcode = lineList[copyPoint].opcode;
        while (!(lineList[copyPoint].ToString().Contains("SoundDefOf")))
        {
            copyPoint++;
        }
        CodeInstruction SoundDefOf = new CodeInstruction(OpCodes.Add, null);
        SoundDefOf.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
        SoundDefOf.opcode = lineList[copyPoint].opcode;
        while (!(lineList[copyPoint].ToString().Contains("PlayOneShotOnCamera")))
        {
            copyPoint++;
        }
        CodeInstruction PlayOneShot = new CodeInstruction(OpCodes.Add, null);
        PlayOneShot.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
        PlayOneShot.opcode = lineList[copyPoint].opcode;


        if (startInt != -1 && endInt != -1)
        {

            //this section creates the if statement
            List<CodeInstruction> myInstructs = new List<CodeInstruction>();
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));
            myInstructs.Add(ldfld_billconfig_bill);
            myInstructs.Add(get_pawnRestriction);//this is callvirt instance class Verse.Pawn RimWorld.Bill::get_PawnRestriction()?
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//might need to be true
            int jf1 = myInstructs.Count - 1;
            Type[] forFunc = { typeof(string), typeof(Int32), typeof(string)};



            //if statment for if(blackListAddon.checkInstance(bill))


            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(ldfld_billconfig_bill);//this is ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "checkInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse));//need to add operand label to this
            int jf2 = myInstructs.Count - 1;//need to jump to second if statement: else if(listingstandard4.buttontext("NotBlacklisted".Translate()))

            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 21));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "BlackListed"));//blacklisted

            // I dont know for some reason translate doesnt work not going to question it, might need to actually have the word blacklisted in it or something
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            myInstructs.Add(list_button_text);//this is callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//needs to send to start of the next if statement, this is [11]
            int jf3 = myInstructs.Count - 1;


            //end of if statement
            //start of statement
            //BlacklistAddon.changeInstance(bill, );
            //SoundDefOf.CLick.PLayOneShotOnCamera();

            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(ldfld_billconfig_bill);//this is ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 0));
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "changeInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Pop));

            myInstructs.Add(SoundDefOf);//this is ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull, null));//this is ldnull, obviously didnt need to do this one but thought it would be more clear
            myInstructs.Add(PlayOneShot);//call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)
            //end of stateement
            //jump over statement because it needs to be else if, and else needs to be jumped over if it is true

            myInstructs.Add(new CodeInstruction(OpCodes.Br));//need to add an operand of label to this one too
            int jf4 = myInstructs.Count - 1;
            int jt1 = jf4;

            //if statement for if statement section if(listingstandard4.buttontext("NotBlacklisted".Translate()))

            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 21));
            int jt2 = myInstructs.Count - 1;
            myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "Not Blacklisted"));//not blacklisted
            //myInstructs.Add(lineList[endInt + 29]);//this is call valuetype Verse.TaggedString Verse.Translator::Translate(string)
            //myInstructs.Add(lineList[endInt + 30]);//this is call string Verse.TaggedString::op_Implicit(valuetype Verse.TaggedString)
            // I dont know for some reason translate doesnt work not going to question it, might need to actually have the word blacklisted in it or something
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            myInstructs.Add(list_button_text) ;//this is callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//needs to send to after the next statement, this is [11]
            int jf5 = myInstructs.Count - 1;

            //end of if statement
            //start of statement
            //BlacklistAddon.changeInstance(bill, true);
            //SoundDefOf.Click.PlayOneShotOnCamera();

            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(ldfld_billconfig_bill);//this is ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 1));
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "changeInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Pop));

            myInstructs.Add(SoundDefOf);//this is ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull, null));//this is ldnull, obviously didnt need to do this one but thought it would be more clear
            myInstructs.Add(PlayOneShot);//call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)

            //end of statement
            //assignments of jump values, basically just saying at the end of if statements jump to after the new statement that I just made
            ////////////////////////////////////////////////////////////////
            ///definition
            ///j = jump, t = to, f = from, s = saved, number what jumping from or too
            Label jt1Label = il.DefineLabel();
            myInstructs[jt1].labels.Add(jt1Label);

            Label jt2Label = il.DefineLabel();
            myInstructs[jt2].labels.Add(jt2Label);

            Label jtStart = il.DefineLabel();
            myInstructs[0].labels.Add(jtStart);

            myInstructs[jf1].operand = lineList[jt3].operand;
            myInstructs[jf2].operand = jt2Label;
            myInstructs[jf3].operand = jt1Label;
            myInstructs[jf4].operand = lineList[jt3].operand;
            myInstructs[jf5].operand = lineList[jt3].operand;


            //set pawn == null to send to our function, as it checks that before the allowed skill range
            //retrieve it
            while (!(lineList[copyPoint].ToString().Contains("brtrue") && lineList[copyPoint - 1].ToString().Contains("get_PawnRestriction") && lineList[copyPoint - 2].ToString().Contains("Dialog_BillConfig::bill") && lineList[copyPoint + 1].ToString().Contains("ldarg")))
            {
                copyPoint--;
            }
            ////assogn it
            lineList[copyPoint].operand = jtStart;

            //////////////////////////////////////////////////////////////////
            lineList.InsertRange(adjustPoint, myInstructs);

        }

        return lineList;
        //return null;
    }
}
[HarmonyPatch(typeof(Bill))]
[HarmonyPatch(nameof(Bill.PawnAllowedToStartAnew))]
public static class PawnAllowedToStartAnew_Patch
{

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
    {
        var lineList = new List<CodeInstruction>(lines);

        int start = 0;
        bool falseFound = true;
        while(start < lineList.Count && falseFound)
        {
            start++;
            if (lineList[start].opcode == OpCodes.Brfalse_S)
            {
                falseFound = false;
            }
        }
        ++start;
        if(start < lineList.Count && lineList.Count > 0)
        {
            List<CodeInstruction> myInstructs = new List<CodeInstruction>();

            //if statment for if(blackListAddon.checkInstance(bill))

            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            //myInstructs.Add(new CodeInstruction(OpCodes.Newobj, CodeInstruction.Call(typeof(Verse.ThingFilter), ".ctor")));
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "checkInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            myInstructs.Add(new CodeInstruction(OpCodes.Ceq));
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//need to add operand label to this
            int jf1 = myInstructs.Count - 1;
            //needs to jump to after our statement

            //return !(pawn == pawnrestriction)

            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(lineList[1]);//this should be, ldfld class Verse.Pawn RimWorld.Bill::pawnRestriction
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_1));
            myInstructs.Add(new CodeInstruction(OpCodes.Ceq));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            myInstructs.Add(new CodeInstruction(OpCodes.Ceq));
            myInstructs.Add(new CodeInstruction(OpCodes.Ret));


            //if statement jumps
            Label jtAfterLabel = il.DefineLabel();
            lineList[start].labels.Add(jtAfterLabel);


            myInstructs[jf1].operand = jtAfterLabel;

            lineList.InsertRange(start, myInstructs);


        }

        return lineList;
    }
}
[HarmonyPatch(typeof(BillStack))]
[HarmonyPatch(nameof(BillStack.Delete))]
public static class DeleteBill_Change
{

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
    {
        var lineList = new List<CodeInstruction>(lines);

        //find deletebill

        var myInstructs = new List<CodeInstruction>();

        //blacklistAddon.delete(bill)
        myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
        myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "deleteInstance"));
        myInstructs.Add(new CodeInstruction(OpCodes.Pop));

        lineList.InsertRange(0, myInstructs);

        return lineList;
    }
}

