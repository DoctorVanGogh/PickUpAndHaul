﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;


namespace PickUpAndHaul
{
    public class PawnUnloadChecker
    {

        public static void CheckIfPawnShouldUnloadInventory(Pawn pawn, bool forced = false)
        {
            Job job = new Job(PickUpAndHaulJobDefOf.UnloadYourHauledInventory);
            CompHauledToInventory takenToInventory = pawn.TryGetComp<CompHauledToInventory>();

            if (takenToInventory == null) return;

            HashSet<Thing> carriedThing = takenToInventory.GetHashSet();

            if (pawn.Faction != Faction.OfPlayer || !pawn.RaceProps.Humanlike) return;
            if (carriedThing?.Count == 0 || pawn.inventory.innerContainer.Count == 0) return;

            if (carriedThing.Count != 0)
            {
                Thing thing = null;
                try
                {
                    if (carriedThing.Contains(thing))
                    {
                        carriedThing.Remove(thing);
                    }
                }
                catch (Exception arg)
                {
                    Log.Warning("There was an exception thrown by Pick Up And Haul. Pawn will clear inventory. \nException: " + arg);
                    carriedThing.Clear();
                    pawn.inventory.UnloadEverything = true;
                }
            }

            if (forced)
            {
                if (job.TryMakePreToilReservations(pawn))
                {
                    pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
                    return;
                }
            }

            if (MassUtility.EncumbrancePercent(pawn) >= 0.90f || carriedThing.Count >= 1)
            {
                if (job.TryMakePreToilReservations(pawn))
                {
                    pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
                    return;
                }
            }

            if (pawn.inventory.innerContainer?.Count >= 1)
            {
                foreach (Thing rottable in pawn.inventory.innerContainer)
                {
                    CompRottable compRottable = rottable.TryGetComp<CompRottable>();
                    if (compRottable != null)
                    {
                        if (compRottable.TicksUntilRotAtCurrentTemp < 30000)
                        {
                            pawn.jobs.jobQueue.EnqueueFirst(job, new JobTag?(JobTag.Misc));
                            return;
                        }
                    }
                }
            }
            
            if (Find.TickManager.TicksGame % 50 == 0 && pawn.inventory.innerContainer.Count < carriedThing.Count)
            {
                Log.Warning("[PickUpAndHaul] " + pawn + " inventory was found out of sync with haul index. Pawn will drop their inventory.");
                carriedThing.Clear();
                pawn.inventory.UnloadEverything = true;                
            }
        }
    }

    [DefOf]
    public static class PickUpAndHaulJobDefOf
    {
        public static JobDef UnloadYourHauledInventory;
        public static JobDef HaulToInventory;
    }
}