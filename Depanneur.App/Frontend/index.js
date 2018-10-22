import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter as Router } from "react-router-dom";
import ApolloClient, { InMemoryCache, IntrospectionFragmentMatcher } from "apollo-boost";
import { ApolloProvider } from "react-apollo";

import App from "./app";

// TODO: Setup build step to auto-generate this configuration (see https://www.apollographql.com/docs/react/advanced/fragments.html)
const fragmentMatcher = new IntrospectionFragmentMatcher({
    introspectionQueryResultData: {
        "__schema": {
            "types": [
                {
                    "kind": "INTERFACE",
                    "name": "TransactionInterface",
                    "possibleTypes": [
                        {
                            "name": "Purchase"
                        },
                        {
                            "name": "Payment"
                        },
                        {
                            "name": "Adjustment"
                        }
                    ]
                }
            ]
        }
    }
});
const cache = new InMemoryCache({ fragmentMatcher });
const client = new ApolloClient({
    uri: "/graphql",
    cache
});

ReactDOM.render(
    <Router>
        <ApolloProvider client={client}>
            <App />
        </ApolloProvider>
    </Router>,
    document.getElementById("app")
);