using Azure.Core;
using Dynamics.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dynamics.Utility;
using Request = Dynamics.Models.Models.Request;

namespace Dynamics.DataAccess.Repository
{
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public RequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Request entity)
        {
            _db.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Request entity)
        {
            var request = await _db.Requests.FirstOrDefaultAsync(r => r.RequestID == entity.RequestID);
            if (request != null)
            {
                _db.Remove(request);
                await _db.SaveChangesAsync();
            }
        }

        public IQueryable<Request> SearchIdFilter(string searchQuery, string filterQuery, Guid userId)
        {
            var requests = _db.Requests.Include(u => u.User).Where(r => r.RequestID == userId);
            switch (filterQuery)
            {
                case "All":
                    requests = _db.Requests
                        .Where(r => r.RequestTitle.Contains(searchQuery) || r.Content.Contains(searchQuery) ||
                                    r.Location.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
                case "Title":
                    requests = _db.Requests
                        .Where(r => r.RequestTitle.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
                case "Location":
                    requests = _db.Requests
                        .Where(r => r.Location.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
                case "Content":
                    requests = _db.Requests
                        .Where(r => r.Content.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
            }

            return requests;
        }

        public Task<IQueryable<Request>> SearchIndexFilterAsync(string searchQuery, string filterQuery)
        {
            var requests = _db.Requests.AsQueryable();
            switch (filterQuery)
            {
                case "All":
                    requests = _db.Requests
                        .Where(r => r.RequestTitle.Contains(searchQuery) || r.Content.Contains(searchQuery) ||
                                    r.Location.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
                case "Title":
                    requests = _db.Requests
                        .Where(r => r.RequestTitle.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
                case "Location":
                    requests = _db.Requests
                        .Where(r => r.Location.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
                case "Content":
                    requests = _db.Requests
                        .Where(r => r.Content.Contains(searchQuery))
                        .OrderBy(r => r.CreationDate);
                    break;
            }

            return Task.FromResult(requests);
        }

        public async Task<Request> GetAsync(Expression<Func<Request, bool>> filter)
        {
            var query = await _db.Requests.Include(x => x.User).Include(x => x.Project).Where(filter)
                .FirstOrDefaultAsync();
            return query;
        }

        public async Task<List<Request>> GetRequestsAsync()
        {
            // var test = _db.Requests.ToList();
            return await _db.Requests.Include(r => r.User).ToListAsync();
        }

        public IQueryable<Request> GetAllQueryable(Expression<Func<Request, bool>>? predicate = null)
        {
            if (predicate != null)
            {
                return _db.Requests.Where(predicate)
                    .Include(x => x.User)
                    .OrderByDescending(r => r.CreationDate)
                    .AsQueryable();
            }

            var requests = _db.Requests
                .Include(r => r.User)
                .OrderByDescending(r => r.CreationDate)
                .AsQueryable();
            return (requests);
        }

        public int CountRequests(Expression<Func<Request, bool>>? predicate)
        {
            if (predicate != null)
            {
                return _db.Requests.Where(predicate).Count();
            }

            return _db.Requests.Count();
        }

        public async Task UpdateAsync(Request entity)
        {
            var request = await GetAsync(r => entity.RequestID == r.RequestID);
            if (request != null)
            {
                _db.Requests.Update(request);
                await _db.SaveChangesAsync();
            }
        }

        public IQueryable<Request> GetAllById(Guid id)
        {
            var query = _db.Requests.Where(r => r.UserID == id)
                .Include(u => u.User);
            return (query);
        }

        public async Task<List<Request>> PaginateAsync(IQueryable<Request> requestQuery, int pageNumber, int pageSize)
        {
            return await requestQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Request> GetByIdAsync(Expression<Func<Request, bool>> filter, string role, Guid id)
        {
            var query = _db.Requests.Include(r => r.User).Where(filter).AsQueryable();
            var request = await query.Where(filter).FirstOrDefaultAsync();
            if (role == RoleConstants.User && request.UserID == id)
            {
                return request;
            }
            else if (role == "Admin")
            {
                return request;
            }

            return null;
        }

        public Task<IQueryable<Request>> GetRequestDateFilterAsync(IQueryable<Request> requests, DateOnly dateFrom,
            DateOnly dateTo)
        {
            var datetimeFrom = DateTime.Parse(dateFrom.ToString("yyyy-MM-dd"));
            var datetimeTo = DateTime.Parse(dateTo.ToString("yyyy-MM-dd"));
            return Task.FromResult(requests.Where(r => r.CreationDate >= datetimeFrom && r.CreationDate <= datetimeTo));
        }
    }
}