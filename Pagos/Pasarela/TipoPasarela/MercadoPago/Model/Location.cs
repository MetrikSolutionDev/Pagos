using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class Location
    {
        public string street_number { get; set; }

        public string street_name { get; set; }

        public string city_name { get; set; }

        public string state_name { get; set; }

        public double latitude { get; set; }

        public double longitude { get; set; }
        
        public string reference { get; set; }
    }
}
