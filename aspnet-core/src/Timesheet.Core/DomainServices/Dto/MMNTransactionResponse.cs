using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.MMN.Dto
{
    public class MMNTransactionResponse
    {
        [JsonProperty("data")]
        public MMNTransactionData Data { get; set; }
    }

    public class MMNTransactionData
    {
        [JsonProperty("transaction")]
        public MMNTransactionDetails Transaction { get; set; }
    }

    public class MMNTransactionDetails
    {
        [JsonProperty("chain_id")]
        public string ChainId { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("block_number")]
        public long BlockNumber { get; set; }

        [JsonProperty("from_address")]
        public string FromAddress { get; set; }

        [JsonProperty("to_address")]
        public string ToAddress { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("transaction_timestamp")]
        public long TransactionTimestamp { get; set; }

        [JsonProperty("text_data")]
        public string TextData { get; set; }

        [JsonProperty("extra_info")]
        public string ExtraInfo { get; set; }
    }

    public class MMNTransactionInfo
    {
        public string Hash { get; set; }
        public string Value { get; set; }
        public long TransactionTimestamp { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
    }
}
