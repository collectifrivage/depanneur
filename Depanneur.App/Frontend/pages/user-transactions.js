import gql from "graphql-tag";
import React from "react";
import { Query } from "react-apollo";

import TransactionsList from "../components/transactions-list";
import Loading from "../components/loading-indicator";
import Paging from "../components/paging";
import {Checkbox} from "../components/forms";


const transactionsQuery = gql`
  query UserTransactions($user: String, $types: [TransactionType], $page: Int) {
    user(id:$user) {
      id
      name
      transactions(types: $types, page: $page) {
        currentPage
        pageCount
        items {
          __typename
          id
          timestamp
          amount
          newBalance
          ... on Purchase {
            quantity
            pricePerUnit
            itemName
            wasFromSubscription
            canDelete
          }
          ... on Adjustment {
            description
          }
        }
      }
    }
  }
`;

class MyTransactions extends React.Component {

    constructor() {
        super();

        this.state = {
            currentPage: 1,
            showPurchases: true,
            showPayments: true,
            showAdjustments: true
        }
    }

    render() {
        let types = [];
        if (this.state.showPurchases) types.push("PURCHASE");
        if (this.state.showPayments) types.push("PAYMENT");
        if (this.state.showAdjustments) types.push("ADJUSTMENT");

        return (
            <Query query={transactionsQuery} variables={{user: this.props.match.params.userid, types, page: this.state.currentPage}} fetchPolicy="cache-and-network">
                {
                    ({error, data}) => {
                        if (error && (!data.user || data.user.transactions.currentPage != this.state.currentPage)) { return <div>Erreur</div> }
                        if (!data.user || data.user.transactions.currentPage != this.state.currentPage) return <Loading />;

                        return (
                            <div>
                                <h1>Transactions de {data.user.name}</h1>
                
                                <p>
                                    Afficher:
                                    <Checkbox label="Achats" checked={this.state.showPurchases} onChecked={c => this.setState({showPurchases: c, currentPage: 1})} />
                                    <Checkbox label="Paiements" checked={this.state.showPayments} onChecked={c => this.setState({showPayments: c, currentPage: 1})} />
                                    <Checkbox label="Ajustements" checked={this.state.showAdjustments} onChecked={c => this.setState({showAdjustments: c, currentPage: 1})} />
                                </p>
                
                                <TransactionsList transactions={data.user.transactions.items} />
                                {
                                    data.user.transactions.pageCount > 1
                                        ? <Paging style={{marginTop: 20}} 
                                                  current={this.state.currentPage} 
                                                  count={data.user.transactions.pageCount}
                                                  onPageChanged={p => this.setState({currentPage: p})} />
                                        : null
                                }
                            </div>
                        );
                    }
                }
            </Query>
        );
    }

}

export default MyTransactions;