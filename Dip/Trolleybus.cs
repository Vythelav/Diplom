using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dip
{
    public class Trolleybus
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int StatusId { get; set; }
        public DateTime Data { get; set; }
        public string StatusName { get; set; } // Новое свойство для имени статуса
    }



}
