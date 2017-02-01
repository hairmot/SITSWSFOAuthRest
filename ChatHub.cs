using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WSFOAUTHREST
{
    public class ChatHub : Hub
    {
        public void Update(string student)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(student);
        }
    }
}