﻿namespace etcetera
{
    using System;
    using System.Linq;
    using RestSharp;

    public class EtcdClient
    {
        readonly IRestClient _client;
        readonly Uri _root;
        readonly Uri _keysRoot;

        public EtcdClient(Uri etcdLocation)
        {
            var uriBuilder = new UriBuilder(etcdLocation)
            {
                Path = ""
            };
            _root = uriBuilder.Uri;
            _keysRoot = new Uri(_root, "/v2/keys");
            _client = new RestClient(_root.ToString());
        }

        //ttl overload
        public EtcdResponse Set(string key, object value)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var putRequest = new RestRequest(requestUrl, Method.PUT);
            putRequest.AddParameter("value", value);

            var response = _client.Execute<EtcdResponse>(putRequest);
            return response.Data;
        }

        public EtcdResponse Get(string key)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var getRequest = new RestRequest(requestUrl, Method.GET);

            var response = _client.Execute<EtcdResponse>(getRequest);
            return response.Data;
        }

        public EtcdResponse Delete(string key)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var getRequest = new RestRequest(requestUrl, Method.DELETE);

            var response = _client.Execute<EtcdResponse>(getRequest);
            return response.Data;
        }

        //watch is where i need to level myself up
    }

    public class EtcdResponse
    {
        public string Action { get; set; }
        public Node Node { get; set; }
    }
    public static class EtcResponseHelpers
    {
        public static int EtcIndex(this IRestResponse response)
        {
            return (int)response.Headers.First(x=>x.Name == "X-Etcd-Index").Value;
        }

        public static int EtcRaftIndex(this IRestResponse response)
        {
            return (int)response.Headers.First(x=>x.Name == "X-Raft-Index").Value;
        }

        public static int EtcRaftTerm(this IRestResponse response)
        {
            return (int)response.Headers.First(x => x.Name == "X-Raft-Term").Value;
        }
    }
    public class Node
    {
        public int CreatedIndex { get; set; }
        public string Key { get; set; }
        public int ModifiedIndex { get; set; }
        public string Value { get; set; }
    }
    public static class UriHelpers
    {
        public static Uri AppendPath(this Uri uri, string path)
        {
            var path1 = uri.AbsolutePath.TrimEnd(new []
            {
                '/'
            }) + "/" + path;
            return new UriBuilder(uri.Scheme, uri.Host, uri.Port, path1, uri.Query).Uri;
        }
    }
}