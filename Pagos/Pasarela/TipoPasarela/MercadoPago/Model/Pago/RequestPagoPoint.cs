﻿
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class RequestPagoPoint
    {
        public string id { get; set; }

        public string deviceId { get; set; }

        public AdicionalInfo additional_info { get; set; }

        public int amount { get; set; }

        public SolicitudPago Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
