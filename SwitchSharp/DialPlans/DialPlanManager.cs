﻿using SipSharp.Transactions;

namespace SwitchSharp.DialPlans
{
    public class DialPlanManager
    {
        /// <summary>
        /// Will include default action.
        /// </summary>
        /// <param name="dialPlan">Dial plan to invoke.</param>
        public void PreProcess(DialPlan dialPlan)
        {
        }

        /// <summary>
        /// Process a dial plan.
        /// </summary>
        /// <param name="dialPlan">Dial plan.</param>
        /// <param name="transaction">Transaction to user waiting on dial plan.</param>
        public void Process(DialPlan dialPlan, IServerTransaction transaction)
        {
        }
    }
}