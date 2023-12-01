﻿using System;
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
        //this removes the code if ti finds it


        if (startInt != -1 && endInt != -1)
        {
            int jt3 = startInt - 1;

            //this section creates the if statement
            int myStatementStartPos = endInt + 1;
            List<CodeInstruction> myInstructs = new List<CodeInstruction>();
            myInstructs.Add(new CodeInstruction(OpCodes.Neg));
            myInstructs[0].opcode = lineList[startInt + 2].opcode;
            myInstructs[0].operand = lineList[startInt + 2].operand;
            myInstructs.Add(new CodeInstruction(OpCodes.Neg));
            myInstructs[1].opcode = OpCodes.Ldfld;
            myInstructs[1].operand = lineList[startInt + 3].operand;
            myInstructs.Add(lineList[startInt - 11]);//this is callvirt instance class Verse.Pawn RimWorld.Bill::get_PawnRestriction()?
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//might need to be true
            int jf1 = myInstructs.Count - 1;
            //test just words statement
            //myInstructs.Add(new CodeInstruction(OpCodes.Br, lineList[jt3].operand));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 24));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "Hithere"));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, -1));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));
            Type[] forFunc = { typeof(string), typeof(Int32), typeof(string)};
            //myInstructs.Add(CodeInstruction.Call(typeof(Verse.Listing_Standard), "Label", forFunc));
            //myInstructs.Add(new CodeInstruction(OpCodes.Pop));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_1));


            //end of if statement
            //start of button statements
            //add gap statement
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 24));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            //myInstructs.Add(CodeInstruction.Call(typeof(Verse.Listing), "Gap"));

            //add begin rect statement

            //myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 24));
            //myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_1));
            //myInstructs.Add(CodeInstruction.Call(typeof(Verse.Listing), "Begin"));


            //if statment for if(blackListAddon.checkInstance(bill))


            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(lineList[startInt + 3]);//this is ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "checkInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse));//need to add operand label to this
            int jf2 = myInstructs.Count - 1;//need to jump to second if statement: else if(listingstandard4.buttontext("NotBlacklisted".Translate()))
            //like
            //myInstructs[thirdSaved].operand = wherever ist supposed to go


            //end of if statement
            //if statement for if(listingstandard4.buttontext("Blacklist".Translate()))

            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 24));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "Blacklisted"));
            //myInstructs.Add(lineList[endInt + 29]);//this is call valuetype Verse.TaggedString Verse.Translator::Translate(string)
            //myInstructs.Add(lineList[endInt + 30]);//this is call string Verse.TaggedString::op_Implicit(valuetype Verse.TaggedString)
            // I dont know for some reason translate doesnt work not going to question it, might need to actually have the word blacklisted in it or something
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            myInstructs.Add(lineList[endInt + 33]);//this is callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//needs to send to start of the next if statement, this is [11]
            int jf3 = myInstructs.Count - 1;


            //end of if statement
            //start of statement
            //BlacklistAddon.changeInstance(bill, );
            //SoundDefOf.CLick.PLayOneShotOnCamera();

            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(lineList[startInt + 3]);//this is ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 0));
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "changeInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Pop));

            myInstructs.Add(lineList[endInt + 39]);//this is ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            myInstructs.Add(lineList[endInt + 40]);//this is ldnull, obviously didnt need to do this one but thought it would be more clear
            myInstructs.Add(lineList[endInt + 41]);//call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)
            //end of stateement
            //jump over statement because it needs to be else if, and else needs to be jumped over if it is true

            myInstructs.Add(new CodeInstruction(OpCodes.Br));//need to add an operand of label to this one too
            int jf4 = myInstructs.Count - 1;
            int jt1 = jf4;
            //like
            //myInstructs[secondSaved].operand = wherever ist supposed to go
            //end of jump over statements
            //if statement for if statement section if(listingstandard4.buttontext("NotBlacklisted".Translate()))

            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, 24));
            int jt2 = myInstructs.Count - 1;
            myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "Not Blacklisted"));
            //myInstructs.Add(lineList[endInt + 29]);//this is call valuetype Verse.TaggedString Verse.Translator::Translate(string)
            //myInstructs.Add(lineList[endInt + 30]);//this is call string Verse.TaggedString::op_Implicit(valuetype Verse.TaggedString)
            // I dont know for some reason translate doesnt work not going to question it, might need to actually have the word blacklisted in it or something
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            myInstructs.Add(lineList[endInt + 33]);//this is callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse_S));//needs to send to after the next statement, this is [11]
            int jf5 = myInstructs.Count - 1;

            //end of if statement
            //start of statement
            //BlacklistAddon.changeInstance(bill, true);
            //SoundDefOf.Click.PlayOneShotOnCamera();

            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));
            myInstructs.Add(lineList[startInt + 3]);//this is ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 1));
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "changeInstance"));
            myInstructs.Add(new CodeInstruction(OpCodes.Pop));

            myInstructs.Add(lineList[endInt + 39]);//this is ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            myInstructs.Add(lineList[endInt + 40]);//this is ldnull, obviously didnt need to do this one but thought it would be more clear
            myInstructs.Add(lineList[endInt + 41]);//call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)

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
            int brtrueCount = 0;
            int brAdjust = startInt;
            while (brtrueCount < 2 && brAdjust > 0)
            {
                brAdjust--;
                if (lineList[brAdjust].opcode == OpCodes.Brtrue_S)
                {
                    ++brtrueCount;
                }
            }
            //assogn it
            lineList[brAdjust].operand = jtStart;

            ////////////////////////////////////////////////////////////////
            lineList.InsertRange(myStatementStartPos, myInstructs);

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

