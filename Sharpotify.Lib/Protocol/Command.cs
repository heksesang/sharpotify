using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Protocol
{
    public class Command 
    {
        /* Core functionality. */
	    public const int COMMAND_SECRETBLK    = 0x02;
	    public const int COMMAND_PING         = 0x04;
	    public const int COMMAND_GETSUBSTREAM = 0x08;
	    public const int COMMAND_CHANNELDATA  = 0x09;
	    public const int COMMAND_CHANNELERR   = 0x0a;
	    public const int COMMAND_CHANNELABRT  = 0x0b;
	    public const int COMMAND_REQKEY       = 0x0c;
	    public const int COMMAND_AESKEY       = 0x0d;
	    public const int COMMAND_AESKEYERR    = 0x0e;
	    public const int COMMAND_CACHEHASH    = 0x0f;
	    public const int COMMAND_SHAHASH      = 0x10;
	    public const int COMMAND_IMAGE        = 0x19;

	    /* Rights management. */
	    public const int COMMAND_COUNTRYCODE = 0x1b;

	    /* P2P related. */
	    public const int COMMAND_P2P_SETUP   = 0x20;
	    public const int COMMAND_P2P_INITBLK = 0x21;

	    /* Search and metadata. */
	    public const int COMMAND_BROWSE          = 0x30;
	    public const int COMMAND_SEARCH_OLD      = 0x31;
	    public const int COMMAND_PLAYLISTCHANGED = 0x34;
	    public const int COMMAND_GETPLAYLIST     = 0x35;
	    public const int COMMAND_CHANGEPLAYLIST  = 0x36;
	    public const int COMMAND_GETTOPLIST      = 0x38;
	    public const int COMMAND_SEARCH          = 0x39;

	    /* Session management. */
	    public const int COMMAND_NOTIFY      = 0x42;
	    public const int COMMAND_LOG         = 0x48;
	    public const int COMMAND_PONG        = 0x49;
	    public const int COMMAND_PONGACK     = 0x4a;
	    public const int COMMAND_PAUSE       = 0x4b;
	    public const int COMMAND_REQUESTAD   = 0x4e;
	    public const int COMMAND_REQUESTPLAY = 0x4f;

	    /* Internal. */
	    public const int COMMAND_PRODINFO = 0x50;
        public const int COMMAND_WELCOME = 0x69;
    }
}
