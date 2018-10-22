import React from "react";
import gql from "graphql-tag";
import { Query } from "react-apollo";

const myPermissionsQuery = gql`
    query MyPermissions {
        me { 
            id 
            permissions 
        }
    }
`;

export default ({name, children}) => {
    return (
        <Query query={myPermissionsQuery} pollInterval={30 * 1000}>
            {
                ({data}) => {
                    if (data.me && data.me.permissions.indexOf(name) >= 0) {
                        return children;
                    } else {
                        return null;
                    }
                }
            }
        </Query>
    );
}