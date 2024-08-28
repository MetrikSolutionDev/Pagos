using Core.Common.Mail;
using Pagos.Pasarela;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Pagos.Configuracion;

namespace Pagos
{
    public class Pago : IPago
    {
        private IPago _pago;

        public event RespuestaExternaHandler OnRespuesta;

        public void OnRespuestaEvent(object sender, RespuestaExternaEventArgs e)
        {
            RespuestaExternaHandler handler = OnRespuesta;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public Pago()
        {
        }

        public Pago(Configuracion xConfiguracion) 
        {
            _pago = PagoFactory.Instance(xConfiguracion);
            _pago.OnRespuesta += OnRespuestaEvent;
        }

        public static List<EquivalenciaTarjeta> ReadEquivalencias() 
        {
            List<EquivalenciaTarjeta> sList = new List<EquivalenciaTarjeta>();

            sList.Add(new EquivalenciaTarjeta() { Codigo = "VI", Descripcion = "VISA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "EL", Descripcion = "VISA DEBITO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MC", Descripcion = "MASTERCARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "PD", Descripcion = "MASTERCARD DEBIT" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MA", Descripcion = "MAESTRO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "AM", Descripcion = "AMERICAN EXPRESS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "BC", Descripcion = "BANCAT" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "IA", Descripcion = "CREDITO ARGENTINO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "IT", Descripcion = "ITALCRED" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "PO", Descripcion = "PLATINO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CO", Descripcion = "CONFIABLE" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "NA", Descripcion = "TARJETA NARANJA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "PY", Descripcion = "PYME NACION" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MI", Descripcion = "MILENIA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "UR", Descripcion = "UNIRED" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "NA", Descripcion = "NATIVA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "PN", Descripcion = "NATIVA-MC" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "BC", Descripcion = "BANCOR" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CR", Descripcion = "CENTROCARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CH", Descripcion = "CREDICASH" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CE", Descripcion = "CREDIFE" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "WO", Descripcion = "CREDIQUEN" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CX", Descripcion = "CONSUMAX" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MU", Descripcion = "MUTUAL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "FL", Descripcion = "FERTIL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "ST", Descripcion = "SOY TIGRE" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "ID", Descripcion = "IRED" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "DC", Descripcion = "CLARIN" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CN", Descripcion = "CLUB LA NACION" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CS", Descripcion = "CUPONSTAR" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "GS", Descripcion = "GIFT CARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "WG", Descripcion = "WISH GIFT" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CA", Descripcion = "CABAL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "C2", Descripcion = "CABAL DEBITO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CT", Descripcion = "CTC GROUP" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "SS", Descripcion = "SOY SOLAR" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "SR", Descripcion = "SOY RECOLETA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "FY", Descripcion = "FINANYA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "BB", Descripcion = "BBPS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MR", Descripcion = "MONTEMAR" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "OG", Descripcion = "OH!GIFT CARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "AD", Descripcion = "ADANCARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CP", Descripcion = "COOPEPLUS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "SO", Descripcion = "TARJETA SOL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "DL", Descripcion = "CREDENCIAL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "PT", Descripcion = "PATAGONIA365" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "TY", Descripcion = "TUYA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CS", Descripcion = "CF SA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "SV", Descripcion = "Carrefour" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "X2", Descripcion = "LA VOZ" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "ND", Descripcion = "LOS ANDES" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "CG", Descripcion = "CREDIGUIA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "ET", Descripcion = "CLUB EL TRIBUNO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "RO", Descripcion = "RIO NEGRO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "JS", Descripcion = "JERARQUICOS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "LG", Descripcion = "CLUB LA GACETA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "BU", Descripcion = "BQB CLUB" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "LS", Descripcion = "T EL DIARIO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "TX", Descripcion = "TARJETA APLA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "IS", Descripcion = "MUTUAL TAIS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "RT", Descripcion = "RTASGLOBAL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "YN", Descripcion = "SOY NORTE" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "SB", Descripcion = "SUTEBA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "HT", Descripcion = "HSBCbenefit" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "AY", Descripcion = "APSA S.A." });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "RS", Descripcion = "C.REGALO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "JC", Descripcion = "T.CIUDADANA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MG", Descripcion = "CLUB BENEFICIOS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "MW", Descripcion = "MOVISTAR" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "BF", Descripcion = "BENEFICIOS" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "FA", Descripcion = "FAVACARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "VY", Descripcion = "VYCARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "UA", Descripcion = "UNICA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "PS", Descripcion = "VIP" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "3M", Descripcion = "3M" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "SU", Descripcion = "SU CREDITO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "JK", Descripcion = "CREDIARIO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "QU", Descripcion = "QUIERO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "BV", Descripcion = "BBVA CUPON" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "QZ", Descripcion = "CODIGO BANCARIO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "GR", Descripcion = "Gestor RPA" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "DO", Descripcion = "DONCREDITO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "LL", Descripcion = "TARJETA LOCAL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "FD", Descripcion = "FULLCARD" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "IM", Descripcion = "CRED.MOVIL" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "EX", Descripcion = "EFECTIVA MAX" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "FR", Descripcion = "FARO" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "HU", Descripcion = "HUILEN" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "GP", Descripcion = "GRUPAR" });
            sList.Add(new EquivalenciaTarjeta() { Codigo = "LZ", Descripcion = "LA ZONAL" });

            return sList;
        }

        public void EnviarCancelacionPago(Cancelacion xModel)
        {
            _pago.EnviarCancelacionPago(xModel);
        }

        public void EnviarCancelacionPago()
        {
            _pago.EnviarCancelacionPago();
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            _pago.EnviarSolicitudPago(xModel);
        }

        public void EnviarSolicitudReversion(SolicitudReversion xModel)
        {
            _pago.EnviarSolicitudReversion(xModel);
        }

        public void ReiniciarConsultaEstadoPago()
        {
            _pago.ReiniciarConsultaEstadoPago();
        }

        public void ReiniciarConsultaEstadoReversion()
        {
            _pago.ReiniciarConsultaEstadoReversion();
        }

        public void EnviarCancelacionReversion(Cancelacion xModel)
        {
            _pago.EnviarCancelacionReversion(xModel);
        }

        public void EnviarCancelacionReversion()
        {
            _pago.EnviarCancelacionReversion();
        }

        public void EnviarSolicitudDevolucion(SolicitudDevolucion xModel)
        {
            _pago.EnviarSolicitudDevolucion(xModel);
        }

        public void ReiniciarConsultaEstadoDevolucion()
        {
            _pago.ReiniciarConsultaEstadoDevolucion();
        }

        public void EnviarCancelacionDevolucion(Cancelacion xModel)
        {
            _pago.EnviarCancelacionDevolucion(xModel);
        }

        public void EnviarCancelacionDevolucion()
        {
            _pago.EnviarCancelacionDevolucion();
        }

        public void EnviarSolicitudCierre(SolicitudCierre xModel)
        {
            _pago.EnviarSolicitudCierre(xModel);
        }
    }
}
