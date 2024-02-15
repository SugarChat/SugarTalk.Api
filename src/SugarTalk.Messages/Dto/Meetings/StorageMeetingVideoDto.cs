using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Messages.Dto.Meetings
{
    public class StorageMeetingVideoDto
    {
        [JsonProperty("meetingTitle")]
        public string MeetingTitle { get; set; }

        [JsonProperty("startDate")]
        public long StartDate { get; set; }

        [JsonProperty("endDate")]
        public long EndDate { get; set; }

        [JsonProperty("meetingNumber")]
        public string MeetingNumber { get; set; }

        /// <summary>
        /// 记录编号
        /// </summary>
        [JsonProperty("recordNumber")]
        public string RecordNumber { get; set; }

        [JsonProperty("viderUrl")]
        public string VideoUrl { get; set; }
    }
}
