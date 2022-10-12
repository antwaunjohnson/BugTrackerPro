﻿using BugTrackerPro.Models;

namespace BugTrackerPro.Services.Interfaces;

public interface IBTProTicketHistoryService
{
    Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);

    Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId);

    Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);
}
