using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNKUBosSinifBulucu
{
    // Data Transfer Object
    public class Day_DTO
    {
       public int Day { get; set; }
       public List<Time_DTO> time {get; set;}
    }
    public class Time_DTO
    {
        public string time { get; set; }
        public List<Lesson_DTO> lesson { get; set; }

    }
    public class Lesson_DTO
    {
        public string code { get; set; }
        public string name { get; set; }
        public string faculty { get; set; }
        public string teacher { get; set; }
        public string @class { get;set;}
    }
    public class fakulte_DTO
    {
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string foldername { get; set; }
        public override string ToString()
        {
            return name.ToString();
        }

    }
    public class bolum_DTO
    {
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string filename { get; set; }
        public string fakulte { get; set; }
        public string link { get; set; }
        public override string ToString()
        {
            return name.ToString();
        }

    }

}
