[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly YourDbContext _context;

    public TripsController(YourDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
    {
        return await _context.Trips.OrderByDescending(t => t.StartDate).ToListAsync();
    }

    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientDto clientDto)
    {
        var trip = await _context.Trips.FindAsync(idTrip);
        if (trip == null)
        {
            return NotFound("Trip not found.");
        }

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientDto.Pesel);
        if (client == null)
        {
            client = new Client
            {
                Pesel = clientDto.Pesel,
                // inne w³aœciwoœci z clientDto
            };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        if (await _context.ClientTrips.AnyAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip))
        {
            return BadRequest("Client already assigned to this trip.");
        }

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientDto.PaymentDate
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClientTrip), new { idClient = client.IdClient, idTrip = idTrip }, clientTrip);
    }

    // Metoda pomocnicza do uzyskania szczegó³ów przypisania klienta do wycieczki (opcjonalnie)
    [HttpGet("{idTrip}/clients/{idClient}")]
    public async Task<ActionResult<ClientTrip>> GetClientTrip(int idTrip, int idClient)
    {
        var clientTrip = await _context.ClientTrips.FindAsync(idClient, idTrip);

        if (clientTrip == null)
        {
            return NotFound();
        }

        return clientTrip;
    }
}

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly YourDbContext _context;

    public ClientsController(YourDbContext context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients.Include(c => c.ClientTrips).FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
        {
            return NotFound();
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest("Client has assigned trips, cannot delete.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}