using ColossalFramework;
using UnityEngine;
using Boformer.Redirection;

namespace WG_BalancedPopMod
{
    [TargetType(typeof(CommonBuildingAI))]
    class AI_CommonBuilding : CommonBuildingAI
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildingID"></param>
        /// <param name="data"></param>
        /// <param name="crimeAccumulation"></param>
        /// <param name="citizenCount"></param>
        [RedirectMethod(true)]
        protected void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            int number = 10;
            if (crimeAccumulation != 0)
            {
                ItemClass ic = data.Info.m_class;
                int percentage = (int) ((ic.m_level >= 0) ? ic.m_level : 0);

                // TODO -  Should be replaced by height of building??
                // Values to be determined and will not be added to XML yet
                if (ic.m_service == ItemClass.Service.Residential)
                {
                    number = 16;

                    if (ic.m_subService == ItemClass.SubService.ResidentialHigh)
                    {
                        crimeAccumulation /= 2;
                    }
                }
                else if (ic.m_service == ItemClass.Service.Office)
                {
                    crimeAccumulation /= 5; // Not enough?
                }
                else if (ic.m_subService == ItemClass.SubService.CommercialHigh)
                {
                    crimeAccumulation /= 3;
                }

                // Percentage reduction
                crimeAccumulation = (crimeAccumulation * (number - percentage)) / number;

                // ----- End of changes -----

                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    // crime multiplies by 1.25 at night
                    crimeAccumulation = crimeAccumulation * 5 >> 2;
                }

                if (data.m_eventIndex != 0)
                {
                    EventManager instance = Singleton<EventManager>.instance;
                    EventInfo info = instance.m_events.m_buffer[(int)data.m_eventIndex].Info;
                    crimeAccumulation = info.m_eventAI.GetCrimeAccumulation(data.m_eventIndex, ref instance.m_events.m_buffer[(int)data.m_eventIndex], crimeAccumulation);
                }
                crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
                if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    crimeAccumulation = 0;
                }
            }
            data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, (int)data.m_crimeBuffer + crimeAccumulation);
//Debugging.writeDebugToFile(data.Info.gameObject.name + " -> number:" + citizenCount + ", crime_buffer: " + data.m_crimeBuffer + " + " + crimeAccumulation);
            int crimeBuffer = (int)data.m_crimeBuffer;
            if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
            {
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                this.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref num, ref num2, ref num3, ref num4);
                if (num == 0)
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = crimeBuffer / Mathf.Max(1, citizenCount * 10);
                    offer.Building = buildingID;
                    offer.Position = data.m_position;
                    offer.Amount = 1;
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                }
            }
            Notification.Problem problem = Notification.RemoveProblems(data.m_problems, Notification.Problem.Crime);
            if ((int)data.m_crimeBuffer > citizenCount * 90)
            {
                problem = Notification.AddProblems(problem, Notification.Problem.Crime | Notification.Problem.MajorProblem);
            }
            else if ((int)data.m_crimeBuffer > citizenCount * 60)
            {
                problem = Notification.AddProblems(problem, Notification.Problem.Crime);
            }
            data.m_problems = problem;
        }

    }
}
