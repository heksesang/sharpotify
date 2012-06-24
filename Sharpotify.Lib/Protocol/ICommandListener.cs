using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Protocol
{
    public interface ICommandListener
    {
        void CommandReceived(int command, byte[] payload);
    }
}
