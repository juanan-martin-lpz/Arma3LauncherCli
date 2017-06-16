using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServerWebManager.Models;

namespace ServerWebManager.Controllers
{
    public class ServersController : ApiController
    {
        private DataModel db = new DataModel();

        // GET: api/Servers
        public IQueryable<ServerSet> GetServerSet()
        {
            return db.ServerSet;
        }

        // GET: api/Servers/5
        [ResponseType(typeof(ServerSet))]
        public async Task<IHttpActionResult> GetServerSet(int id)
        {
            ServerSet serverSet = await db.ServerSet.FindAsync(id);
            if (serverSet == null)
            {
                return NotFound();
            }

            return Ok(serverSet);
        }

        // PUT: api/Servers/5
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Administradores")]
        public async Task<IHttpActionResult> PutServerSet(int id, ServerSet serverSet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != serverSet.Id)
            {
                return BadRequest();
            }

            db.Entry(serverSet).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServerSetExists(id))
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

        // POST: api/Servers
        [ResponseType(typeof(ServerSet))]
        [Authorize(Roles = "Administradores")]
        public async Task<IHttpActionResult> PostServerSet(ServerSet serverSet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ServerSet.Add(serverSet);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = serverSet.Id }, serverSet);
        }

        // DELETE: api/Servers/5
        [ResponseType(typeof(ServerSet))]
        [Authorize(Roles = "Administradores")]
        public async Task<IHttpActionResult> DeleteServerSet(int id)
        {
            ServerSet serverSet = await db.ServerSet.FindAsync(id);
            if (serverSet == null)
            {
                return NotFound();
            }

            db.ServerSet.Remove(serverSet);
            await db.SaveChangesAsync();

            return Ok(serverSet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ServerSetExists(int id)
        {
            return db.ServerSet.Count(e => e.Id == id) > 0;
        }
    }
}