using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ProjetoDM106.br.com.correios.ws;
using ProjetoDM106.CRMClient;
using ProjetoDM106.Models;

namespace ProjetoDM106.Controllers
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private ProjetoDM106Context db = new ProjetoDM106Context();
        string meuCep;

        // GET: api/Order/getfrete?id=1
        [Authorize]
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("getfrete")]
        public IHttpActionResult getFrete(int id)
        {
            Order pedido = db.Orders.Find(id);

            if (pedido == null)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }

            if (pedido.Status != "novo")
            {
                return Content(HttpStatusCode.NotFound, " Pedido com status diferente de 'novo'");
            }

            if ((User.Identity.Name == pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                var cep = this.ObtemCEP(pedido.userEmail).ToString();
                if (cep == null)
                {
                    return Content(HttpStatusCode.NotFound, "Impossibilidade de acessar o serviço de CRM");
                }
                var list = pedido.OrderItems.ToList();
                decimal altura = 0, comprimento = 0, diametro = 0, largura = 0, peso = 0, preco = 0;
                int testlist = 0;
                string pesoTotal;
                foreach (var item in list)
                {
                    testlist += 1;
                    if (altura < item.Product.altura)
                        altura = item.Product.altura;
                    if (comprimento < item.Product.comprimento)
                        comprimento += item.Product.comprimento;
                    if (diametro < item.Product.diametro)
                        diametro += item.Product.diametro;
                    largura += item.Product.largura;
                    peso += item.Product.peso;
                    preco += item.Product.preco;
                }

                if (testlist < 1)
                {
                    return Content(HttpStatusCode.NotFound, "Pedido sem itens");
                }

                pesoTotal = peso.ToString();
                return (this.CalculaFrete(meuCep, pesoTotal, comprimento, altura, largura, diametro, preco, id));
                 
            }
            else
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

        }

        // GET: api/Order/cep?email=email
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("cep")]
        public IHttpActionResult ObtemCEP(string email)
        {
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(email);

            if (customer != null)
            {
                meuCep = customer.zip;
                return Ok(customer.zip);
            }
            else
            {
                return BadRequest("Falha ao consultar o CRM");
            }
        }

        // GET: api/Order/frete?id=1&cep=37540000&peso=20&comprimento=20&altura=20&largura=20&diametro=20&preco=20
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(string cep, string peso, decimal comprimento, decimal altura, decimal largura, decimal diametro, decimal preco, int id)
        {
            string frete;

            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
            cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "37540000", cep, peso, 1, comprimento, altura, largura, diametro, "N", preco, "S");
            if (resultado.Servicos[0].Erro.Equals("0"))
            {
                frete = "Valor do frete: " + resultado.Servicos[0].Valor + " - Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega + " dia(s)";
                Order pedido = db.Orders.Find(id);
                pedido.precoFrete = resultado.Servicos[0].Valor;
                pedido.DateDelivery = resultado.Servicos[0].PrazoEntrega + " Dias apartir de " + TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToLocalTime().ToString();
                db.Entry(pedido).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok(frete);
            }
            else
            {
                return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + resultado.Servicos[0].MsgErro);
            }
        }

        // GET: api/Order/byemail?email=email
        [Authorize]
        [ResponseType(typeof(Product))]
        [HttpGet]
        [Route("byemail")]
        public IHttpActionResult GetProductByEmail(string email)
        {
            if ((User.Identity.Name == email) || User.IsInRole("ADMIN"))
            {
                var order = db.Orders.Where(o => o.userEmail == email).ToList();
                return Ok(order);
            }
            else
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
        }

        // POST: api/Orders/recover?id=5
        [Authorize]
        [ResponseType(typeof(Order))]
        [HttpPost]
        [Route("recover")]
        public IHttpActionResult PostRecoverOrder(int id)
        {

            Order pedido = db.Orders.Find(id);

            if (pedido == null)
            {
                return NotFound();
            }

            if ((User.Identity.Name == pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                Order pedidoModificado = db.Orders.Find(id);
                if (pedidoModificado.Status.Equals("fechado") || pedidoModificado.Status.Equals("cancelado"))
                {
                    pedidoModificado.Status = "novo";
                    db.Entry(pedidoModificado).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderExists(id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return Ok(pedidoModificado);
                }
                else
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
        }

        // GET: api/Orders
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            return db.Orders.Include(order => order.OrderItems).ToList();
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [Authorize]
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            order.Status = "novo";
            order.ItemWeight = 0;
            order.precoFrete = "0";
            order.ItemPrice = 0;
            order.DateOrder = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).ToLocalTime();
            order.DateDelivery = "";
            order.userEmail = User.Identity.Name;
            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // POST: api/Orders/5
        [Authorize]
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrderStatus(int id)
        {

            Order pedido = db.Orders.Find(id);

            if (pedido == null)
            {
                return NotFound();
            }

            if (pedido.precoFrete == "0")
            {
                return StatusCode(HttpStatusCode.BadRequest);
            }

            if ((User.Identity.Name == pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                Order pedidoModificado = db.Orders.Find(id);
                if (pedidoModificado.Status.Equals("novo") || pedidoModificado.Status.Equals("cancelado"))
                {
                    pedidoModificado.Status = "fechado";
                    db.Entry(pedidoModificado).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!OrderExists(id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return Ok(pedidoModificado);
                }
                else
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {

            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if ((User.Identity.Name == order.userEmail) || User.IsInRole("ADMIN"))
            {
                db.Orders.Remove(order);
                db.SaveChanges();

                return Ok(order);
            }
            else
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}