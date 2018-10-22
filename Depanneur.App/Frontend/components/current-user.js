import gql from "graphql-tag";
import React from "react";
import { Query } from "react-apollo";

import Amount from "./amount";
import LinkButton from "./link-button";

const currentUserQuery = gql`
    query CurrentUser {
        me {
            id
            name
            balance
        }
    }
`;

class CurrentUser extends React.Component {
    
    render() {
        const TEN_MINUTES = 10 * 60 * 1000;

        return (
            <Query query={currentUserQuery} pollInterval={TEN_MINUTES}>
                {
                    ({ data }) => {
                        if (!data.me) return null;
                        
                        return (
                            <div>
                                <span>{data.me.name}</span> (<LinkButton onClick={this.handleLogout}>logout</LinkButton>)
                                {data.me.balance !== null ? <div>Solde: <Amount amount={data.me.balance} /></div> : null}
                            </div>
                        );
                    }
                }
            </Query>
        );
    }

    handleLogout = () => {
        window.location.href = "/account/logout";
    }
}

export default CurrentUser;