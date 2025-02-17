using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IChatServices
    {
        Task StreamChatAsync(string userInput, Func<string, Task> onChunkReceived, CancellationToken cancellationToken);

    }
}
