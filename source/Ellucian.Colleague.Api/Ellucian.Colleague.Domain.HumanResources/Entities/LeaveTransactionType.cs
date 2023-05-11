/* Copyright 2018-2022 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public enum LeaveTransactionType
    {

        Earned, // A
        Used, // U
        Adjusted, // J
        LeaveReporting, // L
        StartingBalanceAdjustment, // S
        StartingBalance, // B
        MidYearBalanceAdjustment, // C
        Rollover // R
    }
}
