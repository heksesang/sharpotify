using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sharpotify.Media.Parser
{
    public class XMLUserParser : XMLParser
    {
        #region Factory
        /// <summary>
        /// Create a new stream parser from the given input stream.
        /// </summary>
        /// <param name="stream">An stream to parse.</param>
        private XMLUserParser(Stream stream) : base(stream)
        {
	    }
        #endregion
        #region Private Methods
        /// <summary>
        /// Parse the input stream as a <see cref="User"/> object.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>An <see cref="User"/> object.</returns>
        private User Parse(User user)
        {
            string name;

            /* Check if reader is currently on a start element. */
            if (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Check current element name and start parsing it. */
                if (name.Equals("products"))
                {
                    return this.ParseUser(user);
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }
            }

            throw new XMLParserException("Reader is not on a start element!");
        }
        private User ParseUser(User user) 
        {
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

			    if(name.Equals("product")){
				    this.ParseProduct(user);
			    }
			    else if(name.Equals("token")){
				    this.GetElementString(); /* Skip. */
			    }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
		    }

		    return user;
	    }
        private void ParseProduct(User user)
        { 
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;
                user.Properties.Add(name, this.GetElementString());
                this.Next();
            }
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Parse <code>xml</code> into an object.
        /// </summary>
        /// <param name="data">The xml as bytes.</param>
        /// <param name="user"></param>
        /// <returns>An object if successful, null if not.</returns>
        public static User Parse(byte[] data, User user)
        {
            try
            {
                XMLUserParser parser = new XMLUserParser(new MemoryStream(data));

                return parser.Parse(user);
            }
            catch (XMLParserException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Parse <code>xml</code> into an <see cref="User"/>.
        /// </summary>
        /// <param name="data">The xml as bytes.</param>
        /// <param name="user"></param>
        /// <returns>A <see cref="User"/> object if successful, null if not.</returns>
        public static User ParseUser(byte[] data, User user)
        {
            return Parse(data, user);
        }
        #endregion
    }
}
