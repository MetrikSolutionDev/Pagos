
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class SolicitudReversion
    {
        public string Cuit_cuil { get; set; }

        public string Id_entorno { get; set; }

        public string Pago_id { get; set; }

        public decimal Importe { get; set; }

        /// <summary>
        /// Texto que se va a ver reflejado en la terminal
        /// </summary>
        public string Texto_terminal { get; set; }

        /// <summary>
        /// En nuestro caso MS3
        /// </summary>
        public string Nombre_integrador { get; set; }

        /// <summary>
        /// En nuestro caso MS3 POS
        /// </summary>
        public string Nombre_sistema_integrador { get; set; }

        /// <summary>
        /// En nuestro caso 1.0.0
        /// </summary>
        public string Version_sistema_integrador { get; set; }

        /// <summary>
        /// Para retirar efectivo de pagos con debito
        /// </summary>
        public decimal Importe_cambio { get; set; }

        /// <summary>
        /// Campo opcional, puede hacer referencia a un identificador externo, como numero de factura, pedido, etc
        /// </summary>
        public string Referencia { get; set; }

        /// <summary>
        /// Cantidad de cuotas. Si no se manda, en el objeto enviar null
        /// </summary>
        public int Cuotas { get; set; }

        /// <summary>
        /// Es el tipo de cuenta del banco (CAJA DE AHORRO, CUENTA CORRIENTE, ETC)
        /// </summary>
        public string Tipo_cuenta { get; set; }

        public string Identificacion_plan_pago { get; set; }

        /// <summary>
        /// Fiscal o No Fiscal
        /// Para argentina se utiliza siempre No Fiscal
        /// </summary>
        public CommonPago.MetodoImpresion Metodo_impresion { get; set; }

        /// <summary>
        /// NINGUNO, SOLO_COMERCIANTE, SOLO_CLIENTE, AMBOS
        /// </summary>
        public CommonPago.CopiasComprobantePago Copias_comprobante_pago { get; set; }

        /// <summary>
        /// Si no se manda, se envia en null
        /// </summary>
        public string Marca_tarjeta { get; set; }

        public List<string> Lista_terminales { get; set; }

        /// <summary>
        /// TARJETA, QR
        /// </summary>
        public CommonPago.MetodoOperacion Metodo_operacion { get; set; }

        public bool Admite_tarjeta_beneficio { get; set; }

        /// <summary>
        /// Nota impresa en el ticket
        /// </summary>
        public string Nota_impresion_ticket { get; set; }

        /// <summary>
        /// Campo opcional para insertar el DNI del cliente
        /// </summary>
        public string Dni_cliente { get; set; }

        public int Nro_intento_generacion_token { get; set; }
    }
}
