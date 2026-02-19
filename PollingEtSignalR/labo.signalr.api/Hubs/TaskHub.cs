using System.Threading.Tasks;
using labo.signalr.api.Data;
using Microsoft.AspNetCore.SignalR;

namespace labo.signalr.api.Hubs
{

    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }

    public class TaskHub : Hub
    {
        ApplicationDbContext _context;

        public TaskHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);

            await Clients.Caller.SendAsync("Tasklist", _context.UselessTasks.ToList());
            await Clients.All.SendAsync("UserCount", UserHandler.ConnectedIds.Count);
            await base.OnConnectedAsync();


        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);

            await Clients.All.SendAsync("UserCount", UserHandler.ConnectedIds.Count);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task CompleteTask(int taskid)
        {
            var task = _context.UselessTasks.Single(t => t.Id == taskid);
            task.Completed = true;
            _context.SaveChanges();
            await Clients.All.SendAsync("Tasklist", _context.UselessTasks.ToList());
        }
        public async Task AddTask(string task)
        {
            _context.UselessTasks.Add(new Models.UselessTask() { Text = task });
            _context.SaveChanges();
            await Clients.All.SendAsync("Tasklist", _context.UselessTasks.ToList());

        }

    }
}
