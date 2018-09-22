// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IEthosExtended
    {
        /// <summary>
        /// Dictionary of string, string that contains the Ethos Extended Data to send into the CTX
        /// key is column name
        /// value is value to save in, if empty string then this means it is meant to remove the data from colleague
        /// </summary>
        Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        /// <summary>
        /// Takes the EthosExtendedDataList dictionary and splits it into two List string to be passed to Colleague CTX 
        /// </summary>
        /// <returns>T1 is the list of keys, T2 is a list values that match up. Returns null if the list is empty</returns>
        Tuple<List<string>, List<string>> GetEthosExtendedDataLists();
    }
}
