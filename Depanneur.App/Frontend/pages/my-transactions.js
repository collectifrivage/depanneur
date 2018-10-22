import gql from "graphql-tag";
import React from "react";
import { Query, Mutation } from "react-apollo";

import { Checkbox } from "../components/forms";
import Loading from "../components/loading-indicator";
import Paging from "../components/paging";
import TransactionsList from "../components/transactions-list";
import { describeTransaction } from "../helpers/transaction-helper";


const transactionsQuery = gql`
  query MyTransactions($types: [TransactionType], $page: Int) {
    me {
      id
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
const cancelPurchaseMutation = gql`
  mutation CancelPurchase($purchaseId: Int!) {
    cancelPurchases(purchaseIds: [$purchaseId]) {
      user {
        id
        balance
      }
      products {
        id
        stats {
          total
          month
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
            <div>
                <h1>Mes transactions</h1>

                <p>
                    Afficher:
                    <Checkbox label="Achats" checked={this.state.showPurchases} onChecked={c => this.setState({showPurchases: c, currentPage: 1})} />
                    <Checkbox label="Paiements" checked={this.state.showPayments} onChecked={c => this.setState({showPayments: c, currentPage: 1})} />
                    <Checkbox label="Ajustements" checked={this.state.showAdjustments} onChecked={c => this.setState({showAdjustments: c, currentPage: 1})} />
                </p>

                <Query query={transactionsQuery} variables={{types, page: this.state.currentPage}} fetchPolicy="cache-and-network">
                    {
                        ({error, data, refetch}) => {
                            if (error && (!data || !data.me || data.me.transactions.currentPage != this.state.currentPage)) { return <div>Erreur</div> }
                            if (!data.me || data.me.transactions.currentPage != this.state.currentPage) return <Loading />;

                            return (
                                <div>
                                    <Mutation mutation={cancelPurchaseMutation}>
                                        {
                                            (mutate, {loading}) => {
                                                if (loading) return null;

                                                return (<TransactionsList transactions={data.me.transactions.items} onDelete={t => this.onDelete(t, mutate).then(refetch)} />);
                                            }
                                        }
                                    </Mutation>
                                    {data.me.transactions.pageCount > 1
                                        ? <Paging style={{marginTop: 20}} 
                                                  current={this.state.currentPage} 
                                                  count={data.me.transactions.pageCount}
                                                  onPageChanged={p => this.setState({currentPage: p})} />
                                        : null
                                    }
                                </div>
                            )
                        }
                    }
                </Query>
            </div>
        )
    }

    onDelete(transaction, mutate) {
        if (!confirm("Supprimer cette transaction ?\n" + describeTransaction(transaction))) return Promise.reject();
        
        return mutate({variables: {purchaseId: transaction.id}});
    }

}

export default MyTransactions;