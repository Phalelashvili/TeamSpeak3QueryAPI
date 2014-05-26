﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamSpeak3QueryApi.Net
{
    internal class QueryCommand
    {
        public string Command { get; private set; }
        public string[] Options { get; private set; }
        public IReadOnlyCollection<Parameter> Parameters { get; private set; }
        public string SentText { get; private set; }
        public TaskCompletionSource<QueryResponseDictionary[]> Defer { get; private set; }

        public string RawResponse { get; set; }
        public QueryResponseDictionary[] ResponseDictionary { get; set; }
        public QueryError Error { get; set; }

        public QueryCommand(string cmd, IReadOnlyCollection<Parameter> parameters, string[] options, TaskCompletionSource<QueryResponseDictionary[]> defer, string sentText)
        {
            Command = cmd;
            Parameters = parameters;
            Options = options;
            Defer = defer;
            SentText = sentText;
        }
    }
}