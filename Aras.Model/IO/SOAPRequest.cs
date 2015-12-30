﻿/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.IO;

namespace Aras.Model.IO
{
    internal enum SOAPOperation { ValidateUser, ApplyItem, ApplyAML };

    internal class SOAPRequest
    {
        internal SOAPOperation Operation { get; private set; }

        internal Database Database { get; private set; }

        internal String Username { get; private set; }

        internal String Password { get; private set; }

        internal IEnumerable<Item> Items { get; private set; }

        private HttpWebRequest _request;
        private HttpWebRequest Request
        {
            get
            {
                if (this._request == null)
                {
                    this._request = (HttpWebRequest)WebRequest.Create(this.Database.Server.URL + "/Server/InnovatorServer.aspx");
                    this._request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    this._request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    this._request.Headers.Add("Cache-Control", "no-cache");
                    this._request.Method = "POST";
                    this._request.ContentType = "text/xml; charset=utf-8";
                    this._request.Headers.Add("AUTHPASSWORD", this.Password);
                    this._request.Headers.Add("AUTHUSER", this.Username);
                    this._request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    this._request.Headers.Add("DATABASE", this.Database.Name);
                    this._request.Headers.Add("SOAPACTION", this.Operation.ToString());
                    this._request.Headers.Add("TIMEZONE_NAME", "GMT Standard Time");

                    // Get bytes for SOAP Header
                    String headerstring = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><" + this.Operation.ToString() + ">";

                    if (this.Operation == SOAPOperation.ApplyAML)
                    {
                        headerstring += "<AML>";
                    }

                    byte[] header = System.Text.Encoding.ASCII.GetBytes(headerstring);
                    
                    // Get bytes for SOAP Data
                    List<byte[]> datalist = new List<byte[]>();
                    int datalength = 0;

                    if (this.Items != null)
                    {
                        foreach (Item item in this.Items)
                        {
                            byte[] thisdata = item.GetBytes();
                            datalength += thisdata.Length;
                            datalist.Add(thisdata);
                        }
                    }

                    // Get Bytes for SOAP Footer
                    String footerstring = "</" + this.Operation.ToString() + "></SOAP-ENV:Body></SOAP-ENV:Envelope>";

                    if (this.Operation == SOAPOperation.ApplyAML)
                    {
                        footerstring = "</AML>" + footerstring;
                    }

                    byte[] footer = System.Text.Encoding.ASCII.GetBytes(footerstring);

                    // Write SOAP Message to Request and update length
                    this._request.ContentLength = header.Length + datalength + footer.Length;

                    using (Stream poststream = this._request.GetRequestStream())
                    {
                        poststream.Write(header, 0, header.Length);

                        foreach (byte[] data in datalist)
                        {
                            poststream.Write(data, 0, data.Length);
                        }

                        poststream.Write(footer, 0, footer.Length);
                    }
                }

                return this._request;
            }
        }

        internal SOAPResponse Execute()
        {
            try
            {
                using (WebResponse webresponse = this.Request.GetResponse())
                {
                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        return new SOAPResponse(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }
        }

        internal async Task<SOAPResponse> ExecuteAsync()
        {
            try
            {
                using (Task<WebResponse> task = this.Request.GetResponseAsync())
                {
                    WebResponse webresponse = await task;

                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        return new SOAPResponse(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }
        }

        internal SOAPRequest(SOAPOperation Operation, Session Session, Item Item)
        {
            this.Operation = Operation;
            this.Database = Session.Database;
            this.Username = Session.Username;
            this.Password = Session.Password;
            this.Items  = new List<Item>() { Item };
        }

        internal SOAPRequest(SOAPOperation Operation, Session Session, IEnumerable<Item> Items)
        {
            this.Operation = Operation;
            this.Database = Session.Database;
            this.Username = Session.Username;
            this.Password = Session.Password;
            this.Items = Items;
        }

        internal SOAPRequest(SOAPOperation Operation, Database Database, String Username, String Password)
        {
            this.Operation = Operation;
            this.Database = Database;
            this.Username = Username;
            this.Password = Password;
            this.Items = null;
        }
    }
}