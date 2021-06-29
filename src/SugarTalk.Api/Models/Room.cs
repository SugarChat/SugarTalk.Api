using System;

namespace SugarTalk.Api.Models
{
    public class Room
    {
        public string RoomID { set; get; }
        public DateTime CreateDate { set; get; }
        public int CreateUserID { set; get; }
        public string CreateUserName { set; get; }
        public string Path { set; get; }
        public string CarNo { set; get; }
    }
}
