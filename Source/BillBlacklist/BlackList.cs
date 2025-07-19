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

            Harmony harmony = new Harmony("rimworld.mod.Nutmeg.BillBlackList");
            Harmony.DEBUG = false;//lol
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

        for (int i = 0; i < lineList.Count; i++)
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

        /*
         * Having Looked at the code again I have decided that I will write what I want the ilcode to look like, and the c# code it is creating here for future reference
         * 
         * C# ORIGINAL:
         * 
         *     Listing_Standard listing_Standard4 = listing_Standard.BeginSection(WorkerSelectionSubdialogHeight);
	     *     Widgets.Dropdown(buttonLabel: (bill.PawnRestriction != null) ? bill.PawnRestriction.LabelShortCap : ((ModsConfig.IdeologyActive && bill.SlavesOnly) ? ((string)"AnySlave".Translate()) : ((ModsConfig.BiotechActive && bill.recipe.mechanitorOnlyRecipe) ? ((string)"AnyMechanitor".Translate()) : ((ModsConfig.BiotechActive && bill.MechsOnly) ? ((string)"AnyMech".Translate()) : ((!ModsConfig.BiotechActive || !bill.NonMechsOnly) ? ((string)"AnyWorker".Translate()) : ((string)"AnyNonMech".Translate()))))), rect: listing_Standard4.GetRect(30f), target: bill, getPayload: (Bill_Production b) => b.PawnRestriction, menuGenerator: (Bill_Production b) => GeneratePawnRestrictionOptions());
	     *     if (bill.PawnRestriction == null && bill.recipe.workSkill != null && !bill.MechsOnly)
	     *      {
		 *          listing_Standard4.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label) + ":");
		 *          listing_Standard4.IntRange(ref bill.allowedSkillRange, 0, 20);
	     *      }
	     *      listing_Standard.EndSection(listing_Standard4);
	     * 
	     * C# Our Adjustments:
	     * 
	     *      Listing_Standard listing_Standard4 = listing_Standard.BeginSection(WorkerSelectionSubdialogHeight);
	     *      Widgets.Dropdown(buttonLabel: (bill.PawnRestriction != null) ? bill.PawnRestriction.LabelShortCap : ((ModsConfig.IdeologyActive && bill.SlavesOnly) ? ((string)"AnySlave".Translate()) : ((ModsConfig.BiotechActive && bill.recipe.mechanitorOnlyRecipe) ? ((string)"AnyMechanitor".Translate()) : ((ModsConfig.BiotechActive && bill.MechsOnly) ? ((string)"AnyMech".Translate()) : ((!ModsConfig.BiotechActive || !bill.NonMechsOnly) ? ((string)"AnyWorker".Translate()) : ((string)"AnyNonMech".Translate()))))), rect: listing_Standard4.GetRect(30f), target: bill, getPayload: (Bill_Production b) => b.PawnRestriction, menuGenerator: (Bill_Production b) => GeneratePawnRestrictionOptions());
	     *      if(bill.PawnRestriction != null)
	     *      {
	     *          if(blackListAddon.checkInstance(bill))
	     *          { 
	     *              if(listing_Standard4.ButtonText("BlackListed")
	     *              {
	     *                  blackListAddon.changeInstance(bill);
	     *                  SoundDefOf.Click.PlayOneShotOnCamera();
	     *              }
	     *          }
	     *          else if(listing_Standard4.ButtonText("NotBlackListed"))
	     *          {
	     *              blackListAddon.ChangeInstance(bill));
	     *              SoundDefOfClick.PlayOneShotOnCamera();
	     *          }
	     *      }
	     *      else
	     *      {
	     *          if (bill.recipe.workSkill != null && !bill.MechsOnly)
	     *          {
		 *              listing_Standard4.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label) + ":");
		 *              listing_Standard4.IntRange(ref bill.allowedSkillRange, 0, 20);
	     *          }
	     *      }
	     *      listing_Standard.EndSection(listing_Standard4);
	     *      
	     * IL Original: (Its a bit long so only including the relevant sections from the above C# code)
	     *      
	     *      IL_0a5d: call void Verse.Widgets::Dropdown<class RimWorld.Bill_Production, class Verse.Pawn>(valuetype [UnityEngine.CoreModule]UnityEngine.Rect, !!0, class [mscorlib]System.Func`2<!!0, !!1>, class [mscorlib]System.Func`2<!!0, class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype Verse.Widgets/DropdownMenuElement`1<!!1>>>, string, class [UnityEngine.CoreModule]UnityEngine.Texture2D, string, class [UnityEngine.CoreModule]UnityEngine.Texture2D, class [mscorlib]System.Action, bool)
	     *      if(bill.PawnRestriction == null)
	     *      IL_0a62: ldarg.0
	     *      IL_0a63: ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      IL_0a68: callvirt instance class Verse.Pawn RimWorld.Bill::get_PawnRestriction()
	     *      IL_0a6d: brtrue.s IL_0ae0
	     *      if(bill.recipe.workSkill != null)
         *      IL_0a6f: ldarg.0
	     *      IL_0a70: ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      IL_0a75: ldfld class Verse.RecipeDef RimWorld.Bill::recipe
	     *      IL_0a7a: ldfld class RimWorld.SkillDef Verse.RecipeDef::workSkill
	     *      IL_0a7f: brfalse.s IL_0ae0
	     *      if(!bill.MechsOnly)
	     *      IL_0a81: ldarg.0
	     *      IL_0a82: ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      IL_0a87: callvirt instance bool RimWorld.Bill::get_MechsOnly()
	     *      IL_0a8c: brtrue.s IL_0ae0
	     *      begins listing_Standard4.Label("AllowedSkillRange".translate(bill.recipe.workSkill.label) + ":");
	     *      IL_0a8e: ldloc.s 23
	     *      IL_0a90: ldstr "AllowedSkillRange"
	     *      IL_0a95: ldarg.0
	     *      ...
	     *      ...
	     *      ...
	     *      listing_Standard.EndSection(listing_Standard4);
	     *      IL_0ae0: ldloc.s 5
	     *      IL_0ae2: ldloc.s 23
	     *      IL_0ae4: callvirt instance void Verse.Listing_Standard::EndSection(class Verse.Listing_Standard)
	     *      listing_Standard.End();
	     *      IL_0ae9: ldloc.s 5
	     *      IL_0aeb: callvirt instance void Verse.Listing::End()
	     *      
	     * IL Our Adjustments:
	     *      
	     *      IL_0a5d: call void Verse.Widgets::Dropdown<class RimWorld.Bill_Production, class Verse.Pawn>(valuetype [UnityEngine.CoreModule]UnityEngine.Rect, !!0, class [mscorlib]System.Func`2<!!0, !!1>, class [mscorlib]System.Func`2<!!0, class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype Verse.Widgets/DropdownMenuElement`1<!!1>>>, string, class [UnityEngine.CoreModule]UnityEngine.Texture2D, string, class [UnityEngine.CoreModule]UnityEngine.Texture2D, class [mscorlib]System.Action, bool)
	     *      if(bill.PawnRestriction != null)
	     *      IL_0a62: ldarg.0
	     *      IL_0a63: ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      IL_0a68: callvirt instance class Verse.Pawn RimWorld.Bill::get_PawnRestriction()
	     *      IL_0a6d: brfalse.s IL_0a6f
	     *      
	     *      (no IL_Labels because we don't create them)
	     *      if(blackListAddon.checkInstance(bill))
	     *      ldarg.0
	     *      ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      call static System.Int32 BillBlackListAddon::checkInstance(class RimWorld.Bill)
	     *      brfalse_s L1
	     *      
	     *      if(listing_Standard4.ButtonText("BlackListed")
	     *      ldloc.s listing_Standard4 Address
	     *      ldstr "BlackListed"
	     *      ldNull null
	     *      ldc.r4 1
	     *      callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
	     *      brfalse.s IL_0ae0
	     *      
	     *      blackListAddon.changeInstance(bill, 0);
	     *      ldarg.0
	     *      ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      ldc.i4 0
	     *      call static System.Int32 BillBlackListAddon::changeInstance(class RimWorld.bill, System.Int32 change)
	     *      
	     *      SoundDefOf.Click.PlayOneShotOnCamera();
	     *      ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
	     *      ldnull
	     *      call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)
	     *      br IL_0ae0
	     *      
	     *      else if(listing_Standard4.ButtonText("NotBlackListed"))
	     * L1   ldloc.s listing_Standard4 Address
	     *      ldstr "NotBlackListed"
	     *      ldNull null
	     *      ldc.r4 1
	     *      callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
	     *      brfalse.s IL_0ae0
	     *      
	     *      blackListAddon.changeInstance(bill, 1);
	     *      ldarg.0
	     *      ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      ldc.i4 1
	     *      call static System.Int32 BillBlackListAddon::changeInstance(class RimWorld.bill, System.Int32 change)
	     *      
	     *      SoundDefOf.Click.PlayOneShotOnCamera();
	     *      ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
	     *      ldnull
	     *      call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)
	     *      
	     *      else
	     *      br.s IL_0ae0
	     *      
	     *      if(bill.recipe.workSkill != null)
         *      IL_0a6f: ldarg.0
	     *      IL_0a70: ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      IL_0a75: ldfld class Verse.RecipeDef RimWorld.Bill::recipe
	     *      IL_0a7a: ldfld class RimWorld.SkillDef Verse.RecipeDef::workSkill
	     *      IL_0a7f: brfalse.s IL_0ae0
	     *      if(!bill.MechsOnly)
	     *      IL_0a81: ldarg.0
	     *      IL_0a82: ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
	     *      IL_0a87: callvirt instance bool RimWorld.Bill::get_MechsOnly()
	     *      IL_0a8c: brtrue.s IL_0ae0
	     *      begins listing_Standard4.Label("AllowedSkillRange".translate(bill.recipe.workSkill.label) + ":");
	     *      IL_0a8e: ldloc.s 23
	     *      IL_0a90: ldstr "AllowedSkillRange"
	     *      IL_0a95: ldarg.0
	     *      ...
	     *      ...
	     *      ...
	     *      listing_Standard.EndSection(listing_Standard4);
	     *      IL_0ae0: ldloc.s 5
	     *      IL_0ae2: ldloc.s 23
	     *      IL_0ae4: callvirt instance void Verse.Listing_Standard::EndSection(class Verse.Listing_Standard)
	     *      listing_Standard.End();
	     *      IL_0ae9: ldloc.s 5
	     *      IL_0aeb: callvirt instance void Verse.Listing::End()
        */

        if (startInt != -1 && endInt != -1)
        {
            //this section copies il operations that I don't know how to write
            int copyPoint = 2;

            //copies the location of the address that listing_standard4 is saved
            while (!(lineList[copyPoint].ToString().Contains("stloc") && lineList[copyPoint - 1].ToString().Contains("Listing_Standard::BeginSection") && lineList[copyPoint + 3].ToString().Contains("get_PawnRestriction")))
            {
                copyPoint++;
            }
            int list_Standard4Address = copyPoint;


            //copies the location jump_label from if (bill.PawnRestriction == null && bill.recipe.workSkill != null && !bill.MechsOnly)
            //which currently is IL_0ae0
            while (!(lineList[copyPoint].ToString().Contains("brtrue") && lineList[copyPoint + 1].ToString().Contains("ldloc") && lineList[copyPoint + 2].ToString().Contains("AllowedSkillRange")))
            {
                copyPoint++;
            }
            int jumpPawnRestrictionIsNull = copyPoint;

            //copies the load field operation ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            while (!(lineList[copyPoint].ToString().Contains("Dialog_BillConfig::bill")))
            {
                copyPoint++;
            }
            CodeInstruction ldfld_dialog_billconfig_bill = new CodeInstruction(OpCodes.Add, null);
            ldfld_dialog_billconfig_bill.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
            ldfld_dialog_billconfig_bill.opcode = lineList[copyPoint].opcode;

            //copies the call operation callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            while (!(lineList[copyPoint].ToString().Contains("ButtonText")))
            {
                copyPoint++;
            }
            CodeInstruction call_buttontext = new CodeInstruction(OpCodes.Add, null);
            call_buttontext.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
            call_buttontext.opcode = lineList[copyPoint].opcode;

            //copies the load static field operation ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            while (!(lineList[copyPoint].ToString().Contains("SoundDefOf")))
            {
                copyPoint++;
            }
            CodeInstruction ldsfld_sounddefof = new CodeInstruction(OpCodes.Add, null);
            ldsfld_sounddefof.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
            ldsfld_sounddefof.opcode = lineList[copyPoint].opcode;

            //copies the call operation of call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)
            while (!(lineList[copyPoint].ToString().Contains("PlayOneShotOnCamera")))
            {
                copyPoint++;
            }
            CodeInstruction call_playoneshotoncamera = new CodeInstruction(OpCodes.Add, null);
            call_playoneshotoncamera.operand = lineList[copyPoint].operand != null ? lineList[copyPoint].operand : null;
            call_playoneshotoncamera.opcode = lineList[copyPoint].opcode;

            //finds the insert location
            int adjustPoint = 7;
            while (!(lineList[adjustPoint].ToString().Contains("brtrue") && lineList[adjustPoint - 1].ToString().Contains("get_PawnRestriction") && lineList[adjustPoint - 2].ToString().Contains("Dialog_BillConfig::bill") && lineList[adjustPoint + 1].ToString().Contains("ldarg")))
            {
                adjustPoint++;
            }


            //our lines that we insert into the provided lines
            List<CodeInstruction> myInstructs = new List<CodeInstruction>();

            //changes the if (bill.PawnRestriction == null) into if (bill.PawnRestriction != null)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse));//brfalse_s IL_0ae0
            int jumpNull = myInstructs.Count - 1;//saves the location, yes i know its zero to insert the jump label operand later

            //if(blackListAddon.checkInstance(bill))
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));//ldarg.0
            myInstructs.Add(ldfld_dialog_billconfig_bill);//ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "checkInstance"));//call static System.Int32 BillBlackListAddon::checkInstance(class RimWorld.Bill)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse));//brfalse_s L1
            int jumpInstance = myInstructs.Count - 1;

            //if(listing_Standard4.ButtonText("BlackListed")
            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, lineList[list_Standard4Address].operand));//ldloc.s listing_Standard4 Address
            myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "BlackListed"));//ldstr "BlackListed"
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));//ldNull null
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));//ldc.r4 1
            myInstructs.Add(call_buttontext);//callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse));//brfalse.s IL_0ae0
            List<int> jumpNotNull = new List<int>();
            jumpNotNull.Add(myInstructs.Count - 1);

            //blackListAddon.changeInstance(bill, 0);
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));//ldarg.0
            myInstructs.Add(ldfld_dialog_billconfig_bill);//ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 0));//ldc.i4 0
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "changeInstance"));//call static System.Int32 BillBlackListAddon::changeInstance(class RimWorld.bill, System.Int32 change)

            //      SoundDefOf.Click.PlayOneShotOnCamera();
            myInstructs.Add(ldsfld_sounddefof);//ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull, null));//ldnull
            myInstructs.Add(call_playoneshotoncamera);//call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)
            myInstructs.Add(new CodeInstruction(OpCodes.Br));//br IL_0ae0
            jumpNotNull.Add(myInstructs.Count - 1);

            //else if(listing_Standard4.ButtonText("NotBlackListed"))
            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_S, lineList[list_Standard4Address].operand));//ldloc.s listing_Standard4 Address
            int labelElse = myInstructs.Count - 1;//location to jump to from if(blackListAddon.checkInstance(bill))
            myInstructs.Add(new CodeInstruction(OpCodes.Ldstr, "Not Blacklisted"));//ldstr "NotBlackListed"
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull));//ldNull null
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));//ldc.r4 1
            myInstructs.Add(call_buttontext);//callvirt instance bool Verse.Listing_Standard::ButtonText(string, string, float32)
            myInstructs.Add(new CodeInstruction(OpCodes.Brfalse));//brfalse.s IL_0ae0
            jumpNotNull.Add(myInstructs.Count - 1);

            //blackListAddon.changeInstance(bill, 1);
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0));//ldarg.0
            myInstructs.Add(ldfld_dialog_billconfig_bill);//ldfld class RimWorld.Bill_Production RimWorld.Dialog_BillConfig::bill
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 1));//ldc.i4 1
            myInstructs.Add(CodeInstruction.Call(typeof(BillBlacklist.BlacklistAddon), "changeInstance"));//call static System.Int32 BillBlackListAddon::changeInstance(class RimWorld.bill, System.Int32 change)

            //      SoundDefOf.Click.PlayOneShotOnCamera();
            myInstructs.Add(ldsfld_sounddefof);//ldsfld class Verse.SoundDef RimWorld.SoundDefOf::Click
            myInstructs.Add(new CodeInstruction(OpCodes.Ldnull, null));//ldnull
            myInstructs.Add(call_playoneshotoncamera);//call void Verse.Sound.SoundStarter::PlayOneShotOnCamera(class Verse.SoundDef, class Verse.Map)     

            //"else"
            myInstructs.Add(new CodeInstruction(OpCodes.Br));//br.s IL_0ae0
            jumpNotNull.Add(myInstructs.Count - 1);

            //label assignments

            //for jumping past the bill.PawnRestrictions == null block
            //Label jumpNullLabel = il.DefineLabel();//remove
            for (int i = 0; i < jumpNotNull.Count; i++)
            {
                myInstructs[jumpNotNull[i]].operand = lineList[jumpPawnRestrictionIsNull].operand;
                //myInstructs[jumpNotNull[i]].operand = jumpNullLabel;
            }

            //for jumping to if (listing_Standard4.ButtonText("NotBlackListed"))
            Label jumpInstanceLabel = il.DefineLabel();
            myInstructs[labelElse].labels.Add(jumpInstanceLabel);
            myInstructs[jumpInstance].operand = jumpInstanceLabel;


            //for jumping into the bill.PawnRestrictions == null block
            Label jumpNullLabel = il.DefineLabel();
            while (!(lineList[copyPoint].ToString().Contains("ldarg") && lineList[copyPoint + 1].ToString().Contains("Dialog_BillConfig::bill") && lineList[copyPoint + 2].ToString().Contains("Bill::recipe") && lineList[copyPoint + 3].ToString().Contains("RecipeDef::workSkill") && lineList[copyPoint + 4].ToString().Contains("brfalse")))
            {
                copyPoint--;
            }
            lineList[copyPoint].labels.Add(jumpNullLabel);
            myInstructs[jumpNull].operand = jumpNullLabel;

            lineList.RemoveAt(adjustPoint);//remove brtrue.s IL_0ae0 to be replaced by brfalse.s IL_0a6f


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

